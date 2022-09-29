
using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace epi.utilities.deviceMonitor
{
    public class DeviceMonitorFactory : EssentialsPluginDeviceFactory<DeviceMonitor>
    {
        public DeviceMonitorFactory()
        {
            // Set the minimum Essentials Framework Version
            MinimumEssentialsFrameworkVersion = "1.8.4";

            // In the constructor we initialize the list with the typenames that will build an instance of this device
            TypeNames = new List<string> { "devicemonitor" };
        }

        // Builds and returns an instance of EssentialsPluginDeviceTemplate
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new device from type: {0}", dc.Type);

            return new DeviceMonitor(dc.Key, dc.Name, dc);
        }
    }
}