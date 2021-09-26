using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions; 
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Renci.SshNet;
using SPOJ.Models;
using SPOJ.ViewModels;

namespace SPOJ.Controllers
{
    public class TaskController : Controller
    {
        SpojContext db;
        UserManager<User> _userManager;
        SignInManager<User> _signInManager;
        IWebHostEnvironment _appEnvironment;


        public TaskController(SpojContext context, IWebHostEnvironment appEnvironment, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            db = context;
            _appEnvironment = appEnvironment;
            _userManager = userManager;
            _signInManager = signInManager;

        }

        public IActionResult Task_list()
        {
            if (User.IsInRole("teacher"))
            {
                IQueryable<Models.Task> tasks = db.Tasks;
                string usrName = User.Identity.Name;
                tasks = tasks.Where(p => p.teacher_login == usrName);

                return View(tasks);
            }
            else if (User.IsInRole("student"))
            {
                string usrName = User.Identity.Name;
                //var allUsers = db.Users.Include(c => c.UserTasks).ThenInclude(sc => sc.Task).ToList();
                var allUsers = db.Users.Include(c => c.UserGroupTasks).ThenInclude(s => s.Group).Include(c => c.UserGroupTasks).ThenInclude(s => s.Task).Include(c => c.UserGroupTasks_Copy).ThenInclude(s => s.Group).Include(c => c.UserGroupTasks_Copy).ThenInclude(s => s.Task).Include(c => c.UserGroupTasks_Copy).ThenInclude(s => s.Answers).ToList();

                List<Models.Task> userTaskList = new List<Models.Task>();
                List<UserGroupTask> userGroupTaskList = new List<UserGroupTask>();
                List<string> oceny = new List<string>();

                foreach (User usr in allUsers)
                {
                    if(usr.UserName== usrName)
                    {
                        foreach (UserGroupTask usrt in usr.UserGroupTasks)
                        {
                            foreach(UserGroupTask_Copy usrt_copy in usr.UserGroupTasks_Copy)
                            {
                                if(usrt_copy.GroupId==usrt.GroupId&&usrt_copy.TaskId==usrt.TaskId&&usrt_copy.program_language==usrt.program_language&&usrt_copy.UserId==usrt.UserId)
                                {
                                    var an=usrt_copy.Answers.LastOrDefault();
                                    if(an!=null) oceny.Add(an.Ocena);
                                    else oceny.Add("nothing");
                                    break;
                                }
                            }
                            
                            userTaskList.Add(usrt.Task);
                            userGroupTaskList.Add(usrt);

                        }
                        
                    }
                    
                }
                ViewBag.bag=userGroupTaskList;
                ViewBag.oceny=oceny;
                
                return View(userTaskList);
            }
            else return RedirectToAction("AuthErr", "Account");
        }

