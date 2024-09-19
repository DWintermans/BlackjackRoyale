namespace BlackjackDAL
{
	public class DBConnection
	{
		private const string _SERVER = "localhost";
		private const string _USER = "root";
		private const string _PASSWORD = "usbw";
		private const string _DATABASE = "blackjackroyale";

		public string ConnectionString()
		{
			return $"server={_SERVER};user={_USER};password={_PASSWORD};database={_DATABASE};";
		}
	}
}
