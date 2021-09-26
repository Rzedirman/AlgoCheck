using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPOJ.Models
{
    public class User : IdentityUser
    {
        public int nr_albumu { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        // public List<UserTask> UserTasks { get; set; }
        public List<UserGroup> UserGroups { get; set; }
        public List<UserGroupTask> UserGroupTasks { get; set; }
        public List<UserGroupTask_Copy> UserGroupTasks_Copy { get; set; }
        public User()
        {
            // UserTasks = new List<UserTask>();
            UserGroups = new List<UserGroup>();
            UserGroupTasks = new List<UserGroupTask>();
            UserGroupTasks_Copy = new List<UserGroupTask_Copy>();
        }
    }
 
}
