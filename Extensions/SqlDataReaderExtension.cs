using Microsoft.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace RawSqlLib.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class SqlDataReaderExtensions
    {
        public static T? ObterValor<T>(this SqlDataReader reader, int index)
        {
            if (reader.IsDBNull(index))
                return default;

            return (T)reader.GetValue(index);
        }

        public static bool TemValores(this SqlDataReader? reader)
        {
            return reader is not null && reader.HasRows;
        }
    }
}
