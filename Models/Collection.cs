using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace API_CLIENT.Models
{
    public class Collection
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "New Collection";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<ApiRequest> Requests { get; set; } = new List<ApiRequest>();
    }
}
