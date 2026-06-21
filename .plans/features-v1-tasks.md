# **D20Tek.Serialization v1.0 — Implementation Plan & Task Breakdown**

> Companion to [`features-v1.md`](./features-v1.md). This document decomposes the v1.0
> specification into ordered, actionable tasks. Each task has a checkbox so progress can be
> tracked directly in the file.

---

## **0. Goals & Scope**

Deliver two shippable NuGet packages for .NET 10:

- **`D20Tek.Serialization.Core`** — format-agnostic abstractions, metadata, converters, DOM, and source-generator contracts.
- **`D20Tek.Serialization.Binary`** — CBOR-based binary serializer (AOT source-gen + reflection fallback, strict/lenient decoding, zero-copy reading, advanced error reporting, DOM).

Out of scope for v1.0: YAML/TOML/CSV and other formats (architecture must allow them later, but they are not built now).

### Definition of Done (release-level)
- [ ] All public APIs in the spec are implemented and documented with XML doc comments.
- [ ] Source generator produces serializers for `[Serializable]` types and a per-assembly registry.
- [ ] Round-trip (serialize → deserialize) works for all CBOR encoding rules in §3.1.
- [ ] Resolution order (converter → generated → reflection) verified by tests.
- [ ] Strict and lenient decoding behaviors verified by tests.
- [ ] Zero-copy reader paths allocate no managed strings for strings/numbers/property names (validated).
- [ ] AOT/trimming: Binary sample app publishes with NativeAOT and round-trips successfully.
- [ ] CI builds, runs tests, and packs both packages.

---

## **1. Solution & Repository Setup**

The repo currently contains only `src/D20Tek.Serialization.Core` with a placeholder `Class1.cs`.
These tasks establish the project layout that the rest of the plan depends on.

- [x] **1.1** Add `Directory.Build.props` at repo root (shared `LangVersion`, `Nullable=enable`, `ImplicitUsings=enable`, `TreatWarningsAsErrors`, deterministic build, package metadata defaults).
- [x] **1.2** Add `Directory.Packages.props` for Central Package Management (pin `System.Formats.Cbor`, test deps, source-gen testing deps).
- [x] **1.3** Remove placeholder `src/D20Tek.Serialization.Core/Class1.cs`.
- [x] **1.4** Create project `src/D20Tek.Serialization.Binary/D20Tek.Serialization.Binary.csproj` (`net10.0`), referencing Core.
- [x] **1.5** Create source-generator project `src/D20Tek.Serialization.Generator/D20Tek.Serialization.Generator.csproj` (`netstandard2.0`, `Microsoft.CodeAnalysis.CSharp`, analyzer packaging).
- [x] **1.6** Wire the generator into Core/Binary as an analyzer (`OutputItemType="Analyzer"`, `ReferenceOutputAssembly="false"`).
- [x] **1.7** Create `tests/D20Tek.Serialization.Core.Tests` (MSTest per house style; add coverage collector).
- [x] **1.8** Create `tests/D20Tek.Serialization.Binary.Tests`.
- [x] **1.9** Create `tests/D20Tek.Serialization.Generator.Tests` (uses `Microsoft.CodeAnalysis.*.Testing` / snapshot verification).
- [x] **1.10** Create `samples/D20Tek.Serialization.Binary.Sample` console app (used for AOT publish validation).
- [x] **1.11** Update `d20tek-serialization.slnx` to include all new projects and the tasks doc.
- [ ] **1.12** Add CI workflow (`.github/workflows/build.yml`): restore, build, test (+coverage), pack on tag.

---

## **2. Phase 1 — Core Foundation** (`D20Tek.Serialization.Core`)

Namespaces: `D20Tek.Serialization`, `D20Tek.Serialization.Generation`, `D20Tek.Serialization.Dom`.

### 2.1 Attributes (spec §2.2)
- [ ] **2.1.1** `SerializableAttribute` (`Class | Struct`, `sealed`).
- [ ] **2.1.2** `SerializedNameAttribute(string name)` with `Name` property (`Property | Field`).
- [ ] **2.1.3** `IgnoreSerializedAttribute` (`Property | Field`).
- [ ] **2.1.4** `RequiredSerializedAttribute` (`Property | Field`).
- [ ] **2.1.5** Unit tests for attribute usage targets and property values.

