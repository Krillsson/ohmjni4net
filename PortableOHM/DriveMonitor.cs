using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenHardwareMonitor.Hardware;

namespace OHMWrapper
{
    public class DriveMonitor : OHMMonitor
    {
        public string LogicalName { get; private set; }
        public OHMSensor Temperature { get; private set; }
        public OHMSensor RemainingLife { get; private set; }
        public OHMSensor[] LifecycleData { get; private set; }

        public DriveMonitor(IHardware hardware) : base(hardware)
        {
            List<OHMSensor> sensorList = new List<OHMSensor>();

            InitTemperature(sensorList);
            InitRemainingLife(sensorList);
            InitLifecycleData(sensorList);
            InitName();

            Sensors = sensorList.ToArray();
        }

        void InitTemperature(List<OHMSensor> sensors)
        {
            ISensor tempSensor = _hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature);

            if (tempSensor != null)
            {
                Temperature = new OHMSensor(tempSensor, DataType.Celcius, "Temp");
                sensors.Add(Temperature);
            }
        }

        void InitRemainingLife(List<OHMSensor> sensors)
        {
            ISensor sensor = _hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Level && s.Name == "Remaining Life");

            if (sensor != null)
            {
                Temperature = new OHMSensor(sensor, DataType.Percent, "Remaining life");
                sensors.Add(Temperature);
            }
        }

        void InitLifecycleData(List<OHMSensor> sensors)
        {
            List<OHMSensor> dataSensors = _hardware.Sensors.Where(s => s.SensorType == SensorType.Data).Select(d => new OHMSensor(d, DataType.Gigabyte, d.Name)).ToList();
            if (dataSensors != null && dataSensors.Count > 0)
            {
                LifecycleData = dataSensors.ToArray();
                sensors.AddRange(LifecycleData);
            }
        }

        void InitName()
        {
            System.IO.DriveInfo[] info = PropertyHelper.GetPrivateFieldValue<System.IO.DriveInfo[]>(_hardware,
                "driveInfos");
                LogicalName = info.FirstOrDefault()?.RootDirectory.Name;
        }
    }
}
