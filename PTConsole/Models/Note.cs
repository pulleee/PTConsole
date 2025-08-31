namespace PTConsole.Models
{
    public class Note : AbstractEntity
    {
        public User User { get; set; }
        public string Message { get; set; } = string.Empty;
        public Project? Project { get; set; }

        public Note() { }

        public Note(User user, string message)
        {
            User = user;
            Message = message;
        }
    }
}
