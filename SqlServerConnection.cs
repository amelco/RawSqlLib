using Microsoft.Data.SqlClient;
using RawSqlLib.Exceptions;
using System.Data;
using System.Diagnostics.CodeAnalysis;
namespace RawSqlLib
{
    [ExcludeFromCodeCoverage]
    public class SqlServerConnectionManager : IDatabaseConnection
    {
        private SqlConnection? _conexao;
        private SqlCommand? _comando;
        private SqlDataReader? _reader;
        private SqlTransaction? transacao;
        private string _connString = "";
        private bool _debug = false;

        public SqlServerConnectionManager(string connString, string sql, bool debug = false)
        {
            _connString = connString;
            AbreComString(sql);
            _debug = debug;
        }

        public SqlServerConnectionManager(string connString, string[] sql)
        {
            _connString = connString;
            AbreTransacao(sql);
        }


        public async Task<T?> QueryAsync<T>(List<SqlParam>? parametros, Func<SqlDataReader, T> funcao, CancellationToken cancellationToken)
        {
            var sql = QueryText(parametros);
            if (sql is null)
            {
                return default(T);
            }

            _reader = await _comando!.ExecuteReaderAsync(cancellationToken);

            if (_reader is null)
            {
                Fecha();
                return default(T);
            }

            if (!_reader.HasRows)
            {
                Fecha();
                return default(T);
            }

            T? retorno = default(T);
            if (_reader.Read())
            {
                retorno = funcao(_reader);
            }

            Fecha();

            return retorno;
        }

        public string? QueryText(List<SqlParam>? parametros)
        {
            if (_comando is null)
            {
                Fecha();
                return default;
            }

            if (parametros is not null && parametros.Count > 0)
            {
                foreach (var p in parametros)
                {
                    CriaParametroSql(p.ParamName, p.ParamDbType, p.ParamValue);
                }
            }

            return GetFullSqlString(_comando.CommandText, parametros);
        }

        public async Task<bool> NonQueryAsync(List<SqlParam>? parametros, CancellationToken cancellationToken)
        {
            if (_comando is null)
            {
                Fecha();
                return false;
            }

            if (parametros is not null && parametros.Count > 0)
            {
                foreach (var p in parametros)
                {
                    CriaParametroSql(p.ParamName, p.ParamDbType, p.ParamValue);
                }
            }

            // imprime o comando SQL completo com todos os parametros
            //Console.WriteLine(_comando.CommandText);

            var linhasAfetadas = await _comando.ExecuteNonQueryAsync(cancellationToken);
            if (linhasAfetadas <= 0)
            {
                Fecha();
                return false;
            }

            Fecha();
            return true;
        }

        public async Task<T?> ExecuteScalarAsync<T>(List<SqlParam>? parametros, CancellationToken cancellationToken)
        {
            var sql = QueryText(parametros);
            if (sql is null)
            {
                return default(T);
            }

            var retorno = await _comando!.ExecuteScalarAsync(cancellationToken);

            Fecha();

            return (T)retorno;
        }

        public async Task<bool> ExecuteTransactionAsync(string[] sql, CancellationToken cancellationToken)
        {
            if (_comando is null || transacao is null)
            {
                Fecha();
                return false;
            }

            foreach (var s in sql)
            {
                _comando.CommandText = s;
                try
                {
                    await _comando.ExecuteNonQueryAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    transacao.Rollback();
                    Fecha();
                    throw new RawSqlException(e.Message);
                    //return false;
                }
            }
            transacao.Commit();
            Fecha();
            return true;
        }

        private void AbreComString(string sql)
        {
            try
            {
                _conexao = new SqlConnection(_connString);
                if (_conexao == null)
                {
                    return;
                }
                _conexao.Open();
                if (_debug) Console.WriteLine("Abriu conexao!");
                var cmd = new SqlCommand(sql, _conexao);
                if (cmd == null)
                {
                    return;
                }
                cmd.Parameters.Clear();
                _comando = cmd;
            }
            catch (Exception e)
            {
                throw new RawSqlException(e.Message);
            }
        }

