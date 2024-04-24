using Common.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace FlagpoleCRM.Models
{
    public class Template
    {
        public Template()
        {
            Campaigns = new HashSet<Campaign>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int? Type { get; set; }
        public string? Content { get; set; }
        public int? WebsiteId { get; set; }
        public string? WebsiteGuid { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }

        public string? TypeAtView
        {
            get
            {
                if (Type == null) return "-";
                return Enum.GetName(typeof(EChannelSubscribe), Type);
            }
        }

        public virtual ICollection<Campaign> Campaigns { get; set; }
    }
}
