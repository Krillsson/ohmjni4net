using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace OHMWrapper
{

    public class NetworkMonitor
    {
        internal const string CATEGORYNAME = "Network Interface";

        public NetworkMonitor()
        {

            string[] _instances = new PerformanceCounterCategory(CATEGORYNAME).GetInstanceNames();

            NetworkInterface[] _nics = NetworkInterface.GetAllNetworkInterfaces().Where(n =>
                n.OperationalStatus == OperationalStatus.Up &&
                new NetworkInterfaceType[2] { NetworkInterfaceType.Ethernet, NetworkInterfaceType.Wireless80211 }.Contains(n.NetworkInterfaceType)
                ).ToArray();

            Regex _regex = new Regex("[^A-Za-z]");

            Nics = _instances.Join(_nics, i => _regex.Replace(i, ""), n => _regex.Replace(n.Description, ""), (i, n) => new NicInfo(i, n.Description, string.Join(":", n.GetPhysicalAddress().GetAddressBytes().Select(b => b.ToString("X2")))), StringComparer.Ordinal).ToArray();
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

        public NicInfo(string instance, string name, string physicalAddress)
        {
            Instance = instance;
            Name = name;
            PhysicalAddress = physicalAddress;
            InBandwidth = new Bandwidth(
                new PerformanceCounter(NetworkMonitor.CATEGORYNAME, BYTESRECEIVEDPERSECOND, instance),
                "In"
                );

            OutBandwidth = new Bandwidth(
                new PerformanceCounter(NetworkMonitor.CATEGORYNAME, BYTESSENTPERSECOND, instance),
                "Out"
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
        public string PhysicalAddress { get; private set; }

        public Bandwidth InBandwidth { get; private set; }

        public Bandwidth OutBandwidth { get; private set; }
    }

    public class Bandwidth : IDisposable
    {
        public Bandwidth(PerformanceCounter counter, string label)
        {
            _counter = counter;

            Label = label;
        }

        public void Update()
        {
            Value = _counter.NextValue() / 128d;
        }

        public void Dispose()
        {
            if (_counter != null)
            {
                _counter.Dispose();
            }
        }

        public string Label { get; private set; }
        public double Value { get; private set; }

        public string Text()
        {
            string _format;
            double value = Value;
            Data.MinifyKiloBitsPerSecond(ref value, out _format);
            return string.Format("{0}: {1:#,##0.##} {2}", Label, value, _format);
        }

        private PerformanceCounter _counter { get; set; }
    }
}