### 2.2 Naming Policy (spec §2.3)
- [ ] **2.2.1** `abstract class NamingPolicy` with `abstract string ConvertName(string)`.
- [ ] **2.2.2** Static `Default => null` and `CamelCase` accessors.
- [ ] **2.2.3** `CamelCaseNamingPolicy` implementation (handles empty/edge-case names, already-camel input).
- [ ] **2.2.4** Unit tests covering camelCase conversion + edge cases.

### 2.3 Serializer Options (spec §2.5)
- [ ] **2.3.1** `abstract class SerializerOptions` with `PropertyNamingPolicy`, `IgnoreNullValues`, `IList<Converter> Converters`.
- [ ] **2.3.2** Unit tests for default values and converter list behavior.

### 2.4 Format-Agnostic Reader/Writer (spec §2.6)
- [ ] **2.4.1** `enum ValueKind { Null, Boolean, Number, String, Object, Array }`.
- [ ] **2.4.2** `interface IFormatWriter` (object/array/property + Null/Boolean/Number(long)/Number(double)/String).
- [ ] **2.4.3** `interface IFormatReader` (ValueKind, read structural tokens, `TryReadPropertyName`, typed getters).
- [ ] **2.4.4** Zero-copy members on reader: `ReadOnlySpan<byte> GetRawStringBytes()`, `GetRawNumberBytes()`.
- [ ] **2.4.5** XML docs describing contract expectations (cursor movement, ordering).

### 2.5 Converters (spec §2.4)
- [ ] **2.5.1** `abstract class Converter` with `abstract bool CanConvert(Type)`.
- [ ] **2.5.2** `abstract class Converter<T> : Converter` with default `CanConvert`, abstract `Read`/`Write` against `IFormatReader/IFormatWriter`.
- [ ] **2.5.3** Unit tests for `CanConvert` default behavior.

### 2.6 Error Reporting (spec §2.7)
- [ ] **2.6.1** `sealed class SerializationException : Exception` with `Path`, `Expected`, `Actual` and the specified constructor.
- [ ] **2.6.2** Define JSONPath path-building helper (segments for `.property` and `[index]`) reusable by readers.
- [ ] **2.6.3** Unit tests for path formatting (`$.user.address.street`, `$.items[3].price`) and expected/actual capture.

### 2.7 Type Metadata — Reflection Fallback (spec §2.8)
- [ ] **2.7.1** `internal sealed class MemberMetadata` (Name, SerializedName, MemberType, IsRequired, IgnoreNull, compiled `Getter`/`Setter`).
- [ ] **2.7.2** `internal sealed class TypeMetadata` (Type + `IReadOnlyList<MemberMetadata>`).
- [ ] **2.7.3** Metadata builder: reflect properties (+fields when enabled), apply `[SerializedName]`, `[IgnoreSerialized]`, `[RequiredSerialized]`, naming policy.
- [ ] **2.7.4** Compile getters/setters via expression trees / delegates for performance; cache per `(Type, options)`.
- [ ] **2.7.5** Unit tests for metadata extraction + caching.

### 2.8 Source Generator Contracts (spec §2.9)
- [ ] **2.8.1** `interface IGeneratedSerializer<T>` (`Write`, `Read`).
- [ ] **2.8.2** `interface IGeneratedSerializerRegistry` (`bool TryGetSerializer<T>(out ...)`).
- [ ] **2.8.3** Unit tests with a hand-written fake registry/serializer to lock the contract.

### 2.9 Shared DOM — Node (spec §2.10)
- [ ] **2.9.1** `enum NodeKind { Null, Boolean, Number, String, Object, Array }`.
- [ ] **2.9.2** `readonly struct Node` (Kind, Get* accessors, object/array enumerators, `this[string]`, `this[int]`).
- [ ] **2.9.3** `readonly struct NodeProperty` (Name, Value).
- [ ] **2.9.4** Define construction/factory surface for building `Node` trees (used by Binary DOM parser).
- [ ] **2.9.5** Unit tests for indexing, kind checks, and invalid-access exceptions.

