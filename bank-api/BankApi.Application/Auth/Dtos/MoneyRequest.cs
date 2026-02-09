using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApi.Application.Auth.Dtos
{
    public class MoneyRequest
    {
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}
