using System.Data;
using Microsoft.Data.SqlClient;
using RawSqlLib.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace RawSqlLib
{
    [ExcludeFromCodeCoverage]
    public static class SqlGeneralConnection
    {
/*
        public static string? QueryText(SqlConnection conexao, SqlDataReader reader, DatabaseType dbType, List<SqlParam>? parametros, string _connString, string sql)
        {
            Abre(ref conexao, _connString);

            SqlCommand? comando = new SqlCommand(sql, conexao);
            if (comando == null)
            {
                Fecha(ref reader, ref conexao);
                return default;
            }

            comando.Parameters.Clear();
            if (parametros is not null && parametros.Count > 0)
            {
                foreach (var p in parametros)
                {
                    CriaParametroSql(comando, p.ParamName, p.ParamDbType, p.ParamValue);
                }
            }

            return GetFullSqlString(comando.CommandText, parametros);
        }
*/

        public static TEntidade? Query<TEntidade>(string connString, DatabaseType dbType, string sql, List<SqlParam>? parametros, Func<SqlDataReader, TEntidade> mapeamento)
        {
            SqlConnection? conexao = null;
            SqlDataReader reader = null;
            Abre(ref conexao, connString);
            if (conexao is null)
            {
                Fecha(ref reader, ref conexao);
                throw new Exception("Erro ao estabelecer conexão com o banco de dados.");
            }
            SqlCommand? comando = new SqlCommand(sql, conexao);
            if (comando == null)
            {
                Fecha(ref reader, ref conexao);
                throw new Exception("Erro ao criar comando SQL.");
            }
            comando.Parameters.Clear();
            if (parametros is not null && parametros.Count > 0)
            {
                foreach (var p in parametros)
                {
                    CriaParametroSql(comando, p.ParamName, p.ParamDbType, p.ParamValue);
                }
            }
            
            reader = comando!.ExecuteReader();

            if (reader is null)
            {
                Fecha(ref reader, ref conexao);
                return default(TEntidade);
            }

            if (reader.HasRows is false)
            {
                Fecha(ref reader, ref conexao);
                return default(TEntidade);
            }

            TEntidade? retorno = default(TEntidade);
            if (reader.Read())
            {
                retorno = mapeamento(reader);
            }

            Fecha(ref reader, ref conexao);

            return retorno;
        }

        public static Task<T?> ExecuteScalarAsync<T>(List<SqlParam>? parametros, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
        public static Task<bool> NonQueryAsync(List<SqlParam> parametros, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
        public static Task<bool> ExecuteTransactionAsync(string[] sql, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }




        // Métodos privados

        private static void Abre(ref SqlConnection _conexao, string _connString)
        {
            try
            {
                _conexao = new SqlConnection(_connString);
                if (_conexao == null)
                {
                    return;
                }
                _conexao.Open();
                Console.WriteLine("Abriu conexao!");
            }
            catch (Exception e)
            {
                throw new RawSqlException(e.Message);
            }
        }

        private static void Fecha(ref SqlDataReader _reader, ref SqlConnection _conexao)
        {
            if (_reader != null)
                _reader.Close();
            if (_conexao != null)
                _conexao.Close();
            Console.WriteLine("Fechou conexao!");
        }

        private static void CriaParametroSql(SqlCommand _comando, string parametro, SqlDbType tipoSql, object? valor)
        {
            _comando!.Parameters.Add(parametro, tipoSql).Value = valor ?? DBNull.Value;
        }

        private static string GetFullSqlString(string commandText, List<SqlParam>? parametros)
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
            Console.WriteLine(sql);
            return sql;
        }
    }
}
