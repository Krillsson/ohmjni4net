# OpenHardwareMonitor + JNI4Net
This project enables the usage of OpenHardwareMonitorLib.dll on Java. OpenHardwareMonitor mainly uses visitor pattern
in it's API. Which is great, but not so great when you try to use it with JNI4Net. On top of that JNI4Net currently does not support generics.
Therefore, this project simply wraps OpenHardwareMonitor and exposes an API such as: GetCpu, GetGpu and GetDisks.

## Building the C# library
This should be as easy as any other C# library project. There is a simple console based application that you can use
to quickly test out stuff.

## Building the Proxy .dll and .jar files using Proxygen
Now here's the tricky part. Since there are so many different factors at play.

Proxygen works with either .NET 2.0 or .NET 4.0. So make sure you have the right version of the csc compiler. My Windows 10 machine came
with v4.0.30319 and the csc executable was located at `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`

Also make sure you have a new JDK installed. JDK8 is fine.

Take a look at the Proxygen directory in this repo. Due to licensing and practical reasons, a few binaries 
are missing from this directory. You can find them over at [JNI4Net's repo](https://github.com/jni4net) and [Open Hardware Monitor's repo](https://github.com/openhardwaremonitor/openhardwaremonitor)
respectively.

- OhmJniWrapper.dll (the artifact from this repo)
- proxygen.exe
- proxygen.exe.config
- OpenHardwareMonitorLib.dll
- jni4net.n-0.8.8.0.dll
- jni4net.n.w32.v20-0.8.8.0.dl
- jni4net.n.w64.v20-0.8.8.0.dll
- jni4net.n.w32.v40-0.8.8.0.dll
- jni4net.n.w64.v40-0.8.8.0.dll
- jni4net.j-0.8.8.0.jar

Once you have downloaded all the required binaries, you can run the `generateProxies.cmd`. It should generate a bunch of files in the work directory.

The file of interest is `build.cmd` which is used to run the final product. This file contains calls to both the java compiler, the jar packager
as well as the C# compiler. A reference to `OpenHardwareMonitorLib.dll` also need to be added to the .NET compiler call.

If you are like me and don't have a proper path setup in Windows you probably also need to provide absolute paths to the compilers.

I've included a `build.cmd.example` for reference where I've switched out the executable calls to their absolute path versions as well as added the reference to `OpenHardwareMonitorLib.dll`
in the call to `csc.exe`.

Once it's built you will end up with two files: `OhmJniWrapper.j4n.jar` and `OhmJniWrapper.j4n.dll`. Stick those together with
the files listed above except `proxygen.exe` and `proxygen.exe.config` in a lib folder in your java project.

Take a look at [WindowsInfoProvider](https://github.com/Krillsson/sys-API/blob/master/server/src/main/java/com/krillsson/sysapi/core/windows/WindowsInfoProvider.java) to find out how to use the lib from Java.