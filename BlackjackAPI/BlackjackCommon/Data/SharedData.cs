using BlackjackCommon.Models;

namespace BlackjackCommon.Data.SharedData
{
    public static class SharedData
    {
        public static Dictionary<string, string> userIDToCliendIdMap = new Dictionary<string, string>(); //user_id client_id(uuid)

        public static Dictionary<int, Player> Players = new Dictionary<int, Player>();
        public static Dictionary<string, Group> Groups = new Dictionary<string, Group>();

        public static Group? GetGroupForPlayer(Player player)
        {
            foreach (var group in Groups.Values)
            {
                if (group.Members.Any(p => p.User_ID == player.User_ID))
                {
                    return group;
                }
            }

            return null;
        }

        public static Group? GetGroupForWaitingroomPlayer(Player player)
        {
            foreach (var group in Groups.Values)
            {
                if (group.WaitingRoom.Any(p => p.User_ID == player.User_ID))
                {
                    return group;
                }
            }

            return null;
        }

        public static Player? TryGetExistingPlayer(int user_id)
        {
            SharedData.Players.TryGetValue(user_id, out var player);
            return player;
        }

    }
}
