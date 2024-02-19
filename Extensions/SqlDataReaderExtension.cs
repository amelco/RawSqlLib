using Microsoft.Data.SqlClient;
using RawSqlLib.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace RawSqlLib.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class SqlDataReaderExtensions
    {
        public static T? ObterValor<T>(this SqlDataReader reader, int index, bool required = false)
        {
            if (reader.IsDBNull(index))
            {
                if (required)
                {
                    throw new RawSqlExceptionRequiredField("Campo é obrigatório");
                }
                return default;
            }

            return (T)reader.GetValue(index);
        }

        public static bool TemValores(this SqlDataReader? reader)
        {
            return reader is not null && reader.HasRows;
        }
    }
}
