# OpenHardwareMonitor + JNI4Net
This project enables the usage of OpenHardwareMonitorLib.dll on Java. OpenHardwareMonitor uses a visitor pattern
as it's API which is great, but not so great when you try to use through JNI4Net. Therefore this project simply wraps
OpenHardwareMonitor and exposes an API such as: GetCpu, GetGpu and GetDisks.

## Building the C# library
This should be as easy as any other C# library project. There is a simple console based application that you can use
to quickly test out stuff.

## Building the Proxy .dll and .jar files using Proxygen
Now here's the tricky part since there are so many different factors in play.

Proxygen works with either .NET 2.0 or .NET 4.0. So make sure you have the csc compiler for that. My Windows 10 machine came
with v4.0.30319 and the csc executable was located at "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

Then make sure you have a new JDK installed. JDK8 is fine.

Take a look at the Proxygen directory in this repo. Due to licensing and practical reasons. There are a few binaries that
are missing from this directory. Which you can find over at [JNI4Net's repo](https://github.com/jni4net) and [Open Hardware Monitor's repo](https://github.com/openhardwaremonitor/openhardwaremonitor)
respectively.

- OhmJniWrapper.dll
- proxygen.exe
- proxygen.exe.config
- OpenHardwareMonitorLib.dll
- jni4net.n-0.8.8.0.dll
- jni4net.n.w32.v20-0.8.8.0.dl
- jni4net.n.w64.v20-0.8.8.0.dll
- jni4net.n.w32.v40-0.8.8.0.dll
- jni4net.n.w64.v40-0.8.8.0.dll
- jni4net.j-0.8.8.0.jar

And of course you also need to put the artifact from this repo in that directory. Once you have downloaded all the required binaries, you
can run the generateProxies.cmd. It should generate a bunch of files in the work directory.

The file of interest is a build.cmd which is used to run the final product. This file contains calls to both the java compiler, the jar package
as well as the .NET compiler. A reference to OpenHardwareMonitorLib.dll also need to be added to the .NET compiler call.

If you are like me and don't have a proper path setup in Windows you probably also need to provide absolute paths to the compilers.

I've included an build.cmd.example for reference

Once it's built you will end up with two files: OhmJniWrapper.j4n.jar and OhmJniWrapper.j4n.dll. Stick those together with
the files above in a lib folder in your java project (only the JNI4Net runtime not proxygen).

Take a look at [WindowsInfoProvider](https://github.com/Krillsson/sys-API/blob/master/server/src/main/java/com/krillsson/sysapi/core/windows/WindowsInfoProvider.java) to find out how to use the lib from Java