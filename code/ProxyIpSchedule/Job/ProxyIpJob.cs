using ProxyIpSchedule.Helper;
using Quartz;
using System;
using System.Threading.Tasks;

namespace ProxyIpSchedule.Job
{

    public class ProxyIpJob : IJob
    {
        private static bool isRun = false;
        public Task Execute(IJobExecutionContext context)
        {
            if (!isRun)
            {
                return Task.Run(() =>
                {
                    isRun = true;
                    Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 定时任务开始执行");
                    ProxyIpHelper proxyhelper = new ProxyIpHelper();
                    proxyhelper.start();
                    Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 定时任务执行完毕");
                    isRun = false;
                });
            }
            else
            {
                return Task.Run(() =>
                {
                });
            }

        }
    }
}
