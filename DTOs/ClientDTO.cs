// Data/SportComplexContext.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using SportComplexAPI.Models;

namespace SportComplexAPI.DTOs
{
    public class ClientDTO
    {
        public int ClientId {  get; set; }
        public string ClientFullName { get; set; }
        public string Gender { get; set; }
        public string ClientPhoneNumber { get; set; }
    }
}