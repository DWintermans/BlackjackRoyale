using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BlackjackCommon.ViewModels
{
    public class ReplayModel
    {
        public required string type { get; set; }
        public required int round { get; set; }
        public DateTime? datetime { get; set; }
        public required string payload { get; set; }
    }
}
