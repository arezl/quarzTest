using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SampleQuartz.Jobs
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution] 
    public class SayHelloJob : IJob
    {
        /// <summary>
        /// Job参数，用来打招呼的用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 是否执行成功
        /// </summary>
        public bool RunSuccess { get; set; }

        /// <summary>
        /// IJob接口
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                Console.WriteLine($"Prev Run Success:{RunSuccess}");
                Console.WriteLine($"Hello {UserName}!");
                Console.WriteLine($"Hello {DateTime.Now}!");
                context.JobDetail.JobDataMap.Put("RunSuccess", true);
            });
        }
    }
}
