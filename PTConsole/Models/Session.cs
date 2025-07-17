namespace PTConsole.Models
{
    /*
     * Represents the Working Session on a Project. Time spend this Session gets summed up in the Projects Duration.  
     */
    public class Session : AbstractEntity
    {
        public User User { get; set; }
        public Project Project { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        public Session(User user, Project project)
        {
            User = user;
            Project = project;            
        }

        public void Begin()
        {
            StartDateTime = DateTime.Now;
        }

        public void End()
        {
            EndDateTime = DateTime.Now;

            User.TotalTimeWorking += EndDateTime - StartDateTime;
            Project.Duration += EndDateTime - StartDateTime;
        }
    }
}
