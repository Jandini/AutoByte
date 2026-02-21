# AutoByte Design Document

## Overview

AutoByte is a high-performance .NET library for deserializing binary data structures. It combines C# source generators with a zero-allocation parsing utility to provide compile-time code generation and runtime efficiency for binary protocol parsing.

## Architecture

### Layered Design

```
┌─────────────────────────────────────────────────────┐
│  User Code (decorated classes)                      │
├─────────────────────────────────────────────────────┤
│  AutoByte Core Library                              │
│  ┌─────────────────────────────────────────────┐    │
│  │ IByteStructure Interface                    │    │
│  │ [AutoByteStructure] Attribute               │    │
│  │ [AutoByteField] Attribute                   │    │
│  │ [AutoByteString] Attribute                  │    │
│  └─────────────────────────────────────────────┘    │
├─────────────────────────────────────────────────────┤
│  ByteSlide - Parsing Utility                        │
│  ┌─────────────────────────────────────────────┐    │
│  │ ReadOnlySpan<byte> wrapper                  │    │
│  │ Position tracking                           │    │
│  │ Primitive type readers                      │    │
│  │ Array/String readers                        │    │
│  │ Structure recursion support                 │    │
│  └─────────────────────────────────────────────┘    │
├─────────────────────────────────────────────────────┤
│  AutoByte.Generators (Compile-Time)                 │
│  ┌─────────────────────────────────────────────┐    │
│  │ ISourceGenerator Implementation             │    │
│  │ Syntax tree analysis                        │    │
│  │ Code generation logic                       │    │
│  └─────────────────────────────────────────────┘    │
├─────────────────────────────────────────────────────┤
│  System.Buffers.Binary (BinaryPrimitives)           │
│  ReadOnlySpan<byte> (Framework)                     │
└─────────────────────────────────────────────────────┘
```

## Core Components

### 1. IByteStructure Interface

**Location:** `AutoByte/IByteStructure.cs`

**Purpose:** Contract for deserialization implementations

```csharp
public partial interface IByteStructure
{
    /// <summary>
    /// Deserialize bytes using byte slide.
    /// </summary>
    /// <param name="slide">Byte slide deserializer</param>
    /// <returns>Return the size of the structure to move the byte pointer by the size 
    /// of the structure. Return zero to skip moving the byte pointer.</returns>
    int Deserialize(ref ByteSlide slide);
}
```

**Design Rationale:**
- Ref parameter for ByteSlide allows position tracking without reference types
- Return value enables flexible pointer movement (0 for manual, or size for automatic skip)
- Partial interface allows extension through partial class declarations

### 2. ByteSlide - The Parsing Engine

**Location:** `AutoByte/ByteSlide.cs`

**Type:** `ref struct` (stack-allocated, cannot be boxed or stored in fields)

**Key Design Decisions:**

#### Zero-Allocation Performance
- Uses `ReadOnlySpan<byte>` instead of managed arrays
- Ref struct allocation on stack instead of heap
- Slicing with `[..]` syntax creates new span views without copying data

#### Position Tracking
```csharp
private ReadOnlySpan<byte> _slide;

public ReadOnlySpan<byte> Slide(int size)
{
    if (size > _slide.Length)
        throw new ByteSlideException(...);
    
    var slice = _slide[..size];      // Get first 'size' bytes
    _slide = _slide[size..];          // Advance pointer
    return slice;
}
```

#### Little-Endian and Big-Endian Support
- Delegates to `System.Buffers.Binary.BinaryPrimitives`
- Both conversion methods provided:
  - `GetInt32LittleEndian()` / `GetInt32BigEndian()`
  - Generic versions: `GetInt32LittleEndian<T>()`

#### Method Optimization
- All reader methods marked with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- JIT compiler inlines these methods for maximum performance
- Reduces method call overhead for tight loops

### 3. Attribute System

#### AutoByteStructure Attribute

**Location:** `AutoByte/AutoByteStructureAttribute.cs`

Marks a `partial class` for code generation.

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class AutoByteStructureAttribute : Attribute
{
}
```

**Usage Pattern:**
```csharp
[AutoByteStructure]
public partial class MyStructure
{
    public int Field1 { get; set; }
}
```

#### AutoByteField Attribute

**Location:** `AutoByte/AutoByteFieldAttribute.cs`

Configures variable-length byte array parsing.

```csharp
public class AutoByteFieldAttribute : Attribute
{
    /// <summary>Skip number of bytes before reading the field.</summary>
    public int Skip { get; set; }

    /// <summary>Size of the field or length of the array.</summary>
    public int Size { get; set; }

