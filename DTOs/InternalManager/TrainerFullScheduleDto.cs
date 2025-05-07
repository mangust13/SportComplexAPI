namespace SportComplexAPI.DTOs.InternalManager
{
    public class TrainerFullScheduleDto
    {
        public int TrainerId { get; set; }
        public string TrainerFullName { get; set; } = null!;
        public string TrainerPhoneNumber { get; set; } = null!;
        public string TrainerGender { get; set; } = null!;
        public string TrainerAddress { get; set; } = null!;
        public string TrainerCity { get; set; } = null!;
        public List<TrainerScheduleEntryDto> Schedule { get; set; } = new();
    }

    public class TrainerScheduleEntryDto
    {
        public int ScheduleId { get; set; }
        public string DayName { get; set; } = null!;
        public string ActivityName { get; set; } = null!;
        public string StartTime { get; set; } = null!;
        public string EndTime { get; set; } = null!;
    }

}
