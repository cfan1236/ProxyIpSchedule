using ProxyIpSchedule.Helper;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProxyIpSchedule.Job
{
    public class ProxyIpJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 定时任务开始执行");
                ProxyIpHelper proxyhelper = new ProxyIpHelper();
                proxyhelper.start();
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 定时任务执行完毕");
            });

        }
    }
}