    /// <summary>Use deserialized property value as the size of decorated field.</summary>
    public string SizeFromProperty { get; set; }
}
```

**Design Patterns:**

1. **Fixed Size:**
   ```csharp
   [AutoByteField(Size = 3)]
   public byte[] StartingCHS { get; set; }
   ```

2. **Dynamic Size from Previous Property:**
   ```csharp
   public short FileNameLength { get; set; }
   
   [AutoByteField(SizeFromProperty = "FileNameLength")]
   public byte[] FileName { get; set; }
   ```

3. **Skip Bytes:**
   ```csharp
   [AutoByteField(Skip = 4)]
   public byte[] Padding { get; set; }
   ```

#### AutoByteString Attribute

**Location:** `AutoByte/AutoByteStringAttribute.cs`

Specialized version of AutoByteField for UTF-8 strings.

Uses same configuration as AutoByteField but returns `string` instead of `byte[]`.

### 4. ByteExtensions Helper

**Location:** `AutoByte/ByteExtensions.cs`

**Purpose:** Convenience method for parsing structures from spans

```csharp
public static T GetStructure<T>(this ReadOnlySpan<byte> span) 
    where T : IByteStructure, new()
{
    var structure = new T();
    var slide = new ByteSlide(span);
    structure.Deserialize(ref slide);
    return structure;
}
```

**Usage:**
```csharp
byte[] data = GetBinaryData();
var result = data.GetStructure<MyStructure>();
```

## Source Generator Design

### AutoByteSourceGenerator

**Location:** `AutoByte.Generators/AutoByteSourceGenerator.cs`

**Type:** Roslyn `ISourceGenerator` implementation

**Execution Pipeline:**

```
1. Initialize
   └─ Register SyntaxReceiver for syntax tree notifications

2. Execute
   ├─ Get all candidate classes decorated with [AutoByteStructure]
   ├─ Build semantic model for each class
   ├─ Extract properties and analyze attributes
   ├─ Generate Deserialize method implementation
   └─ Add to compilation as new source file
```

### Key Generation Phases

#### Phase 1: Syntax Reception
```csharp
public class SyntaxReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> CandidateClasses { get; }
    
    public void OnVisitSyntaxNode(SyntaxNode node)
    {
        // Collect all partial classes with [AutoByteStructure]
    }
}
```

**Purpose:** Pre-filter syntax tree to identify target classes before semantic analysis (performance optimization)

#### Phase 2: Semantic Analysis
- Resolve class symbols using semantic model
- Extract attribute information
- Identify property types and names
- Validate structure

#### Phase 3: Code Generation
- Build C# code as string
- Use StringBuilder for efficiency
- Generate complete Deserialize method
- Handle nested structures recursively

### Generated Code Example

**Input:**
```csharp
[AutoByteStructure]
public partial class ZipFileHeader
{
    public int Signature { get; set; }
    public short VersionNeededToExtract { get; set; }
    
    [AutoByteString(SizeFromProperty = "FileNameLength")]
    public string FileName { get; set; }
}
```

**Generated Output:**
```csharp
public partial class ZipFileHeader : IByteStructure
{
    public int Deserialize(ref ByteSlide slide)
    {
        Signature = slide.GetInt32LittleEndian();
        VersionNeededToExtract = slide.GetInt16LittleEndian();
        FileName = slide.GetUtf8String(FileNameLength);
        return 0;
    }
}
```

### Generation Strategy

**Type Mapping:**
| C# Type | ByteSlide Method |
|---------|------------------|
| `byte` | `GetByte()` |
| `short` | `GetInt16LittleEndian()` |
| `int` | `GetInt32LittleEndian()` |
| `long` | `GetInt64LittleEndian()` |
| `ushort` | `GetUInt16LittleEndian()` |
| `uint` | `GetUInt32LittleEndian()` |
| `ulong` | `GetUInt64LittleEndian()` |
| `byte[]` | `Slide().ToArray()` (with size) |
| `string` | `GetUtf8String()` (with size) |
| `T : IByteStructure` | `GetStructure<T>()` |

## Design Patterns Used

### 1. Source Generator Pattern
Uses Roslyn APIs to generate code at compile-time, eliminating runtime reflection and improving performance.

**Benefit:** Zero-cost abstractions, type safety verified at compile-time

### 2. Ref Struct Pattern
`ByteSlide` uses ref struct to remain stack-allocated, preventing accidental heap allocations.

**Benefit:** Predictable memory layout, no GC pressure for tight parsing loops

### 3. Extension Method Pattern
`ByteExtensions.GetStructure<T>()` provides convenience API on `ReadOnlySpan<byte>`.

**Benefit:** Fluent API, discoverable through IntelliSense

### 4. Attribute-Driven Configuration
Properties can be configured declaratively via attributes.

**Benefit:** Declarative, separates concern from implementation

### 5. Partial Class Extension
Generated code extends user-written partial classes.

**Benefit:** Clean separation, generated code invisible in editor, but still modifiable

## Data Flow

### Deserialization Flow

```
Binary Data (byte[])
    ↓
ByteSlide (ref struct, ReadOnlySpan<byte>)
    ├─ Slide(4) → reads 4 bytes, advances position
    ├─ GetInt32LittleEndian() → Slide(4) + BinaryPrimitives conversion
    ├─ GetUtf8String(len) → Slide(len) + UTF-8 decode
    └─ GetStructure<T>() → Deserialize(ref this)
    ↓
Generated Deserialize Method (IByteStructure)
    ├─ Signature = slide.GetInt32LittleEndian()
    ├─ Version = slide.GetInt16LittleEndian()
    ├─ FileName = slide.GetUtf8String(FileNameLength)
    └─ return 0
    ↓
