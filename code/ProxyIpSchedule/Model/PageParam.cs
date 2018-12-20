using System;
using System.Collections.Generic;
using System.Text;

namespace ProxyIpSchedule.Model
{
    public class PageParam
    {
        public int pageIndex { get; set; }
        public int pageSize { get; set; }

        public PageParam(int pageIndex, int pageSize)
        {
            this.pageIndex = pageIndex;
            this.pageSize = pageSize;
        }
    }
}
