using BlackjackCommon.Models;

namespace BlackjackCommon.Interfaces.Logic
{
	public interface IPlayerLogic
	{
		void SetCredits(Player player);
		void UpdateCredits(Player player, int credits);
		void UpdateStatistics(Player player, int gameWins, int gameLosses, int earnings, int losses);
	}
}