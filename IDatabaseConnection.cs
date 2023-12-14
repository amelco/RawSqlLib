using Microsoft.Data.SqlClient;

namespace RawSqlLib
{
    public interface IDatabaseConnection
    {
        public Task<T?> QueryAsync<T>(List<SqlParam> parametros, Func<SqlDataReader, T> funcao, CancellationToken cancellationToken);
        public Task<bool> NonQueryAsync(List<SqlParam> parametros, CancellationToken cancellationToken);
    }
}
