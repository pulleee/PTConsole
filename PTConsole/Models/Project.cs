
using Spectre.Console;
using System.ComponentModel.DataAnnotations.Schema;

namespace PTConsole.Models
{
    public class Project : AbstractEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
        public string ColorHex { get; set; }
        public Client? Client { get; set; }
        public ICollection<WorkTask>? Tasks { get; set; } = [];
        public ICollection<User> Users { get; set; } = [];

        [NotMapped]
        public Color Color
        {
            get => Color.FromHex(ColorHex);
            set => ColorHex = value.ToHex();
        }
    }
}
