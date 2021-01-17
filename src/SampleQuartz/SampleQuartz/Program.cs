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
        private static IScheduler scheduler;
        private static HolidayCalendar calandar;

        static void Main(string[] args)
        {
            Task.Run(()=> {
                MainAsync().Wait();
                var canlendar = scheduler.GetCalendar("holidayCalendar").GetAwaiter().GetResult() as HolidayCalendar;

               
                canlendar.AddExcludedDate(DateTime.Now.AddDays(1));
               
                // canlendar.DeleteCalendar("holidayCalendar");
                scheduler.AddCalendar("holidayCalendar", canlendar, true, true).Wait();
                Console.WriteLine("holidayCalendar");
                Task.Delay(TimeSpan.FromSeconds(10)).Wait();
                var canlendar2 = scheduler.GetCalendar("holidayCalendar").GetAwaiter().GetResult() as HolidayCalendar;
                canlendar2.RemoveExcludedDate(DateTime.Today);
                    scheduler.AddCalendar("holidayCalendar", canlendar2, true, true).Wait();
            
                var jobDetail =  scheduler.GetJobDetail(new JobKey("SayHelloJob-Tom", "DemoGroup")).GetAwaiter().GetResult();
             var trigger=   scheduler.GetTriggersOfJob(new JobKey("SayHelloJob-Tom", "DemoGroup")).GetAwaiter().GetResult();
                if (jobDetail != null)
                {
                    //jobDetail.PersistJobDataAfterExecution
                    var runSuccess = jobDetail.JobDataMap.Get("RunSuccess");
                    Console.WriteLine($"{jobDetail.Key} run success: {runSuccess}");
                }

                //DailyCalendar dailyCalendar = new DailyCalendar(DateBuilder.DateOf(22, 56, 0).DateTime,
                //                                         DateBuilder.DateOf(23, 0, 0).DateTime);
                Console.WriteLine("22holidayCalendar");
            });
          //  Task.Delay(TimeSpan.FromSeconds(3)).Wait();
            //   var calandar = new HolidayCalendar();
          
            //    scheduler.DeleteCalendar("holidayCalendar");
            //     scheduler.GetCalendarNames
            //  calandar.AddExcludedDate()
            //    scheduler.AddCalendar("holidayCalendar", calandar, false, false).Wait();

            Task.Delay(TimeSpan.FromSeconds(100)).Wait();
            //等待输入，阻塞应用程序退出
            Console.ReadKey();
        }

        static async Task MainAsync()
        {
            var schedulerFactory = new StdSchedulerFactory();
            scheduler = await schedulerFactory.GetScheduler();
            await scheduler.Start();
            Console.WriteLine($"任务调度器已启动");

            //添加Listener
          scheduler.ListenerManager.AddJobListener(new MyJobListener(), GroupMatcher<JobKey>.AnyGroup());
           //scheduler.ListenerManager.AddTriggerListener(new MyTriggerListener(), GroupMatcher<TriggerKey>.AnyGroup());

            //await ScheduleHelloQuartzJob(scheduler);
            await ScheduleSayHelloJob(scheduler);

            await Task.Delay(TimeSpan.FromSeconds(5));
         
            //获取JobDetail
            var jobDetail = await scheduler.GetJobDetail(new JobKey("SayHelloJob-Tom", "DemoGroup"));
            if (jobDetail != null)
            {
                //jobDetail.PersistJobDataAfterExecution
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
            //HourlyCalendar cal = new MinuteCalendar();
            //cal.setMinuteExcluded(47);
            //cal.setMinuteExcluded(48);
            //cal.setMinuteExcluded(49);
            //cal.setMinuteExcluded(50);
            calandar = new HolidayCalendar();
            DailyCalendar dailyCalendar = new DailyCalendar(DateBuilder.DateOf(22, 56, 0).DateTime,
                                                            DateBuilder.DateOf(23, 0, 0).DateTime);
       //   calandar.AddExcludedDate(DateTime.Today);

            await scheduler.AddCalendar("holidayCalendar", calandar, false, false);

            var trigger = TriggerBuilder.Create()
                                        .WithCronSchedule("*/1 * * * * ?")
                                        .WithIdentity("test")
                                         .ModifiedByCalendar("holidayCalendar")
                                        .Build();

            //添加调度
            await scheduler.ScheduleJob(jobDetail, trigger);
        }
    }
}
