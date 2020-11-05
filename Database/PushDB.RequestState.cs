using System.Data;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using NetCore_PushServer.Enums;

namespace NetCore_PushServer.Database
{
    public partial class PushDB
    {
		public static async Task RequestStateAsync(PushType type, long requestNo, PushState state)
		{
			using (var conn = new MySqlConnection(GetConnectionString()))
			{
				await SpRequestStateAsync(conn, null, type, requestNo, (sbyte)state);
			}
		}

		private static async Task SpRequestStateAsync(MySqlConnection conn, MySqlTransaction trxn, PushType type, long requestNo, sbyte state)
		{
			var parameters = new DynamicParameters();
			parameters.Add("_requestNo", requestNo);
			parameters.Add("_state", state);

			var sp = type == PushType.Google ? "spFcmRequestState" : "spApnRequestState";
			await conn.ExecuteAsync(sp, parameters, trxn, commandType: CommandType.StoredProcedure);
		}
	}
}
