namespace Monitoring_Support_Server.Models.ViewModels
{
    public class ResultModel
    {
        public ResultModel(bool Execute, string Message)
        {
            this.Execute = Execute;
            this.Message = Message;
        }
        public ResultModel(bool Execute, string Message, int Count)
        {
            this.Execute = Execute;
            this.Message = Message;
            this.Count = Count;
        }
        public ResultModel() { }
        public bool Execute { get; set; }
        public string Message { get; set; }
        public int? Count { get; set; } = null;
    }

    public class ApiResponse
    {
        public string Message { get; set; }
        public object? Data { get; set; } = null;
    }
}
