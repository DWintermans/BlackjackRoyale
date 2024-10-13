using BlackjackCommon.Interfaces.Logic;
using BlackjackCommon.Interfaces.Repository;
using BlackjackCommon.Models;
using Group = BlackjackCommon.Models.Group;
using Player = BlackjackCommon.Models.Player;
using BlackjackCommon.Data.SharedData;


namespace BlackjackLogic
{
	public class PlayerLogic: IPlayerLogic
	{
		private readonly BlackjackCommon.Models.Player _player;

		public PlayerLogic(Player player)
		{
			_player = player;
		}

		public void AddCard(string card)
		{
			_player.Hand.Add(card);
		}

		public void ClearHand()
		{
			_player.Hand.Clear();
		}

		public void SetReadyStatus(bool status)
		{
			_player.IsReady = status;
		}

	}
}
