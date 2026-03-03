using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BankApi.Application.Auth.Dtos
{
   public record RegisterRequest(string Username, string Password);
}
