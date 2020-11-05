using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using NetCore_PushServer.Enums;
using NetCore_PushServer.Models;

namespace NetCore_PushServer.Database
{
    public partial class PushDB
    {
		public static async Task<List<PushToken>> GetPushTokenAsync(PushType type, long sentSeq, string targets, int limit)
		{
			using (var conn = new MySqlConnection(GetConnectionString()))
			{
				return await SpGetPushTokenAsync(conn, null, type, sentSeq, targets, limit);
			}
		}

		private static async Task<List<PushToken>> SpGetPushTokenAsync(MySqlConnection conn, MySqlTransaction trxn, PushType type, long sentSeq, string targets, int limit)
		{
			var parameters = new DynamicParameters();
			parameters.Add("_sentSeq", sentSeq);
			parameters.Add("_targets", targets);
			parameters.Add("_limit", limit);

			var sp = type == PushType.Google ? "spFcmDeviceTokenGetNextList" : "spApnDeviceTokenGetNextList";
			var list = await conn.QueryAsync<PushToken>(sp, parameters, trxn, commandType: CommandType.StoredProcedure);
			return list.AsList();
		}
	}
}
