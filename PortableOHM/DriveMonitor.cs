using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenHardwareMonitor.Hardware;

namespace OHMWrapper
{
    public class DriveMonitor : OHMMonitor
    {
        private System.IO.DriveInfo _info;
        private PerformanceCounter _counterReadRate { get; set; }
        private PerformanceCounter _counterWriteRate { get; set; }
        private const string BYTESREADPERSECOND = "Disk Read Bytes/sec";
        private const string BYTESWRITEPERSECOND = "Disk Write Bytes/sec";
        internal const string CATEGORYNAME = "LogicalDisk";


        public string LogicalName { get; private set; }
        public OHMSensor Temperature { get; private set; }
        public OHMSensor RemainingLife { get; private set; }
        public OHMSensor[] LifecycleData { get; private set; }
        public double ReadRate { get; private set; }
        public double WriteRate { get; private set; }

        public DriveMonitor(IHardware hardware) : base(hardware)
        {
            List<OHMSensor> sensorList = new List<OHMSensor>();

            InitTemperature(sensorList);
            InitRemainingLife(sensorList);
            InitLifecycleData(sensorList);
            readDriveInfo();
            InitName();
            initReadWriteRate();

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
                RemainingLife = new OHMSensor(sensor, DataType.Percent, "Remaining life");
                sensors.Add(RemainingLife);
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

        protected override void UpdateHardware()
        {
            base.UpdateHardware();
            if (_counterReadRate != null && _counterWriteRate != null)
            {
                ReadRate = _counterReadRate.NextValue() / 1024d;
                WriteRate = _counterWriteRate.NextValue() / 1024d;
            }
        }

        void InitName()
        {
            LogicalName = _info?.RootDirectory.Name;
        }

        void initReadWriteRate()
        {
            String name = _info.Name.Replace("\\", "");
            Regex _regex = new Regex("^["+ name + "]:$");
            List<String> names= new PerformanceCounterCategory(CATEGORYNAME).GetInstanceNames()
                .Where(n => _regex.IsMatch(n))
                .OrderBy(d => d[0])
                .ToList();
            _counterReadRate = new PerformanceCounter(DriveInfoMonitor.CATEGORYNAME, BYTESREADPERSECOND, name);
            _counterWriteRate = new PerformanceCounter(DriveInfoMonitor.CATEGORYNAME, BYTESWRITEPERSECOND, name);
        }

        private void readDriveInfo()
        {
            _info = PropertyHelper.GetPrivateFieldValue<System.IO.DriveInfo[]>(_hardware,
                "driveInfos").FirstOrDefault();
        }


    }
}
