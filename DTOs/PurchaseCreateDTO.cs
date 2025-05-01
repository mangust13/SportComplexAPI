// Data/SportComplexContext.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using SportComplexAPI.Models;

namespace SportComplexAPI.DTOs
{
    public class PurchaseCreateDTO
    {
        public int ClientId { get; set; }
        public int SubscriptionId { get; set; }
        public int PaymentMethodId { get; set; }
    }
}

