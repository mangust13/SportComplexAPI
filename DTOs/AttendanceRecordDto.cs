namespace SportComplexAPI.DTOs
{
    public class AttendanceRecordDto
    {
        public int AttendanceId { get; set; }
        public int PurchaseNumber { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string ClientFullName { get; set; } = null!;
        public string SubscriptionName { get; set; } = null!;
        public string SubscriptionTerm { get; set; } = null!;
        public string SubscriptionVisitTime { get; set; } = null!;
        public List<ActivityDto> SubscriptionActivities { get; set; } = new();
        public int GymNumber { get; set; }
        public string SportComplexAddress { get; set; } = null!;
        public string SportComplexCity { get; set; } = null!;
        public DateTime AttendanceDateTime { get; set; }
        public TimeSpan TrainingStartTime { get; set; }
        public TimeSpan TrainingEndTime { get; set; }
        public string TrainingActivity { get; set; } = null!;
    }


    public class ActivityDto
    {
        public int ActivityId { get; set; }
        public string ActivityName { get; set; } = null!;
    }
}
