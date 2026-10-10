# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [v0.10.4]

### Added
- Fix zero-copy regression in span-based `Deserialize`
- Polymorphic / type-discriminator serialization
- Streaming (`Stream` + async) overloads
- Record / init-only property support audit (audited, gaps documented)
- Converter factories for open generics
- Field rename aliasing
- Source generator: support `init`-only property accessors
- Constructor-based construction for positional records
- Updated reflection and AOT samples to include round-trip serialization of records and polymorphic types.

### Changed

- Split the oversized `ModelBuilderDiagnosticTests` test class in
  `D20Tek.Serialization.Generator.Tests` into two focused classes to improve maintainability:
  - `ModelBuilderDiagnosticTests` now contains only the general diagnostic and member-filtering
	tests (get-only/private setter, unsupported member type, nested type, static property,
	non-public property, indexer exclusion, and member declaration order).
  - `ModelBuilderConstructorTests` is a new test class containing all constructor-resolution
	coverage tests (unmatched constructor parameters, parameter type mismatches, multi-constructor
	matching, non-public/static constructor filtering, and static/indexer/non-public property
	name-collision cases).
