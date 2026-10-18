using Microsoft.AspNetCore.Identity;

namespace Qadaya.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;

        public ICollection<CommitteeMember> CommitteeMemberships { get; set; } = new List<CommitteeMember>();
    }
}
