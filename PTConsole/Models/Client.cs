using Spectre.Console;
using System.ComponentModel.DataAnnotations.Schema;

namespace PTConsole.Models
{
    public class Client : AbstractEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
        public string ColorHex { get; set; }
        public ICollection<Project> Projects { get; set; } = [];

        [NotMapped]
        public Color Color
        {
            get => Color.FromHex(ColorHex);
            set => ColorHex = value.ToHex();
        }
    }
}
