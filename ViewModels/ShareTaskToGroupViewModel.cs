using Microsoft.AspNetCore.Identity;
using SPOJ.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPOJ.ViewModels
{
    public class ShareTaskToGroupViewModel
    {
        public Models.Task task { get; set; }
        public IList<Group> allGroups { get; set; }
        public List<string> curGroups { get; set; }
        public ShareTaskToGroupViewModel()
        {
            allGroups = new List<Group>();
            curGroups = new List<string>();
        }
    }
}
