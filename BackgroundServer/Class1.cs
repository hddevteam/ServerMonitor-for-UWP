using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace BackgroundServer
{
    public sealed class Class1 : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral; // Note: defined at class scope so we can mark it complete inside the OnCancel() callback if we choose to support cancellation
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            //
            // TODO: Insert code to start one or more asynchronous methods using the
            //       await keyword, for example:
            //
            // await ExampleMethodAsync();
            //
            _deferral.Complete();
        }

        #region 查看一个进程外的后台服务是否注册
        /*
         var taskRegistered = false;
         var exampleTaskName = "ExampleBackgroundTask";

        foreach (var task in BackgroundTaskRegistration.AllTasks)
        {
            if (task.Value.Name == exampleTaskName)
            {
                taskRegistered = true;
                break;
            }
        }
         */
        #endregion
    }
}
