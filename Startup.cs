using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SPOJ.Models;

namespace SPOJ
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
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
            //string connection = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<SpojContext>(options => options.UseMySql(connectString));


            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<SpojContext>();
            


            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");
            });
        }
    }
}
