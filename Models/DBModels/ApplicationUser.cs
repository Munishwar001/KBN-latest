using Microsoft.AspNetCore.Identity;

namespace KBN.Models.DBModels
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;

    }
}
