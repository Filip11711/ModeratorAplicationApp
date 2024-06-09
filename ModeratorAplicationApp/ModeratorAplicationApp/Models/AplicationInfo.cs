namespace ModeratorAplicationApp.Models
{
    public class AplicationInfo
    {
        public string Aplicant { get; set; }
        public int AplicationId { get; set; }
        public string PID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool Ended { get; set; }
        public bool CanApplyForQuestioning { get; set; }
        public string Moderator { get; set; }
        public bool CurrentUserInGroup { get; set; }
        public bool Taken { get; set; }
    }
}
