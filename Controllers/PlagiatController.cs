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
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SPOJ.Controllers
{

    public class PlagiatController : Controller
    {
        SpojContext db;
        UserManager<User> _userManager;
        SignInManager<User> _signInManager;
        IWebHostEnvironment _appEnvironment;


        public PlagiatController(SpojContext context, IWebHostEnvironment appEnvironment, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            db = context;
            _appEnvironment = appEnvironment;
            _userManager = userManager;
            _signInManager = signInManager;

        }

        [HttpGet]
        public IActionResult TaskPlagiatList(string TaskId)
        {
            if (User.IsInRole("teacher"))
            {
            // string language = "";
            IEnumerable<string> sortBy = new List<string>
            {
                "Language",
                "Result"
            };
            ViewBag.sortBy = new SelectList(sortBy);
            ViewBag.taskid = TaskId;
            // bool flaga1 = false;
            var tasks = db.Tasks.Where(c => c.TaskId == Convert.ToInt32(TaskId)).Include(c => c.UserGroupTasks_Copy).ThenInclude(sc => sc.Group).Include(c => c.UserGroupTasks_Copy).ThenInclude(sc => sc.User).Include(c => c.UserGroupTasks_Copy).ThenInclude(sc => sc.Answers).ToList();
            // var plagiatList = db.Antyplagiats.ToList();
            List<TaskPlagiatViewModel> viewModel = new List<TaskPlagiatViewModel>();
            List<string> gName1 = new List<string>();
            List<string> gName2 = new List<string>();
            // foreach (Models.Task task in tasks)
            // {
            //     foreach (UserGroupTask_Copy usrgrt in task.UserGroupTasks_Copy)
            //     {
            //         language = usrgrt.program_language;


            //         foreach (var ans in usrgrt.Answers)
            //         {

            //             var selPlag = db.Antyplagiats.Where(c => c.FirstAnswerId == ans.AnswerId || c.SecondAnswerId == ans.AnswerId).ToList();
            //             if (selPlag.Count > 0)
            //             {

            //                 foreach (var el in selPlag)
            //                 {
            //                     flaga1 = false;
            //                     foreach (var vm in viewModel)
            //                     {
            //                         if (vm.FirstAnswerId == el.FirstAnswerId)
            //                         {
            //                             flaga1 = true;
            //                         }
            //                     }
            //                     if (!flaga1)
            //                     {
            //                         var a1=db.Answer.Include(c=>c.UserGroupTask_Copy).ThenInclude(c=>c.Group).FirstOrDefault(c=>c.AnswerId==el.FirstAnswerId);
            //                         var a2=db.Answer.Include(c=>c.UserGroupTask_Copy).ThenInclude(c=>c.Group).FirstOrDefault(c=>c.AnswerId==el.SecondAnswerId);
            //                         gName1.Add(a1.UserGroupTask_Copy.Group.GroupName);
            //                         gName2.Add(a2.UserGroupTask_Copy.Group.GroupName);
            //                         var tpvm = new TaskPlagiatViewModel { language = language, FirstAnswerId = el.FirstAnswerId, SecondAnswerId = el.SecondAnswerId, FirstName1 = el.FirstName1, FirstName2 = el.FirstName2, LastName1 = el.LastName1, LastName2 = el.LastName2, Version1 = el.Version1, Version2 = el.Version2, Plagiatresult = el.Plagiatresult, Plagiat_sameParts = el.Plagiat_sameParts };

            //                         viewModel.Add(tpvm);
            //                     }

            //                 }
            //             }
            //         }


            //     }
            // }
            
                    
                    var antyplagiats = db.Antyplagiats.Where(c=>c.taskId==Convert.ToInt32(TaskId)).ToList();
                    
                    
                    foreach(var plagiat in antyplagiats)
                    {
                        var a1=db.Answer.Include(c=>c.UserGroupTask_Copy).ThenInclude(c=>c.Group).FirstOrDefault(c=>c.AnswerId==plagiat.FirstAnswerId);
                        var a2=db.Answer.Include(c=>c.UserGroupTask_Copy).ThenInclude(c=>c.Group).FirstOrDefault(c=>c.AnswerId==plagiat.SecondAnswerId);
                        gName1.Add(a1.UserGroupTask_Copy.Group.GroupName);
                        gName2.Add(a2.UserGroupTask_Copy.Group.GroupName);
                        var tpvm = new TaskPlagiatViewModel { language = plagiat.language, FirstAnswerId = plagiat.FirstAnswerId, SecondAnswerId = plagiat.SecondAnswerId, FirstName1 = plagiat.FirstName1, FirstName2 = plagiat.FirstName2, LastName1 = plagiat.LastName1, LastName2 = plagiat.LastName2, Version1 = plagiat.Version1, Version2 = plagiat.Version2, Plagiatresult = plagiat.Plagiatresult, Plagiat_sameParts = plagiat.Plagiat_sameParts };

                        viewModel.Add(tpvm);
                    }
            ViewBag.gName1=gName1;
            ViewBag.gName2=gName2;

            return View(viewModel);
            }
            return RedirectToAction("AuthErr", "Account");
        }
        [HttpPost]
        public IActionResult TaskPlagiatList(string taskId, string sort, string SortBy, string direction)
        {
            IEnumerable<string> sortBy = new List<string>
            {
                "Language",
                "Result"
            };
            ViewBag.sortBy = new SelectList(sortBy);
            ViewBag.taskid = taskId;
            // bool flaga1;
            List<Answer> answers= new List<Answer>();
            List<Antyplagiat> antyplagiats= new List<Antyplagiat>();
            List<TaskPlagiatViewModel> viewModel = new List<TaskPlagiatViewModel>();
            List<string> gName1 = new List<string>();
            List<string> gName2 = new List<string>();
            if (sort != "true")
            {
                return RedirectToAction("TaskPlagiatList", new { TaskId = taskId });
            }
            else
            {
                if (SortBy == "Language")
                {
                    if(direction=="Ascending")
                    {
                        var sorPlag = db.Antyplagiats.Where(c=>c.taskId==Convert.ToInt32(taskId)).OrderBy(c=>c.language).ToList();
                        antyplagiats=sorPlag;
                    }
                    else
                    {
                        var sorPlag = db.Antyplagiats.Where(c=>c.taskId==Convert.ToInt32(taskId)).OrderByDescending(c=>c.language).ToList();
                        antyplagiats=sorPlag;
                    }
                    foreach(var plagiat in antyplagiats)
                    {
                        var a1=db.Answer.Include(c=>c.UserGroupTask_Copy).ThenInclude(c=>c.Group).FirstOrDefault(c=>c.AnswerId==plagiat.FirstAnswerId);
                        var a2=db.Answer.Include(c=>c.UserGroupTask_Copy).ThenInclude(c=>c.Group).FirstOrDefault(c=>c.AnswerId==plagiat.SecondAnswerId);
                        gName1.Add(a1.UserGroupTask_Copy.Group.GroupName);
                        gName2.Add(a2.UserGroupTask_Copy.Group.GroupName);
                        var tpvm = new TaskPlagiatViewModel { language = plagiat.language, FirstAnswerId = plagiat.FirstAnswerId, SecondAnswerId = plagiat.SecondAnswerId, FirstName1 = plagiat.FirstName1, FirstName2 = plagiat.FirstName2, LastName1 = plagiat.LastName1, LastName2 = plagiat.LastName2, Version1 = plagiat.Version1, Version2 = plagiat.Version2, Plagiatresult = plagiat.Plagiatresult, Plagiat_sameParts = plagiat.Plagiat_sameParts };

                        viewModel.Add(tpvm);
                    }
                }
                //ONLY CORRECT CODE
                else if(SortBy == "Result")
                {
                    if(direction=="Ascending")
                    {
                        var sorPlag = db.Antyplagiats.Where(c=>c.taskId==Convert.ToInt32(taskId)).OrderBy(c=>c.procent).ToList();
                        antyplagiats=sorPlag;
                    }
                    else
                    {
                        var sorPlag = db.Antyplagiats.Where(c=>c.taskId==Convert.ToInt32(taskId)).OrderByDescending(c=>c.procent).ToList();
                        antyplagiats=sorPlag;
                    }
                    foreach(var plagiat in antyplagiats)
                    {
                        var a1=db.Answer.Include(c=>c.UserGroupTask_Copy).ThenInclude(c=>c.Group).FirstOrDefault(c=>c.AnswerId==plagiat.FirstAnswerId);
                        var a2=db.Answer.Include(c=>c.UserGroupTask_Copy).ThenInclude(c=>c.Group).FirstOrDefault(c=>c.AnswerId==plagiat.SecondAnswerId);
                        gName1.Add(a1.UserGroupTask_Copy.Group.GroupName);
                        gName2.Add(a2.UserGroupTask_Copy.Group.GroupName);
                        var tpvm = new TaskPlagiatViewModel { language = plagiat.language, FirstAnswerId = plagiat.FirstAnswerId, SecondAnswerId = plagiat.SecondAnswerId, FirstName1 = plagiat.FirstName1, FirstName2 = plagiat.FirstName2, LastName1 = plagiat.LastName1, LastName2 = plagiat.LastName2, Version1 = plagiat.Version1, Version2 = plagiat.Version2, Plagiatresult = plagiat.Plagiatresult, Plagiat_sameParts = plagiat.Plagiat_sameParts };

                        viewModel.Add(tpvm);
                    }
                }

            }





            ViewBag.gName1=gName1;
            ViewBag.gName2=gName2;
            return View(viewModel);
        }
        public async Task<IActionResult> PlagiatDetailsAsync(string firstAID, string secondAID, string sameParts)
        {
            if (User.IsInRole("teacher"))
            {
                Answer firstA = await db.Answer.FirstOrDefaultAsync(c=>c.AnswerId==Convert.ToInt32(firstAID));
                Answer secondA = await db.Answer.FirstOrDefaultAsync(c=>c.AnswerId==Convert.ToInt32(secondAID));

                string firstToken=firstA.PlagiatToken;
                string secondToken=secondA.PlagiatToken;
                string firstAContent=firstA.Content;
                string secondAContent=secondA.Content;

                var samePartsArray=sameParts.Split(" = ");
                string part1=samePartsArray[0];
                string part2=samePartsArray[1];

                var part1_Array = part1.Split();
                var part2_Array = part2.Split();

                ViewBag.firstToken=firstToken;
                ViewBag.secondToken=secondToken;
                ViewBag.part1_Array=part1_Array;
                ViewBag.part2_Array=part2_Array;
                ViewBag.firstAContent=firstAContent;
                ViewBag.secondAContent=secondAContent;
                
                
                

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
