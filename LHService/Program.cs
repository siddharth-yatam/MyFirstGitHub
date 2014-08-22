using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace LandingHTML
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new GenerateHTML() 
            };
            ServiceBase.Run(ServicesToRun);

//#if (!DEBUG)
//            System.ServiceProcess.ServiceBase[] ServicesToRun;
//            ServicesToRun = new System.ServiceProcess.ServiceBase[] { new Service1() };
//            System.ServiceProcess.ServiceBase.Run(ServicesToRun);
//#else
//            //Debug code: this allows the process to run as a non-service.
//            //It will kick off the service start point, but never kill it.
//            //Shut down the debugger to exit
//            GenerateHTML service = new GenerateHTML();
//            service.timerAction(null);

//            //Put a breakpoint on the following line to always catch
//            //your service when it has finished its work
//            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
//#endif

        }
    }
}
