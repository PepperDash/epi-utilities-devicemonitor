using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace epi.utilities.deviceMonitor
{
    public class DeviceMonitorProperties
    {
		public string DynFusionKey { get; set; }
		public List<string> LogToDeviceKeys; 
        public Dictionary<string, DeviceMonitorDevice> Devices { get; set; }


        public uint defaultTimeout { get; set; }

        public uint essentialsDevicesJoinOffset { get; set; }

    }

    public class DeviceMonitorDevice
    {
        public string name { get; set; }
        public uint joinNumber { get; set; }
		public bool logToProcessor { get; set; }
		public bool logToDevices { get; set; }
        public uint warningTimeout { get; set; }
        public uint errorTimeout { get; set; }
		public string deviceKey { get; set; }
    }


}