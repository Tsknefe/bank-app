using BankApi.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApi.Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }= Guid.NewGuid();
        public Guid AccountId { get; set; }
        public Account Account { get; set; } = null!;
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public Guid? RelatedAccountId { get; set; }
        public DateTime CreatedAt { get; set; }= DateTime.UtcNow;

    }
}
