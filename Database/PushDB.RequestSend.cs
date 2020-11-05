using System.Data;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using NetCore_PushServer.Enums;

namespace NetCore_PushServer.Database
{
    public partial class PushDB
    {
        public static async Task RequestSendAsync(PushType type, long requestNo, long sentSeq, PushState state)
        {
            using (var conn = new MySqlConnection(GetConnectionString()))
            {
                await SpRequestSendAsync(conn, null, type, requestNo, sentSeq, (sbyte)state);
            }
        }

        private static async Task SpRequestSendAsync(MySqlConnection conn, MySqlTransaction trxn, PushType type, long requestNo, long sentSeq, sbyte state)
        {
            var parameters = new DynamicParameters();
            parameters.Add("_requestNo", requestNo);
            parameters.Add("_sentSeq", sentSeq);
            parameters.Add("_state", state);

            var sp = type == PushType.Google ? "spFcmRequestAllGet" : "spApnRequestAllGet";
            await conn.ExecuteAsync(sp, parameters, trxn, commandType: CommandType.StoredProcedure);
        }
    }
}
