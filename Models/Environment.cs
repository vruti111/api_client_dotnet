using System;
using System.ComponentModel.DataAnnotations;

namespace API_CLIENT.Models
{
    public class Environment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "New Environment";

        // JSON mapping of Key-Value pairs for variables
        // { "baseUrl": "http://localhost:8000", "token": "abc..." }
        public string? VariablesJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
