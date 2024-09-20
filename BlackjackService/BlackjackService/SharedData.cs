namespace BlackjackService
{
	public static class SharedData
	{
		public static Dictionary<string, string> userIDToCliendIdMap = new Dictionary<string, string>();
		public static Dictionary<string, List<int>> groupMembers = new Dictionary<string, List<int>>();
	}
}
