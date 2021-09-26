using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentScheduler;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SPOJ.Models;


namespace SPOJ
{
    public class Program
    {
        public static async System.Threading.Tasks.Task Main(string[] args)
        {
            string[] lines = System.IO.File.ReadAllLines(@"init.txt");
            string DB_Server="";
            string DB_User="";
            string DB_Password="";
            string DB_Name="";
            string VM_IP="";
            string VM_Port="";
            string VM_Name="";
            string VM_Password="";
            foreach(string line in lines)
            {
                string[] el = new string[2];
                el=line.Split(' ');
                if(el[0]=="DB_Server:")
                {
                    DB_Server=el[1];
                }
                else if(el[0]=="DB_User:")
                {
                    DB_User=el[1];
                }
                else if(el[0]=="DB_Password:")
                {
                    DB_Password=el[1];
                }
                else if(el[0]=="DB_Name:")
                {
                    DB_Name=el[1];
                }
                else if(el[0]=="VM_IP:")
                {
                    VM_IP=el[1];
                }
                else if(el[0]=="VM_Port:")
                {
                    VM_Port=el[1];
                }
                else if(el[0]=="VM_Name:")
                {
                    VM_Name=el[1];
                }
                else if(el[0]=="VM_Password:")
                {
                    VM_Password=el[1];
                }
            }
            string connectString="server="+DB_Server+";UserId="+DB_User+";Password="+DB_Password+";database="+DB_Name+";";
            
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var userManager = services.GetRequiredService<UserManager<User>>();
                    var rolesManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    await RoleInitializer.InitializeAsync(userManager, rolesManager);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
            var contextOptions = new DbContextOptionsBuilder<SpojContext>().UseMySql(connectString).Options;
            using var context = new SpojContext(contextOptions);
            using var context2 = new SpojContext(contextOptions);

            // JobManager.Initialize(new TestJob());
            JobManager.Initialize(new CheckTasksDaemon(context,VM_IP, VM_Port, VM_Name, VM_Password));
            JobManager.Initialize(new AntyplagiatDaemon(context2));
            host.Run();
            




        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
