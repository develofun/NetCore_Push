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
		public static async Task<PushRequest> RequestAllGetAsync(PushType type)
		{
			using (var conn = new MySqlConnection(GetConnectionString()))
			{
				return await SpRequestAllGetAsync(conn, null, type);
			}
		}

		private static async Task<PushRequest> SpRequestAllGetAsync(MySqlConnection conn, MySqlTransaction trxn, PushType type)
		{
			var parameters = new DynamicParameters();
			parameters.Add("_requestNo", dbType: DbType.Int64, direction: ParameterDirection.Output);
			parameters.Add("_payload", dbType: DbType.String, direction: ParameterDirection.Output, size: 2048);
			parameters.Add("_locale", dbType: DbType.String, direction: ParameterDirection.Output, size: 2048);
			parameters.Add("_sentSeq", dbType: DbType.Int64, direction: ParameterDirection.Output);

			var sp = type == PushType.Google ? "spFcmRequestAllGet" : "spApnRequestAllGet";
			await conn.ExecuteAsync(sp, parameters, trxn, commandType: CommandType.StoredProcedure);

			long requestNo = parameters.Get<long>("_requestNo");
			if (requestNo > 0)
			{
				return new PushRequest
				{
					RequestNo = requestNo,
					Payload = parameters.Get<string>("_payload"),
					Locale = parameters.Get<string>("_locale"),
					SentSeq = parameters.Get<long>("_sentSeq")
				};
			}
			else
			{
				return null;
			}
		}
	}
}
