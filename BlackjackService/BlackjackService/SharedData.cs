namespace BlackjackService
{
	public static class SharedData
	{
		public static Dictionary<string, string> userIDToCliendIdMap = new Dictionary<string, string>(); //user_id client_id(uuid)

		public static Dictionary<int, Player> Players = new Dictionary<int, Player>();
		public static Dictionary<string, Group> Groups = new Dictionary<string, Group>();

	}
}
