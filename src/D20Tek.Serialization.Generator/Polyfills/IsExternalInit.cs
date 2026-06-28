// Polyfill so that the netstandard2.0 source generator can use C# init-only setters and
// record positional/init members. This type is required by the compiler at compile time only.
namespace System.Runtime.CompilerServices;

using System.ComponentModel;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IsExternalInit
{
}