### 2.10 Source Generator — Basic (spec §2.9, Phase 1.9)
- [ ] **2.10.1** Implement `IIncrementalGenerator` that discovers `[Serializable]` types.
- [ ] **2.10.2** Build a serialization model (members, serialized names, required/ignore/null rules) mirroring `TypeMetadata`.
- [ ] **2.10.3** Emit one `IGeneratedSerializer<T>` per `[Serializable]` type (Write/Read against `IFormatWriter/IFormatReader`).
- [ ] **2.10.4** Emit a per-assembly `IGeneratedSerializerRegistry` implementation.
- [ ] **2.10.5** Emit module/assembly hook so the registry is discoverable at runtime (e.g., `[assembly:]` attribute or registration entry point).
- [ ] **2.10.6** Emit diagnostics for unsupported member types / inaccessible setters.
- [ ] **2.10.7** Generator tests: snapshot generated code + compile-and-run validation.

### Phase 1 Exit Criteria
- [ ] Core compiles, all Core unit tests pass.
- [ ] Generator produces compiling serializers + registry for sample `[Serializable]` types.

---

## **3. Phase 2 — Binary Infrastructure** (`D20Tek.Serialization.Binary`)

References `System.Formats.Cbor`. Implements the CBOR encoding rules in spec §3.1.

### 3.1 Binary Options (spec §3.2)
- [ ] **3.1.1** `enum BinaryDecodingMode { Strict, Lenient }`.
- [ ] **3.1.2** `abstract class BinaryProfile` (`Name`, virtual `Configure(BinarySerializerOptions)`).
- [ ] **3.1.3** `sealed class BinarySerializerOptions : SerializerOptions` (`IncludeFields`, `DecodingMode = Lenient`, `Profile`).
- [ ] **3.1.4** Apply `Profile.Configure` during options resolution.
- [ ] **3.1.5** Unit tests for defaults and profile application.

### 3.2 CborFormatWriter (spec §3.1, Phase 2.2)
- [ ] **3.2.1** Implement `IFormatWriter` over `CborWriter` (definite lengths only — indefinite disallowed).
- [ ] **3.2.2** Object → CBOR map (string keys only); array → CBOR array.
- [ ] **3.2.3** Null → simple value 22; Boolean → CBOR bool; Integer → CBOR int; Float → **always 64-bit** IEEE-754.
- [ ] **3.2.4** String → text string; expose write path for raw byte strings (byte[] → bstr).
- [ ] **3.2.5** Encode buffer to `IBufferWriter<byte>` / `byte[]`.
- [ ] **3.2.6** Unit tests asserting exact CBOR byte output per encoding rule.

### 3.3 CborFormatReader — zero-copy + error paths (spec §3.4, §3.5)
- [ ] **3.3.1** Implement `IFormatReader` over `CborReader` with `ValueKind` mapping.
- [ ] **3.3.2** Structural reads (`ReadStartObject/EndObject/StartArray/EndArray`), `TryReadPropertyName`.
- [ ] **3.3.3** Typed getters (`IsNull`, `GetBoolean`, `GetInt64`, `GetDouble`, `GetString`).
- [ ] **3.3.4** Zero-copy `GetRawStringBytes()` / `GetRawNumberBytes()` returning UTF-8 / numeric slices without allocation (careful slice tracking over the source buffer).
- [ ] **3.3.5** Maintain `Stack<object>` path stack (property name / array index); build JSONPath on error.
- [ ] **3.3.6** Throw `SerializationException` with message, path, expected, actual on type mismatch.
- [ ] **3.3.7** Strict mode: error on unknown tags and unknown properties.
- [ ] **3.3.8** Lenient mode: skip unknown tags/properties.
- [ ] **3.3.9** Reject indefinite-length items (v1 invariant).
- [ ] **3.3.10** Unit tests: zero-copy correctness, path accuracy, strict vs lenient, malformed input.

