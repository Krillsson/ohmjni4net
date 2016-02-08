using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using OpenHardwareMonitor.Hardware;

namespace OHMWrapper
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
            _driveMonitors = new List<DriveMonitor>();

            UpdateBoard();
            SetupMainboardMonitor();
            SetupDriveMonitors();
            SetupCpuMonitors();
            SetupRamMonitor();
            SetupGpuMonitors();
            SetupNetworkMonitor();
            SetupDriveMonitor();
        }

        public void Update()
        {
            UpdateBoard();

            OHMMonitorsList.ForEach(i => i.Update());
            _cpuMonitors.ForEach(i => i.Update());
            _gpuMonitors.ForEach(i => i.Update());
            _driveMonitors.ForEach(i => i.Update());


            MainboardMonitor.Update();
            RamMonitor.Update();
            NetworkMonitor.Update();
            DriveInfoMonitor.Update();
        }

        public void Dispose()
        {
            OHMMonitorsList.ForEach(i => i.Dispose());
            _cpuMonitors.ForEach(i => i.Dispose());
            _gpuMonitors.ForEach(i => i.Dispose());
            _driveMonitors.ForEach(i => i.Dispose());

            MainboardMonitor.Dispose();
            RamMonitor.Dispose();
            NetworkMonitor.Dispose();
            DriveInfoMonitor.Dispose();
        }

        private IEnumerable<IHardware> GetHardware(params HardwareType[] types)
        {
            return _computer.Hardware.Where(h => types.Contains(h.HardwareType));
        }

        private void SetupDriveMonitor()
        {
            DriveInfoMonitor = new DriveInfoMonitor();
        }

        private void SetupNetworkMonitor()
        {
            NetworkMonitor = new NetworkMonitor();
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

        private void SetupGpuMonitors()
        {
            HardwareType[] hardwareTypes = { HardwareType.GpuNvidia, HardwareType.GpuAti };
            foreach (IHardware _hardware in GetHardware(hardwareTypes))
            {
                _gpuMonitors.Add(new GpuMonitor(_hardware));
            }
        }

        private List<GpuMonitor> _gpuMonitors;

        public GpuMonitor[] GpuMonitors()
        {
            return _gpuMonitors.ToArray();
        }

        private void SetupCpuMonitors()
        {
            HardwareType[] hardwareTypes = { HardwareType.CPU };
            foreach (IHardware _hardware in GetHardware(hardwareTypes))
            {
                _cpuMonitors.Add(new CpuMonitor(_hardware, _board));
            }
        }

        private List<CpuMonitor> _cpuMonitors;

        public CpuMonitor[] CpuMonitors()
        {
            return _cpuMonitors.ToArray();
        }

        private void SetupDriveMonitors()
        {
            HardwareType[] hardwareTypes = { HardwareType.HDD };
            foreach (IHardware _hardware in GetHardware(hardwareTypes))
            {
                _driveMonitors.Add(new DriveMonitor(_hardware));
            }
        }

        private List<DriveMonitor> _driveMonitors;

        public DriveMonitor[] DriveMonitors()
        {
            return _driveMonitors.ToArray();
        }

        private void SetupRamMonitor()
        {
            HardwareType[] hardwareTypes = { HardwareType.RAM };
            RamMonitor = new RamMonitor(GetHardware(hardwareTypes[0]).FirstOrDefault(), _board);
        }

        private void SetupMainboardMonitor()
        {
            MainboardMonitor = new MainboardMonitor(_board);
        }


        public MainboardMonitor MainboardMonitor { get; private set; }
        public RamMonitor RamMonitor { get; private set; }
        public DriveInfoMonitor DriveInfoMonitor { get; private set; }
        public NetworkMonitor NetworkMonitor { get; private set; }

        private IComputer _computer { get; set; }

        private IHardware _board { get; set; }
    }

    public class OHMMonitor
    {
        public OHMMonitor(IHardware hardware)
        {
            Name = hardware.Name;

            ShowName = true;

            _hardware = hardware;

            UpdateHardware();
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

        protected virtual void UpdateHardware()
        {
            _hardware.Update();

            foreach (IHardware _subHardware in _hardware.SubHardware)
            {
                _subHardware.Update();
            }
        }

        public string Name { get; protected set; }

        public bool ShowName { get; protected set; }

        public OHMSensor[] Sensors { get; protected set; }

        protected IHardware _hardware { get; set; }
    }



    [Serializable]
    public enum MonitorType : byte
    {
        CPU,
        RAM,
        GPU,
        HD,
        Network,
        MainBoard
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