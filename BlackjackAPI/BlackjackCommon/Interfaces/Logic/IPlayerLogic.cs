using BlackjackCommon.Models;

namespace BlackjackCommon.Interfaces.Logic
{
	public interface IPlayerLogic
	{
		void AddCard(Player player, string card);
		void ClearHand(Player player);
		void SetReadyStatus(Player player, bool status);
	}
}