User Object (T : IByteStructure)
    (all properties populated from binary data)
```

### Example: ZIP Header Parsing

```
Input: byte[] { 0x50, 0x4B, 0x03, 0x04, ... }
         └─ ZIP signature: 0x04034B50 (little-endian)

↓

var slide = new ByteSlide(data);
var header = slide.GetStructure<ZipFileHeader>();

↓

Generated Code:
    Signature = slide.GetInt32LittleEndian();  // reads bytes [0:4]
    Version = slide.GetInt16LittleEndian();    // reads bytes [4:6]
    // ... more properties ...

↓

Output: ZipFileHeader
{
    Signature = 0x04034B50,
    Version = 20,
    // ... properties populated ...
}
```

## Exception Handling

### ByteSlideException

**Location:** `AutoByte/ByteSlideException.cs`

Thrown when attempting to read beyond available bytes.

```csharp
public class ByteSlideException : Exception
{
    // Custom exception for buffer overrun detection
}
```

**Example:**
```csharp
var slide = new ByteSlide(new byte[10]);
slide.GetInt32LittleEndian();  // OK, 4 bytes available
slide.GetInt32LittleEndian();  // OK, 4 bytes available
slide.GetInt32LittleEndian();  // Throws: only 2 bytes left
```

## Performance Characteristics

### Time Complexity
- **Deserialization:** O(n) where n = total bytes to read
- **Attribute Resolution:** O(p) during generation, where p = properties
- **Code Generation:** O(p) where p = properties

### Space Complexity
- **ByteSlide:** O(1) - only stores span reference and length
- **Generated Code:** O(p) - one method with p statements
- **Runtime Memory:** O(n) for object properties, no intermediate allocations

### Optimization Techniques

1. **Method Inlining:** All ByteSlide readers marked `[MethodImpl(MethodImplOptions.AggressiveInlining)]`

2. **Stack Allocation:** ByteSlide is ref struct, never heap-allocated

3. **Zero-Copy Slicing:** `ReadOnlySpan<byte>[..]` creates views without copying

4. **Compile-Time Code Generation:** No reflection or dynamic invocation at runtime

## Extension Points

### Custom Deserialization

Users can implement `IByteStructure` manually for complex logic:

```csharp
public class ComplexStructure : IByteStructure
{
    public byte[] BootCode { get; set; }
    public PartitionEntry[] PartitionEntries { get; set; }

    public int Deserialize(ref ByteSlide slide)
    {
        // Manual parsing with custom logic
        BootCode = slide.GetByteArray(446);
        
        PartitionEntries = new PartitionEntry[4];
        for (int i = 0; i < 4; i++)
            PartitionEntries[i] = slide.GetStructure<PartitionEntry>();
        
        return 0;
    }
}
```

### Endianness Handling

ByteSlide provides both:
- `GetInt32LittleEndian()`
- `GetInt32BigEndian()`

Choose appropriate method for your binary format.

### Nested Structures

Structures can contain other structures:

```csharp
[AutoByteStructure]
public partial class Outer
{
    [AutoByteStructure]
    public partial class Inner
    {
        public int Value { get; set; }
    }
    
    public Inner Child { get; set; }  // Generated: GetStructure<Inner>()
}
```

## Compatibility

- **Target Frameworks:** .NET Standard 2.0, 2.1
- **C# Version:** 10 (for source generators)
- **Dependencies:**
  - `System.Buffers.Binary` (included in .NET Standard 2.0+)
  - Microsoft.CodeAnalysis.CSharp (only for generators)

## Testing Strategy

### Unit Tests in AutoByte.Tests

**Test Categories:**

1. **Code Generation Tests (`CodeGenerator_Must.cs`)**
   - Verify generated code produces correct deserialization
   - Example: ZIP header, Master Boot Record

2. **ByteSlide Tests (`ByteSlide_Must.cs`)**
   - Boundary conditions (reading at end of buffer)
   - Byte order conversions
   - String parsing

3. **Structure Tests (`ByteStructure_Must.cs`)**
   - End-to-end deserialization
   - Nested structures
   - Dynamic sizing with `SizeFromProperty`

4. **Real-World Format Tests**
   - Structures in `Structures/` folder (ZipFileHeader, MasterBootRecord, etc.)
   - Validates against actual binary file formats

## Future Enhancement Opportunities

1. **Big-Endian Code Generation:** Generate big-endian readers automatically
2. **Validation Attributes:** Add validation rules (min/max, enum checks)
3. **Offset Tracking:** Return byte offset information from Deserialize
4. **Streaming Support:** Parse from Stream instead of just byte arrays
5. **Serialization:** Generate serialization (Serialize method) to complement deserialization
6. **Compression Support:** Built-in DEFLATE/GZIP decompression
7. **Custom Type Converters:** Allow user-defined type mapping

## Summary

AutoByte combines three powerful concepts:

1. **C# Source Generators** for compile-time code generation
2. **Ref Structs** for zero-allocation parsing
3. **Binary Primitives** for efficient endianness-aware reading

The result is a type-safe, high-performance binary parsing library with zero runtime overhead and maximum developer ergonomics.
