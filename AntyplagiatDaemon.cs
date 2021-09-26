using FluentScheduler;
using Microsoft.EntityFrameworkCore;
using SPOJ.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPOJ
{
    public class AntyplagiatDaemon : Registry
    {
        SpojContext db;
        
        public AntyplagiatDaemon(SpojContext context)
        {
            db = context;

            myJob();
        }

        public StringBuilder decide(string codePart, string[] inne, string[] calk)
        {
            StringBuilder tokenSB = new StringBuilder();
            if (calk.Contains(codePart))
            {
                tokenSB.Append("C");
            }
            else if (inne.Contains(codePart))
            {
                tokenSB.Append("X");
            }
            else if (codePart == "True" || codePart == "False")
            {
                tokenSB.Append("S");
            }
            else if (codePart == "float" || codePart == "double" || codePart == "decimal")
            {
                tokenSB.Append("P");
            }
            else if (codePart == "char")
            {
                tokenSB.Append("R");
            }
            else if (codePart == "string")
            {
                tokenSB.Append("I");
            }
            else if (codePart == "bool")
            {
                tokenSB.Append("B");
            }
            else if (codePart == "return")
            {
                tokenSB.Append("N");
            }
            else if (codePart == "if")
            {
                tokenSB.Append("A");
            }
            else if (codePart == "else")
            {
                tokenSB.Append("E");
            }
            else if (codePart == "for" || codePart == "while" || codePart == "do" || codePart == "foreach")
            {
                tokenSB.Append("F");
            }
            else if (codePart == "break")
            {
                tokenSB.Append("K");
            }
            else if (codePart == "continue")
            {
                tokenSB.Append("T");
            }
            else
            {
                tokenSB.Append("Z");
            }
            return tokenSB;
        }

        public StringBuilder Tokenize(string code)
        {
            string[] inne = new string[] {"Console", "WriteLine", "Convert", "ToString", "ToInt32", "ToInt64", "ToDouble", "StringBuilder", "new"
                                            , "ReadLine", "ReadKey", "Void","static","class","public","private"};
            string[] calk = new string[] { "int", "long", "short", "byte", "sbyte", "ushort", "ulong", "uint" };
            string allowedChar = "_qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM";
            string numbers = "1234567890";
            string operators = "+-=<>*/^&|";
            bool flagaKowyczki = false;
            bool flagaLiczba = false;



            string inputCode = code;
            StringBuilder sb = new StringBuilder();
            StringBuilder tokenSB = new StringBuilder();

            foreach (char c in inputCode)
            {
                if (!flagaKowyczki && (c == '\'' || c == '\"'))
                {
                    flagaKowyczki = true;
                    continue;
                }
                if (flagaKowyczki && (c == '\'' || c == '\"'))
                {
                    flagaKowyczki = false;
                    tokenSB.Append("S");
                    continue;
                }

                if (!flagaKowyczki && sb.Length == 0 && !flagaLiczba && numbers.Contains(c))
                {
                    flagaLiczba = true;
                    continue;
                }
                if (!flagaKowyczki && flagaLiczba && operators.Contains(c))
                {
                    flagaLiczba = false;
                    tokenSB.Append("S");
                    tokenSB.Append("O");
                    continue;
                }
                if (!flagaKowyczki && flagaLiczba && (c == ';' || c == ' ' || c == '[' || c == ']' || c == '{' || c == '}' || c == ')' || c == '(' || c == ','))
                {
                    flagaLiczba = false;
                    tokenSB.Append("S");
                    continue;
                }

                if (!flagaKowyczki && !flagaLiczba && (allowedChar.Contains(c) || numbers.Contains(c)))
                {
                    sb.Append(c);
                    continue;
                }
                if (!flagaKowyczki && !flagaLiczba && sb.Length != 0 && (c == ';' || c == ' ' || c == '[' || c == ']' || c == '{' || c == '}' || c == '.' || c == '}' || c == ')' || c == '(' || c == ','))
                {
                    string codePart = sb.ToString();
                    sb.Clear();
                    string token = decide(codePart, inne, calk).ToString();
                    tokenSB.Append(token);
                    continue;
                }
                if (!flagaKowyczki && !flagaLiczba && sb.Length != 0 && (operators.Contains(c)))
                {
                    string codePart = sb.ToString();
                    sb.Clear();
                    string token = decide(codePart, inne, calk).ToString();
                    tokenSB.Append(token);
                    tokenSB.Append("O");
                    continue;
                }
                //==================
                if (!flagaKowyczki && operators.Contains(c) && !flagaLiczba)
                {
                    tokenSB.Append("O");
                    continue;
                }


            }

            while (tokenSB.ToString().Contains("SOS") || tokenSB.ToString().Contains("CC") || tokenSB.ToString().Contains("PP") || tokenSB.ToString().Contains("RR") || tokenSB.ToString().Contains("BB") || tokenSB.ToString().Contains("II") || tokenSB.ToString().Contains("ZOCOZOXZOCO") || tokenSB.ToString().Contains("OO"))
            {
                tokenSB.Replace("SOS", "S");
                tokenSB.Replace("CC", "C");
                tokenSB.Replace("PP", "P");
                tokenSB.Replace("RR", "R");
                tokenSB.Replace("BB", "B");
                tokenSB.Replace("II", "I");
                tokenSB.Replace("OO", "O");
                tokenSB.Replace("ZOCOZOXZOCO", "XZOXX");
            }
            while(tokenSB.ToString().StartsWith("Z"))
            {
                tokenSB.Remove(0,1);
            }
            return tokenSB;
        }

        public string Levenstain(string token1,string token2, ref string sameParts, ref double procent)
        {
            int m = token1.Length;
            List<string> samePart1= new List<string>();
            int n = token2.Length;
            List<string> samePart2= new List<string>();

            int[,] d = new int[m+1, n+1];
            

            for(int i=0;i<=m;++i)
            {
                d[i, 0] = i;
            }
            

            for (int j = 1; j <=n; ++j)
            {
                d[0,j] = j;
            }
            

            int cost=-1;
            for (int i = 1; i <=m; ++i)
            {
                for (int j = 1; j <=n; ++j)
                {
                    if (token1[i-1] == token2[j-1])
                    {
                        cost = 0;
                    }
                    else cost = 1;
                    d[i,j] = Math.Min(d[i - 1, j] + 1, Math.Min(d[i, j - 1] + 1, d[i - 1, j - 1] + cost));

                }
                
            }
            int i2=m,j2=n;
            while(i2>=0 && j2>=0)
            {
                if(i2!=0)
                {
                    if(d[i2,j2]==d[i2-1,j2]&&d[i2,j2-1]==d[i2-1,j2-1])
                    {
                        j2-=1;
                    }
                    else
                    {
                        if(d[i2,j2]==d[i2-1,j2-1])
                        {
                            samePart1.Add(Convert.ToString(i2));
                            samePart2.Add(Convert.ToString(j2));
                        }
                        i2-=1;
                        j2-=1;
                    }
                }
                else break;
            }
            samePart1.Reverse();
            samePart2.Reverse();




            int c=0;
            foreach(string el in samePart1)
            {
                c++;
                sameParts = sameParts+el+" ";
                if(c==samePart1.Count) sameParts = sameParts+"= ";
            }
            c=0;
            foreach(string el in samePart2)
            {
                c++;
                if(c!=samePart2.Count)
                    sameParts = sameParts+el+" ";
                else sameParts = sameParts+el;
                
            }
            double max = Math.Max(m,n);
            double odl = d[m, n];
            procent =Math.Round(100 - (odl*100/max),2);

            return Convert.ToString(d[m, n])+" / "+procent+"%";
        }



        private void myJob()
        {
            Schedule(() =>
            {
               
            Answer ans = new Answer();
            try
            {
                Console.WriteLine("Antyplagiat starts");
                string result="",sameParts="";
                string token1="", token2="";
                int firstAnswerId=-1;
                int secondAnswerId=-1;
                double procent=-1;
              
                
                string language="";
                bool flaga=false;
                
                

                var allTasks = db.Tasks.Include(c => c.UserGroupTasks_Copy).ThenInclude(sc => sc.Group).Include(c => c.UserGroupTasks_Copy).ThenInclude(sc => sc.User).Include(c => c.UserGroupTasks_Copy).ThenInclude(sc => sc.Answers).ToList();
                UserGroupTask_Copy usrGrTask = new UserGroupTask_Copy();
                
                Models.Task tsk = new Models.Task();
                var antyplagiatTable=db.Antyplagiats.ToList();
                foreach (Models.Task task in allTasks)
                {
                    foreach (UserGroupTask_Copy usrgrt in task.UserGroupTasks_Copy)
                    {
                        foreach (Answer answ in usrgrt.Answers)
                        {
                            if (answ.PlagiatCheckd!="True"&&answ.Checkd=="True"&&answ.Errors=="")
                            {
                                if(answ.PlagiatToken==null||answ.PlagiatToken=="")
                                {
                                    token1=Tokenize(answ.Content).ToString();
                                    answ.PlagiatToken=token1;
                                    db.SaveChanges();
                                }
                                else token1=answ.PlagiatToken;
                                usrGrTask=usrgrt;
                                ans=answ;
                                tsk=task;
                                firstAnswerId=answ.AnswerId;
                                language=usrgrt.program_language;
                                flaga = true;
                                answ.PlagiatCheckd="True";
                                db.SaveChanges();
                                break;
                            }
                        }
                        if (flaga) break;
                    }
                    if (flaga) break;   
                }
                
                if (flaga)
                {
                    foreach (UserGroupTask_Copy usrgrt in tsk.UserGroupTasks_Copy)
                    {
                        if(usrgrt!=usrGrTask&&usrgrt.program_language==language)
                        {
                            foreach (Answer answ in usrgrt.Answers)
                            {
                                
                                if(answ.PlagiatToken==null||answ.PlagiatToken=="")
                                {
                                    token2=Tokenize(answ.Content).ToString();
                                    answ.PlagiatToken=token2;
                                    db.SaveChanges();
                                }
                                else token2=answ.PlagiatToken;
                                
                                if(answ.Checkd=="True"&&answ.Errors=="")
                                {
                                    var existedPlagiatCheck = db.Antyplagiats.Where(w=>((w.FirstAnswerId==ans.AnswerId&&w.SecondAnswerId==answ.AnswerId)||(w.SecondAnswerId==ans.AnswerId&&w.FirstAnswerId==answ.AnswerId))).ToList();
                                    if(existedPlagiatCheck.Count!=0)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        bool flaga2=false;
                                        string tempToken;
                                        int tempAnswerId;
                                        secondAnswerId=answ.AnswerId;
                                        if (token1.Length>token2.Length)
                                        {
                                            tempToken=token1;
                                            token1=token2;
                                            token2=tempToken;

                                            tempAnswerId=firstAnswerId;
                                            firstAnswerId=secondAnswerId;
                                            secondAnswerId=tempAnswerId;
                                            flaga2=true;
                                        }
                                        result=Levenstain(token1, token2, ref sameParts, ref procent);

                                        string FirstName1="";
                                        string LastName1="";
                                        string Version1="";
                                        string FirstName2="";
                                        string LastName2="";
                                        string Version2="";
                                        if(!flaga2)
                                        {
                                            FirstName1=usrGrTask.User.firstName;
                                            LastName1=usrGrTask.User.lastName;
                                            Version1=ans.Version;

                                            FirstName2=usrgrt.User.firstName;
                                            LastName2=usrgrt.User.lastName;
                                            Version2=answ.Version;
                                        }
                                        else
                                        {
                                            FirstName1=usrgrt.User.firstName;
                                            LastName1=usrgrt.User.lastName;
                                            Version1=answ.Version;

                                            FirstName2=usrGrTask.User.firstName;
                                            LastName2=usrGrTask.User.lastName;
                                            Version2=ans.Version;
                                        }



                                        Antyplagiat model =new Antyplagiat
                                        {
                                            FirstAnswerId=firstAnswerId,
                                            FirstName1=FirstName1,
                                            LastName1=LastName1,
                                            Version1=Version1,
                                            SecondAnswerId=secondAnswerId,
                                            FirstName2=FirstName2,
                                            LastName2=LastName2,
                                            Version2=Version2,
                                            Plagiatresult=result,
                                            Plagiat_sameParts=sameParts,
                                            procent=procent,
                                            taskId=tsk.TaskId,
                                            language=ans.program_language
                                        };
                                        db.Antyplagiats.Add(model);
                                        db.SaveChanges();
                                    }

                                }
                                    
                                
                            }
                            
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("An error ocure while antyplagiat testing");
                ans.PlagiatCheckd="False";
            }
            
            Console.WriteLine("End of plagiat check");

            }).ToRunNow().AndEvery(5).Seconds();
        }
    }
}
