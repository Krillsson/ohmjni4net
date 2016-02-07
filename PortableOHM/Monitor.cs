﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using OpenHardwareMonitor.Hardware;

namespace PortableOHM
{
    public class MonitorManager : IDisposable
    {
        public MonitorManager(IComputer computer)
        {
            _computer = computer;
            _board = GetHardware(HardwareType.Mainboard).FirstOrDefault();

            OHMMonitorsList = new List<OHMMonitor>();
            _gpuMonitors = new List<GpuMonitor>();
            _cpuMonitors = new List<CpuMonitor>();

            UpdateBoard();

            foreach (MonitorConfig _config in MonitorConfig.Default)
            {
                InitWithConfig(_config);
            }
        }

        public void InitWithConfig(MonitorConfig config)
        {
            switch (config.Type)
            {
                case MonitorType.CPU:
                    SetupCpuMonitors(config.Params);
                    return;
                
                case MonitorType.RAM:
                    OHMPanel(
                        config.Type,
                        config.Params,
                        HardwareType.RAM
                        );
                    return;

                case MonitorType.GPU:
                       SetupGpuMonitors(config.Params);
                    return;

                case MonitorType.HD:
                    DrivePanel(config.Type, config.Params);
                    return;

                case MonitorType.Network:
                    NetworkPanel(config.Type, config.Params);
                    return;
            }
        }

        public void Update()
        {
            UpdateBoard();

            OHMMonitorsList.ForEach(i => i.Update());
            _cpuMonitors.ForEach(i => i.Update());
            _gpuMonitors.ForEach(i => i.Update());
            
            NetworkMonitor.Update();
            DriveMonitor.Update();
        }

        public void Dispose()
        {
            OHMMonitorsList.ForEach(i => i.Dispose());
            _cpuMonitors.ForEach(i => i.Dispose());
            _gpuMonitors.ForEach(i => i.Dispose());

            NetworkMonitor.Dispose();
            DriveMonitor.Dispose();
        }

        private IEnumerable<IHardware> GetHardware(params HardwareType[] types)
        {
            return _computer.Hardware.Where(h => types.Contains(h.HardwareType));
        }

        private void OHMPanel(MonitorType type, ConfigParam[] parameters, params HardwareType[] hardwareTypes)
        {
          
            foreach (IHardware _hardware in GetHardware(hardwareTypes))
            {
                OHMMonitorsList.Add(new OHMMonitor(type, _hardware, _board, parameters));
            }

        }

        private void DrivePanel(MonitorType type, ConfigParam[] parameters)
        {
            DriveMonitor = new DriveMonitor(parameters);
        }

        private void NetworkPanel(MonitorType type, ConfigParam[] parameters)
        {
            NetworkMonitor = new NetworkMonitor(parameters);
        }

        private void UpdateBoard()
        {
            _board.Update();

            foreach (IHardware _subhardware in _board.SubHardware)
            {
                _subhardware.Update();
            }
        }

        private List<OHMMonitor> OHMMonitorsList { get; set; }
        public OHMMonitor[] OHMMonitors()
        {
            return OHMMonitorsList.ToArray();
        }

        private void SetupGpuMonitors(ConfigParam[] parameters)
        {
            HardwareType[] hardwareTypes = {HardwareType.GpuNvidia, HardwareType.GpuAti};
            foreach (IHardware _hardware in GetHardware(hardwareTypes))
            {
                _gpuMonitors.Add(new GpuMonitor(_hardware, _board, parameters));
            }
        }

        private List<GpuMonitor> _gpuMonitors; 

        public GpuMonitor[] GpuMonitors()
        {
            return _gpuMonitors.ToArray();
        }

        private void SetupCpuMonitors(ConfigParam[] parameters)
        {
            HardwareType[] hardwareTypes = { HardwareType.CPU };
            foreach (IHardware _hardware in GetHardware(hardwareTypes))
            {
                _cpuMonitors.Add(new CpuMonitor(_hardware, _board, parameters));
            }
        }

        private List<CpuMonitor> _cpuMonitors;

        public CpuMonitor[] CpuMonitors()
        {
            return _cpuMonitors.ToArray();
        }

