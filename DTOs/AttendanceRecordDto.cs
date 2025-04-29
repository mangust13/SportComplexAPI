// Data/SportComplexContext.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using SportComplexAPI.Models;

namespace SportComplexAPI.DTOs
{
    public class AttendanceRecordDto
    {
        public int AttendanceId { get; set; }
        public int PurchaseNumber { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string SubscriptionName { get; set; } = null!;
        public string SubscriptionTerm { get; set; } = null!;
        public string SubscriptionVisitTime { get; set; } = null!;
        public string ActivityName { get; set; } = null!;
        public int ActivityTypeAmount { get; set; }
        public string TrainerName { get; set; } = null!;
        public string TrainerSpecialization { get; set; } = null!;
        public int GymNumber { get; set; }
        public string SportComplexAddress { get; set; } = null!;
    }
}