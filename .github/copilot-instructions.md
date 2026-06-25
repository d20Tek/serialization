# Copilot Instructions

## Project Guidelines
- In the D20Tek.Serialization Core test project, unit tests use plain MSTest Assert (e.g., Assert.AreEqual, Assert.IsTrue) rather than FluentAssertions. Use [TestMethod] with [DataRow] for data-driven tests (not the obsolete [DataTestMethod]).