        public DriveMonitor DriveMonitor { get; private set; }
        public NetworkMonitor NetworkMonitor { get; private set; }

        private IComputer _computer { get; set; }

        private IHardware _board { get; set; }

        private IHardware findIOChipWithTemp()
        {
            return null;
        }
    }

    public class OHMMonitor
    {
        public OHMMonitor(MonitorType type, IHardware hardware, IHardware board, ConfigParam[] parameters)
        {
            Name = hardware.Name;

            ShowName = parameters.GetValue<bool>(ParamKey.HardwareNames);

            _hardware = hardware;

            UpdateHardware();

            switch (type)
            {

                case MonitorType.RAM:
                    InitRAM(
                        board
                        );
                    break;
            }
        }

        public void Update()
        {
            UpdateHardware();

            foreach (OHMSensor _sensor in Sensors)
            {
                _sensor.Update();
            }
        }

        public void Dispose()
        {
        }

        protected void UpdateHardware()
        {
            _hardware.Update();

            foreach (IHardware _subHardware in _hardware.SubHardware)
            {
                _subHardware.Update();
            }
        }

        

        public void InitRAM(IHardware board)
        {
            List<OHMSensor> _sensorList = new List<OHMSensor>();

            ISensor _ramClock = _hardware.Sensors.Where(s => s.SensorType == SensorType.Clock).FirstOrDefault();

            if (_ramClock != null)
            {
                _sensorList.Add(new OHMSensor(_ramClock, DataType.Clock, "Clock", true));
            }

            ISensor _voltage = null;

            if (board != null)
            {
                _voltage = board.Sensors.Where(s => s.SensorType == SensorType.Voltage && s.Name.Contains("RAM")).FirstOrDefault();
            }

            if (_voltage == null)
            {
                _voltage = _hardware.Sensors.Where(s => s.SensorType == SensorType.Voltage).FirstOrDefault();
            }

            if (_voltage != null)
            {
                _sensorList.Add(new OHMSensor(_voltage, DataType.Voltage, "Voltage"));
            }

            ISensor _loadSensor = _hardware.Sensors.Where(s => s.SensorType == SensorType.Load && s.Index == 0).FirstOrDefault();

            if (_loadSensor != null)
            {
                _sensorList.Add(new OHMSensor(_loadSensor, DataType.Percent, "Load"));
            }

            ISensor _usedSensor = _hardware.Sensors.Where(s => s.SensorType == SensorType.Data && s.Index == 0).FirstOrDefault();

            if (_usedSensor != null)
            {
                _sensorList.Add(new OHMSensor(_usedSensor, DataType.Gigabyte, "Used"));
            }

            ISensor _availSensor = _hardware.Sensors.Where(s => s.SensorType == SensorType.Data && s.Index == 1).FirstOrDefault();

            if (_availSensor != null)
            {
                _sensorList.Add(new OHMSensor(_availSensor, DataType.Gigabyte, "Free"));
            }

            Sensors = _sensorList.ToArray();
        }


        public string Name { get; protected set; }

        public bool ShowName { get; protected set; }

        public OHMSensor[] Sensors { get; protected set; }

        protected IHardware _hardware { get; set; }
    }

    public class DriveMonitor
    {
        internal const string CATEGORYNAME = "LogicalDisk";

        public DriveMonitor(ConfigParam[] parameters)
        {            
            bool _showDetails = parameters.GetValue<bool>(ParamKey.DriveDetails);
            int _usedSpaceAlert = parameters.GetValue<int>(ParamKey.UsedSpaceAlert);

            Regex _regex = new Regex("^[A-Z]:$");

            Drives = new PerformanceCounterCategory(CATEGORYNAME).GetInstanceNames().Where(n => _regex.IsMatch(n)).OrderBy(d => d[0]).Select(n => new DriveInfo(n, _showDetails, _usedSpaceAlert)).ToArray();
        }

        public void Update()
        {
            foreach (DriveInfo _drive in Drives)
            {
                _drive.Update();
            }
        }

        public void Dispose()
        {
            foreach (DriveInfo _drive in Drives)
            {
                _drive.Dispose();
            }
        }

