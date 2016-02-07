using System.Collections.Generic;
using System.Linq;
using OpenHardwareMonitor.Hardware;

namespace OHMWrapper
{
    public class GpuMonitor : OHMMonitor
    {
        public OHMSensor CoreClock { get; private set ; }
        public OHMSensor MemoryClock { get; private set ; }
        public OHMSensor CoreLoad { get; private set ; }
        public OHMSensor MemoryLoad { get; private set ; }
        public OHMSensor Voltage { get; private set ; }
        public OHMSensor Temperature { get; private set ; }
        public OHMSensor FanPercent { get; private set ; }
        public OHMSensor FanRPM { get; private set ; }

        public GpuMonitor(IHardware hardware) : base(hardware)
        {
            InitGPU();
        }

        public void InitGPU()
        {
            List<OHMSensor> _sensorList = new List<OHMSensor>();

            ISensor _coreClock = _hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Clock && s.Index == 0);

            if (_coreClock != null)
            {
                CoreClock = new OHMSensor(_coreClock, DataType.Clock, "Core", true);
                _sensorList.Add(CoreClock);
            }

            ISensor _memoryClock = _hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Clock && s.Index == 1);

            if (_memoryClock != null)
            {
                MemoryClock = new OHMSensor(_memoryClock, DataType.Clock, "VRAM", true);
                _sensorList.Add(MemoryClock);
            }

            ISensor _coreLoad = _hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Index == 0);

            if (_coreLoad != null)
            {
                CoreLoad = new OHMSensor(_coreLoad, DataType.Percent, "Core");
                _sensorList.Add(CoreLoad);
            }

            ISensor _memoryLoad = _hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Index == 3);

            if (_memoryLoad != null)
            {
                MemoryLoad = new OHMSensor(_memoryLoad, DataType.Percent, "VRAM");
                _sensorList.Add(MemoryLoad);
            }

            ISensor _voltage = _hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Voltage && s.Index == 0);

            if (_voltage != null)
            {
                Voltage = new OHMSensor(_voltage, DataType.Voltage, "Voltage");
                _sensorList.Add(Voltage);
            }

            ISensor _tempSensor = _hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature && s.Index == 0);

            if (_tempSensor != null)
            {
                Temperature = new OHMSensor(_tempSensor, DataType.Celcius, "Temp", false);
                _sensorList.Add(Temperature);
            }

            ISensor _fanSensor = _hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Control && s.Index == 0);

            if (_fanSensor != null)
            {
                FanPercent = new OHMSensor(_fanSensor, DataType.Percent, "Fan");
                _sensorList.Add(FanPercent);
            }

            ISensor _fanRpmSensor = _hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Fan && s.Index == 0);

            if (_fanRpmSensor != null)
            {
                FanRPM = new OHMSensor(_fanRpmSensor, DataType.RPM, "Fan");
                _sensorList.Add(FanRPM);
            }


            Sensors = _sensorList.ToArray();
        }
    }
}