### 3.4 Reflection Binary Serializer (spec §3.6)
- [ ] **3.4.1** `ReflectionBinarySerializer` using `TypeMetadata` to write/read objects.
- [ ] **3.4.2** Apply naming policy, null handling (`IgnoreNullValues`), required-member enforcement, `IncludeFields`.
- [ ] **3.4.3** Required-member-missing → `SerializationException` with path.
- [ ] **3.4.4** Unit tests for each option's behavior + nested object/array round-trips.

### 3.5 Built-in Converters (spec §3.7)
- [ ] **3.5.1** `GuidBinaryConverter` — CBOR **Tag 37 + 16-byte bstr**.
- [ ] **3.5.2** `DateTimeBinaryConverter` — **Tag 1 + epoch ms (UTC)**.
- [ ] **3.5.3** `DateTimeOffsetBinaryConverter` — same encoding as DateTime.
- [ ] **3.5.4** `DecimalBinaryConverter` — encode as **string**.
- [ ] **3.5.5** `EnumBinaryConverter<TEnum>` — encode as integer.
- [ ] **3.5.6** Register all built-ins by default in options resolution.
- [ ] **3.5.7** Unit tests for each converter (round-trip + exact bytes/tags).

### 3.6 BinarySerializer Facade (spec §3.3)
- [ ] **3.6.1** `static T? Deserialize<T>(ReadOnlySpan<byte>, BinarySerializerOptions?)`.
- [ ] **3.6.2** `static object? Deserialize(ReadOnlySpan<byte>, Type, BinarySerializerOptions?)`.
- [ ] **3.6.3** `static byte[] SerializeToByteArray<T>(T, BinarySerializerOptions?)`.
- [ ] **3.6.4** `static void Serialize<T>(IBufferWriter<byte>, T, BinarySerializerOptions?)`.
- [ ] **3.6.5** Implement resolution order: **1) custom converter → 2) generated serializer → 3) reflection fallback** (non-AOT only).
- [ ] **3.6.6** Default options instance when `options == null`.
- [ ] **3.6.7** Unit tests proving resolution order and each entry-point round-trips.

### Phase 2 Exit Criteria
- [ ] All CBOR encoding rules round-trip via `BinarySerializer`.
- [ ] Strict/lenient + zero-copy + error-path behaviors verified.

---

## **4. Phase 3 — DOM + Polish**

### 4.1 CBOR → Node Parser (spec §2.10, Phase 3.1)
- [ ] **4.1.1** Parser that reads CBOR into the shared `Node` tree (maps → object nodes, arrays → array nodes, scalars per encoding rules).
- [ ] **4.1.2** Decode tagged values (Guid/DateTime) into appropriate node representations.
- [ ] **4.1.3** Unit tests for parser correctness across all value kinds.

### 4.2 BinaryDocument / BinaryElement (spec §3.8)
- [ ] **4.2.1** `enum BinaryValueKind` and `readonly struct BinaryProperty`.
- [ ] **4.2.2** `sealed class BinaryDocument : IDisposable` (materialized `byte[]` + parsed `Node` tree, `RootElement`, `static Parse(ReadOnlySpan<byte>)`).
- [ ] **4.2.3** `readonly struct BinaryElement` (`ValueKind`, `GetString/GetInt32/GetInt64/GetDouble/GetBoolean`, `this[string]`, `this[int]`, `EnumerateObject`, `EnumerateArray`).
- [ ] **4.2.4** Dispose semantics (return pooled buffers if used).
- [ ] **4.2.5** Unit tests: navigation, enumeration, indexing, disposal.

### 4.3 AOT Configuration (spec Phase 3.3)
- [ ] **4.3.1** Annotate reflection paths with trimming/AOT attributes (`RequiresUnreferencedCode` / `RequiresDynamicCode` on reflection fallback).
- [ ] **4.3.2** Ensure generated-serializer path is fully AOT-safe (no reflection).
- [ ] **4.3.3** Configure sample app for NativeAOT publish; verify trim warnings are clean for the source-gen path.
- [ ] **4.3.4** Document AOT usage guidance (use `[Serializable]` + generated registry; reflection fallback is non-AOT).