        public DriveInfo[] Drives { get; private set; }
    }

    public class DriveInfo : IDisposable, INotifyPropertyChanged
    {
        private const string FREEMB = "Free Megabytes";
        private const string PERCENTFREE = "% Free Space";
        private const string BYTESREADPERSECOND = "Disk Read Bytes/sec";
        private const string BYTESWRITEPERSECOND = "Disk Write Bytes/sec";

        public DriveInfo(string name, bool showDetails = false, double usedSpaceAlert = 0)
        {
            Label = Instance = name;
            ShowDetails = showDetails;
            UsedSpaceAlert = usedSpaceAlert;

            _counterFreeMB = new PerformanceCounter(DriveMonitor.CATEGORYNAME, FREEMB, name);
            _counterFreePercent = new PerformanceCounter(DriveMonitor.CATEGORYNAME, PERCENTFREE, name);

            if (showDetails)
            {
                _counterReadRate = new PerformanceCounter(DriveMonitor.CATEGORYNAME, BYTESREADPERSECOND, name);
                _counterWriteRate = new PerformanceCounter(DriveMonitor.CATEGORYNAME, BYTESWRITEPERSECOND, name);
            }
        }

        public void Update()
        {
            if (!PerformanceCounterCategory.InstanceExists(Instance, DriveMonitor.CATEGORYNAME))
            {
                return;
            }

            double _freeGB = _counterFreeMB.NextValue() / 1024d;
            double _freePercent = _counterFreePercent.NextValue();

            double _usedPercent = 100d - _freePercent;

            double _totalGB = _freeGB / (_freePercent / 100);
            double _usedGB = _totalGB - _freeGB;

            Value = _usedPercent;

            if (ShowDetails)
            {
                Load = string.Format("Load: {0:#,##0.##}%", _usedPercent);
                UsedGB = string.Format("Used: {0:#,##0.##} GB", _usedGB);
                FreeGB = string.Format("Free: {0:#,##0.##} GB", _freeGB);

                double _readRate = _counterReadRate.NextValue() / 1024d;

                string _readFormat;
                Data.MinifyKiloBytesPerSecond(ref _readRate, out _readFormat);

                ReadRate = string.Format("Read: {0:#,##0.##} {1}", _readRate, _readFormat);

                double _writeRate = _counterWriteRate.NextValue() / 1024d;

                string _writeFormat;
                Data.MinifyKiloBytesPerSecond(ref _writeRate, out _writeFormat);

                WriteRate = string.Format("Write: {0:#,##0.##} {1}", _writeRate, _writeFormat);
            }

            if (UsedSpaceAlert > 0 && UsedSpaceAlert <= _usedPercent)
            {
                if (!IsAlert)
                {
                    IsAlert = true;
                }
            }
            else if (IsAlert)
            {
                IsAlert = false;
            }
        }

        public void Dispose()
        {
            if (_counterFreeMB != null)
            {
                _counterFreeMB.Dispose();
            }

            if (_counterFreePercent != null)
            {
                _counterFreePercent.Dispose();
            }

            if (_counterReadRate != null)
            {
                _counterReadRate.Dispose();
            }

            if (_counterWriteRate != null)
            {
                _counterWriteRate.Dispose();
            }
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler _handler = PropertyChanged;

            if (_handler != null)
            {
                _handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Instance { get; private set; }

        public string Label { get; private set; }

        private double _value { get; set; }

        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;

                NotifyPropertyChanged("Value");
            }
        }
        
        private string _load { get; set; }

        public string Load
        {
            get
            {
                return _load;
            }
            set
            {
                _load = value;

                NotifyPropertyChanged("Load");
            }
        }

        private string _usedGB { get; set; }

        public string UsedGB
        {
            get
            {
                return _usedGB;
            }
            set
            {
                _usedGB = value;

                NotifyPropertyChanged("UsedGB");
            }
        }

        private string _freeGB { get; set; }

        public string FreeGB
        {
            get
            {
                return _freeGB;
            }
            set
            {
                _freeGB = value;

                NotifyPropertyChanged("FreeGB");
            }
        }

