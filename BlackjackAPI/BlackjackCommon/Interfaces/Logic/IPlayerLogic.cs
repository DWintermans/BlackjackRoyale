namespace BlackjackCommon.Interfaces.Logic
{
	public interface IPlayerLogic
	{
		void AddCard(string card);
		void ClearHand();
		void SetReadyStatus(bool status);
	}
}