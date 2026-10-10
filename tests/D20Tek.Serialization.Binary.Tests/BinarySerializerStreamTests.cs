using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Serialization.Binary.Tests;

[TestClass]
[SuppressMessage("Trimming", "IL2026")]
[SuppressMessage("AOT", "IL3050")]
public sealed class BinarySerializerStreamTests
{
    // --- Serialize<T>(Stream, ...) / Deserialize<T>(Stream, ...) round-trip ---

    [TestMethod]
    public void Serialize_Deserialize_Stream_RoundTrip()
    {
        // arrange
        var original = new SimpleModel { Name = "Alice", Age = 30, IsActive = true };
        using var stream = new MemoryStream();

        // act
        BinarySerializer.Serialize(stream, original);
        stream.Position = 0;
        var result = BinarySerializer.Deserialize<SimpleModel>(stream);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Alice", result.Name);
        Assert.AreEqual(30, result.Age);
        Assert.IsTrue(result.IsActive);
    }

    [TestMethod]
    public void Serialize_Deserialize_Stream_NullOptions_RoundTrip()
    {
        // arrange
        var original = new SimpleModel { Name = "Bob", Age = 25, IsActive = false };
        using var stream = new MemoryStream();

        // act
        BinarySerializer.Serialize(stream, original, null);
        stream.Position = 0;
        var result = BinarySerializer.Deserialize<SimpleModel>(stream, null);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Bob", result.Name);
        Assert.AreEqual(25, result.Age);
        Assert.IsFalse(result.IsActive);
    }

    [TestMethod]
    public void Serialize_Stream_NullStream_Throws()
    {
        // arrange
        Stream? stream = null;

        // act & assert
        Assert.ThrowsExactly<ArgumentNullException>(
            [ExcludeFromCodeCoverage] () => BinarySerializer.Serialize(stream!, new SimpleModel()));
    }

    [TestMethod]
    public void Deserialize_Stream_NullStream_Throws()
    {
        // arrange
        Stream? stream = null;

        // act & assert
        Assert.ThrowsExactly<ArgumentNullException>(
            [ExcludeFromCodeCoverage] () => BinarySerializer.Deserialize<SimpleModel>(stream!));
    }

    [TestMethod]
    public void Deserialize_Stream_FromNonSeekableNetworkLikeStream_RoundTrip()
    {
        // arrange — use a non-MemoryStream wrapper to exercise the CopyTo fallback path
        var original = new SimpleModel { Name = "Dana", Age = 50, IsActive = true };
        using var backing = new MemoryStream();
        BinarySerializer.Serialize(backing, original);
        backing.Position = 0;
        using var nonMemoryStream = new NonMemoryReadOnlyStream(backing.ToArray());

        // act
        var result = BinarySerializer.Deserialize<SimpleModel>(nonMemoryStream);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Dana", result.Name);
        Assert.AreEqual(50, result.Age);
        Assert.IsTrue(result.IsActive);
    }

    // --- Deserialize(Stream, Type, ...) non-generic ---

    [TestMethod]
    public void Serialize_Deserialize_Stream_NonGeneric_RoundTrip()
    {
        // arrange
        var original = new SimpleModel { Name = "Eli", Age = 60, IsActive = false };
        using var stream = new MemoryStream();

        // act
        BinarySerializer.Serialize(stream, original);
        stream.Position = 0;
        var result = BinarySerializer.Deserialize(stream, typeof(SimpleModel));

        // assert
        var model = Assert.IsInstanceOfType<SimpleModel>(result);
        Assert.AreEqual("Eli", model.Name);
        Assert.AreEqual(60, model.Age);
        Assert.IsFalse(model.IsActive);
    }

    [TestMethod]
    public void Deserialize_Stream_NonGeneric_NullStream_Throws()
    {
        // arrange
        Stream? stream = null;

        // act & assert
        Assert.ThrowsExactly<ArgumentNullException>(
            [ExcludeFromCodeCoverage] () => BinarySerializer.Deserialize(stream!, typeof(SimpleModel)));
    }

