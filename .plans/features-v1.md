# **D20Tek.Serialization v1.0 — Full Specification**

# **1. Overview**

The D20Tek.Serialization ecosystem provides a **format‑agnostic serialization platform** for .NET 10+, inspired by System.Text.Json and Rust Serde.

The v1.0 release includes:

- `D20Tek.Serialization.Core` — shared abstractions, metadata, converters, DOM, and source‑generator contracts.
- `D20Tek.Serialization.Binary` — a CBOR‑based binary serializer with:
  - AOT‑friendly source generation  
  - Reflection fallback  
  - Strict/lenient decoding  
  - Zero‑copy reading  
  - Advanced error reporting  
  - A DOM (`BinaryDocument` / `BinaryElement`)  

The architecture is designed so additional formats (YAML, TOML, CSV, etc.) can be added later with minimal duplication.

---

# **2. D20Tek.Serialization.Core**

## **2.1 Namespaces**

- `D20Tek.Serialization`
- `D20Tek.Serialization.Generation`
- `D20Tek.Serialization.Dom`

---

# **2.2 Attributes**

These attributes drive source generation and metadata extraction.

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class SerializableAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class SerializedNameAttribute : Attribute
{
    public SerializedNameAttribute(string name) => Name = name;
    public string Name { get; }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class IgnoreSerializedAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class RequiredSerializedAttribute : Attribute { }
```

---

# **2.3 Naming Policy**

```csharp
public abstract class NamingPolicy
{
    public abstract string ConvertName(string name);

    public static NamingPolicy? Default => null;
    public static NamingPolicy CamelCase { get; } = new CamelCaseNamingPolicy();
}
```

---

# **2.4 Converters**

Format‑agnostic base types.

```csharp
public abstract class Converter
{
    public abstract bool CanConvert(Type typeToConvert);
}

public abstract class Converter<T> : Converter
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert == typeof(T);

    public abstract T? Read(IFormatReader reader, SerializerOptions options);
    public abstract void Write(IFormatWriter writer, T value, SerializerOptions options);
}
```

---

# **2.5 SerializerOptions**

```csharp
public abstract class SerializerOptions
{
    public NamingPolicy? PropertyNamingPolicy { get; set; }
    public bool IgnoreNullValues { get; set; }
    public IList<Converter> Converters { get; } = new List<Converter>();
}
```

---

# **2.6 Format‑Agnostic Reader/Writer Interfaces**

These are the **core abstraction** that all formats implement.

```csharp
public enum ValueKind
{
    Null, Boolean, Number, String, Object, Array
}

public interface IFormatWriter
{
    void WriteStartObject();
    void WriteEndObject();

    void WriteStartArray();
    void WriteEndArray();

    void WritePropertyName(string name);

    void WriteNull();
    void WriteBoolean(bool value);
    void WriteNumber(long value);
    void WriteNumber(double value);
    void WriteString(string value);
}

public interface IFormatReader
{
    ValueKind ValueKind { get; }

    void ReadStartObject();
    void ReadEndObject();

    void ReadStartArray();
    void ReadEndArray();

    bool TryReadPropertyName(out string name);

    bool IsNull();
    bool GetBoolean();
    long GetInt64();
    double GetDouble();
    string GetString();

    // NEW: Zero-copy APIs
    ReadOnlySpan<byte> GetRawStringBytes();
    ReadOnlySpan<byte> GetRawNumberBytes();
}
```

Zero‑copy APIs allow formats like CBOR to expose raw slices without allocating.

---

# **2.7 Advanced Error Reporting**

All serializers must throw `SerializationException` with:

```csharp
public sealed class SerializationException : Exception
{
    public string Path { get; }
    public ValueKind? Expected { get; }
    public ValueKind? Actual { get; }

    public SerializationException(string message, string path,
        ValueKind? expected = null, ValueKind? actual = null)
        : base(message)
    {
        Path = path;
        Expected = expected;
        Actual = actual;
    }
}
```

The **path** uses JSONPath syntax:

```
$.user.address.street
$.items[3].price
```

The reader maintains a **stack** of:

- property names  
- array indices  

---

# **2.8 Type Metadata (Reflection Fallback)**

Internal types:

```csharp
internal sealed class TypeMetadata
{
    public Type Type { get; }
    public IReadOnlyList<MemberMetadata> Members { get; }
}

internal sealed class MemberMetadata
{
    public string Name { get; }
    public string SerializedName { get; }
    public Type MemberType { get; }
    public bool IsRequired { get; }
    public bool IgnoreNull { get; }
    public Func<object, object?> Getter { get; }
    public Action<object, object?> Setter { get; }
}
```

---

# **2.9 Source Generator Contracts**

```csharp
public interface IGeneratedSerializerRegistry
{
    bool TryGetSerializer<T>(out IGeneratedSerializer<T> serializer);
}

public interface IGeneratedSerializer<T>
{
    void Write(IFormatWriter writer, T value, SerializerOptions options);
    T? Read(IFormatReader reader, SerializerOptions options);
}
```

The generator emits:

- A registry per assembly  
- A serializer per `[Serializable]` type  

---

# **2.10 Shared DOM (Node)**

```csharp
public enum NodeKind { Null, Boolean, Number, String, Object, Array }

