namespace ModeratorAplicationApp.Models
{
    public class TaskInfo
    {
        public string TID { get; set; }
        public string TaskKey { get; set; }
        public string TaskName { get; set; }
        public string Aplicant { get; set; }
        public int AplicationId { get; set; }
        public string PID { get; set; }
        public DateTime StartTime { get; set; }
        public string Moderator { get; set; }
        public string Email { get; set; }
        public string LanguageKnowledge { get; set; }
        public string MotivationalLetter { get; set; }
        public string Question1 { get; set; }
        public string Question2 { get; set; }
        public string Question3 { get; set; }
        public string Answer1 { get; set; }
        public string Answer2 { get; set; }
        public string Answer3 { get; set; }
    }
}