        private void AbreTransacao(string[] sql)
        {
            try
            {
                _conexao = new SqlConnection(_connString);
                if (_conexao == null)
                {
                    return;
                }
                _conexao.Open();
                if (_debug) Console.WriteLine("Abriu conexao!");
                var cmd = _conexao.CreateCommand();
                if (cmd == null)
                {
                    return;
                }
                cmd.Parameters.Clear();
                _comando = cmd;
                transacao = _conexao.BeginTransaction();
                _comando.Transaction = transacao;
            }
            catch (Exception e)
            {
                throw new RawSqlException(e.Message);
            }
        }

        private void Fecha()
        {
            if (_reader != null)
                _reader.Close();
            if (_conexao != null)
                _conexao.Close();
            if (_debug) Console.WriteLine("Fechou conexao!");
        }
        private void CriaParametroSql(string parametro, SqlDbType tipoSql, object? valor)
        {
            _comando!.Parameters.Add(parametro, tipoSql).Value = valor ?? DBNull.Value;
        }

        private string GetFullSqlString(string commandText, List<SqlParam>? parametros)
        {
            // replace all parameters of kind @paramName of commandText with their values
            string sql = commandText;
            if (parametros is not null && parametros.Count > 0)
            {
                foreach (var p in parametros)
                {
                    string replacement;
                    bool quote = (
                           p.ParamDbType == SqlDbType.Char
                        || p.ParamDbType == SqlDbType.VarChar
                        || p.ParamDbType == SqlDbType.NVarChar
                        || p.ParamDbType == SqlDbType.Text
                        || p.ParamDbType == SqlDbType.NText
                        || p.ParamDbType == SqlDbType.UniqueIdentifier
                        || p.ParamDbType == SqlDbType.DateTime
                        || p.ParamDbType == SqlDbType.SmallDateTime
                        || p.ParamDbType == SqlDbType.Time
                        || p.ParamDbType == SqlDbType.Timestamp
                        || p.ParamDbType == SqlDbType.Date
                        || p.ParamDbType == SqlDbType.DateTime2
                        || p.ParamDbType == SqlDbType.DateTimeOffset
                        || p.ParamDbType == SqlDbType.Udt
                    );

                    if (p.ParamDbType == SqlDbType.Bit && p.ParamValue is not null)
                        replacement = (bool)p.ParamValue ? "1" : "0";
                    else if (p.ParamDbType == SqlDbType.DateTime && p.ParamValue is not null)
                        replacement = ((DateTime)p.ParamValue).ToString("s");
                    else if (
                           p.ParamDbType == SqlDbType.DateTime2
                        || p.ParamDbType == SqlDbType.SmallDateTime
                        || p.ParamDbType == SqlDbType.DateTimeOffset
                        || p.ParamDbType == SqlDbType.Binary
                        || p.ParamDbType == SqlDbType.VarBinary
                        || p.ParamDbType == SqlDbType.Xml
                        || p.ParamDbType == SqlDbType.Image
                        || p.ParamDbType == SqlDbType.Money
                        || p.ParamDbType == SqlDbType.SmallMoney
                        || p.ParamDbType == SqlDbType.Timestamp
                        || p.ParamDbType == SqlDbType.Variant
                        || p.ParamDbType == SqlDbType.Udt
                        || p.ParamDbType == SqlDbType.Structured
                        || p.ParamDbType == SqlDbType.Date
                        || p.ParamDbType == SqlDbType.Time
                    )
                        throw new Exception($"[ERROR]: Dado do tipo SqlDbType={p.ParamDbType} ainda não implementado em GetFullSqlString");
                    else
                        replacement = p.ParamValue?.ToString() ?? "NULL";

                    if (quote && !p.WithoutQuote) replacement = "'" + replacement + "'";

                    sql = sql.Replace(p.ParamName, replacement);
                }
            }
            if (sql[sql.Length - 1] != ';') sql += ";";
            if (_debug) Console.WriteLine(sql);
            return sql;
        }
    }
}
