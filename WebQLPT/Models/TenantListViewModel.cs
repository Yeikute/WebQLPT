using System.Collections.Generic;
using WebQLPT.Models;

namespace WebQLPT.Models
{
    public class TenantListViewModel
    {
        public List<ApplicationUser> RegisteredUsers { get; set; } = new List<ApplicationUser>();
        public List<KhachThue> ManualTenants { get; set; } = new List<KhachThue>();
        public string CurrentUserId { get; set; }
        // map userId -> whether admin/chutro can assign room to this user
        public Dictionary<string, bool> RegisteredUserCanAssign { get; set; } = new Dictionary<string, bool>();
    }
}
