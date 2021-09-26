using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SPOJ.Models;
using SPOJ.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Renci.SshNet;
using System.IO;
using System.Security.Claims;

namespace SPOJ.Controllers
{
    
    public class GroupController : Controller
    {
        SpojContext db;
        UserManager<User> _userManager;
        SignInManager<User> _signInManager;
        IWebHostEnvironment _appEnvironment;
        

        public GroupController(SpojContext context, IWebHostEnvironment appEnvironment, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            db = context;
            _appEnvironment = appEnvironment;
            _userManager = userManager;
            _signInManager = signInManager;
            
        }

        public IActionResult GroupList()
        {
            if (User.IsInRole("teacher"))
            {
                IQueryable<Group> groups = db.Groups;
                string usrName = User.Identity.Name;
                groups = groups.Where(p => p.Creator == usrName);

                return View(groups);
            }
            else if (User.IsInRole("student"))
            {
                string usrName = User.Identity.Name;
                var allUsers = db.Users.Include(c => c.UserGroups).ThenInclude(sc => sc.Group).ToList();


                List<Group> userGroupList = new List<Group>();
                foreach (User usr in allUsers)
                {
                    if(usr.UserName== usrName)
                    {
                        foreach (UserGroup usrg in usr.UserGroups)
                        {
                            if(usrg.Status=="Accepted")
                                userGroupList.Add(usrg.Group);
                        }
                        
                    }
                    
                }
                
                return View(userGroupList);
            }
            return RedirectToAction("AuthErr", "Account");

        }
        


        [HttpGet]
        public IActionResult CreateNewGroup()
        {
            if (User.IsInRole("teacher"))
            {
                return View();
            }
            else return RedirectToAction("AuthErr", "Account");
        }

        [HttpPost]
        public IActionResult CreateNewGroup(string _groupName)
        {
            Group group = new Group
            {
                Creator = User.Identity.Name,
                GroupName = _groupName,
                creatorID = User.FindFirstValue(ClaimTypes.NameIdentifier)
                
            };
            db.Groups.Add(group);
            db.SaveChanges();



            return RedirectToAction("GroupList");
        }

        public IActionResult AllGroupList()
        {
            if (User.IsInRole("student"))
            {
                IQueryable<Group> groups = db.Groups.OrderBy(el=>el.GroupName).ThenBy(el=>el.Creator);
                return View(groups);
            }
            else return RedirectToAction("AuthErr", "Account");
        }

        public async Task<IActionResult> ApplyAsync(int GroupId)
        {
            if (User.IsInRole("student"))
            {
                var group = db.Groups.Find(GroupId);
                var groups = db.Groups.Include(c => c.UserGroups).Where(c=>c.GroupId==GroupId);
                var gr=groups.FirstOrDefault();
                string usrName = User.Identity.Name;
                User user = await _userManager.FindByNameAsync(usrName);
                
                bool flaga=false;
                foreach(var ug in gr.UserGroups)
                {
                    if(ug.GroupId==GroupId&&ug.UserId == user.Id)
                     {
                        flaga=true;
                    }
                }
                if(!flaga)
                {
                    group.UserGroups.Add(new UserGroup { GroupId = GroupId, UserId = user.Id,Status="Wait" });
                    db.SaveChanges();
                }
                return RedirectToAction("GroupList");
            }
            else return RedirectToAction("AuthErr", "Account");
        }

        [HttpGet]
        public IActionResult ApplicationList(int GroupId)
        {
            if (User.IsInRole("teacher"))
            {
               var curGroup = db.Groups.Find(GroupId);
                var allGroups = db.Groups.Include(c => c.UserGroups).ThenInclude(sc => sc.User).ToList();

                List<string> appUsersID = new List<string>();
                List<User> appStudents = new List<User>();
                
                foreach (var group in allGroups)
                {
                    if(group.GroupId== GroupId)
                    {
                        foreach (var usgr in group.UserGroups)
                        {
                            if (usgr.Status=="Wait")
                            {
                                appUsersID.Add(usgr.UserId);
                                appStudents.Add(usgr.User);
                            }
                        }
                        break;
                    }
                }
                ApplicationListViewModel model = new ApplicationListViewModel
                    {
                        group = curGroup,
                        appUsersID = appUsersID,
                        appStudents=appStudents
                    };



                return View(model);
            }
            else return RedirectToAction("AuthErr", "Account");
        }

        [HttpPost]
        public IActionResult ApplicationList(int GroupId,List<string> students)
        {
            var group = db.Groups.Find(GroupId);
            var curGroupUser = db.Groups.Include(c => c.UserGroups).ThenInclude(sc => sc.User).FirstOrDefault(c => c.GroupId == GroupId);
            int count = curGroupUser.UserGroups.Count;
            
            //foreach(var usrgr in curGroupUser.UserGroups)
            for(int i=0;i<count;++i)
            {
                var usrgr=curGroupUser.UserGroups[i];
                if(students.Contains(usrgr.UserId))
                {
                    usrgr.Status="Accepted";
                }
                else if(usrgr.Status=="Wait")
                {
                    var userGroup = curGroupUser.UserGroups.FirstOrDefault(s => s.UserId == usrgr.UserId);
                    curGroupUser.UserGroups.Remove(userGroup);
                    count--;
                }
                if(curGroupUser.UserGroups.Count==0)
                    break;
            }
            db.SaveChanges();
            

            
            return RedirectToAction("GroupList");
            
        }

        public IActionResult InGroupStudentsList(int GroupId)
        {
            if (User.IsInRole("teacher"))
            {
                var curGroupUser = db.Groups.Include(c => c.UserGroups).ThenInclude(sc => sc.User).FirstOrDefault(c => c.GroupId == GroupId);
                List<User> groupStudents = new List<User>();
                
                foreach(var usrgr in curGroupUser.UserGroups)
                {
                    if(usrgr.Status=="Accepted")
                        groupStudents.Add(usrgr.User);
                }
                ViewBag.GroupId=GroupId;
                ViewBag.GroupName=curGroupUser.GroupName;


                return View(groupStudents);
            }
            else return RedirectToAction("AuthErr", "Account");
        }

        public IActionResult DeleteUserFromGroup(string userId, int GroupId)
        {
            if (User.IsInRole("teacher"))
            {
                var curGroupUser = db.Groups.Include(c => c.UserGroups).FirstOrDefault(c => c.GroupId == GroupId);
                
                var userGroup = curGroupUser.UserGroups.FirstOrDefault(s => s.UserId == userId);
                curGroupUser.UserGroups.Remove(userGroup);
                
                db.SaveChanges();
                return RedirectToAction("InGroupStudentsList",new {GroupId=GroupId});
            }
            else return RedirectToAction("AuthErr", "Account");
        }



        public IActionResult Privacy()
        {

            //if (User.Identity.IsAuthenticated)
            if (User.IsInRole("admin"))
            {
                return View();
            }
            return RedirectToAction("AuthErr", "Account");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
