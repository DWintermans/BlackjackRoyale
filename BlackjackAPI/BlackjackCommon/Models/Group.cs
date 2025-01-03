﻿namespace BlackjackCommon.Models
{
    public class Group
    {
        public string Group_ID { get; private set; }
        public string Unique_Group_ID { get; private set; }
        public GroupStatus Status { get; set; }
        public List<Player> Members { get; private set; }
        public List<string> Deck { get; private set; }
        public List<string> DealerHand { get; private set; }
        public string? HoleCard { get; set; }
        public List<Player> WaitingRoom { get; private set; }

        public Dictionary<Player, int> Bets { get; private set; }
        public int Round { get; set; }

        public Group(string group_id, string unique_id)
        {
            Group_ID = group_id;
            Unique_Group_ID = unique_id;
            Members = new List<Player>();
            Deck = new List<string>();
            DealerHand = new List<string>();
            WaitingRoom = new List<Player>();
            Bets = new Dictionary<Player, int>();
            Round = 0;
        }

        public enum GroupStatus
        {
            WAITING,
            PLAYING,
            BETTING,
        }
    }
}