        [HttpGet]
        public IActionResult AddAnswersToTask(string taskId, string groupId,string language)
        {
            if (User.Identity.IsAuthenticated)
            {
                var tasks = db.Tasks.Include(c => c.UserGroupTasks_Copy).ThenInclude(sc => sc.User).Include(c => c.UserGroupTasks_Copy).ThenInclude(sc => sc.Group).ToList();
                Models.Task task = null;
                foreach (var ts in tasks)
                {
                    if (ts.TaskId == Convert.ToInt32(taskId))
                    {
                        task = ts;
                        break;
                    }

                }
                if (task != null)
                {
                    ViewBag.task = task;
                    ViewBag.groupId = groupId;
                    ViewBag.language = language;
                    return View();
                }

                return NotFound();
            }
            else return RedirectToAction("AuthErr", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> AddAnswersToTaskAsync(string taskId, string groupId,string language,string answer)
        {
            if (User.Identity.IsAuthenticated)
            {
                string usrName = User.Identity.Name;
                User user = await _userManager.FindByNameAsync(usrName);
                string userID = user.Id;
                // var tasks = db.Tasks.Include(c => c.UserTasks).ThenInclude(sc => sc.User).ToList();
                var tasks = db.Tasks.Include(c => c.UserGroupTasks_Copy).ThenInclude(sc => sc.User).Include(c => c.UserGroupTasks_Copy).ThenInclude(sc => sc.Group).Include(c => c.UserGroupTasks_Copy).ThenInclude(sc => sc.Answers).ToList();
                var tsk = db.Tasks.Where(c=>c.TaskId==Convert.ToInt32(taskId)).Include(c => c.UserGroupTasks_Copy).ThenInclude(sc => sc.User).Include(c => c.UserGroupTasks_Copy).ThenInclude(sc => sc.Group).Include(c => c.UserGroupTasks_Copy).ThenInclude(sc => sc.Answers).ToList();

                foreach (var ts in tsk)
                {
                    // if (ts.TaskId == Convert.ToInt32(taskId))
                    // {
                        foreach (var usrt in ts.UserGroupTasks_Copy)
                        {
                            if(usrt.UserId== userID && usrt.GroupId == Convert.ToInt32(groupId) && usrt.program_language== language)
                            {
                                string ver="1";
                                var an=usrt.Answers.LastOrDefault();
                                if(an!=null) 
                                {
                                    an.Last="0";
                                    ver=Convert.ToString(Convert.ToInt32(an.Version)+1);
                                }
                                
                                Answer answ=new Answer
                                { 
                                    Content=answer,
                                    Checkd ="False",
                                    Ocena ="...",
                                    Errors ="",
                                    Last="1",
                                    Version=ver,
                                    program_language=usrt.program_language
                                };
                                usrt.Answers.Add(answ);
                                break;
                            }
                        }
                    //     break;
                    // }

                }
                db.SaveChanges();
                return RedirectToAction("Task_list");
            }
            else return RedirectToAction("AuthErr", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> AddUsersToTaskAsync(string taskId)
        {
            if (User.Identity.IsAuthenticated)
            {
                string usrName = User.Identity.Name;
                User user = await _userManager.FindByNameAsync(usrName);
                string userID = user.Id;

                IEnumerable<string> languages = new List<string>
                {
                    "C#",
                    // "JAVA"
                };
                ViewBag.languages=new SelectList(languages);
                //var task = db.Tasks.Find(Convert.ToInt32(taskId));
                var tasks = db.Tasks.Include(c => c.UserGroupTasks).ThenInclude(sc => sc.Group).Include(c => c.UserGroupTasks).ThenInclude(sc => sc.User).ToList();
                Models.Task task=null;
                foreach (var ts in tasks)
                {
                    if (ts.TaskId == Convert.ToInt32(taskId))
                    {
                        task = ts;
                        break;
                    }

                }
                if (task != null)
                {
                    
                    // IList<User> allStudents = await _userManager.GetUsersInRoleAsync("student");
                    var allGroups = db.Groups.Where(c=>c.creatorID==userID).ToList();
                    // List<string> curUsers = new List<string>();
                    List<string> curGroups = new List<string>();
                    foreach (var group in allGroups)
                    {
                        foreach (UserGroupTask ut in task.UserGroupTasks)
                        {
                            if(ut.GroupId == group.GroupId)
                            {
                                curGroups.Add(Convert.ToString(group.GroupId));
                                break;
                            }

                        }
                    }
                    
                    ShareTaskToGroupViewModel model = new ShareTaskToGroupViewModel
                    {
                        task = task,
                        allGroups = allGroups,
                        curGroups = curGroups
                    };




                    return View(model);
                }

                return NotFound();
            }
            else return RedirectToAction("AuthErr", "Account");
        }

        [HttpPost]
        public IActionResult AddUsersToTask(string taskId, string language, List<string> groups)
        {
            if (User.Identity.IsAuthenticated)
            {
                var task = db.Tasks.Find(Convert.ToInt32(taskId));
                var delTask = db.Tasks.Include(c => c.UserGroupTasks).FirstOrDefault(c => c.TaskId == Convert.ToInt32(taskId));

                // var tasksUsers2 = db.Tasks.Include(c => c.UserTasks).ThenInclude(sc => sc.User).ToList();
                var tasksGroup = db.Tasks.Include(c => c.UserGroupTasks).ThenInclude(sc => sc.Group).ToList();
                

                List<string> curGroup = new List<string>();
                foreach (var ts in tasksGroup)
                {
                    if(ts.TaskId== Convert.ToInt32(taskId))
                    {
                        foreach (var gr in ts.UserGroupTasks)
                        {
                            curGroup.Add(Convert.ToString(gr.GroupId));
                        }
                    }
                    
                }
                var addedGroups = groups.Except(curGroup);
                var removedGroups = curGroup.Except(groups);

                
                foreach (string gr in addedGroups)
                {
                    
                    var groupUser = db.Groups.Where(c=>c.GroupId==Convert.ToInt32(gr)).Include(c => c.UserGroups).ThenInclude(sc => sc.User).ToList();
                    var group = groupUser.FirstOrDefault();
                    foreach (var usrg in group.UserGroups)
                    {
                    
                        group.UserGroupTasks.Add(new UserGroupTask { TaskId = Convert.ToInt32(taskId), GroupId = Convert.ToInt32(gr),program_language=language,UserId=usrg.UserId });
                        group.UserGroupTasks_Copy.Add(new UserGroupTask_Copy { TaskId = Convert.ToInt32(taskId), GroupId = Convert.ToInt32(gr),program_language=language,UserId=usrg.UserId });
                    }
                }
                
                db.SaveChanges();
                
                foreach (string gr in removedGroups)
                {
                    
                    var groupUserTasks = db.Groups.Where(c=>c.GroupId==Convert.ToInt32(gr)).Include(c => c.UserGroupTasks).ThenInclude(s => s.User).Include(c => c.UserGroupTasks).ThenInclude(s => s.Task);
                    var group = groupUserTasks.FirstOrDefault();

                    foreach (var usrg in group.UserGroupTasks)
                    {
                    
                        group.UserGroupTasks.Remove(usrg);
                        if(group.UserGroupTasks.Count==0)
                            break;
                    }
                }
                

                db.SaveChanges();
                return RedirectToAction("Task_list");

            }
            else return RedirectToAction("AuthErr", "Account");
        }

        [HttpGet]
        public IActionResult CreateTask()
        {
            if (User.IsInRole("teacher"))
            {
            return View();
            }
            else return RedirectToAction("AuthErr", "Account");
        }

        
        public IActionResult ShowAnswer(string answContent, string student, string language, string group, string taskName)
        {
            if (User.IsInRole("teacher"))
            {
                ViewBag.answContent=answContent;
                ViewBag.student=student;
                ViewBag.language=language;
                ViewBag.group=group;
                ViewBag.taskName=taskName;




                return View();
            }
            else return RedirectToAction("AuthErr", "Account");
        }

        [HttpGet]
        public IActionResult StudentMarks(string TaskId)
        {
            if (User.IsInRole("teacher"))
            {
                var UsrGrtTsks = db.UserGroupTask_Copy.Where(c=>c.TaskId==Convert.ToInt32(TaskId)).OrderBy(c=>c.program_language).Include(c=>c.Group).Include(c=>c.Task).Include(c=>c.Answers).Include(c=>c.User).ToList();
                List<string> students=new List<string>();
                List<string> languages=new List<string>();
                List<string> groups=new List<string>();
                List<string> marks=new List<string>();
                List<string> answs=new List<string>();
                string taskName="";
                foreach(var UsrGrtTsk in UsrGrtTsks)
                {
                    if(UsrGrtTsk.Answers.Count!=0)
                    {
                        if(UsrGrtTsk.Answers.LastOrDefault().Ocena!=null)
                        {
                        students.Add(UsrGrtTsk.User.firstName +" "+ UsrGrtTsk.User.lastName);
                        languages.Add(UsrGrtTsk.program_language);
                        groups.Add(UsrGrtTsk.Group.GroupName);
                        marks.Add(UsrGrtTsk.Answers.LastOrDefault().Ocena);
                        answs.Add(UsrGrtTsk.Answers.LastOrDefault().Content);
                        taskName = UsrGrtTsk.Task.taskName;
                        }
                    }
                    
                    
                }
                ViewBag.students=students;
                ViewBag.languages=languages;
                ViewBag.groups=groups;
                ViewBag.marks=marks;
                ViewBag.answs=answs;
                ViewBag.TaskId=TaskId;
                ViewBag.taskName=taskName;

                return View();
            }
            else return RedirectToAction("AuthErr", "Account");
        }



        [HttpPost]
        public IActionResult StudentMarks(string TaskId, string regex)
        {
            if (User.IsInRole("teacher"))
            {
                if(regex==""||regex==null)
                {
                    return RedirectToAction("StudentMarks", new { TaskId = TaskId });
                }
                var UsrGrtTsks = db.UserGroupTask_Copy.Where(c=>c.TaskId==Convert.ToInt32(TaskId)).OrderBy(c=>c.program_language).Include(c=>c.Group).Include(c=>c.Task).Include(c=>c.Answers).Include(c=>c.User).ToList();
                List<string> students=new List<string>();
                List<string> languages=new List<string>();
                List<string> groups=new List<string>();
                List<string> marks=new List<string>();
                List<string> answs=new List<string>();
                string taskName="";
                Regex re = new Regex(regex);
                foreach(var UsrGrtTsk in UsrGrtTsks)
                {
                    if(re.IsMatch(UsrGrtTsk.Group.GroupName)&&UsrGrtTsk.Answers.LastOrDefault().Ocena!=null)
                    {
                        students.Add(UsrGrtTsk.User.firstName +" "+ UsrGrtTsk.User.lastName);
                        languages.Add(UsrGrtTsk.program_language);
                        groups.Add(UsrGrtTsk.Group.GroupName);
                        marks.Add(UsrGrtTsk.Answers.LastOrDefault().Ocena);
                        answs.Add(UsrGrtTsk.Answers.LastOrDefault().Content);
                        taskName = UsrGrtTsk.Task.taskName;
                    }
                    
                }
                ViewBag.students=students;
                ViewBag.languages=languages;
                ViewBag.groups=groups;
                ViewBag.marks=marks;
                ViewBag.answs=answs;
                ViewBag.TaskId=TaskId;
                ViewBag.taskName=taskName;

                return View();
            }
            else return RedirectToAction("AuthErr", "Account");
        }



        [HttpPost]
        public async Task<IActionResult> CreateTaskAsync(string _taskName, string _task_content,List<string> weights, List<string> testContents, List<string> testResults)
        {
            string s = "";
            StringBuilder sb = new StringBuilder();
            string tr = "";
            StringBuilder sb2 = new StringBuilder();
            User user = await _userManager.FindByNameAsync(User.Identity.Name);
            string teacher_FN=user.firstName;
            string teacher_LN=user.lastName;

            for (int i = 0;i<weights.Count;++i)
            {
                sb.Append(weights[i]+Environment.NewLine);
                sb.Append(testContents[i] + Environment.NewLine);
                sb.Append("=====" + Environment.NewLine);
                sb2.Append(testResults[i] + Environment.NewLine);
                sb2.Append("=====" + Environment.NewLine);
            }
            s = sb.ToString();
            tr = sb2.ToString();

            Models.Task task = new Models.Task
            {
                teacher_login = User.Identity.Name,
                teacherID = User.FindFirstValue(ClaimTypes.NameIdentifier),
                taskName = _taskName,
                task_content = _task_content,
                Tests = s,
                testResults= tr,
                teacher_firstName = teacher_FN,
                teacher_lastName = teacher_LN
            };
            db.Tasks.Add(task);
            db.SaveChanges();
            


            return RedirectToAction("Task_list");
        }
    }
}
