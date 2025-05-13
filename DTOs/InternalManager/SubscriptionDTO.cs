// Data/SportComplexContext.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using SportComplexAPI.Models;

namespace SportComplexAPI.DTOs.InternalManager
{
    public class SubscriptionDto
    {
        public int SubscriptionId { get; set; }
        public string SubscriptionName { get; set; } = null!;
        public decimal SubscriptionTotalCost { get; set; }
        public string SubscriptionTerm { get; set; }
        public string SubscriptionVisitTime { get; set; }
        public List<ActivityDto> Activities { get; set; } = new();
    }

    public class SubscriptionUpdateDto
    {
        public decimal SubscriptionTotalCost { get; set; }
        public string SubscriptionTerm { get; set; } = null!;
        public string SubscriptionVisitTime { get; set; } = null!;
        public List<ActivityDto> Activities { get; set; } = new();
    }

}
