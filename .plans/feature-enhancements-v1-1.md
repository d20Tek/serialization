# **D20Tek.Serialization v1.1 — Enhancement Proposals**

> Companion to [`features-v1.md`](./features-v1.md) and [`features-v1-tasks.md`](./features-v1-tasks.md).
> This document captures candidate improvements identified after the v1.0 release of
> `D20Tek.Serialization.Core`, `D20Tek.Serialization.Binary`, and the source generator.
> Items are not yet scheduled into a release; use the priority column to triage into v1.1/v1.2
> milestones.

---

## Priority Legend

| Priority | Meaning |
|----------|---------|
| **P0** | Correctness/design defect or defeats a stated v1.0 goal. Should be fixed before any new feature work. |
| **P1** | High-value feature or gap vs. comparable serializers (e.g., `System.Text.Json`). Strong candidate for v1.1. |
| **P2** | Useful enhancement, but not blocking adoption. Candidate for v1.1 or v1.2 depending on bandwidth. |
| **P3** | Nice-to-have / polish / documentation. Opportunistic, non-blocking. |

---

## 1. CBOR / Binary Serializer

- [x] **P0 — Fix zero-copy regression in `BinarySerializer.Deserialize<T>(ReadOnlySpan<byte>, ...)`.**
  The span overload currently calls `data.ToArray()` before constructing `CborFormatReader`,
  which defeats the zero-copy reading goal for the primary public entry point. Either make
  `CborFormatReader` accept `ReadOnlySpan<byte>` / `ReadOnlyMemory<byte>` directly, or
  restructure so the span-based overload never allocates an intermediate array.
  *(Resolved: both `Deserialize<T>` and the non-generic `Deserialize` overload now accept
  `ReadOnlyMemory<byte>` and pass it straight through to `CborFormatReader` without copying.)*

- [x] **P1 — Polymorphic / type-discriminator serialization.**
  No current support for serializing interface-typed or abstract-typed members/collections
  (e.g., `IAnimal` holding `Dog`/`Cat` instances). This is a common gap versus
  `System.Text.Json`'s polymorphism support and likely needed for real-world object graphs.
  *(Resolved: added `[Polymorphic]` and `[SerializedDerivedType]` attributes in
  `D20Tek.Serialization.Core`, plus a cached `PolymorphicTypeResolver`. The reflection fallback
  serializer now writes/reads a type discriminator property — `$type` by default, configurable
  via `Polymorphic.TypeDiscriminatorPropertyName` — as the first member of a polymorphic value,
  and resolves the concrete derived type on read. Applies to base/interface-typed members and
  root values. Source-generator support is not yet implemented; polymorphic types currently go
  through the reflection path.)*

- [x] **P1 — Streaming support (`Stream`-based and async overloads).**
  Add `Serialize`/`Deserialize` overloads that read from and write to `Stream` (with async
  variants) so large payloads don't require materializing a full `byte[]` up front.
  *(Resolved: `BinarySerializer` now exposes `Serialize`/`Deserialize` and
  `SerializeAsync`/`DeserializeAsync` overloads that accept a `Stream`. Serialization encodes
  to an in-memory CBOR buffer and writes it to the stream in one call (sync or async); CBOR
  decoding still requires buffering the full document, so deserialization reads the stream to
  completion — via the existing buffer directly for `MemoryStream`, or `CopyTo`/`CopyToAsync`
  otherwise — before decoding through the existing byte-buffer path.)*

- [x] **P1 — Record / init-only property support audit.**
  Verify both the source generator and the reflection fallback correctly handle `record` types
  and `init`-only accessors, not just mutable POCOs. This is a common pain point for modern C#
  codebases.
  *(Audited — gaps found, not yet fixed. See `RecordAndInitOnlyGeneratorTests` and
  `RecordAndInitOnlyReflectionTests`:*
  - *`record` types with ordinary `get; set;` properties work today in both the generator and
    the reflection fallback (records behave like classes when there's no init-only accessor or
    primary constructor involved).*
  - *`init`-only accessors are **not rejected** by `ModelBuilder`'s setter-accessibility check
    (an `init` accessor reports as publicly accessible), but `SerializerEmitter.EmitReadMember`
    emits a plain `result.Member = value;` assignment after construction. For `init`-only
    properties this is illegal post-construction and fails to compile with `CS8852`. The
    generator currently has no way to route `init`-only members through an object-initializer
    or constructor-based construction path.*
  - *The reflection fallback (`TypeMetadataBuilder`/`ReflectionBinarySerializer`) can actually
    set `init`-only properties successfully, because `Expression.Assign` against the property
    setter is permitted here (the C# `init` restriction is enforced by the compiler at the call
    site, not by the CLR), and `ReadObject` constructs the instance before assigning members. So
    `init`-only reflection round-trips work correctly today.*
  - *Positional records (and any type without a public parameterless constructor) are not
    supported by the reflection fallback — `ReadObject` calls
    `Activator.CreateInstance(actualType)` with no arguments, which throws
    `MissingMethodException` for such types. The generator has the same limitation since it
    emits `new {Type}()`.*
  - *Follow-up work (not yet implemented): generator support for `init`-only members via
    object-initializer syntax in `Read`, and/or constructor-based construction for both the
    generator and reflection fallback to support positional records.)*

