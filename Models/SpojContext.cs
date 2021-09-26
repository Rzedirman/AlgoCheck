using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SPOJ.Models
{
    public class SpojContext : IdentityDbContext<User>
    {
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Answer> Answer { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Antyplagiat> Antyplagiats { get; set; }
        public DbSet<UserGroupTask_Copy> UserGroupTask_Copy { get; set; }
        // public DbSet<Answer> Answers { get; set; }
        public SpojContext(DbContextOptions<SpojContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
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
            optionsBuilder.UseMySql(connectString);
        }
        //public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        //     modelBuilder.Entity<UserTask>()
        //         .HasKey(t => new { t.UserId, t.TaskId });

        //     //modelBuilder.Entity<User>()
        //     //    .HasKey(t => new { t.Id });

        //     modelBuilder.Entity<UserTask>()
        //         .HasOne(sc => sc.User)
        //         .WithMany(s => s.UserTasks)
        //         .HasForeignKey(sc => sc.UserId);

        //     modelBuilder.Entity<UserTask>()
        //         .HasOne(sc => sc.Task)
        //         .WithMany(c => c.UserTasks)
        //         .HasForeignKey(sc => sc.TaskId);




            modelBuilder.Entity<UserGroup>()
                .HasKey(t => new { t.UserId, t.GroupId });

            modelBuilder.Entity<UserGroup>()
                .HasOne(sc => sc.User)
                .WithMany(s => s.UserGroups)
                .HasForeignKey(sc => sc.UserId);

            modelBuilder.Entity<UserGroup>()
                .HasOne(sc => sc.Group)
                .WithMany(c => c.UserGroups)
                .HasForeignKey(sc => sc.GroupId);




            modelBuilder.Entity<UserGroupTask>()
                .HasKey(t => new { t.UserId, t.GroupId,t.TaskId, t.program_language });

            modelBuilder.Entity<UserGroupTask>()
                .HasOne(sc => sc.User)
                .WithMany(s => s.UserGroupTasks)
                .HasForeignKey(sc => sc.UserId);

            modelBuilder.Entity<UserGroupTask>()
                .HasOne(sc => sc.Group)
                .WithMany(c => c.UserGroupTasks)
                .HasForeignKey(sc => sc.GroupId);

            modelBuilder.Entity<UserGroupTask>()
                .HasOne(sc => sc.Task)
                .WithMany(s => s.UserGroupTasks)
                .HasForeignKey(sc => sc.TaskId);



            modelBuilder.Entity<UserGroupTask_Copy>()
                .HasKey(t => new { t.UserId, t.GroupId,t.TaskId, t.program_language });

            modelBuilder.Entity<UserGroupTask_Copy>()
                .HasOne(sc => sc.User)
                .WithMany(s => s.UserGroupTasks_Copy)
                .HasForeignKey(sc => sc.UserId);

            modelBuilder.Entity<UserGroupTask_Copy>()
                .HasOne(sc => sc.Group)
                .WithMany(c => c.UserGroupTasks_Copy)
                .HasForeignKey(sc => sc.GroupId);

            modelBuilder.Entity<UserGroupTask_Copy>()
                .HasOne(sc => sc.Task)
                .WithMany(s => s.UserGroupTasks_Copy)
                .HasForeignKey(sc => sc.TaskId);




            modelBuilder.Entity<Antyplagiat>()
                .HasKey(t => new { t.FirstAnswerId,t.SecondAnswerId });

            

                

            base.OnModelCreating(modelBuilder);
        }

    }
    // public class UserTask
    // {
    //     public string UserId { get; set; }
    //     public User User { get; set; }

    //     public int TaskId { get; set; }
    //     public Task Task { get; set; }
    //     public string Answer { get; set; }
    //     public string Checkd { get; set; }
    //     public string Ocena { get; set; }
    //     public string Errors { get; set; }
    // }

    public class UserGroup
    {
        public string UserId { get; set; }
        public User User { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }
        public string Status { get; set; }
        
    }

    public class UserGroupTask
    {
        public string UserId { get; set; }
        public User User { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }

        public int TaskId { get; set; }
        public Task Task { get; set; }
        
        public string program_language { get; set; }
        
    }

    public class UserGroupTask_Copy
    {
        public string UserId { get; set; }
        public User User { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }

        public int TaskId { get; set; }
        public Task Task { get; set; }
        
        public string program_language { get; set; }
        public List<Answer> Answers { get; set; } = new List<Answer>();
    }

    public class Answer
    {
        public int AnswerId { get; set; }
         public string Content { get; set; }
        public string Checkd { get; set; }
        public string Ocena { get; set; }
        public string Errors { get; set; }
        public string Last { get; set; }
        public string Version { get; set; }


        public string PlagiatCheckd { get; set; }
        public string PlagiatToken { get; set; }
        //public string Plagiat_sameParts { get; set; }


        public string UserId { get; set; }
        
        public int GroupId { get; set; }
        
        public int TaskId { get; set; }
        
        public string program_language { get; set; }
        public UserGroupTask_Copy UserGroupTask_Copy { get; set; }
    }

    public class Antyplagiat
    {
        public int FirstAnswerId { get; set; }
        public string FirstName1 { get; set; }
        public string LastName1 { get; set; }
        public string Version1 { get; set; }
        public int SecondAnswerId { get; set; }
        public string FirstName2 { get; set; }
        public string LastName2 { get; set; }
        public string Version2 { get; set; }
        public string Plagiatresult { get; set; }
        public double procent { get; set; }
        public int taskId{ get; set; }
        public string language { get; set; }
        public string Plagiat_sameParts { get; set; }
        
    }

}
