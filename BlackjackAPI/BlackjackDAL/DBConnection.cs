using BlackjackCommon.Entities.User;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlackjackDAL
{
	public class DBConnection
	{
		static DBConnection()
		{
			Env.Load();
		}

		private const string _SERVER = "DB_SERVER";
		private const string _USER = "DB_USER";
		private const string _PASSWORD = "DB_PASSWORD";
		private const string _DATABASE = "DB_DATABASE";

		public string ConnectionString()
		{
			string server = Env.GetString(_SERVER);
			string user = Env.GetString(_USER);
			string password = Env.GetString(_PASSWORD);
			string database = Env.GetString(_DATABASE);

			if (server.Contains("localhost", StringComparison.OrdinalIgnoreCase))
			{
				return $"Server={server};Database={database};User Id={user};Password={password};";
			}
			else
			{
				return $"Server={server};Database={database};User Id={user};Password={password};Encrypt=True;TrustServerCertificate=True;";
			}
		}
	}
}
