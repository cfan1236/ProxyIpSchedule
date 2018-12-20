
using Microsoft.Extensions.Configuration;
using ProxyIpSchedule.Job;
using Quartz;
using Quartz.Impl;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ProxyIpSchedule
{
    class Program
    {
        private static IConfigurationRoot Configuration { get; set; }
        private static Task<IScheduler> scheduler = null;
        static void Main(string[] args)
        {
            #region 配置文件
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            //注意文件及路径的大小写 linux上很敏感
            .AddJsonFile("Configs/AppSettings.json");
            Configuration = builder.Build();
            #endregion

            //创建定时任务
            var cor = Configuration["ProxyJobExpression"];
            scheduler = StdSchedulerFactory.GetDefaultScheduler();
            CreateJob<ProxyIpJob>("proxyHelper", cor);
            scheduler.Result.Start();
            //开启时输出控制台, nlog日志组件只用于业务层面。
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 调度管理器启动成功...");
            Console.ReadKey();

        }

        /// <summary>
        /// 创建Job
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uid"></param>
        /// <param name="cronExpression"></param>
        private static void CreateJob<T>(string uid, string cronExpression) where T : IJob
        {
            var job = JobBuilder.Create<T>()
                .WithIdentity("job" + uid, "group" + uid)
                .Build();
            var cronTrigger = (ICronTrigger)TriggerBuilder.Create()
                                                .WithIdentity("trigger" + uid, "group" + uid)
                                                .StartNow()
                                                .WithCronSchedule(cronExpression)
                                                .Build();
            var ft = scheduler.Result.ScheduleJob(job, cronTrigger);
        }
    }
}