### 4.4 Tests, Samples & Docs (spec Phase 3.4)
- [ ] **4.4.1** End-to-end round-trip test suite across nested objects, arrays, all built-in types.
- [ ] **4.4.2** Allocation tests (e.g., BenchmarkDotNet or allocation asserts) proving zero-copy reader paths.
- [ ] **4.4.3** Strict vs lenient interop tests (forward/backward-compat scenarios with unknown fields/tags).
- [ ] **4.4.4** Sample app demonstrating: source-gen serialization, custom converter, DOM read, AOT publish.
- [ ] **4.4.5** Update `README.md` with Binary quick-start + AOT notes.
- [ ] **4.4.6** XML doc comment pass over all public APIs; enable `GenerateDocumentationFile`.

### 4.5 Packaging & Release
- [ ] **4.5.1** Package metadata for both packages (icon, license, repo URL, README, symbols/snupkg).
- [ ] **4.5.2** Ensure generator ships inside Core package under `analyzers/dotnet/cs`.
- [ ] **4.5.3** `dotnet pack` produces valid packages; smoke-test install in a clean consumer project.
- [ ] **4.5.4** Tag-driven publish step in CI.

### Phase 3 Exit Criteria
- [ ] DOM read works for documents produced by the writer.
- [ ] AOT sample publishes and round-trips.
- [ ] Both packages pack and install cleanly.

---

## **5. Dependency Order (Critical Path)**

```
1. Solution/project setup
		│
2. Core: Attributes → NamingPolicy → SerializerOptions → ValueKind/IFormatWriter/IFormatReader
		│                                                          │
		│                                                  2. Converters
		▼                                                          │
   SerializationException ──► TypeMetadata ──► Generator contracts ─┤
		│                                                          ▼
		└──────────────► Node DOM ──────────────► Source Generator (basic)
														  │
3. Binary: BinaryOptions → CborFormatWriter → CborFormatReader ────┤
								   │                                ▼
								   ├──► Built-in Converters ──► ReflectionBinarySerializer
								   │                                │
								   └──────────────► BinarySerializer (resolution order)
														  │
4. DOM: CBOR→Node parser ──► BinaryDocument/BinaryElement ─┘
														  │
												  AOT config ──► Tests/Samples/Docs ──► Packaging
```

---

## **6. Cross-Cutting Risks & Decisions**

- [ ] **Zero-copy lifetime**: `GetRawStringBytes/GetRawNumberBytes` slices must stay valid only while the reader's source buffer is alive — document and test lifetime constraints.
- [ ] **`System.Formats.Cbor` raw-slice access**: verify the library exposes (or can be wrapped to expose) byte slices without copying; if not, design a thin CBOR scanner over `ReadOnlySpan<byte>` to back the reader.
- [ ] **Indefinite-length rejection**: enforce consistently in both reader and DOM parser.
- [ ] **Required-member semantics**: align reflection and generated paths (missing required → `SerializationException` at the correct path).
- [ ] **Registry discovery across assemblies**: define how `BinarySerializer` locates generated registries (module initializer vs. explicit registration) — must be AOT-safe.
- [ ] **`struct` support**: `[Serializable]` allows structs; ensure generator and reflection handle value types (boxing-free where possible).
- [ ] **Test framework choice**: confirm house style (MSTest vs xUnit) before scaffolding test projects.

---

## **7. Progress Summary**

| Phase | Area | Status |
|-------|------|--------|
| 1 | Solution/project setup | ☐ Not started |
| 1 | Core abstractions | ☐ Not started |
| 1 | Source generator (basic) | ☐ Not started |
| 2 | Binary options + CBOR writer | ☐ Not started |
| 2 | CBOR reader (zero-copy + errors) | ☐ Not started |
| 2 | Reflection serializer + converters | ☐ Not started |
| 2 | BinarySerializer facade | ☐ Not started |
| 3 | DOM (parser + document) | ☐ Not started |
| 3 | AOT + tests + samples | ☐ Not started |
| 3 | Packaging & release | ☐ Not started |
