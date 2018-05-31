using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.Services.RequestServices
{
    /// <summary>
    /// 创建者:xb 创建时间: 2018/04
    /// </summary>
    public interface IRequest
    {
        Task<bool> MakeRequest();
    }
}
