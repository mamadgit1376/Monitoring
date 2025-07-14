namespace Monitoring_Support_Server.Models.ViewModels
{
    public class ShowUserModels
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Role {  get; set; }
        public string Companies { get; set; }
        public string FullName { get; set; }
        public DateTime? removeDate { get; set; }
    }
}
