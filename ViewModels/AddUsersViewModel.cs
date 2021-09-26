using Microsoft.AspNetCore.Identity;
using SPOJ.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPOJ.ViewModels
{
    public class AddUsersViewModel
    {
        public Models.Task task { get; set; }
        public IList<User> AllStudents { get; set; }
        public List<string> curUsers { get; set; }
        public AddUsersViewModel()
        {
            AllStudents = new List<User>();
            curUsers = new List<string>();
        }
    }
}
