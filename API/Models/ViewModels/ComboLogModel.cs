namespace Monitoring_Support_Server.Models.ViewModels
{
    public class ComboLogModel
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
    }

    public class ClientLogCombos
    {
        public List<ComboLogModel> FunctionLogs { get; set; } = new List<ComboLogModel>();
        public List<ComboLogModel> TypeLogs { get; set; } = new List<ComboLogModel>();
    }
}
