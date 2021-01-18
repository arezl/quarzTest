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
                var canlendar = scheduler.GetCalendar("holidayCalendar").GetAwaiter().GetResult() as DailyCalendar;
                // canlendar.AddExcludedDate(DateTime.Today);
                Task.Delay(TimeSpan.FromSeconds(10));

                canlendar.SetTimeRange("07:59", "08:00");
                canlendar.InvertTimeRange = true;
                //    
                scheduler.AddCalendar("holidayCalendar", canlendar, true, true);
                // retrieve the trigger
                ITrigger oldTrigger = scheduler.GetTrigger(new TriggerKey("test")).GetAwaiter().GetResult();

                // obtain a builder that would produce the trigger
                TriggerBuilder tb = oldTrigger.GetTriggerBuilder();

                // update the schedule associated with the builder, and build the new trigger// (other builder methods could be called, to change the trigger in any desired way)
                ITrigger newTrigger = tb.ModifiedByCalendar("holidayCalendar")
                                        .WithCronSchedule("*/1 * * * * ?")
                                        .WithIdentity("test")
                .Build();

                scheduler.RescheduleJob(new TriggerKey("test"), newTrigger);
                var test= scheduler.GetTrigger(new TriggerKey("test")).GetAwaiter().GetResult();
               // DateBuilder.DateOf(23, 0, 0).DateTime))
                Console.WriteLine("11111111holidayCalendar");
            });
          //  Task.Delay(TimeSpan.FromSeconds(3)).Wait();
            //   var calandar = new HolidayCalendar();
          
            //    scheduler.DeleteCalendar("holidayCalendar");
            //     scheduler.GetCalendarNames
            //  calandar.AddExcludedDate()
            //    scheduler.AddCalendar("holidayCalendar", calandar, false, false).Wait();

            Task.Delay(TimeSpan.FromSeconds(1000)).Wait();
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
            //scheduler.ListenerManager.AddJobListener(new MyJobListener(), GroupMatcher<JobKey>.AnyGroup());
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
                                        .StoreDurably(true)
                                        .Build();
            //HourlyCalendar cal = new MinuteCalendar();
            //cal.setMinuteExcluded(47);
            //cal.setMinuteExcluded(48);
            //cal.setMinuteExcluded(49);
            //cal.setMinuteExcluded(50);
            calandar = new HolidayCalendar();
            DailyCalendar dailyCalendar = new DailyCalendar("23:18", "23:20");
            dailyCalendar.InvertTimeRange = true;
            
            // calandar.AddExcludedDate(DateTime.Today);

            await scheduler.AddCalendar("holidayCalendar", dailyCalendar, false, false);

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
