namespace BlackjackCommon.Models
{
	public class Player
	{
		public int User_ID { get; private set; }
		public string Name { get; private set; }
		public List<string> Hand { get; private set; }
		public bool IsReady { get; set; }

		public Player(int user_id, string name)
		{
			User_ID = user_id;
			Name = name;
			Hand = new List<string>();
			IsReady = false;
		}
	}
}
