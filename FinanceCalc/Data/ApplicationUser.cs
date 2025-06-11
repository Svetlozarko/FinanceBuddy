using Microsoft.AspNetCore.Identity;

namespace FinanceCalc.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public int Age { get; set; }
        public string Country { get; set; }
    }

}
