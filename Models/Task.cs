using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPOJ.Models
{
    public class Task
    {
        public int TaskId { get; set; }
        public string teacherID { get; set; }
        public string teacher_login { get; set; }
        public string teacher_firstName { get; set; }
        public string teacher_lastName { get; set; }
        public string taskName { get; set; }
        public string task_content { get; set; }
        
        public string Tests { get; set; }
        public string testResults { get; set; }

        // public List<UserTask> UserTasks { get; set; }
        public List<UserGroupTask> UserGroupTasks { get; set; }
        public List<UserGroupTask_Copy> UserGroupTasks_Copy { get; set; }
        public Task()
        {
            // UserTasks = new List<UserTask>();
            UserGroupTasks = new List<UserGroupTask>();
            UserGroupTasks_Copy = new List<UserGroupTask_Copy>();
        }
    }
}
