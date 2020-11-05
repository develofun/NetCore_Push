using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using NetCore_PushServer.Enums;

namespace NetCore_PushServer.Database
{
    public partial class PushDB
    {
		public static async Task ExpirePushTokensAsync(PushType type, List<long> seqs)
		{
			if (seqs == null || seqs.Count == 0) return;
			var text = StringEx.ToStringWithComma(seqs);

			using (var conn = new MySqlConnection(GetConnectionString()))
			{
				await SpExpirePushTokensAsync(conn, null, type, text);
			}
		}

		private static async Task SpExpirePushTokensAsync(MySqlConnection conn, MySqlTransaction trxn, PushType type, string seqs)
		{
			var parameters = new DynamicParameters();
			parameters.Add("_seqs", seqs);

			var sp = type == PushType.Google ? "spFcmDeviceTokenDeleteList" : "spApnDeviceTokenDeleteList";
			await conn.ExecuteAsync(sp, parameters, trxn, commandType: CommandType.StoredProcedure);
		}
	}
}
