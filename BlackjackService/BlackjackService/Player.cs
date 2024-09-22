namespace BlackjackService
{
	public class Player
	{
		public int User_ID { get; private set; }
		public List<string> Hand { get; private set; }
		public bool IsReady { get; set; }

		public Player(int user_id)
		{
			User_ID = user_id;
			Hand = new List<string>();
		}

		public void AddCard(string card)
		{
			Hand.Add(card);
		}

		public void ClearHand()
		{
			Hand.Clear();
		}
	}
}
