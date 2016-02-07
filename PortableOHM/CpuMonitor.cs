using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenHardwareMonitor.Hardware;

namespace OHMWrapper
{
    public class CpuMonitor : OHMMonitor
    {
        public OHMSensor[] CoreClocks { get; private set; }
        public OHMSensor[] CoreLoads { get; private set; }
        public OHMSensor TotalLoad { get; private set; }
        public OHMSensor Voltage { get; private set; }
        public OHMSensor[] Temperatures { get; private set; }
        public OHMSensor PackageTemperature { get; private set; }
        public OHMSensor FanRPM { get; private set; }
        public OHMSensor FanPercent { get; private set; }

        public CpuMonitor(IHardware hardware, IHardware board) : base(hardware)
        {
            InitCPU(board);
        }

        private void InitCPU(IHardware board)
        {
            List<OHMSensor> _sensorList = new List<OHMSensor>();

            InitClocks(_sensorList);
            InitLoad(_sensorList);
            InitTemperature(board, _sensorList);
            InitFans(board, _sensorList);
            InitVoltage(board, _sensorList);

            Sensors = _sensorList.ToArray();
        }

        private void InitVoltage(IHardware board, List<OHMSensor> _sensorList)
        {
            ISensor voltage = null;

            if (board != null)
            {
                voltage =
                    board.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Voltage && s.Name.Contains("CPU"));
            }

            if (voltage == null && board != null)
            {
                voltage =
                    board.SubHardware.SelectMany(h => h.Sensors)
                        .FirstOrDefault(s => s.SensorType == SensorType.Voltage && s.Name.Contains("CPU"));
            }

            if (voltage == null)
            {
                voltage = _hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Voltage);
            }

