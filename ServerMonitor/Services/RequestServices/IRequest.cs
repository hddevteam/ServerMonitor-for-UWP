using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.Services.RequestServices
{
    public interface IRequest
    {
        Task<bool> MakeRequest();
    }
}
