using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PortableOHM;

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
                                if (gpu.Fan != null)
                                {
                                    Console.WriteLine(gpu.Fan.Text());
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
                        break;
                    case ConsoleKey.Q:
                        System.Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("1: Testgames();\n2:TestVideos();\n3: PrintTitleOfGame();\nQ: Quit");
                        break;
                }

            }
            while (key != ConsoleKey.Q);




            // wait until we press a key so the screen won't disappear
            Console.ReadKey();
        }
    }
}