- [x] **P2 — Source generator: support `init`-only property accessors.**
  `ModelBuilder` accepts `init`-only properties as having an accessible setter, but
  `SerializerEmitter.EmitReadMember` assigns them via `result.Member = value;` after
  construction, which fails to compile with `CS8852` for `init`-only properties. Emit via
  object-initializer syntax (`new T { Member = value, ... }`) instead of post-construction
  assignment for types whose serializable members are all settable via an object initializer,
  so `init`-only properties are supported as documented in the v1.1 record/init-only audit
  above.
  *(Resolved: `MemberModel` now carries an `IsInitOnly` flag, populated in `ModelBuilder` from
  the property's `SetMethod.IsInitOnly`. `ReadMemberEmitter.EmitRead` detects whether any
  member on the type is `init`-only; if so, it declares a local variable per member, assigns
  into those locals during the read loop, and constructs the result via an object initializer
  after the required-member checks. Types with no `init`-only members continue to use the
  original post-construction assignment path unchanged.)*

- [x] **P2 — Constructor-based construction for positional records / types without a
  parameterless constructor.**
  Both the reflection fallback (`ReflectionBinarySerializer.ReadObject`, via
  `Activator.CreateInstance(actualType)`) and the generated `Read` method
  (`new {FullyQualifiedName}()`) require a public parameterless constructor, so positional
  records (e.g. `public sealed record Person(int Id, string? Name);`) throw
  `MissingMethodException` on deserialize. Add support for matching serializable members to a
  public constructor's parameters (by name, case-insensitively) and invoking that constructor
  with buffered member values, falling back to the current parameterless-constructor path when
  one exists.

- [ ] **P2 — `IBufferWriter<byte>` support for `CborFormatWriter`.**
  Allow writing directly to `IBufferWriter<byte>` to avoid intermediate buffer copies when
  integrating with sockets, pipes, or `PipeWriter`-based I/O.

- [ ] **P2 — Collection converter coverage audit.**
  Confirm/extend built-in converter coverage for `IEnumerable<T>`, `IReadOnlyList<T>`,
  `HashSet<T>`, `Dictionary<TKey, TValue>` with non-string keys, and multi-dimensional/jagged
  arrays.

- [ ] **P2 — Circular reference guard.**
  Add a depth limit or reference-tracking mechanism to prevent stack overflows on
  self-referencing object graphs, especially in the reflection fallback path.

- [ ] **P2 — Source-gen inheritance support audit.**
  Confirm the generator correctly handles serializable base classes with derived types
  (member inheritance), not only polymorphic dispatch (see P1 item above — related but
  distinct concern).

- [ ] **P3 — CBOR semantic tag support.**
  `System.Formats.Cbor` supports semantic tags (e.g., tag 0/1 for dates, tag 2/3 for bignums).
  Expose a hook for custom tag handling to improve interoperability with other CBOR
  implementations.

- [ ] **P3 — Benchmark suite.**
  Add a dedicated BenchmarkDotNet project comparing generated vs. reflection paths, and
  against `System.Text.Json`, to validate the "zero-copy" and AOT performance claims with
  real numbers and catch regressions over time.

---

## 2. Core Library

- [x] **P1 — Converter factories for open generic types.**
  `Converter<T>` only covers closed generic types. A `ConverterFactory` abstraction (similar to
  `JsonConverterFactory`) would let a single factory handle open generics (e.g.,
  `Nullable<T>`, custom `Dictionary<TKey, TValue>` variants) without combinatorial explosion of
  converter classes.
  *(Resolved: added `D20Tek.Serialization.ConverterFactory`, an abstract `Converter` subtype
  with `CreateConverter(Type)`. Register factory instances in `SerializerOptions.Converters`
  alongside ordinary converters; `Converter.CanConvert(Type)` still gates whether a given
  factory applies. `SerializerResolutionHelper` resolves factories for both the generic
  (`TryGetConverter<T>`) and non-generic (`BinarySerializer.Deserialize(..., Type, ...)` via
  `ResolveConverter`) paths, and caches the converter produced per factory-instance/closed-type
  pair in a `ConditionalWeakTable` so repeated resolution doesn't re-invoke the factory. Covered
  by `ConverterFactoryTests` (Core) and `ConverterFactoryResolutionTests` (Binary, round-trips
  through `BinarySerializer` for multiple closed generic types).)*

