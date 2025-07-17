namespace PTConsole.Models
{
    public class WorkTask : AbstractEntity
    {
        public int Priority { get; set; }
        public string Description { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
    }
}
