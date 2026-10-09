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

- [ ] **P1 — Streaming support (`Stream`-based and async overloads).**
  Add `Serialize`/`Deserialize` overloads that read from and write to `Stream` (with async
  variants) so large payloads don't require materializing a full `byte[]` up front.

- [ ] **P1 — Record / init-only property support audit.**
  Verify both the source generator and the reflection fallback correctly handle `record` types
  and `init`-only accessors, not just mutable POCOs. This is a common pain point for modern C#
  codebases.

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

- [ ] **P1 — Converter factories for open generic types.**
  `Converter<T>` only covers closed generic types. A `ConverterFactory` abstraction (similar to
  `JsonConverterFactory`) would let a single factory handle open generics (e.g.,
  `Nullable<T>`, custom `Dictionary<TKey, TValue>` variants) without combinatorial explosion of
  converter classes.

- [ ] **P1 — Field rename aliasing for schema evolution.**
  Since lenient mode already supports unknown-field skipping, add an explicit alias mechanism
  (e.g., `[NameSerialized("new_name", "old_name")]`) to support renaming fields without
  breaking older payloads.

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
| P0 | Fix zero-copy regression in span-based `Deserialize` | Binary |
| P1 | ~~Polymorphic / type-discriminator serialization~~ (done) | Binary |
| P1 | Streaming (`Stream` + async) overloads | Binary |
| P1 | Record / init-only property support audit | Binary |
| P1 | Converter factories for open generics | Core |
| P1 | Field rename aliasing | Core |

Remaining P2/P3 items can be deferred to v1.2 or picked up opportunistically.