        public string _readRate { get; set; }

        public string ReadRate
        {
            get
            {
                return _readRate;
            }
            set
            {
                _readRate = value;

                NotifyPropertyChanged("ReadRate");
            }
        }

        private string _writeRate { get; set; }

        public string WriteRate
        {
            get
            {
                return _writeRate;
            }
            set
            {
                _writeRate = value;

                NotifyPropertyChanged("WriteRate");
            }
        }

        private bool _isAlert { get; set; }

        public bool IsAlert
        {
            get
            {
                return _isAlert;
            }
            set
            {
                _isAlert = value;

                NotifyPropertyChanged("IsAlert");
            }
        }

        public bool ShowDetails { get; private set; }

        public double UsedSpaceAlert { get; private set; }

        private PerformanceCounter _counterFreeMB { get; set; }

        private PerformanceCounter _counterFreePercent { get; set; }

        private PerformanceCounter _counterReadRate { get; set; }

        private PerformanceCounter _counterWriteRate { get; set; }
    }

    public class NetworkMonitor
    {
        internal const string CATEGORYNAME = "Network Interface";

        public NetworkMonitor(ConfigParam[] parameters)
        {
            bool _showName = parameters.GetValue<bool>(ParamKey.HardwareNames);
            bool _useBytes = parameters.GetValue<bool>(ParamKey.UseBytes);
            int _bandwidthInAlert = parameters.GetValue<int>(ParamKey.BandwidthInAlert);
            int _bandwidthOutAlert = parameters.GetValue<int>(ParamKey.BandwidthOutAlert);

            string[] _instances = new PerformanceCounterCategory(CATEGORYNAME).GetInstanceNames();

            NetworkInterface[] _nics = NetworkInterface.GetAllNetworkInterfaces().Where(n =>
                    n.OperationalStatus == OperationalStatus.Up &&
                    new NetworkInterfaceType[2] { NetworkInterfaceType.Ethernet, NetworkInterfaceType.Wireless80211 }.Contains(n.NetworkInterfaceType)
                    ).ToArray();

            Regex _regex = new Regex("[^A-Za-z]");

            Nics = _instances.Join(_nics, i => _regex.Replace(i, ""), n => _regex.Replace(n.Description, ""), (i, n) => new NicInfo(i, n.Description, _showName, _useBytes, _bandwidthInAlert, _bandwidthOutAlert), StringComparer.Ordinal).ToArray();
        }

        public void Update()
        {
            foreach (NicInfo _nic in Nics)
            {
                _nic.Update();
            }
        }

        public void Dispose()
        {
            foreach (NicInfo _nic in Nics)
            {
                _nic.Dispose();
            }
        }
        
        public NicInfo[] Nics { get; private set; }
    }

    public class NicInfo : IDisposable
    {
        private const string BYTESRECEIVEDPERSECOND = "Bytes Received/sec";
        private const string BYTESSENTPERSECOND = "Bytes Sent/sec";

        public NicInfo(string instance, string name, bool showName = true, bool useBytes = false, double bandwidthInAlert = 0, double bandwidthOutAlert = 0)
        {
            Instance = instance;
            Name = name;
            ShowName = showName;

            InBandwidth = new Bandwidth(
                new PerformanceCounter(NetworkMonitor.CATEGORYNAME, BYTESRECEIVEDPERSECOND, instance),
                "In",
                useBytes,
                bandwidthInAlert
                );

            OutBandwidth = new Bandwidth(
                new PerformanceCounter(NetworkMonitor.CATEGORYNAME, BYTESSENTPERSECOND, instance),
                "Out",
                useBytes,
                bandwidthOutAlert
                );
        }

        public void Update()
        {
            if (!PerformanceCounterCategory.InstanceExists(Instance, NetworkMonitor.CATEGORYNAME))
            {
                return;
            }

            InBandwidth.Update();
            OutBandwidth.Update();
        }

        public void Dispose()
        {
            InBandwidth.Dispose();
            OutBandwidth.Dispose();
        }

        public string Instance { get; private set; }

        public string Name { get; private set; }

        public bool ShowName { get; private set; }
        
