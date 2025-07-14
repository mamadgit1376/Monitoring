namespace Monitoring_Support_Server.Models.ViewModels
{
    public class LogsModel
    {
        public int CompanyId { get; set; }

        public string? AddDate { get; set; }
        public string? AddTime { get; set; }
        public string? FinishTime { get; set; }
        public string? NameOfUser { get; set; }
        public string? FunctionDescription { get; set; }
        public string? LogType { get; set; }
        public double Time { get; set; }
        public string? IP { get; set; }
        public string? Message { get; set; }
        public string? FunctionName { get; set; }
        public string? ClassName { get; set; }
        public string? StackTrace { get; set; }
        public string? ExtraInfo { get; set; }
        public string? BrowserAddress { get; set; }
        public string? AgentName { get; set; }
    }
    public class FilteredLogsModel
    {
        public int CompanyId { get; set; }
        public string? FilterDate { get; set; }
        public string? FromTime { get; set; }
        public string? ToTime { get; set; }
        public string? LogTypeCodes { get; set; }
        public string? FilteredUsedIds { get; set; }
        public string? EnteredUsedIds { get; set; }
        public string? FunctionIds { get; set; }
        public string? FilterText { get; set; }
        public string? FilterIp { get; set; }
    }
}
