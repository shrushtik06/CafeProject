namespace RolesAuth.Models
{
    public class OrderAnalyticsViewModel
    {
        public List<string> Categories { get; set; }
        public List<int> OrderCounts { get; set; }
        public List<double> TotalAmounts { get; set; }
    }
}
