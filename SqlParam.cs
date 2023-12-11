
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace RawSqlLib
{
    [ExcludeFromCodeCoverage]
    public class SqlParam
    {
        public string ParamName { get; set; }
        public SqlDbType ParamDbType { get; set; }
        public object ParamValue { get; set; }
    }
}
