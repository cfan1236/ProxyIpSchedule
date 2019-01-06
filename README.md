# ProxyIpSchedule

代理IP自动获取任务


----------
### 开发
基于dotnet core开发的代理IP自动获取定时任务。

**抓取**

> 网上免费代理IP网站提供给的IP数据，像爬虫一样抓取指定页面并解析得到IP、端口等关键数据。.Net中用网页解析推荐使用 *HtmlAgilityPack* 它可以用xpath 来解析html。有些网站好像也提供数据接口直接获取IP数据或直接返回纯文本数据，这样就不需要解析网页数据了。


 **校验**

> 获取到IP数据后并不是都可以使用的，免费网站提供的数据80%其实都是无用的，这个时候我们需要对IP数据进行校验看IP是否有效。竟然要校验那么我们的程序在发送http请求时就要将代理IP添加进去，.Net中用HttpWebRequest的Proxy属性。
 *HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
         request.Proxy = new WebProxy(122.10.101.1, 8080);*
         
>*注意*
>1. 校验是不能单纯的看服务器是否返回200,有时候代理服务会返回一段无效的账号说明等。比如我们使用代理IP去访问百度，返回结果的内容并不是百度网页，而是代理服务器返回的无效提示。所以校验时一定判断返回内容是否时正确的。
>1. 发送校验请求时一定加一个1-2s的超时设置，有些代理IP很慢，请求一个需要好几秒，这个时候我们可以直接判断为无效的。
>1. 如果我们使用的高匿代理IP，发送校验请求时最好访问ip138、ip.cn等这些查询IP的网站，发送请求后查看返回IP是不是我们设置的IP。

**保存**

> 将校验成功的IP数据暂时存放到List中，稍后将数据保存到本地文本文件或数据库都行。因为免费代理IP并不是可以永久使用的，所以需要定时更新数据，如果不需要将IP数据进行管理等其他操作，建议保存到文件中。IO操作速度块，文本文件插入可以及时覆盖。

定时任务.Net中推以使用*quartz.net* 可以通过cron表达灵活的设置执行时间。


### 注意
* 定时任务切勿太频繁,因为这些免费网站也有爬虫限制，访问过多也会被限制访问的。

* 如果dotnet core程序部署在linux上对于文件路径的书写一定要注意大小写问题，linux对大小写是非常敏感的。

* 如果代理IP用于爬虫项目，那么最好使用高匿的代理IP,这样爬取数据的服务器是无法知道你原本IP并且无法知道你是使用过代理IP。


### 项目目录

```
ProxyIpSchedule
│   │
│   Configs
│   │─ AppSettings.json   //app配置文件 quartz.net执行时间配置于此
│   │
│   Helper
│   │─ NetHttpHelper.cs   //网络请求处理类。
│   │─ ProxyIpHelper.cs   //抓取任务主要处理代码。
│   │
│   Job
│   │─ ProxyIpJob.cs   //quartz job任务 
│   │
│   Model
│   │─ PageParam.cs   //抓取网页分页参数信息
│   │─ Proxy.cs   //抓取到代理信息 ip、port等
│   │
│   Nlog.config  //Nlog组件配置文件
│   │  
│   Program.cs   //程序入口 
│   │  

```


