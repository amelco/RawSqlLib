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
        private bool _debug = false;
        private DatabaseType _databaseType = DatabaseType.SQLServer;

        public RawSql(string connString, DatabaseType databaseType, bool debug = false)
        {
            _connString = connString;
            _databaseType = databaseType;
            _debug = debug;
        }

        public static TEntidade? QueryStatic<TEntidade>(string _connString, DatabaseType dbType, string sql, List<SqlParam>? parametros, Func<SqlDataReader, TEntidade> mapeamento)
        {
           var res = SqlGeneralConnection.Query<TEntidade>(_connString, dbType, sql, parametros, mapeamento);
           return res;
        }
        
        /// <summary>
        /// Retorna o comando SQL completo com os valores dos parâmetros.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>Uma string com o comando SQL. Retorna null caso não exista parâmetros na consulta.</returns>
        public string? QueryText(string sql)
        {
            IDatabaseConnection? conexao = null;
            if (_databaseType == DatabaseType.SQLServer)
                conexao = new SqlServerConnectionManager(_connString, sql);
            //if (_databaseType == DatabaseType.SQLLite)
            //    conexao = new SqlLiteConnectionManager(_connString, sql);

            if (conexao is null) return null;

            var resultado = conexao.QueryText(parametros.Count == 0 ? null : parametros);
            return resultado;

        }

        /// <summary>
        /// Executa um comando SQL que retorna dados.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="mapeamento"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>O objeto de mapeamento.</returns>
        public Task<T?> QueryAsync<T>(string sql, Func<SqlDataReader, T> mapeamento, CancellationToken cancellationToken)
        {
            var conexao = new SqlServerConnectionManager(_connString, sql);
            Task<T?> resultado = conexao.QueryAsync(parametros.Count == 0 ? null : parametros, mapeamento, cancellationToken);
            parametros.Clear();
            return resultado;
        }

        /// <summary>
        /// Executa um comando SQL que retorna um dado apenas.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="mapeamento"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Um objeto do tipo definido.</returns>
        public Task<T?> ExecuteScalarAsync<T>(string sql, CancellationToken cancellationToken)
        {
            var conexao = new SqlServerConnectionManager(_connString, sql);
            Task<T?> resultado = conexao.ExecuteScalarAsync<T>(parametros.Count == 0 ? null : parametros, cancellationToken);
            parametros.Clear();
            return resultado;
        }

        /// <summary>
        /// Executa um comando SQL que não retorna dados.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="fullSql"></param>
        /// <returns></returns>
        public Task<bool> NonQueryAsync(string sql, CancellationToken cancellationToken)
        {
            var conexao = new SqlServerConnectionManager(_connString, sql);
            Task<bool> resultado = conexao.NonQueryAsync(parametros, cancellationToken);
            parametros.Clear();
            return resultado;
        }

        /// <summary>
        /// Executa uma transação com uma lista de comandos SQL.
        /// Fecha ou executa rollback automático em caso de sucesso ou falha da transação.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Retorna true se todos os comandos forem executados com sucesso. Caso contrário, retorna false.
        /// Caso algum erro ocorra, uma exceção do tipo RawSqlException será lançada.</returns>
        /// <exception cref="RawSqlException"></exception>
        public Task<bool> TransactionAsync(string[] sql, CancellationToken cancellationToken)
        {
            var conexao = new SqlServerConnectionManager(_connString, sql);
            Task<bool> resultado = conexao.ExecuteTransactionAsync(sql, cancellationToken);
            parametros.Clear();
            return resultado;
        }

        /// <summary>
        /// Adiciona um parâmetro NULLABLE à lista de parâmetros.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        public void AddNullableParam(string name, SqlDbType dbType, object? value)
        {
            parametros.Add(new SqlParam { ParamName = name, ParamDbType = dbType, ParamValue = value });
        }

        /// <summary>
        /// Adiciona um parâmetro à lista de parâmetros.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        public void AddParam(string name, SqlDbType dbType, object value, bool withoutQuote = false)
        {
            parametros.Add(new SqlParam { ParamName = name, ParamDbType = dbType, ParamValue = value, WithoutQuote = withoutQuote });
        }
    }
}
