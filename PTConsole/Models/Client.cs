using System.Drawing;

namespace PTConsole.Models
{
    public class Client : AbstractEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
        public Color Color { get; set; } = Color.Red;
        public ICollection<Project> Projects { get; set; } = [];
    }
}
