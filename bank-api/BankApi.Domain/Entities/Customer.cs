namespace BankApi.Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string IdentityNumber { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public List<CreditCard> CreditCards { get; set; } = new();

    }
}
