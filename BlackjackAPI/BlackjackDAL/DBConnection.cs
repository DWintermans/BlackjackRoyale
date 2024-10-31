using DotNetEnv;

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

			return $"server={server};user={user};password={password};database={database};";
		}
	}
}
