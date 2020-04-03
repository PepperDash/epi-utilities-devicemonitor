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
        public List<SimplDevice> SimplDevices { get; set; }
        public List<EssentialsDevice> EssentialsDevices { get; set; }


        public uint defaultTimeout { get; set; }

        public uint essentialsDevicesJoinOffset { get; set; }
    }

    public class SimplDevice
    {
        public string name { get; set; }
        public uint joinNumber { get; set; }
        public bool useInRoomHealth { get; set; }
        public uint warningTimeout { get; set; }
        public uint errorTimeout { get; set; }
        public string monitorType { get; set; }
    }

    public class EssentialsDevice
    {
        public string deviceKey { get; set; }
        public uint joinNumber { get; set; }
        public bool useInRoomHealth { get; set; }
    }
}