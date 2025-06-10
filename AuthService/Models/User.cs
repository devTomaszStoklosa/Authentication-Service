using System;
using Newtonsoft.Json;

namespace AuthService.Models
{
    public class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } 
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Age { get; set; }   }
}