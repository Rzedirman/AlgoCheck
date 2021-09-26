using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPOJ.Models
{
    public class Group
    {
        public int GroupId { get; set; }
        public string creatorID { get; set; }
        public string GroupName { get; set; }
        public string Creator { get; set; }
       

        public List<UserGroup> UserGroups { get; set; }
        public List<UserGroupTask> UserGroupTasks { get; set; }
        public List<UserGroupTask_Copy> UserGroupTasks_Copy { get; set; }
        public Group()
        {
            UserGroups = new List<UserGroup>();
            UserGroupTasks = new List<UserGroupTask>();
            UserGroupTasks_Copy = new List<UserGroupTask_Copy>();
        }
    }
}
