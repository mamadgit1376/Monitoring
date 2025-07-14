namespace Monitoring_Support_Server.Models.ViewModels
{
    public class ShowMonitoringModel
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int PercentSuccess { get; set; }
    }
}
