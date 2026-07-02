# Copilot Instructions

## Project Guidelines
- In the D20Tek.Serialization Core test project, unit tests use plain MSTest Assert (e.g., Assert.AreEqual, Assert.IsTrue) rather than FluentAssertions. Use [TestMethod] with [DataRow] for data-driven tests (not the obsolete [DataTestMethod]).
- Always structure unit tests using the Arrange-Act-Assert pattern, with explicit "// arrange", "// act", and "// assert" comments delimiting each section. Omit the Arrange comment only when a test has no setup.
- Ensure comprehensive branch and condition coverage in unit tests — target all code blocks and conditions in the implementation, not just happy paths.
- Keep the CBOR converter classes public since they are in a sub-namespace (D20Tek.Serialization.Binary.Cbor.Converters) and won't pollute the main namespace.
- Use a separate test class for each implementation class, rather than combining tests for multiple classes into a single test file (e.g., one test class per converter, not a combined CborConverterTests).