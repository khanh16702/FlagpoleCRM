using FlagpoleCRM.Models;

namespace FlagpoleCRM.DTO
{
    public class CampaignDTO : Campaign
    {
        public string? AccountId { get; set; }
        public string? AccountEmail { get; set; }
    }
}
