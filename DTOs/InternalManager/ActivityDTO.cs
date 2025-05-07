// Data/SportComplexContext.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using SportComplexAPI.Models;

namespace SportComplexAPI.DTOs.InternalManager
{
    public class ActivityDto
    {
        public int ActivityId { get; set; }
        public string ActivityName { get; set; } = null!;
        public decimal ActivityPrice { get; set; }
        public string? ActivityDescription { get; set; }
        public int ActivityTypeAmount { get; set; }
    }
}
