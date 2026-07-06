# **D20Tek.Serialization**

A modern, multi‑format serialization platform for .NET. Built for **AOT**, **performance**, **zero‑copy reading**, and **consistent APIs** across binary, text, and tabular formats.

D20Tek.Serialization brings a unified architecture to serialization in .NET, inspired by the clarity of System.Text.Json and the extensibility of Serde. It provides:

- A shared **core** (metadata, converters, DOM, source generator contracts)  
- High‑performance **binary serializers** (CBOR - Concise Binary Object Representation, TLV - Type Length Value, and custom binary formats)  
- Human‑readable **text serializers** (YAML, TOML, JSON5, RON, XML‑subset)
- Row‑based **tabular serializers** (CSV, TSV, PSV, fixed‑width)  
- A consistent developer experience across all formats  

The initial release will support the shared core types and functionality and a CBOR binary serializer.

---

## **Features**

- **Unified architecture** across all formats  
  Shared abstractions, options, converters, and source‑generated serializers.

- **AOT‑friendly source generation**  
  Deterministic, reflection‑free serializers for NativeAOT and trimming.

- **Zero‑copy reading**  
  Formats like CBOR and TLV expose raw `ReadOnlySpan<byte>` slices without allocating.

- **Advanced error reporting**  
  JSONPath‑style error paths (e.g., `$.items[3].price`) with expected/actual type info.

- **Strict and lenient decoding modes**  
  Choose between safety and flexibility.

- **Format‑agnostic DOM**  
  A shared `Node` tree powers all document models (BinaryDocument, YamlDocument, etc.).

- **Extensible converter model**  
  Add custom converters for any type.

---

## **📦 Packages**

### **Core**
- **D20Tek.Serialization.Core**  
  Shared abstractions, metadata, converters, DOM, and source‑generator contracts.

### **Binary Formats**
- **D20Tek.Serialization.Binary**  
  High‑performance binary serializers including CBOR and TLV.

### **Text Formats**
- **D20Tek.Serialization.Text**  
  YAML, TOML, JSON5, RON, XML‑subset.

### **Tabular Formats**
- **D20Tek.Serialization.Tabular**  
  CSV, TSV, PSV, and fixed‑width row serializers.

---

## **Repository Structure**

```
D20Tek.Serialization/
│
├── src/
│   ├── D20Tek.Serialization.Core/
│   ├── D20Tek.Serialization.Binary/
│   ├── D20Tek.Serialization.Text/
│   └── D20Tek.Serialization.Tabular/
│
├── samples/
│   ├── BinarySamples/
│   ├── TextSamples/
│   ├── TabularSamples/
│   └── AotSamples/
│
├── docs/
│
└── tests/
    ├── CoreTests/
    ├── BinaryTests/
    ├── TextTests/
    └── DelimitedTests/
```

---

## **Getting Started**

### Install (example for binary serializer)

```bash
dotnet add package D20Tek.Serialization.Binary
```

### Basic Usage

```csharp
[Serializable]
public sealed class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

var person = new Person { Name = "Alice", Age = 30 };

byte[] data = BinarySerializer.SerializeToByteArray(person);

Person? clone = BinarySerializer.Deserialize<Person>(data);
```

### AOT‑Friendly Source Generation

Just annotate your types with `[Serializable]` and enable the generator:

```xml
<ItemGroup>
  <CompilerVisibleProperty Include="EmitCompilerGeneratedFiles" />
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
</ItemGroup>
```

---

## **NativeAOT & Trimming Guidance**

D20Tek.Serialization supports two serialization paths with different AOT compatibility:

### AOT‑Safe Path (Source Generation)

The source generator emits reflection‑free serializers and a registry for every `[Serializable]` type.
This path is fully compatible with NativeAOT publishing and IL trimming.

1. **Annotate your types** with `[Serializable]`:
   ```csharp
   [Serializable]
   public sealed class Customer
   {
       public int Id { get; set; }
       public string Name { get; set; } = string.Empty;
   }
   ```

2. **Reference the generator** in your project:
   ```xml
   <ProjectReference Include="...\D20Tek.Serialization.Generator.csproj"
                     OutputItemType="Analyzer"
                     ReferenceOutputAssembly="false" />
   ```

3. **Enable AOT** in your project:
   ```xml
   <PropertyGroup>
     <PublishAot>true</PublishAot>
   </PropertyGroup>
   ```

4. **Serialize and deserialize** — the generated serializer is discovered automatically:
   ```csharp
   byte[] data = BinarySerializer.SerializeToByteArray(customer);
   Customer? clone = BinarySerializer.Deserialize<Customer>(data);
   ```

The `BinaryDocument` / `BinaryElement` DOM is also fully AOT‑safe — it parses raw CBOR bytes
without reflection.

### Reflection Fallback (Non‑AOT)

When no generated serializer or custom converter is registered for a type, `BinarySerializer`
falls back to a reflection‑based serializer. This path:

- Uses `TypeMetadataBuilder` to discover members at runtime
- Compiles accessor delegates via expression trees
- Is **not compatible** with NativeAOT or IL trimming

All reflection entry points are annotated with `[RequiresUnreferencedCode]` and
`[RequiresDynamicCode]`, so the compiler will emit warnings if you call them in a trimmed context.

### Summary

| Path | AOT‑Safe | Trimming‑Safe | How to Enable |
|------|----------|---------------|---------------|
| Source‑generated serializer | ✅ | ✅ | `[Serializable]` + generator reference |
| Custom converter | ✅ | ✅ | Register via `options.Converters` |
| BinaryDocument DOM | ✅ | ✅ | `BinaryDocument.Parse(bytes)` |
| Reflection fallback | ❌ | ❌ | Automatic when no generator/converter found |

---

## **Documentation**

Documentation lives in the `/docs` folder and includes:

- architecture overview
- format specifications
- source generator design
- DOM model
- error reporting
- samples and tutorials

---

## **🧪 Samples**

The `/samples` directory includes:

- Binary serialization examples  
- YAML/TOML/CSV examples  
- AOT‑friendly sample apps  
- Format‑to‑format converters  
- Performance benchmarks  

---

## **Roadmap**

Planned features include:

- Canonical/deterministic profiles  
- CRDT‑friendly binary formats  
- Schema generation (JSON Schema, TOML Schema, etc.)  
- Enum tagging strategies  
- Flattening and default value attributes  
- Streaming serializers  
- Zero‑copy DOM cursor  
- Format‑to‑format transformation APIs  

---

## **Contributing**

To contribute, provide feature ideas or bug reports in this repo Issues.

- A **sample project template**  

Just tell me what direction you want to go next.
