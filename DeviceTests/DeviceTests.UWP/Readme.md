**Device Tests - UWP**

The UWP device tests can be run by setting the DeviceTest.UWP project as the startup project and running from within Visual Studio.

Alternatively the UWP device tests can be run using dotnet cake as follows:
- Installing dotnet Cake (https://www.nuget.org/packages/Cake.Tool) 

*dotnet tool install --global Cake.Tool --version 0.38.5*

- Open command prompt as Administrator (required to install/uninstall UWP apps)
- Go to \DeviceTests folder
- Run devices test on uwp

*dotnet cake build.cake -target=test-uwp-emu*

Note: if there are errors relating to the installation of dependencies (namely that a higher version is already installed), these can safely be ignored as they won't impact the running of the tests.


