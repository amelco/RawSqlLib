using Microsoft.Data.SqlClient;

namespace RawSqlLib
{
    public interface IDatabaseConnection
    {
        public string? QueryText(List<SqlParam>? parametros);
        public Task<T?> QueryAsync<T>(List<SqlParam> parametros, Func<SqlDataReader, T> funcao, CancellationToken cancellationToken);
        public Task<bool> NonQueryAsync(List<SqlParam> parametros, CancellationToken cancellationToken);
        public Task<bool> ExecuteTransactionAsync(string[] sql, CancellationToken cancellationToken);
    }
}
