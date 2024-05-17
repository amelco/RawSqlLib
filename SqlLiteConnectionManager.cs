using Microsoft.Data.SqlClient;

namespace RawSqlLib
{
    public class SqlLiteConnectionManager : IDatabaseConnection
    {
        public Task<bool> ExecuteTransactionAsync(string[] sql, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> NonQueryAsync(List<SqlParam> parametros, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<T?> QueryAsync<T>(List<SqlParam> parametros, Func<SqlDataReader, T> funcao, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public string? QueryText(List<SqlParam>? parametros)
        {
            throw new NotImplementedException();
        }
    }
}
