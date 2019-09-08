using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Calendar;
using Quartz.Impl.Matchers;
using SampleQuartz.Jobs;
using SampleQuartz.Listeners;

namespace SampleQuartz
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();

            //等待输入，阻塞应用程序退出
            Console.ReadKey();
        }

        static async Task MainAsync()
        {
            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = await schedulerFactory.GetScheduler();
            await scheduler.Start();
            Console.WriteLine($"任务调度器已启动");

            //添加Listener
            scheduler.ListenerManager.AddJobListener(new MyJobListener(), GroupMatcher<JobKey>.AnyGroup());
            scheduler.ListenerManager.AddTriggerListener(new MyTriggerListener(), GroupMatcher<TriggerKey>.AnyGroup());

            //await ScheduleHelloQuartzJob(scheduler);
            await ScheduleSayHelloJob(scheduler);

            await Task.Delay(TimeSpan.FromSeconds(5));

            //获取JobDetail
            var jobDetail = await scheduler.GetJobDetail(new JobKey("SayHelloJob-Tom", "DemoGroup"));
            if (jobDetail != null)
            {
                var runSuccess = jobDetail.JobDataMap.Get("RunSuccess");
                Console.WriteLine($"{jobDetail.Key} run success: {runSuccess}");
            }
            else
            {
                Console.WriteLine($"获取JobDetail失败");
            }
        }

        private static async Task ScheduleHelloQuartzJob(IScheduler scheduler)
        {
            //创建作业和触发器
            var jobDetail = JobBuilder.Create<HelloQuartzJob>().Build();
            var trigger = TriggerBuilder.Create()
                                        .WithSimpleSchedule(m =>
                                        {
                                            m.WithRepeatCount(3).WithIntervalInSeconds(1);
                                        })
                                        .Build();

            //添加调度
            await scheduler.ScheduleJob(jobDetail, trigger);
        }

        private static async Task ScheduleSayHelloJob(IScheduler scheduler)
        {
            //创建作业和触发器
            var jobDetail = JobBuilder.Create<SayHelloJob>()
                                        .SetJobData(new JobDataMap() {
                                            new KeyValuePair<string, object>("UserName", "Tom"),
                                            new KeyValuePair<string, object>("RunSuccess", false)
                                        })
                                        .WithIdentity("SayHelloJob-Tom", "DemoGroup")
                                        .RequestRecovery(true)
                                        .StoreDurably(true)
                                        .Build();

            var calandar = new HolidayCalendar();
            calandar.AddExcludedDate(DateTime.Today);

            await scheduler.AddCalendar("holidayCalendar", calandar, false, false);

            var trigger = TriggerBuilder.Create()
                                        .WithCronSchedule("*/1 * * * * ?")
                                        //.ModifiedByCalendar("holidayCalendar")
                                        .Build();

            //添加调度
            await scheduler.ScheduleJob(jobDetail, trigger);
        }
    }
}