- [x] **P1 — Field rename aliasing for schema evolution.** *(Done: `NameSerializedAttribute`
  now accepts `params string[] aliases`, e.g. `[NameSerialized("new_name", "old_name")]`.
  Reflection metadata (`MemberMetadata`, `TypeMetadataBuilder`) carries aliases, and
  `ConversionHelper.BuildMemberLookup` registers each alias alongside the canonical serialized
  name (canonical name always wins on collision). The source generator's `MemberModel` and
  `ModelBuilder` parse the same constructor arguments, and `SerializerEmitter` emits read-side
  `__name == canonical || __name == alias...` conditions. Writes always emit only the canonical
  name in both reflection and generated paths; aliases are read-only fallbacks. Required-member
  tracking keys off the member, so satisfying it via an alias still counts. Covered by
  `AttributeTests` (Core), `ReflectionBinarySerializerTests` alias round-trip/required tests
  (Binary), and `ModelBuilderTests`/`SerializerEmitterTests` (Generator).)*

- [ ] **P2 — Additional built-in naming policies.**
  Only `CamelCase` exists today. Add `SnakeCase`, `KebabCase`, and an explicit `PascalCase` to
  match common wire-format conventions.

- [ ] **P2 — Per-member conditional serialization.**
  `IgnoreNullValues` is all-or-nothing today. Consider a per-member override or a
  `ShouldSerialize`-style callback for conditional serialization (default values, empty
  collections, etc.).

- [ ] **P2 — Post-deserialization validation hook.**
  Consider an optional integration point for custom validation beyond
  `[RequiredSerialized]` (e.g., an `IValidatableObject`-style callback) to enforce invariants
  after a value is fully materialized.

- [ ] **P2 — `Node` DOM mutation / builder API.**
  If `Node` / `ObjectNode` / `ArrayNode` are currently read/construct-only, add fluent builder
  or mutation helpers so the DOM is useful for programmatic document construction, not just
  parsing existing payloads.

- [ ] **P3 — Thread-safety documentation and verification.**
  Document (and verify via tests) that `TypeMetadataProvider`'s cache and the generated
  serializer registry lookup are safe under concurrent first-use across threads.

- [ ] **P3 — Exception context enrichment.**
  `SerializationException` already captures `Path`, `Expected`, and `Actual`. Consider also
  capturing the byte offset (for binary formats) to aid debugging of corrupted payloads.

---

## 3. Documentation / Developer Experience

- [ ] **P2 — Migration guide from `System.Text.Json`.**
  Since the API deliberately mirrors `System.Text.Json`, a side-by-side comparison/migration
  doc would lower the adoption barrier for existing STJ users.

- [ ] **P3 — XML doc coverage pass for new additions.**
  Keep `GenerateDocumentationFile` output complete as new APIs are added, per house
  documentation standards.

---

## 4. Suggested Triage for v1.1

Based on priority and risk, a reasonable first pass at v1.1 scope:

| Priority | Item | Area |
|----------|------|------|
| P0 | ~~Fix zero-copy regression in span-based `Deserialize`~~ (done) | Binary |
| P1 | ~~Polymorphic / type-discriminator serialization~~ (done) | Binary |
| P1 | ~~Streaming (`Stream` + async) overloads~~ (done) | Binary |
| P1 | ~~Record / init-only property support audit~~ (audited, gaps documented) | Binary |
| P1 | ~~Converter factories for open generics~~ (done) | Core |
| P1 | ~~Field rename aliasing~~ (done) | Core |
| P2 | ~~Source generator: support `init`-only property accessors~~ (done) | Binary |
| P2 | ~~Constructor-based construction for positional records~~ (done) | Binary |

Remaining P2/P3 items can be deferred to v1.2 or picked up opportunistically.

---

## 5. Suggested Triage for v1.2

The two P1 items from the record/init-only audit (`init`-only property accessor support and
constructor-based construction for positional records) are already tracked in the v1.1 triage
table above. The remaining P2/P3 items from this document are proposed for v1.2:

| Priority | Item | Area |
|----------|------|------|
| P2 | Circular reference guard | Binary |
| P2 | Collection converter coverage audit (`HashSet<T>`, non-string-key dictionaries, jagged arrays) | Binary |
| P2 | Source-gen inheritance support audit | Binary |
| P2 | Additional built-in naming policies (`SnakeCase`, `KebabCase`, `PascalCase`) | Core |
| P2 | Per-member conditional serialization | Core |
| P2 | Post-deserialization validation hook | Core |
| P2 | `IBufferWriter<byte>` support for `CborFormatWriter` | Binary |
| P2 | `Node` DOM mutation / builder API | Core |
| P3 | CBOR semantic tag support | Binary |
| P3 | Thread-safety documentation and verification | Core |
| P3 | Exception context enrichment (byte offset) | Core |
| P3 | Benchmark suite (generated vs. reflection vs. `System.Text.Json`) | Binary |
| P3 | XML doc coverage pass for new additions | Docs |
| P3 | Migration guide from `System.Text.Json` | Docs |

Remaining P3 items can be deferred further or picked up opportunistically as bandwidth allows.
