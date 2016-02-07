using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenHardwareMonitor.Hardware;

namespace PortableOHM
{
    public class CpuMonitor : OHMMonitor
    {
        public OHMSensor[] CoreClocks { get; private set; }
        public OHMSensor[] CoreLoads { get; private set; }
        public OHMSensor TotalLoad { get; private set; }
        public OHMSensor Voltage { get; private set; }
        public OHMSensor[] Temperature { get; private set; }
        public OHMSensor FanRPM { get; private set; }
        public OHMSensor FanPercent { get; private set; }


        public CpuMonitor(IHardware hardware, IHardware board, ConfigParam[] parameters) : base(MonitorType.CPU, hardware, board, parameters)
        {
            InitCPU(board,
                        parameters.GetValue<bool>(ParamKey.AllCoreClocks),
                        parameters.GetValue<bool>(ParamKey.CoreLoads),
                        parameters.GetValue<int>(ParamKey.TempAlert));
        }

        private void InitCPU(IHardware board, bool allCoreClocks, bool coreLoads, double tempAlert)
        {
            List<OHMSensor> _sensorList = new List<OHMSensor>();

            InitClocks(_sensorList);

            initVoltageFanTemp(board, tempAlert, _sensorList);

            initLoad(_sensorList);

            Sensors = _sensorList.ToArray();
        }

        private void initVoltageFanTemp(IHardware board, double tempAlert, List<OHMSensor> _sensorList)
        {
            ISensor _voltage = null;
            ISensor _tempSensor = null;
            ISensor _fanSensor = null;

            if (board != null)
            {
                _voltage =
                    board.Sensors.Where(s => s.SensorType == SensorType.Voltage && s.Name.Contains("CPU")).FirstOrDefault();
                _tempSensor =
                    board.Sensors.Where(s => s.SensorType == SensorType.Temperature && s.Name.Contains("CPU")).FirstOrDefault();
                _fanSensor =
                    board.Sensors.Where(
                        s =>
                            new SensorType[2] {SensorType.Fan, SensorType.Control}.Contains(s.SensorType) &&
                            s.Name.Contains("CPU")).FirstOrDefault();
            }

            if (_voltage == null)
            {
                _voltage = _hardware.Sensors.Where(s => s.SensorType == SensorType.Voltage).FirstOrDefault();
            }

            if (_tempSensor == null)
            {
                _tempSensor =
                    _hardware.Sensors.Where(s => s.SensorType == SensorType.Temperature && s.Name == "CPU Package")
                        .FirstOrDefault() ??
                    _hardware.Sensors.Where(s => s.SensorType == SensorType.Temperature).FirstOrDefault();
            }

            if (_fanSensor == null)
            {
                _fanSensor =
                    _hardware.Sensors.Where(s => new SensorType[2] {SensorType.Fan, SensorType.Control}.Contains(s.SensorType))
                        .FirstOrDefault();
            }

            if (_voltage != null)
            {
                _sensorList.Add(new OHMSensor(_voltage, DataType.Voltage, "Voltage"));
            }

            if (_tempSensor != null)
            {
                _sensorList.Add(new OHMSensor(_tempSensor, DataType.Celcius, "Temp", false, tempAlert));
            }

            if (_fanSensor != null)
            {
                _sensorList.Add(new OHMSensor(_fanSensor, DataType.RPM, "Fan"));
            }
        }

        private void initLoad(List<OHMSensor> _sensorList)
        {
            ISensor[] _loadSensors = _hardware.Sensors.Where(s => s.SensorType == SensorType.Load).ToArray();

            if (_loadSensors.Length > 0)
            {
                ISensor _totalCPU = _loadSensors.Where(s => s.Index == 0).FirstOrDefault();

                if (_totalCPU != null)
                {
                    TotalLoad = new OHMSensor(_totalCPU, DataType.Percent, "Load");
                    _sensorList.Add(TotalLoad);
                }
                
                    int cores = _loadSensors.Max(s => s.Index);
                    CoreLoads = new OHMSensor[cores];
                    for (int i = 1; i <= cores; i++)
                    {
                        ISensor _coreLoad = _loadSensors.Where(s => s.Index == i).FirstOrDefault();

                        if (_coreLoad != null)
                        {
                            CoreLoads[i - 1] = new OHMSensor(_coreLoad, DataType.Percent, string.Format("Core {0}", i - 1));
                            _sensorList.Add(CoreLoads[i - 1]);
                        }
                    }
                
            }
        }

        private void InitClocks( List<OHMSensor> _sensorList)
        {
            ISensor[] _coreClocks =
                _hardware.Sensors.Where(s => s.SensorType == SensorType.Clock && s.Name.Contains("CPU")).ToArray();

            if (_coreClocks.Length > 0)
            {

                    int cores = _coreClocks.Max(s => s.Index);
                    CoreClocks = new OHMSensor[cores];
                    for (int i = 1; i <= cores; i++)
                    {
                        ISensor _coreClock = _coreClocks.Where(s => s.Index == i).FirstOrDefault();

                        if (_coreClock != null)
                        {
                            CoreClocks[i - 1] = new OHMSensor(_coreClock, DataType.Clock, string.Format("Core {0}", i - 1),
                                true);
                            _sensorList.Add(CoreClocks[i - 1]);
                        }
                    }
                
               
            }
        }
    }
}
