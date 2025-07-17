namespace PTConsole.Models
{
    public class Note : AbstractEntity
    {
        public User User { get; set; }
        public string Message { get; set; } = string.Empty;

        public Note(User user, string message)
        {
            User = user;
            Message = message;
        }
    }
}
