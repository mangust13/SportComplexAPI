// Data/SportComplexContext.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using SportComplexAPI.Models;

namespace SportComplexAPI.DTOs
{
    public class PurchaseDto
    {
        public int PurchaseId { get; set; }
        public int TotalAttendances { get; set; }

        public int PurchaseNumber { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string PaymentMethod { get; set; }

        public string ClientFullName { get; set; }
        public string ClientGender { get; set; }
        public string ClientPhoneNumber { get; set; }

        public string SubscriptionName { get; set; }
        public decimal SubscriptionTotalCost { get; set; }
        public string SubscriptionTerm { get; set; }
        public string SubscriptionVisitTime { get; set; }

        public List<ActivityDto> Activities { get; set; } = new();
    }
}