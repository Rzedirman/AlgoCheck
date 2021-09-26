using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPOJ
{
    public class TestJob : Registry
    {
        public TestJob()
        {
            myJob();
        }
        private void myJob()
        {
            Schedule(() =>
            {
                Console.WriteLine("EXECUTED");
            }).ToRunNow().AndEvery(1).Minutes();
        }
    }
}