    [TestMethod]
    public void Deserialize_Stream_NonGeneric_NullType_Throws()
    {
        // arrange
        using var stream = new MemoryStream();

        // act & assert
        Assert.ThrowsExactly<ArgumentNullException>(
            [ExcludeFromCodeCoverage] () => BinarySerializer.Deserialize(stream, null!));
    }

    // --- SerializeAsync / DeserializeAsync<T> round-trip ---

    [TestMethod]
    public async Task SerializeAsync_DeserializeAsync_Stream_RoundTrip()
    {
        // arrange
        var original = new SimpleModel { Name = "Fay", Age = 22, IsActive = true };
        using var stream = new MemoryStream();

        // act
        await BinarySerializer.SerializeAsync(stream, original);
        stream.Position = 0;
        var result = await BinarySerializer.DeserializeAsync<SimpleModel>(stream);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Fay", result.Name);
        Assert.AreEqual(22, result.Age);
        Assert.IsTrue(result.IsActive);
    }

    [TestMethod]
    public async Task SerializeAsync_DeserializeAsync_Stream_WithCancellationToken_RoundTrip()
    {
        // arrange
        var original = new SimpleModel { Name = "Gus", Age = 33, IsActive = false };
        using var stream = new MemoryStream();
        using var cts = new CancellationTokenSource();

        // act
        await BinarySerializer.SerializeAsync(stream, original, options: null, cts.Token);
        stream.Position = 0;
        var result = await BinarySerializer.DeserializeAsync<SimpleModel>(stream, options: null, cts.Token);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Gus", result.Name);
        Assert.AreEqual(33, result.Age);
        Assert.IsFalse(result.IsActive);
    }

    [TestMethod]
    public async Task SerializeAsync_Stream_NullStream_Throws()
    {
        // arrange
        Stream? stream = null;

        // act & assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            [ExcludeFromCodeCoverage] () => BinarySerializer.SerializeAsync(stream!, new SimpleModel()));
    }

    [TestMethod]
    public async Task DeserializeAsync_Stream_NullStream_Throws()
    {
        // arrange
        Stream? stream = null;

        // act & assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            [ExcludeFromCodeCoverage] () => BinarySerializer.DeserializeAsync<SimpleModel>(stream!));
    }

    // --- DeserializeAsync(Stream, Type, ...) non-generic ---

    [TestMethod]
    public async Task SerializeAsync_DeserializeAsync_Stream_NonGeneric_RoundTrip()
    {
        // arrange
        var original = new SimpleModel { Name = "Hana", Age = 44, IsActive = true };
        using var stream = new MemoryStream();

        // act
        await BinarySerializer.SerializeAsync(stream, original);
        stream.Position = 0;
        var result = await BinarySerializer.DeserializeAsync(stream, typeof(SimpleModel));

        // assert
        var model = Assert.IsInstanceOfType<SimpleModel>(result);
        Assert.AreEqual("Hana", model.Name);
        Assert.AreEqual(44, model.Age);
        Assert.IsTrue(model.IsActive);
    }

    [TestMethod]
    public async Task DeserializeAsync_Stream_NonGeneric_NullStream_Throws()
    {
        // arrange
        Stream? stream = null;

        // act & assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            [ExcludeFromCodeCoverage] () => BinarySerializer.DeserializeAsync(stream!, typeof(SimpleModel)));
    }

    [TestMethod]
    public async Task DeserializeAsync_Stream_NonGeneric_NullType_Throws()
    {
        // arrange
        using var stream = new MemoryStream();

        // act & assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            [ExcludeFromCodeCoverage] () => BinarySerializer.DeserializeAsync(stream, null!));
    }

    // --- Test models ---

    public class SimpleModel
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    // --- Test doubles ---

    /// <summary>
    /// A read-only stream wrapper that is deliberately not a <see cref="MemoryStream"/>, to
    /// exercise the CopyTo/CopyToAsync fallback path used when buffering non-memory streams.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private sealed class NonMemoryReadOnlyStream(byte[] data) : Stream
    {
        private readonly MemoryStream _inner = new(data);

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;
        public override long Position
        {
            get => _inner.Position;
            set => throw new NotSupportedException();
        }

        public override void Flush() => _inner.Flush();

        public override int Read(byte[] buffer, int offset, int count) =>
            _inner.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
    }
}
