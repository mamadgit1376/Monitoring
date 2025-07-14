using Monitoring.Support.Server.Models.DatabaseModels;

namespace Monitoring_Support_Server.Models.ViewModels
{
    public class ShowSingleCompanyMonitoring
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public List<ItemLogViewModel> ItemLog { get; set; } 
    }
}
