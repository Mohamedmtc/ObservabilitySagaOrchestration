using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Models
{
    public class OrderPrice
    {
        [Key]
        public Guid OrderId { get; set; } = Guid.NewGuid();

        public double price { get; set; }
        public string PaymentCardNumber { get; set; }

        public string ProductName { get; set; }

        [NotMapped]
        public bool IsCanceled { get; set; }
    }
}
