using Microsoft.Data.SqlClient;
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
        private string _connString = "";

        public SqlServerConnectionManager(string connString, string sql)
        {
            _connString = connString;
            AbreComString(sql);
        }


        public async Task<T?> QueryAsync<T>(List<SqlParam>? parametros, Func<SqlDataReader, T> funcao, CancellationToken cancellationToken)
        {
            if (_comando is null)
            {
                Fecha();
                return default(T);
            }

            if (parametros is not null && parametros.Count > 0)
            {
                foreach (var p in parametros)
                {
                    CriaParametroSql(p.ParamName, p.ParamDbType, p.ParamValue);
                }
            }

            _reader = await _comando.ExecuteReaderAsync(cancellationToken);
            Console.WriteLine(_comando.CommandText);
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

            var linhasAfetadas = await _comando.ExecuteNonQueryAsync(cancellationToken);
            if (linhasAfetadas <= 0)
            {
                Fecha();
                return false;
            }

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
                //Console.WriteLine("Abriu conexao!");
                var cmd = new SqlCommand(sql, _conexao);
                if (cmd == null)
                {
                    return;
                }
                cmd.Parameters.Clear();
                _comando = cmd;
            }
            catch
            {
                throw;
            }
        }

        private void Fecha()
        {
            if (_reader != null)
                _reader.Close();
            if (_conexao != null)
                _conexao.Close();
            //Console.WriteLine("Fechou conexao!");
        }
        private void CriaParametroSql(string parametro, SqlDbType tipoSql, object? valor)
        {
            _comando!.Parameters.Add(parametro, tipoSql).Value = valor ?? DBNull.Value;
        }
    }
}
