using System;
using System.ComponentModel.DataAnnotations;

namespace API_CLIENT.Models
{
    public class ApiRequest
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? CollectionId { get; set; }
        public Collection? Collection { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "Untitled";

        [Required]
        [StringLength(10)]
        public string Method { get; set; } = "GET";

        [Required]
        public string Url { get; set; } = string.Empty;

        // Store Headers as a JSON string for simplicity in the DB
        public string? HeadersJson { get; set; }

        // Store Query Parameters as a JSON string
        public string? QueryParamsJson { get; set; }

        // Body Configuration
        public string BodyType { get; set; } = "none"; // none, json, formdata, text
        public string? Body { get; set; }

        // Authorization Configuration
        public string AuthType { get; set; } = "none"; // none, bearer, basic
        public string? AuthToken { get; set; } // For Bearer
        public string? AuthUsername { get; set; } // For Basic
        public string? AuthPassword { get; set; } // For Basic

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