public readonly struct Node
{
    public NodeKind Kind { get; }
    public string GetString();
    public double GetDouble();
    public bool GetBoolean();
    public IReadOnlyList<Node> GetArray();
    public IReadOnlyList<NodeProperty> GetObject();

    public Node this[string propertyName] { get; }
    public Node this[int index] { get; }
}

public readonly struct NodeProperty
{
    public string Name { get; }
    public Node Value { get; }
}
```

---

# **3. D20Tek.Serialization.Binary**

# **3.1 CBOR Encoding Rules (Final)**

| Type | Encoding |
|------|----------|
| Null | CBOR simple value 22 |
| Boolean | CBOR bool |
| Integer | CBOR int |
| Float | **Always 64‑bit IEEE‑754** |
| Decimal | **String** |
| String | CBOR text string |
| Byte[] | CBOR byte string |
| Guid | **Tag 37 + 16‑byte bstr** |
| DateTime | **Tag 1 + epoch ms (UTC)** |
| DateTimeOffset | Same as DateTime |
| Enum | Integer |
| Object | CBOR map (string keys only) |
| Array | CBOR array |

Indefinite lengths: **disallowed** in v1.

---

# **3.2 BinarySerializerOptions**

```csharp
public enum BinaryDecodingMode { Strict, Lenient }

public abstract class BinaryProfile
{
    public abstract string Name { get; }
    public virtual void Configure(BinarySerializerOptions options) { }
}

public sealed class BinarySerializerOptions : SerializerOptions
{
    public bool IncludeFields { get; set; }
    public BinaryDecodingMode DecodingMode { get; set; } = BinaryDecodingMode.Lenient;
    public BinaryProfile? Profile { get; set; }
}
```

---

# **3.3 BinarySerializer API**

```csharp
public static class BinarySerializer
{
    public static T? Deserialize<T>(
        ReadOnlySpan<byte> data,
        BinarySerializerOptions? options = null);

    public static object? Deserialize(
        ReadOnlySpan<byte> data,
        Type returnType,
        BinarySerializerOptions? options = null);

    public static byte[] SerializeToByteArray<T>(
        T value,
        BinarySerializerOptions? options = null);

    public static void Serialize<T>(
        IBufferWriter<byte> bufferWriter,
        T value,
        BinarySerializerOptions? options = null);
}
```

### Resolution order:
1. Custom converter  
2. Generated serializer  
3. Reflection fallback (non‑AOT only)  

---

# **3.4 Zero‑Copy CBOR Reader**

`CborFormatReader` implements:

- `GetRawStringBytes()` → returns UTF‑8 slice  
- `GetRawNumberBytes()` → returns numeric slice  
- Zero allocations for:
  - strings  
  - numbers  
  - property names  

Internally uses:

```csharp
System.Formats.Cbor.CborReader
```

with careful slice tracking.

---

# **3.5 Advanced Error Reporting**

`CborFormatReader` maintains:

```csharp
Stack<object> _pathStack;
```

Where each entry is:

- `string propertyName`  
- `int arrayIndex`  

On error:

```csharp
throw new SerializationException(
    "Expected string but found number",
    "$.items[3].price",
    expected: ValueKind.String,
    actual: ValueKind.Number);
```

Strict mode errors on:

- Unknown tags  
- Unknown properties  

Lenient mode skips them.

---

# **3.6 Reflection Serializer**

Uses `TypeMetadata` to:

- Apply naming policy  
- Apply null handling  
- Apply required attributes  
- Apply IncludeFields  

---

# **3.7 Built‑in Converters**

- `GuidBinaryConverter`
- `DateTimeBinaryConverter`
- `DateTimeOffsetBinaryConverter`
- `DecimalBinaryConverter`
- `EnumBinaryConverter<TEnum>`

All registered by default.

---

# **3.8 BinaryDocument / BinaryElement**

Backed by:

- Materialized `byte[]`  
- Parsed `Node` tree  

```csharp
public sealed class BinaryDocument : IDisposable
{
    public BinaryElement RootElement { get; }
    public static BinaryDocument Parse(ReadOnlySpan<byte> data);
}
```

```csharp
public readonly struct BinaryElement
{
    public BinaryValueKind ValueKind { get; }
    public string GetString();
    public int GetInt32();
    public long GetInt64();
    public double GetDouble();
    public bool GetBoolean();

    public BinaryElement this[string propertyName] { get; }
    public BinaryElement this[int index] { get; }

    public IEnumerable<BinaryProperty> EnumerateObject();
    public IEnumerable<BinaryElement> EnumerateArray();
}
```

---

# **4. Implementation Order (Optimized)**

## **Phase 1 — Core Foundation**
1. Attributes  
2. NamingPolicy  
3. SerializerOptions  
4. IFormatWriter/IFormatReader  
5. Converters  
6. TypeMetadata  
7. DOM (Node)  
8. Source generator contracts  
9. Source generator (basic)

## **Phase 2 — Binary Infrastructure**
1. BinarySerializerOptions  
2. CborFormatWriter  
3. CborFormatReader (with zero‑copy + error paths)  
4. ReflectionBinarySerializer  
5. Built‑in converters  
6. BinarySerializer  

## **Phase 3 — DOM + Polish**
1. CBOR → Node parser  
2. BinaryDocument / BinaryElement  
3. AOT configuration  
4. Tests & samples  
