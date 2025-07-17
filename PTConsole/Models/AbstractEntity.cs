namespace PTConsole.Models
{
    public abstract class AbstractEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ChangedAt { get; set; }
        
        public AbstractEntity()
        {
            CreatedAt = DateTime.Now;
        }
    }
}
