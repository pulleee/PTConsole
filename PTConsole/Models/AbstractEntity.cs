namespace PTConsole.Models
{
    public abstract class AbstractEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; private set; } = DateTime.Now;
        public DateTime ChangedAt { get; set; } = DateTime.Now;
    }
}