        public Bandwidth InBandwidth { get; private set; }

        public Bandwidth OutBandwidth { get; private set; }
    }

    public class Bandwidth : IDisposable, INotifyPropertyChanged
    {
        public Bandwidth(PerformanceCounter counter, string label, bool useBytes = false, double alertValue = 0)
        {
            _counter = counter;

            Label = label;
            UseBytes = useBytes;
            AlertValue = alertValue;
        }

        public void Update()
        {
            double _value = _counter.NextValue() / (UseBytes ? 1024d : 128d);

            if (AlertValue > 0 && AlertValue <= _value)
            {
                if (!IsAlert)
                {
                    IsAlert = true;
                }
            }
            else if (IsAlert)
            {
                IsAlert = false;
            }

            string _format;

            if (UseBytes)
            {
                Data.MinifyKiloBytesPerSecond(ref _value, out _format);
            }
            else
            {
                Data.MinifyKiloBitsPerSecond(ref _value, out _format);
            }

            Text = string.Format("{0}: {1:#,##0.##} {2}", Label, _value, _format);
        }

        public void Dispose()
        {
            if (_counter != null)
            {
                _counter.Dispose();
            }
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler _handler = PropertyChanged;

            if (_handler != null)
            {
                _handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Label { get; private set; }

        private string _text { get; set; }

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;

                NotifyPropertyChanged("Text");
            }
        }

        private bool _isAlert { get; set; }

        public bool IsAlert
        {
            get
            {
                return _isAlert;
            }
            set
            {
                _isAlert = value;

                NotifyPropertyChanged("IsAlert");
            }
        }

        public bool UseBytes { get; set; }

        public double AlertValue { get; private set; }

        private PerformanceCounter _counter { get; set; }
    }

    [Serializable]
    public enum MonitorType : byte
    {
        CPU,
        RAM,
        GPU,
        HD,
        Network
    }
    
    [Serializable]
    public class MonitorConfig
    {
        public MonitorType Type { get; set; }

        public bool Enabled { get; set; }

        public byte Order { get; set; }

        public ConfigParam[] Params { get; set; }

        public string Name
        {
            get
            {
                return Type.GetDescription();
            }
        }

        public static bool CheckConfig(MonitorConfig[] config, ref MonitorConfig[] output)
        {
            MonitorConfig[] _default = Default;

            if (config == null || config.Length != _default.Length)
            {
                output = _default;
                return false;
            }

            for (int i = 0; i < config.Length; i++)
            {
                MonitorConfig _record = config[i];
                MonitorConfig _defaultRecord = _default[i];

                if (_record == null || _record.Type != _defaultRecord.Type || _record.Params.Length != _defaultRecord.Params.Length)
                {
                    output = _default;
                    return false;
                }

                for (int v = 0; v < _record.Params.Length; v++)
                {
                    ConfigParam _param = _record.Params[v];
                    ConfigParam _defaultParam = _defaultRecord.Params[v];

                    if (_param == null || _param.Key != _defaultParam.Key)
                    {
                        output = _default;
                        return false;
                    }
                }
            }

            return true;
        }

        public static MonitorConfig[] Default
        {
            get
            {
                return new MonitorConfig[5]
                {
                    new MonitorConfig()
                    {
                        Type = MonitorType.CPU,
                        Enabled = true,
                        Order = 1,
                        Params = new ConfigParam[5]
                        {
                            ConfigParam.Defaults.HardwareNames,
                            ConfigParam.Defaults.AllCoreClocks,
                            ConfigParam.Defaults.CoreLoads,
                            ConfigParam.Defaults.UseFahrenheit,
                            ConfigParam.Defaults.TempAlert
                        }
                    },
                    new MonitorConfig()
                    {
                        Type = MonitorType.RAM,
                        Enabled = true,
                        Order = 2,
                        Params = new ConfigParam[1]
                        {
                            ConfigParam.Defaults.NoHardwareNames
                        }
                    },
                    new MonitorConfig()
                    {
                        Type = MonitorType.GPU,
                        Enabled = true,
                        Order = 3,
                        Params = new ConfigParam[3]
                        {
                            ConfigParam.Defaults.HardwareNames,
                            ConfigParam.Defaults.UseFahrenheit,
                            ConfigParam.Defaults.TempAlert
                        }
                    },
                    new MonitorConfig()
                    {
                        Type = MonitorType.HD,
                        Enabled = true,
                        Order = 4,
                        Params = new ConfigParam[2]
                        {
                            ConfigParam.Defaults.DriveDetails,
                            ConfigParam.Defaults.UsedSpaceAlert
                        }
                    },
                    new MonitorConfig()
                    {
                        Type = MonitorType.Network,
                        Enabled = true,
                        Order = 5,
                        Params = new ConfigParam[4]
                        {
                            ConfigParam.Defaults.HardwareNames,
                            ConfigParam.Defaults.UseBytes,
                            ConfigParam.Defaults.BandwidthInAlert,
                            ConfigParam.Defaults.BandwidthOutAlert
                        }
                    }
                };
            }
        }
    }

