namespace SusSuite.Plugins.Sheriff
{
    public class SheriffData
    {
        public int SheriffId { get; set; }
        public bool BeenNotified { get; set; }
        public bool HasShot { get; set; }
        public bool InMeeting { get; set; }
        public int MarkedForDeadId { get; set; }
        public bool ExecutionScheduled { get; set; }
    }
}
