using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApi.Application.Auth.Dtos
{
    public record LoginRequest(string Username, string Password);
}