    [Serializable]
    public class ConfigParam
    {
        public ParamKey Key { get; set; }

        public object Value { get; set; }

        public Type Type
        {
            get
            {
                return Value.GetType();
            }
        }

        public string TypeString
        {
            get
            {
                return Type.ToString();
            }
        }

        public string Name
        {
            get
            {
                switch (Key)
                {
                    case ParamKey.HardwareNames:
                        return "Show Hardware Names";

                    case ParamKey.UseFahrenheit:
                        return "Use Fahrenheit";

                    case ParamKey.AllCoreClocks:
                        return "Show All Core Clocks";

                    case ParamKey.CoreLoads:
                        return "Show Core Loads";

                    case ParamKey.TempAlert:
                        return "Temperature Alert";

                    case ParamKey.DriveDetails:
                        return "Show Drive Details";

                    case ParamKey.UsedSpaceAlert:
                        return "Used Space Alert";

                    case ParamKey.BandwidthInAlert:
                        return "Bandwidth In Alert";

                    case ParamKey.BandwidthOutAlert:
                        return "Bandwidth Out Alert";

                    case ParamKey.UseBytes:
                        return "Use Bytes Per Second";

                    default:
                        return "Unknown";
                }
            }
        }

        public string Tooltip
        {
            get
            {
                switch (Key)
                {
                    case ParamKey.HardwareNames:
                        return "Shows hardware names.";

                    case ParamKey.UseFahrenheit:
                        return "Temperatures for sensors and alerts will be in Fahrenheit instead of Celcius.";

                    case ParamKey.AllCoreClocks:
                        return "Shows the clock speeds of all cores not just the first.";

                    case ParamKey.CoreLoads:
                        return "Shows the percentage load of all cores.";

                    case ParamKey.TempAlert:
                        return "The temperature threshold at which alerts occur. Use 0 to disable.";

                    case ParamKey.DriveDetails:
                        return "Shows extra drive details as text.";

                    case ParamKey.UsedSpaceAlert:
                        return "The percentage threshold at which used space alerts occur. Use 0 to disable.";

                    case ParamKey.BandwidthInAlert:
                        return "The kbps or kBps threshold at which bandwidth received alerts occur. Use 0 to disable.";

                    case ParamKey.BandwidthOutAlert:
                        return "The kbps or kBps threshold at which bandwidth sent alerts occur. Use 0 to disable.";

                    case ParamKey.UseBytes:
                        return "Shows bandwidth in bytes instead of bits per second.";

                    default:
                        return "Unknown";
                }
            }
        }

        public static class Defaults
        {
            public static ConfigParam HardwareNames
            {
                get
                {
                    return new ConfigParam() { Key = ParamKey.HardwareNames, Value = true };
                }
            }

            public static ConfigParam NoHardwareNames
            {
                get
                {
                    return new ConfigParam() { Key = ParamKey.HardwareNames, Value = false };
                }
            }

            public static ConfigParam UseFahrenheit
            {
                get
                {
                    return new ConfigParam() { Key = ParamKey.UseFahrenheit, Value = false };
                }
            }

