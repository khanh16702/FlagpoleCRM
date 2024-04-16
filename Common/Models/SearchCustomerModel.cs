namespace Common.Models
{
    public class SearchCustomerModel : DataTableModel
    {
        public string Condition { get; set; }
        public string Email { get; set; }
        public int Limit { get; set; }
        public string WebsiteId { get; set; }
        public string Rules { get; set; }
    }
}
