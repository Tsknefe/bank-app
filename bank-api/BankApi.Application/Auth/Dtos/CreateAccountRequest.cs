using BankApi.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApi.Application.Auth.Dtos
{
    public class CreateAccountRequest
    {
        public Guid CustomerId { get; set; }
        public string Name { get; set; }
        public string Iban { get; set; }
        public AccountType AccountType { get; set; }

    }
}