            public static ConfigParam AllCoreClocks
            {
                get
                {
                    return new ConfigParam() { Key = ParamKey.AllCoreClocks, Value = false };
                }
            }

            public static ConfigParam CoreLoads
            {
                get
                {
                    return new ConfigParam() { Key = ParamKey.CoreLoads, Value = true };
                }
            }

            public static ConfigParam TempAlert
            {
                get
                {
                    return new ConfigParam() { Key = ParamKey.TempAlert, Value = 0 };
                }
            }

            public static ConfigParam DriveDetails
            {
                get
                {
                    return new ConfigParam() { Key = ParamKey.DriveDetails, Value = false };
                }
            }

            public static ConfigParam UsedSpaceAlert
            {
                get
                {
                    return new ConfigParam() { Key = ParamKey.UsedSpaceAlert, Value = 0 };
                }
            }

            public static ConfigParam BandwidthInAlert
            {
                get
                {
                    return new ConfigParam() { Key = ParamKey.BandwidthInAlert, Value = 0 };
                }
            }

            public static ConfigParam BandwidthOutAlert
            {
                get
                {
                    return new ConfigParam() { Key = ParamKey.BandwidthOutAlert, Value = 0 };
                }
            }

            public static ConfigParam UseBytes
            {
                get
                {
                    return new ConfigParam() { Key = ParamKey.UseBytes, Value = false };
                }
            }
        }
    }

    [Serializable]
    public enum ParamKey : byte
    {
        HardwareNames,
        UseFahrenheit,
        AllCoreClocks,
        CoreLoads,
        TempAlert,
        DriveDetails,
        UsedSpaceAlert,
        BandwidthInAlert,
        BandwidthOutAlert,
        UseBytes
    }

    public enum DataType : byte
    {
        Clock,
        Voltage,
        Percent,
        RPM,
        Celcius,
        Fahrenheit,
        Gigabyte
    }

    public class CelciusToFahrenheit
    {
        private CelciusToFahrenheit() { }

        public void Convert(ref double value)
        {
            value = value * 1.8 + 32;
        }

        public DataType TargetType
        {
            get
            {
                return DataType.Fahrenheit;
            }
        }

        private static CelciusToFahrenheit _instance { get; set; }

        public static CelciusToFahrenheit Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CelciusToFahrenheit();
                }

                return _instance;
            }
        }
    }

    public static class Data
    {
        public static void MinifyKiloBytesPerSecond(ref double input, out string format)
        {
            if (input < 1024d)
            {
                format = "kB/s";
                return;
            }
            else if (input < 1048576d)
            {
                input /= 1024d;
                format = "MB/s";
                return;
            }
            else
            {
                input /= 1048576d;
                format = "GB/s";
                return;
            }
        }

        public static void MinifyKiloBitsPerSecond(ref double input, out string format)
        {
            if (input < 1024d)
            {
                format = "kbps";
                return;
            }
            else if (input < 1048576d)
            {
                input /= 1024d;
                format = "Mbps";
                return;
            }
            else
            {
                input /= 1048576d;
                format = "Gbps";
                return;
            }
        }
    }

    public static class Extensions
    {
        public static string GetDescription(this MonitorType type)
        {
            switch (type)
            {
                case MonitorType.CPU:
                    return "CPU";

                case MonitorType.RAM:
                    return "RAM";

                case MonitorType.GPU:
                    return "GPU";

                case MonitorType.HD:
                    return "Drives";

                case MonitorType.Network:
                    return "Network";

                default:
                    return "Unknown";
            }
        }

        public static T GetValue<T>(this ConfigParam[] parameters, ParamKey key)
        {
            return (T)parameters.Single(p => p.Key == key).Value;
        }

        public static string GetAppend(this DataType type)
        {
            switch (type)
            {
                case DataType.Clock:
                    return " MHz";

                case DataType.Voltage:
                    return " V";

                case DataType.Percent:
                    return "%";

                case DataType.RPM:
                    return " RPM";

                case DataType.Celcius:
                    return " C";

                case DataType.Fahrenheit:
                    return " F";

                case DataType.Gigabyte:
                    return " GB";

                default:
                    return "";
            }
        }
    }
}