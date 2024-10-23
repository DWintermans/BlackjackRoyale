using BlackjackCommon.Interfaces.Logic;
using Player = BlackjackCommon.Models.Player;

namespace BlackjackLogic
{
	public class PlayerLogic : IPlayerLogic
	{
		public void RetrieveCredits(Player player)
		{
			//make db call
			player.Credits = 100;
		}

	}
}
