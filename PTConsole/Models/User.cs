namespace PTConsole.Models
{
    public class User : AbstractEntity
    {        
        public string Alias { get; set; } = string.Empty;
        public TimeSpan TotalTimeWorking { get; set; } = TimeSpan.Zero;
        public ICollection<Project> Projects { get; set; } = [];
        public ICollection<Session> Sessions { get; set; } = [];
    }
}
