using BlackjackCommon.Interfaces.Logic;
using BlackjackCommon.Interfaces.Repository;
using Player = BlackjackCommon.Models.Player;

namespace BlackjackLogic
{
	public class PlayerLogic : IPlayerLogic
	{
		private readonly IUserRepository _userDAL;

		public PlayerLogic(IUserRepository userDAL)
		{
			_userDAL = userDAL;
		}

		public void SetCredits(Player player)
		{
			try 
			{
				player.Credits = _userDAL.RetrieveCredits(player.User_ID);
			} 
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to set credits: {ex.Message}");
			}
		}

		//public void SetStatistics(Player player, int wins, int losses, int win_amt, int loss_amt) 
		//{
			
		//}

		public void UpdateCredits(Player player, int credits)
		{
			try
			{
				_userDAL.UpdateCredits(player.User_ID, credits);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to set credits: {ex.Message}");
			}
		}

	}
}
