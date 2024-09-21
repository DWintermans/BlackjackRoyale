using Microsoft.IdentityModel.Tokens;

namespace BlackjackService
{
	public class Game
	{
		public static async Task HandleGameAction(dynamic message, int user_id)
		{
			switch (message.action.ToString())
			{
				case "bet":
					//await Bet(user_id, message.bet.ToString());

				case "hit":
					//await Hit(user_id);
					break;

				case "stand":
					//await Stand(user_id);
					break;

				default:
					await Websocket.SendNotificationToUserID(user_id, "Unknown group action");
					break;
			}
		}
	
		



	}
}