using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BlackjackCommon.ViewModels
{
    public class ReplayListModel
    {
        public required string group_id { get; set; }
        public required int round { get; set; }
        public DateTime? datetime { get; set; }
        public required int wins { get; set; }
        public required int losses { get; set; }
        public required int earnings_amt { get; set; }
        public required int losses_amt { get; set; }
    }
}
