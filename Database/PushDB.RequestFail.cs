using System.Data;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using NetCore_PushServer.Enums;

namespace NetCore_PushServer.Database
{
    public partial class PushDB
    {
        public static async Task RequestFailAsync(long requestNo, int count, PushType type)
        {
            using (var conn = new MySqlConnection(GetConnectionString()))
            {
                await SpRequestFailAsync(conn, null, requestNo, count, type);
            }
        }

        public static async Task RequestFailAsync(MySqlTransaction trxn, long requestNo, int count, PushType type)
        {
            await SpRequestFailAsync(trxn.Connection, trxn, requestNo, count, type);
        }

        private static async Task SpRequestFailAsync(MySqlConnection conn, MySqlTransaction trxn, long requestNo, int count, PushType type)
        {
            var parameters = new DynamicParameters();
            parameters.Add("_requestNo", requestNo);
            parameters.Add("_count", count);

            var sp = type == PushType.Google ? "spFcmRequestFail" : "spApnRequestFail";
            await conn.ExecuteAsync(sp, parameters, trxn, commandType: CommandType.StoredProcedure);
        }
    }
}
