using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SampleQuartz.Jobs
{
    public class HelloQuartzJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                Console.WriteLine("Hello Quartz.Net");
            });
        }
    }
}
