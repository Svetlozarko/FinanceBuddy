using FinanceCalc.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceCalc.Models
{
    public class BankConnection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ConnectionId { get; set; }

        [Required]
        public string CustomerId { get; set; }

        public string ProviderCode { get; set; }
        public DateTime ConnectedAt { get; set; }

        // Foreign key to ApplicationUser (optional, but recommended)
        [Required]
        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
