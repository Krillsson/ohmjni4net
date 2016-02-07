using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenHardwareMonitor.Hardware;

namespace OHMWrapper
{
    public class OHMManagerFactory
    {
        internal static Computer _computer { get; private set; }
        private MonitorManager monitorManager;

        public OHMManagerFactory()
        {

        }

        public void init()
        {
            _computer = new Computer()
            {
                CPUEnabled = true,
                FanControllerEnabled = true,
                GPUEnabled = true,
                HDDEnabled = true,
                MainboardEnabled = true,
                RAMEnabled = true
            };

            _computer.Open();

            monitorManager = new MonitorManager(_computer);
            monitorManager.Update();
        }

        public void close()
        {
            _computer.Close();
        }

        public MonitorManager GetManager()
        {
            monitorManager.Update();
            return monitorManager;
        }
    }
}
