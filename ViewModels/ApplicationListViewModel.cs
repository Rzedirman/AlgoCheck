using Microsoft.AspNetCore.Identity;
using SPOJ.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPOJ.ViewModels
{
    public class ApplicationListViewModel
    {
        public Group group { get; set; }
        public List<string> appUsersID { get; set; }
        public List<User> appStudents { get; set; }
        public ApplicationListViewModel()
        {
            appUsersID = new List<string>();
            appStudents = new List<User>();
        }
    }
}
