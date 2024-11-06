namespace BlackjackCommon.Models
{
	public class Player
	{
		public int User_ID { get; private set; }
		public string Name { get; private set; }
		public List<Hand> Hands { get; set; }
		public bool IsReady { get; set; }
		public int Credits { get; set; }
		public bool HasFinished { get; set; }
		public bool HasInsurance { get; set; }

		public Player(int user_id, string name)
		{
			User_ID = user_id;
			Name = name;
			Hands = new List<Hand>();
			IsReady = false;
			HasFinished = false;
			HasInsurance = false;
		}

		public class Hand
		{
			public List<string> Cards { get; private set; }
			public bool IsFinished { get; set; }
			public bool IsDoubled { get; set; }

			public Hand()
			{
				Cards = new List<string>();
				IsFinished = false;
				IsDoubled = false;
			}

		}
	}
}
