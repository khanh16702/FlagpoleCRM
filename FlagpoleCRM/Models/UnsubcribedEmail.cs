using Microsoft.AspNetCore.Mvc;

namespace FlagpoleCRM.Models
{
    public class UnsubcribedEmail
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
