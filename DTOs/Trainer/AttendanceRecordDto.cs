namespace SportComplexAPI.DTOs.Trainer
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

    public class TrainerProfileDto
    {
        public string TrainerFullName { get; set; } = null!;
        public int GymNumber { get; set; }

        public List<ActivityDto> Activities { get; set; } = new();
        public List<TrainerScheduleEntryDto> Schedules { get; set; } = new();
    }

    public class TrainerScheduleEntryDto
    {
        public int TrainerScheduleId { get; set; }
        public int ScheduleId { get; set; }
        public string DayName { get; set; } = null!;
        public string StartTime { get; set; } = null!;
        public string EndTime { get; set; } = null!;
        public int ActivityId { get; set; }
        public string ActivityName { get; set; } = null!;
        public int GymNumber { get; set; }
        public string SportComplexAddress { get; set; } = null!;
        public string SportComplexCity { get; set; } = null!;
        public int TrainerId { get; set; }
    }

    public class AddAttendanceDto
    {
        public int ClientId { get; set; }
        public int PurchaseId { get; set; }
        public int TrainerScheduleId { get; set; }
    }

    public class ClientWithPurchasesDto
    {
        public int ClientId { get; set; }
        public string ClientFullName { get; set; } = null!;
        public string ClientPhoneNumber { get; set; } = null!;
        public string ClientGender { get; set; } = null!;
        public List<PurchaseShortDto> Purchases { get; set; } = new();
    }

    public class PurchaseShortDto
    {
        public int PurchaseId { get; set; }
        public int PurchaseNumber { get; set; }
        public string SubscriptionName { get; set; } = null!;
    }
}
