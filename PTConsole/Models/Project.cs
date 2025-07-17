using System.Drawing;

namespace PTConsole.Models
{
    public class Project : AbstractEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
        public Color Color { get; set; }
        public Client? Client { get; set; }
        public ICollection<WorkTask>? Tasks { get; set; } = [];
        public ICollection<User> Users { get; set; } = [];
        
    }
}
