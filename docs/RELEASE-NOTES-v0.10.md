# D20Tek.Serialization v0.10.1 Release Notes

## Overview

The D20Tek.Serialization v0.10.1 release delivers a **format-agnostic serialization platform** for .NET 10, inspired by `System.Text.Json` and Rust's Serde. This initial release ships two NuGet packages and an embedded source generator.

---

## D20Tek.Serialization.Core

The format-agnostic foundation that all D20Tek serializers build on.

### Highlights

- **Format-agnostic reader/writer abstractions** — `IFormatReader` and `IFormatWriter` interfaces that any wire format (CBOR, JSON, YAML, etc.) can implement, enabling converters and serializers to be written once and reused across formats.
- **Extensible converter model** — `Converter<T>` base class for custom type handling. Register converters per-call via `SerializerOptions.Converters`.
- **Serialization attributes** — `[Serializable]`, `[NameSerialized]`, `[IgnoreSerialized]`, and `[RequiredSerialized]` to control which members are serialized, their wire names, and validation requirements.
- **Naming policies** — Built-in `NamingPolicy.CamelCase` and a pluggable `NamingPolicy` base class for custom property-name transforms.
- **Shared DOM** — `Node`, `ObjectNode`, `ArrayNode`, and `ValueNode` types for format-independent document representation.
- **Source generator contracts** — `IGeneratedSerializer<T>` and `IGeneratedSerializerRegistry` interfaces that generated code implements, enabling AOT-safe serialization without reflection.
- **Reflection-based type metadata** — Compiled getter/setter delegates via expression trees with per-`(Type, options)` caching for high-performance reflection fallback.
- **Structured error reporting** — `SerializationException` with JSONPath-style `Path`, `Expected`, and `Actual` properties for precise diagnostics (e.g., `$.user.address.street`).

---

## D20Tek.Serialization.Binary

A CBOR-based binary serializer built on the Core abstractions.

### Highlights

- **Three-tier resolution order** — `BinarySerializer` resolves serializers in order: custom converter → source-generated → reflection fallback. This allows AOT-safe usage when generators are present, with automatic reflection fallback for unattributed types.
- **AOT & trimming safe** — Types decorated with `[Serializable]` get source-generated serializers that require no reflection. Validated with a NativeAOT-published sample application.
- **Zero-copy CBOR reader** — `CborFormatReader` exposes `GetRawStringBytes()` and `GetRawNumberBytes()` for allocation-free access to encoded values directly from the source buffer.
- **Strict & lenient decoding** — `BinarySerializerOptions.StrictMode` controls whether unknown CBOR fields cause exceptions (strict) or are silently skipped (lenient), enabling forward/backward-compatible schema evolution.
- **Full CBOR type coverage** — Built-in converters for all .NET primitives (`bool`, `int`, `long`, `float`, `double`, `string`, `byte[]`, `decimal`, `DateTime`, `DateTimeOffset`, `Guid`, `Uri`), nullable wrappers, arrays, `List<T>`, and `Dictionary<string, T>`.
- **Enum serialization** — `EnumCborConverter<T>` serializes enums as CBOR text strings, with support for nullable enum properties.
- **Binary DOM** — `BinaryDocument.Parse(byte[])` produces a queryable `BinaryElement` tree for read-only CBOR document inspection without a deserialize-to-type step. Supports property indexing, `EnumerateObject()`, `EnumerateArray()`, and typed getters.
- **`BinarySerializerOptions`** — Extends `SerializerOptions` with `StrictMode` and `IgnoreNullValues` for fine-grained control over encoding behavior.

---

## Source Generator (`D20Tek.Serialization.Generator`)

An incremental Roslyn source generator that ships inside the `D20Tek.Serialization.Core` NuGet package (under `analyzers/dotnet/cs`).

### Highlights

- **Automatic serializer generation** — For every class or struct marked with `[Serializable]`, the generator emits a `GeneratedSerializer_<TypeName>` implementing `IGeneratedSerializer<T>` with optimized `Read` and `Write` methods.
- **Per-assembly registry** — Emits a `GeneratedSerializerRegistry` that `BinarySerializer` discovers automatically, mapping types to their generated serializers.
- **Attribute-aware code generation** — Honors `[NameSerialized]`, `[IgnoreSerialized]`, and `[RequiredSerialized]` at compile time, producing wire-name constants and required-field validation in generated code.
- **Struct support** — Correctly handles value types with boxing-free serialization paths.
- **Incremental & cacheable** — Built on Roslyn's `IIncrementalGenerator` for fast IDE feedback and minimal rebuild overhead.

---

## Package Details

| Package | Target | Dependencies |
|---------|--------|-------------|
| `D20Tek.Serialization.Core` | `net10.0` | None (generator embedded) |
| `D20Tek.Serialization.Binary` | `net10.0` | `D20Tek.Serialization.Core`, `System.Formats.Cbor` |

Both packages include:
- XML documentation files
- Symbol packages (`.snupkg`) for Source Link debugging
- MIT license
