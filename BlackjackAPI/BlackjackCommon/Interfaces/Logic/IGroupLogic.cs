using BlackjackCommon.Models;

namespace BlackjackCommon.Interfaces.Logic
{
	public interface IGroupLogic
	{
		Task HandleGroupAction(Player player, dynamic message);
	}
}