using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace RawSqlLib
{
    [ExcludeFromCodeCoverage]
    public class RawSql
    {
        public readonly string _connString = "";
        private List<SqlParam> parametros = new();

        public RawSql(string connString)
        {
            _connString = connString;
        }

        public Task<T?> QueryAsync<T>(string sql, Func<SqlDataReader, T> mapeamento, CancellationToken cancellationToken)
        {
            var conexao = new SqlServerConnectionManager(_connString, sql);
            Task<T?> resultado = conexao.QueryAsync(parametros, mapeamento, cancellationToken);
            parametros.Clear();
            return resultado;
        }

        public Task<bool> NonQueryAsync(string sql, CancellationToken cancellationToken)
        {
            var conexao = new SqlServerConnectionManager(_connString, sql);
            Task<bool> resultado = conexao.NonQueryAsync(parametros, cancellationToken);
            parametros.Clear();
            return resultado;
        }

        public void AddParam(string name, SqlDbType dbType, object value)
        {
            parametros.Add(new SqlParam { ParamName = name, ParamDbType = dbType, ParamValue = value });
        }
    }
}