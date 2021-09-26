using Microsoft.AspNetCore.Identity;
using SPOJ.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPOJ.ViewModels
{
    public class TaskPlagiatViewModel
    {
        public int FirstAnswerId { get; set; }
        public int SecondAnswerId { get; set; }
        public string FirstName1 { get; set; }
        public string LastName1 { get; set; }
        public string Version1 { get; set; }
        public string FirstName2 { get; set; }
        public string LastName2 { get; set; }
        public string Version2 { get; set; }
        public string language { get; set; }
        public string Plagiatresult { get; set; }
        public string Plagiat_sameParts { get; set; }
    }
}
