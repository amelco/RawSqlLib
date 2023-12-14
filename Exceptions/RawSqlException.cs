using System.Diagnostics.CodeAnalysis;

namespace RawSqlLib.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class RawSqlException : Exception
    {
        public RawSqlException(string message) : base("ERROR: RawSqlLib: " + message) { }
    }
}
