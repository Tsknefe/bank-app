using BankApi.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApi.Domain.Entities
{
    public class Account
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string Iban { get; set; } = null!;
        public decimal Balance { get; set; }
        public AccountType AccountType { get; set; }
        public bool IsActive { get; set; } = true;
        public uint xmin { get; set; }
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

    }
}
