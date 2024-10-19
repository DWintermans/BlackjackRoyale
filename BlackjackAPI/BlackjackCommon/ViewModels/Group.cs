namespace BlackjackCommon.ViewModels
{
	public class GroupModel
	{
		public string? Group_ID { get; set; }
		public List<Member>? Members { get; set; } = new List<Member>();
	}

	public class Member
	{
		public int User_ID { get; set; }
		public string Name { get; set; }
		public bool InWaitingRoom { get; set; }
		public bool IsReady { get; set; }
		public int? Credits { get; set; }

		public Member(int user_id, string name, bool inWaitingRoom, bool isReady, int? credits)
		{
			User_ID = user_id;
			Name = name;
			InWaitingRoom = inWaitingRoom;
			IsReady = isReady;
			Credits = credits;
		}
	}
}