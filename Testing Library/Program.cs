using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OHMWrapper;

namespace Testing_Library
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            OHMManagerFactory factory = new OHMManagerFactory();
            factory.init();
            MonitorManager monitorManager = factory.GetManager();
            monitorManager.Update();
            Console.WriteLine("================PortableOHM==========================");
            Console.WriteLine("1: CPU");
            Console.WriteLine("2: GPU");
            Console.WriteLine("3: RAM");
            Console.WriteLine("4: Network");
            Console.WriteLine("5: Drives");
            Console.WriteLine("6: Mainboard");
            Console.WriteLine("7: Network");
            Console.WriteLine("Q: Quit");
            Console.WriteLine("");
            Console.WriteLine("=================END=====OF====SYSTEM=================");
            ConsoleKey key;
            do
            {
                key = Console.ReadKey().Key;
                switch (key)
                {
                    case ConsoleKey.D1:
                        do
                        {
                            Console.Clear();
                            monitorManager.Update();
                            foreach (CpuMonitor cpu in monitorManager.CpuMonitors())
                            {
                                Console.WriteLine(cpu.Name);
                                cpu.CoreClocks.ToList().ForEach(o => Console.WriteLine(o.Text()));
                                cpu.CoreLoads.ToList().ForEach(o => Console.WriteLine(o.Text()));
                                if (cpu.PackageTemperature != null)
                                {
                                    Console.WriteLine(cpu.PackageTemperature.Text());
                                }
                                if (cpu.Temperatures != null && cpu.Temperatures.Length > 0)
                                {
                                    cpu.Temperatures.ToList().ForEach(o => Console.WriteLine(o.Text()));
                                }
                                if (cpu.FanRPM != null)
                                {
                                    Console.WriteLine(cpu.FanRPM.Text());
                                }
                                if (cpu.FanPercent != null)
                                {
                                    Console.WriteLine(cpu.FanPercent.Text());
                                }
                                if (cpu.Voltage != null)
                                {
                                    Console.WriteLine(cpu.Voltage.Text());
                                }

                            }
                            Thread.Sleep(2000);
                        } while (key != ConsoleKey.Q);

                        break;
                    case ConsoleKey.D2:
                        do
                        {
                            Console.Clear();
                            monitorManager.Update();
                            foreach (GpuMonitor gpu in monitorManager.GpuMonitors())
                            {
                                if (gpu.CoreLoad != null)
                                {
                                    Console.WriteLine(gpu.CoreLoad.Text());
                                }
                                if (gpu.CoreClock != null)
                                {
                                    Console.WriteLine(gpu.CoreClock.Text());
                                }
                                if (gpu.FanRPM != null)
                                {
                                    Console.WriteLine(gpu.FanRPM.Text());
                                }
                                if (gpu.FanPercent != null)
                                {
                                    Console.WriteLine(gpu.FanPercent.Text());
                                }
                                if (gpu.MemoryClock != null)
                                {
                                    Console.WriteLine(gpu.MemoryClock.Text());
                                }
                                if (gpu.MemoryLoad != null)
                                {
                                    Console.WriteLine(gpu.MemoryLoad.Text());
                                }

                                if (gpu.Temperature != null)
                                {
                                    Console.WriteLine(gpu.Temperature.Text());
                                }
                                if (gpu.Voltage != null)
                                {
                                    Console.WriteLine(gpu.Voltage.Text());
                                }

                            }
                            Thread.Sleep(2000);
                        } while (key != ConsoleKey.Q);
                        break;
                    case ConsoleKey.D3:
                        do
                        {
                            Console.Clear();
                            monitorManager.Update();
                            RamMonitor ram = monitorManager.RamMonitor;
                            if (monitorManager.RamMonitor.Voltage != null)
                            {
                                Console.WriteLine(ram.Voltage.Text());
                            }
                            if (monitorManager.RamMonitor.Load != null)
                            {
                                Console.WriteLine(ram.Load.Text());
                            }
                            if (monitorManager.RamMonitor.Available != null)
                            {
                                Console.WriteLine(ram.Available.Text());
                            }
                            if (monitorManager.RamMonitor.Used != null)
                            {
                                Console.WriteLine(ram.Used.Text());
                            }
                            if (monitorManager.RamMonitor.Clock != null)
                            {
                                Console.WriteLine(ram.Clock.Text());
                            }
                            Thread.Sleep(2000);
                        } while (key != ConsoleKey.Q);
                        break;
                    case ConsoleKey.D4:
                        do
                        {
                            Console.Clear();
                            monitorManager.Update();
                            NetworkMonitor network = monitorManager.NetworkMonitor;
                            foreach (NicInfo nic in network.Nics)
                            {
                                Console.WriteLine(nic.Name);
                                Console.WriteLine(nic.InBandwidth.Text());
                                Console.WriteLine(nic.OutBandwidth.Text());
                                Console.WriteLine(nic.PhysicalAddress);
                               
                            }
                         
                            Thread.Sleep(2000);
                        } while (key != ConsoleKey.Q);
                        break;
                    case ConsoleKey.D5:
                        do
                        {
                            Console.Clear();
                            monitorManager.Update();
                            foreach (DriveMonitor drive in monitorManager.DriveMonitors())
                            {
                                if (drive.LogicalName != null)
                                {
                                    Console.WriteLine(drive.LogicalName);
                                }
                                if (drive.Temperature != null)
                                {
                                    Console.WriteLine(drive.Temperature.Text());
                                }
                                if (drive.RemainingLife != null)
                                {
                                    Console.WriteLine(drive.RemainingLife.Text());
                                }
                                if (drive.LifecycleData != null)
                                {
                                    drive.LifecycleData.ToList().ForEach(s => Console.WriteLine(s.Text()));    
                                }
                                double _readRate = drive.ReadRate;

                                string _readFormat;
                                Data.MinifyKiloBytesPerSecond(ref _readRate, out _readFormat);

                                String ReadRate = string.Format("Read: {0:#,##0.##} {1}", _readRate, _readFormat);

                                double _writeRate = drive.WriteRate;

                                string _writeFormat;
                                Data.MinifyKiloBytesPerSecond(ref _writeRate, out _writeFormat);
                                String WriteRate = string.Format("Write: {0:#,##0.##} {1}", _writeRate, _writeFormat);


                                Console.WriteLine(ReadRate);
                                Console.WriteLine(WriteRate);
                            }

                            Thread.Sleep(2000);
                        } while (key != ConsoleKey.Q);
                        break;

                    case ConsoleKey.D6:
                        do
                        {
                            Console.Clear();
                            monitorManager.Update();
                            MainboardMonitor mainboardMonitor = monitorManager.MainboardMonitor;
                            Console.WriteLine(mainboardMonitor.Name);
                            mainboardMonitor.BoardFanPercent.ToList().ForEach(s => Console.WriteLine(s.Text()));
                            mainboardMonitor.BoardFanRPM.ToList().ForEach(s => Console.WriteLine(s.Text()));
                            mainboardMonitor.BoardTemperatures.ToList().ForEach(s => Console.WriteLine(s.Text()));
                            mainboardMonitor.HddTemperatures.ToList().ForEach(s => Console.WriteLine(s.Text()));

                            Thread.Sleep(2000);
                        } while (key != ConsoleKey.Q);
                        break;

                    case ConsoleKey.D7:
                        do
                        {
                            Console.Clear();
                            monitorManager.Update();
                            foreach (NicInfo nic in monitorManager.NetworkMonitor.Nics)
                            {
                                if (nic.Name != null)
                                {
                                    Console.WriteLine(nic.Name);
                                }
                                if (nic.InBandwidth != null)
                                {
                                    Console.WriteLine(nic.InBandwidth.Text());
                                }
                                if (nic.OutBandwidth != null)
                                {
                                    Console.WriteLine(nic.OutBandwidth.Text());
                                }
                                if (nic.PhysicalAddress != null)
                                {
                                    Console.WriteLine(nic.PhysicalAddress);
                                }
                            }

                            Thread.Sleep(2000);
                        } while (key != ConsoleKey.Q);
                        break;


                    case ConsoleKey.Q:
                        System.Environment.Exit(0);
                        break;
                    default:
                        break;
                }

            }
            while (key != ConsoleKey.Q);




            // wait until we press a key so the screen won't disappear
            Console.ReadKey();
        }
    }
}
