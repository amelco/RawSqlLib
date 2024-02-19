using System.Diagnostics.CodeAnalysis;

namespace RawSqlLib.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class RawSqlExceptionRequiredField : RawSqlException
    {
        public RawSqlExceptionRequiredField(string message) : base(message) { }
    }
}
