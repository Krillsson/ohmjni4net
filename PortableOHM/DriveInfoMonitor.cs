using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace OHMWrapper
{
    public class DriveInfoMonitor
    {
        internal const string CATEGORYNAME = "LogicalDisk";

        public DriveInfoMonitor(ConfigParam[] parameters)
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

    public class DriveInfo : IDisposable
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

            _counterFreeMB = new PerformanceCounter(DriveInfoMonitor.CATEGORYNAME, FREEMB, name);
            _counterFreePercent = new PerformanceCounter(DriveInfoMonitor.CATEGORYNAME, PERCENTFREE, name);

            if (showDetails)
            {
                _counterReadRate = new PerformanceCounter(DriveInfoMonitor.CATEGORYNAME, BYTESREADPERSECOND, name);
                _counterWriteRate = new PerformanceCounter(DriveInfoMonitor.CATEGORYNAME, BYTESWRITEPERSECOND, name);
            }
        }

        public void Update()
        {
            if (!PerformanceCounterCategory.InstanceExists(Instance, DriveInfoMonitor.CATEGORYNAME))
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
            }
        }

        public bool ShowDetails { get; private set; }

        public double UsedSpaceAlert { get; private set; }

        private PerformanceCounter _counterFreeMB { get; set; }

        private PerformanceCounter _counterFreePercent { get; set; }

        private PerformanceCounter _counterReadRate { get; set; }

        private PerformanceCounter _counterWriteRate { get; set; }
    }


}