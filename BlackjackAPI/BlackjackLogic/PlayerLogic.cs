using BlackjackCommon.Interfaces.Logic;
using Player = BlackjackCommon.Models.Player;


namespace BlackjackLogic
{
	public class PlayerLogic : IPlayerLogic
	{
		public void AddCard(Player player, string card)
		{
			player.Hand.Add(card);
		}

		public void ClearHand(Player player)
		{
			player.Hand.Clear();
		}

		public void SetReadyStatus(Player player, bool status)
		{
			player.IsReady = status;
		}

	}
}
