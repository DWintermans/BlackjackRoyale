using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BlackjackCommon.ViewModels
{
    public class SearchModel
    {
        public required string user_name { get; set; }
        public required int user_id { get; set; }
    }
}
