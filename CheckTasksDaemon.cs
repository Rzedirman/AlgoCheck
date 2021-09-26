using FluentScheduler;
using System;

using System.Diagnostics;
using System.IO;
using System.Linq;

using System.Text;

using Microsoft.EntityFrameworkCore;
using Renci.SshNet;
using SPOJ.Models;


namespace SPOJ
{
    public class CheckTasksDaemon : Registry
    {
        SpojContext db;
        

        public CheckTasksDaemon(SpojContext context,string VM_IP,string VM_Port,string VM_Name,string VM_Password)
        {
            db = context;
            
            
            myJob(VM_IP, VM_Port, VM_Name, VM_Password);
        }
        private void myJob(string VM_IP,string VM_Port,string VM_Name,string VM_Password)
        {
            Schedule(() =>
            {
                
                
                
                Console.WriteLine("Daemon start");

                int er = 0;
                Process cmd_Csharp = new Process();

                cmd_Csharp.StartInfo.FileName = "mcs";
                cmd_Csharp.StartInfo.Arguments = @"-out:compiled_tasks/answer.exe compiled_tasks/answer.cs";
                cmd_Csharp.StartInfo.RedirectStandardOutput = true;
                cmd_Csharp.StartInfo.CreateNoWindow = false;
                cmd_Csharp.StartInfo.UseShellExecute = false;

                Process cmd_Java = new Process();
                cmd_Java.StartInfo.FileName = "javac";
                cmd_Java.StartInfo.Arguments = @"-d compiled_tasks compiled_tasks/Main.java";
                cmd_Java.StartInfo.RedirectStandardOutput = true;
                cmd_Java.StartInfo.CreateNoWindow = false;
                cmd_Java.StartInfo.UseShellExecute = false;
                
                //looking for task for testing 
                var allUsers = db.Users.Include(c => c.UserGroupTasks_Copy).ThenInclude(sc => sc.Group).Include(c => c.UserGroupTasks_Copy).ThenInclude(sc => sc.Task).Include(c => c.UserGroupTasks_Copy).ThenInclude(sc => sc.Answers).ToList();
                User user= new User();
                string answer = "";
                string language="";
                string testResultString = "";
                string testString = "";
                bool flaga = false;
                UserGroupTask_Copy usrGrTask = new UserGroupTask_Copy();
                Answer ans = new Answer();
                foreach (User usr in allUsers)
                {
                    
                    foreach (UserGroupTask_Copy usrgrt in usr.UserGroupTasks_Copy)
                    {
                        foreach (Answer answ in usrgrt.Answers)
                        {
                            if (answ.Checkd=="False")
                            {
                                user = usr;
                                ans = answ;
                                usrGrTask = usrgrt;
                                answer = answ.Content;
                                testString = usrgrt.Task.Tests;
                                testResultString = usrgrt.Task.testResults;
                                flaga = true;
                                language=answ.program_language;
                                break;
                            }
                        }
                        if (flaga) break;
                    }
                    if (flaga) break;
                    
                }

                if (flaga)
                {
                    try
                    {

                    
                    Console.WriteLine("Choosed task {0} and user {1}", usrGrTask.Task.taskName, user.UserName);

                    testString = testString.Replace("\r\n","\n").Replace("\r","\n") ;
                    testResultString = testResultString.Replace("\r\n","\n").Replace("\r","\n");

                    System.IO.Directory.CreateDirectory("compiled_tasks");



                    if(language=="C#")
                    {
                        System.IO.File.WriteAllText(@"compiled_tasks/answer.cs", answer);
                    }
                    else if(language=="JAVA")
                    {
                        System.IO.File.WriteAllText(@"compiled_tasks/Main.java", answer);
                    }



                    cmd_Csharp.Start();
                    
                    cmd_Csharp.WaitForExit();
                    // 7 linij do ERRORS
                    string s = cmd_Csharp.StandardOutput.ReadToEnd();
                    var cOutput = s.Split(Environment.NewLine).ToList();
                    Console.WriteLine(s);
                    StringBuilder errors = new StringBuilder();
                    foreach (string line in cOutput)
                    {
                        if (line.Contains("error"))
                        {
                            errors.Append(line + Environment.NewLine);
                        }
                    }

                    if (errors.Length == 0)
                    {
                        
                        Console.WriteLine("No compilation errors was found");
                        var tests = testString.Split(Environment.NewLine + "=====" + Environment.NewLine).ToList();
                        var testResults = testResultString.Split(Environment.NewLine + "=====" + Environment.NewLine).ToList();
                        int[] weights = new int[tests.Count - 1];
                        double sumWeights = 0;
                        double score = 0;
                        double procScore = 0;
                        double ocena = 0;


                        using (var client = new SftpClient(VM_IP, Convert.ToInt32(VM_Port), VM_Name, VM_Password))
                        {
                            StringBuilder sb = new StringBuilder();
                            string temps = "";
                            FileInfo f=null;

                            if(language=="C#")
                            {
                                f = new FileInfo(@"compiled_tasks/answer.exe");
                            }
                            else if(language=="JAVA")
                            {
                                f = new FileInfo(@"compiled_tasks/Main.class");
                            }
                            

                            string uploadfile = f.FullName;
                            Console.WriteLine(f.Name);
                            Console.WriteLine("uploadfile" + uploadfile);

                            try
                            {
                                client.Connect();
                            }
                            catch
                            {
                                Console.WriteLine("An error occur while connecting to VM");
                                System.IO.Directory.Delete("compiled_tasks", true);
                                ans.Checkd = "False";
                                db.SaveChanges();
                                er++;

                            }
                            if(er==0)
                            {
                                try
                                {
                                    var fileStream = new FileStream(uploadfile, FileMode.Open);
                                    Console.WriteLine("Sending files to VM");

                                    client.BufferSize = 4 * 1024;
                                    client.UploadFile(fileStream, "Desktop/" + f.Name, null);
                                    fileStream.Close();
                                    for (int i = 0; i < tests.Count - 1; ++i)//upload all tests
                                    {
                                        sb.Clear();
                                        var lines = tests[i].Split(Environment.NewLine).ToList();
                                        weights[i] = Convert.ToInt32(lines[0]);
                                        sumWeights += Convert.ToInt32(lines[0]);
                                        for (int j = 1; j < lines.Count; ++j)
                                        {
                                            if (j != lines.Count - 1)
                                                sb.Append(lines[j] + Environment.NewLine);
                                            else sb.Append(lines[j]);
                                        }
                                        temps = sb.ToString();

                                        System.IO.File.WriteAllText(@"compiled_tasks/test" + i + ".in", temps);
                                        f = new FileInfo(@"compiled_tasks/test" + i + ".in");
                                        uploadfile = f.FullName;
                                        fileStream = new FileStream(uploadfile, FileMode.Open);
                                        client.UploadFile(fileStream, "Desktop/" + f.Name, null);
                                        fileStream.Close();

                                    }
                                    client.Disconnect();
                                }
                                catch
                                {
                                    Console.WriteLine("An error occur while sending files to VM");
                                    er++;
                                    System.IO.Directory.Delete("compiled_tasks", true);
                                    client.Disconnect();
                                    ans.Checkd = "False";
                                    db.SaveChanges();
                           
                                }
                            }
                        }


                        using (var client = new SshClient(VM_IP, Convert.ToInt32(VM_Port), VM_Name, VM_Password))
                        {
                            if (er == 0)
                            {
                                try
                                {
                                    client.Connect();
                                }
                                catch
                                {
                                    Console.WriteLine("An error occur while connecting to VM");
                                    er++;
                                    System.IO.Directory.Delete("compiled_tasks", true);
                                    ans.Checkd = "False";
                                    db.SaveChanges();

                                }
                            }

                            if (er == 0)
                            {
                                try
                                {
                                    Console.WriteLine("Testing");
                                    for (int i = 0; i < tests.Count - 1; ++i)//make all tests
                                    {
                                        SshCommand command=null;
                                        if(language=="C#")
                                        {
                                            command = client.CreateCommand("mono Desktop/answer.exe < Desktop/test" + i + ".in");
                                        }
                                        else if(language=="JAVA")
                                        {
                                            command = client.CreateCommand("java -cp Desktop Main < Desktop/test" + i + ".in");
                                        }

                                        
                                        command.CommandTimeout=TimeSpan.FromSeconds(1);
                                        var req = command.Execute();
                                        if (req == testResults[i] + Environment.NewLine)
                                        {
                                            Console.WriteLine("EQUAL " + i);
                                            score += weights[i];
                                        }
                                        else
                                        {
                                            Console.WriteLine("NOT EQUAL " + i);
                                        }
                                        Console.WriteLine(req);
                                    }
                                    procScore = score / sumWeights * 100;
                                    if (procScore <= 50) ocena = 2;
                                    else if (procScore > 50 && procScore <= 60) ocena = 3;
                                    else if (procScore > 60 && procScore <= 70) ocena = 3.5;
                                    else if (procScore > 70 && procScore <= 80) ocena = 4;
                                    else if (procScore > 80 && procScore <= 90) ocena = 4.5;
                                    else ocena = 5;

                                    ans.Ocena = Convert.ToString(ocena);
                                    db.SaveChanges();


                                    //очистка виртуальной машины
                                    var command2 = client.CreateCommand("sudo rm Desktop/answer.exe");
                                    var req2 = command2.Execute();
                                    for (int i = 0; i < tests.Count - 1; ++i)
                                    {
                                        var command = client.CreateCommand("sudo rm Desktop/test" + i + ".in");
                                        var req = command.Execute();
                                    }

                                    client.Disconnect();
                                }
                                catch
                                {
                                    Console.WriteLine("An error occur while testing");
                                    er++;

                                    //очистка виртуальной машины
                                    var command2 = client.CreateCommand("rm Desktop/answer.exe");
                                    var req2 = command2.Execute();
                                    for (int i = 0; i < tests.Count - 1; ++i)
                                    {
                                        var command = client.CreateCommand("rm Desktop/test" + i + ".in");
                                        var req = command.Execute();
                                    }

                                    client.Disconnect();
                                    System.IO.Directory.Delete("compiled_tasks", true);
                                    ans.Checkd = "True";
                                    ans.Ocena = "2 - Cant check your answer";
                                    db.SaveChanges();
                                    
                                }
                            }
                        }
                    }
                    else
                    {//Ошибки компиляции
                        Console.WriteLine("Some compilation errors was found");
                        Console.WriteLine(errors.ToString());
                        ans.Errors = errors.ToString();
                        ans.Checkd = "True";
                        ans.Ocena = "2 - Compile error";
                        db.SaveChanges();
                    }
                    }
                    catch
                    {
                        
                    }
                    try
                    {
                        System.IO.Directory.Delete("compiled_tasks", true);
                    }
                    catch
                    {

                    }
                    try
                    {
                        using (var client = new SshClient(VM_IP, Convert.ToInt32(VM_Port), VM_Name, VM_Password))
                        {
                            var tests = testString.Split(Environment.NewLine + "=====" + Environment.NewLine).ToList();
                            client.Connect();
                            var command2 = client.CreateCommand("rm Desktop/answer.exe");
                            var req2 = command2.Execute();
                            for (int i = 0; i < tests.Count - 1; ++i)
                            {
                                var command = client.CreateCommand("rm Desktop/test" + i + ".in");
                                var req = command.Execute();
                            }

                            client.Disconnect();
                        }
                    }
                    catch
                    {

                    }
                    if (er == 0)
                    {
                        ans.Checkd = "True";
                        db.SaveChanges();
                    }
    
                }

            }).ToRunNow().AndEvery(5).Seconds();
        }
    }
}