            if (voltage != null)
            {
                _sensorList.Add(new OHMSensor(voltage, DataType.Voltage, "Voltage"));
            }
        }

        private void InitTemperature(IHardware board, List<OHMSensor> _sensorList)
        {
            List<ISensor> cpuTemperatures = _hardware.Sensors.Where(s => s.SensorType == SensorType.Temperature).ToList();
            List<ISensor> boardTemperatures = board.Sensors.Where(s => s.SensorType == SensorType.Temperature).ToList();
            boardTemperatures.AddRange(board.SubHardware.SelectMany(s => s.Sensors).Where(s => s.SensorType == SensorType.Temperature));
            List<ISensor> boardCpuTemperatures = boardTemperatures.Where(s => s.Name.Contains("CPU")).ToList();

            if (board.SubHardware.ToList().Any(h => h.Name.Contains("IT8712F")))
            {
                PackageTemperature = new OHMSensor(boardTemperatures.First(), DataType.Celcius, "Package");
                _sensorList.Add(PackageTemperature);
                return;
            }

            if (cpuTemperatures.Count > 0)
            {
                if (cpuTemperatures.Count == 1)
                {
                    PackageTemperature = new OHMSensor(cpuTemperatures[0], DataType.Celcius, "Package");
                    _sensorList.Add(PackageTemperature);
                }
                else
                {
                    PackageTemperature = new OHMSensor(cpuTemperatures.FirstOrDefault(s => s.Name.Contains("Package")), DataType.Celcius, "Package");
                    Temperatures =
                        cpuTemperatures.Where(sensor => !sensor.Name.Contains("Package"))
                            .Select(r => new OHMSensor(r, DataType.Celcius, string.Format("Core {0}", r.Index))).ToArray();
                    if (PackageTemperature != null)
                    {
                        _sensorList.Add(PackageTemperature);
                    }
                    if (Temperatures.Length > 0)
                    {
                        _sensorList.AddRange(Temperatures);
                    }
                }
            }
            else if (boardCpuTemperatures.Count > 0)
            {
                if (boardCpuTemperatures.Count == 1)
                {
                    PackageTemperature = new OHMSensor(cpuTemperatures[0], DataType.Celcius, "Package");
                    _sensorList.Add(PackageTemperature);
                }
                //else: haven't seen any IO circuits that report core temperatures
            }
        }

        private void InitFans(IHardware board, List<OHMSensor> _sensorList)
        {
            List<ISensor> fans = _hardware.Sensors.Where(s => s.SensorType == SensorType.Fan).ToList();
            fans.AddRange(board.Sensors.Where(s => s.SensorType == SensorType.Fan && s.Name.Contains("CPU")));
            fans.AddRange(board.SubHardware.SelectMany(h => h.Sensors).Where(s => s.SensorType == SensorType.Fan && s.Name.Contains("CPU")));

            List<ISensor> controls = _hardware.Sensors.Where(s => s.SensorType == SensorType.Fan).ToList();
            controls.AddRange(board.Sensors.Where(s => s.SensorType == SensorType.Control && s.Name.Contains("CPU")));
            controls.AddRange(board.SubHardware.SelectMany(h => h.Sensors).Where(s => s.SensorType == SensorType.Control && s.Name.Contains("CPU")));

            if (fans.Count < 0)
            {
                FanRPM = new OHMSensor(fans.FirstOrDefault(), DataType.RPM, "Fan");
                _sensorList.Add(FanRPM);
            }
            else
            {
                List<ISensor> fansLastResort = board.Sensors.Where(s => s.SensorType == SensorType.Fan).ToList();
                fansLastResort.AddRange(board.SubHardware.SelectMany(h => h.Sensors).Where(s => s.SensorType == SensorType.Fan));
                if (fansLastResort.Count > 0)
                {
                    FanRPM = new OHMSensor(fansLastResort.FirstOrDefault(), DataType.RPM, "Fan");
                    _sensorList.Add(FanRPM);
                }

            }
            if (controls.Count < 0)
            {
                FanPercent = new OHMSensor(controls.FirstOrDefault(), DataType.Percent, "Fan");
                _sensorList.Add(FanPercent);
            }
            else
            {
                List<ISensor> controlsLastResort = board.Sensors.Where(s => s.SensorType == SensorType.Control).ToList();
                controlsLastResort.AddRange(board.SubHardware.SelectMany(h => h.Sensors).Where(s => s.SensorType == SensorType.Control));
                if (controlsLastResort.Count > 0)
                {
                    FanPercent = new OHMSensor(controlsLastResort.FirstOrDefault(), DataType.Percent, "Fan");
                    _sensorList.Add(FanPercent);
                }

            }
        }

        private void InitLoad(List<OHMSensor> _sensorList)
        {
            ISensor[] loadSensors = _hardware.Sensors.Where(s => s.SensorType == SensorType.Load).ToArray();

            if (loadSensors.Length > 0)
            {
                ISensor totalCpu = loadSensors.Where(s => s.Index == 0).FirstOrDefault();

                if (totalCpu != null)
                {
                    TotalLoad = new OHMSensor(totalCpu, DataType.Percent, "Load");
                    _sensorList.Add(TotalLoad);
                }
                
                    int cores = loadSensors.Max(s => s.Index);
                    CoreLoads = new OHMSensor[cores];
                    for (int i = 1; i <= cores; i++)
                    {
                        ISensor coreLoad = loadSensors.Where(s => s.Index == i).FirstOrDefault();

                        if (coreLoad != null)
                        {
                            CoreLoads[i - 1] = new OHMSensor(coreLoad, DataType.Percent, string.Format("Core {0}", i - 1));
                            _sensorList.Add(CoreLoads[i - 1]);
                        }
                    }
                
            }
        
        }

        private void InitClocks( List<OHMSensor> _sensorList)
        {
            ISensor[] coreClocks =
                _hardware.Sensors.Where(s => s.SensorType == SensorType.Clock && s.Name.Contains("CPU")).ToArray();

            if (coreClocks.Length > 0)
            {

                    int cores = coreClocks.Max(s => s.Index);
                    CoreClocks = new OHMSensor[cores];
                    for (int i = 1; i <= cores; i++)
                    {
                        ISensor coreClock = coreClocks.Where(s => s.Index == i).FirstOrDefault();

                        if (coreClock != null)
                        {
                            CoreClocks[i - 1] = new OHMSensor(coreClock, DataType.Clock, string.Format("Core {0}", i - 1),
                                true);
                            _sensorList.Add(CoreClocks[i - 1]);
                        }
                    }
                
               
            }
        }
    }
}
