using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApi.Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string IdentıtyNumber { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }

        public ICollection<Account> Accounts { get; set; } = new List<Account>();
    }
}
