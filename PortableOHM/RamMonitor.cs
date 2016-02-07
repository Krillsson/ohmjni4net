using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OHMWrapper;
using OpenHardwareMonitor.Hardware;

namespace OHMWrapper
{
    public class RamMonitor : OHMMonitor
    {
        public OHMSensor Clock { get; private set; }
        public OHMSensor Voltage { get; private set; }
        public OHMSensor Load { get; private set; }
        public OHMSensor Used { get; private set; }
        public OHMSensor Available { get; private set; }


        public RamMonitor(IHardware hardware, IHardware board) : base(hardware)
        {
            InitRAM(board);
        }

        public void InitRAM(IHardware board)
        {
            List<OHMSensor> _sensorList = new List<OHMSensor>();

            ISensor _ramClock = _hardware.Sensors.Where(s => s.SensorType == SensorType.Clock).FirstOrDefault();

            if (_ramClock != null)
            {
                Clock = new OHMSensor(_ramClock, DataType.Clock, "Clock", true);
                _sensorList.Add(Clock);
            }

            ISensor _voltage = null;

            if (board != null)
            {
                _voltage = board.SubHardware.SelectMany(h => h.Sensors).Where(s => s.SensorType == SensorType.Voltage && s.Name.Contains("RAM")).FirstOrDefault();
            }

            if (_voltage == null)
            {
                _voltage = _hardware.Sensors.Where(s => s.SensorType == SensorType.Voltage).FirstOrDefault();
            }

            if (_voltage != null)
            {
                Voltage = new OHMSensor(_voltage, DataType.Voltage, "Voltage");
                _sensorList.Add(Voltage);
            }

            ISensor _loadSensor = _hardware.Sensors.Where(s => s.SensorType == SensorType.Load && s.Index == 0).FirstOrDefault();

            if (_loadSensor != null)
            {
                Load = new OHMSensor(_loadSensor, DataType.Percent, "Load");
                _sensorList.Add(Load);
            }

            ISensor _usedSensor = _hardware.Sensors.Where(s => s.SensorType == SensorType.Data && s.Index == 0).FirstOrDefault();

            if (_usedSensor != null)
            {
                Used = new OHMSensor(_usedSensor, DataType.Gigabyte, "Used");
                _sensorList.Add(Used);
            }

            ISensor _availSensor = _hardware.Sensors.Where(s => s.SensorType == SensorType.Data && s.Index == 1).FirstOrDefault();

            if (_availSensor != null)
            {
                Available = new OHMSensor(_availSensor, DataType.Gigabyte, "Free");
                _sensorList.Add(Available);
            }

            Sensors = _sensorList.ToArray();
        }
    }
}
