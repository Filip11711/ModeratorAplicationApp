namespace ModeratorAplicationApp.Models
{
    public class AktivnostiPodaci
    {
        public List<AplicationInfo> ProcessInstances { get; set; }
        public List<TaskInfo> MyTasks { get; set; }
        public List<TaskInfo> ModeratorsTasks { get; set; }

        public IEnumerable<AplicationInfo> ActiveAplications => ProcessInstances.Where(instance => !instance.Ended);

        public IEnumerable<AplicationInfo> FinishedAplications => ProcessInstances.Where(instance => instance.Ended);
    }
}
