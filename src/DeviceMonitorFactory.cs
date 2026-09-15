
using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Plugins
{
    public class DeviceMonitorFactory : EssentialsPluginDeviceFactory<DeviceMonitor>
    {
        public DeviceMonitorFactory()
        {
            // Set the minimum Essentials Framework Version
            MinimumEssentialsFrameworkVersion = "3.0.0";

            // In the constructor we initialize the list with the typenames that will build an instance of this device
            TypeNames = new List<string> { "devicemonitor" };
        }

        // Builds and returns an instance of EssentialsPluginDeviceTemplate
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {            
            Debug.LogDebug("Factory Attempting to create new device from type: {0}", dc.Type);

            return new DeviceMonitor(dc.Key, dc.Name, dc);
        }
    }
}