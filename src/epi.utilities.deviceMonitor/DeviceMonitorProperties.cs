using System.Collections.Generic;
using Newtonsoft.Json;

namespace epi.utilities.deviceMonitor
{
    public class DeviceMonitorProperties
    {
        [JsonProperty("dynFusionKey")]
		public string DynFusionKey { get; set; }
        [JsonProperty("logToDeviceKeys")]
		public List<string> LogToDeviceKeys;
        [JsonProperty("devices")]
        public Dictionary<string, DeviceMonitorDevice> Devices { get; set; }

        [JsonProperty("defaultTimeout")]
        public uint DefaultTimeout { get; set; }
        [JsonProperty("essentialsDevicesJoinOffset")]
        public uint EssentialsDevicesJoinOffset { get; set; }

    }

    public class DeviceMonitorDevice
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("joinNumber")]
        public uint JoinNumber { get; set; }
        [JsonProperty("logToProcessor")]
		public bool LogToProcessor { get; set; }
        [JsonProperty("logToDevices")]
		public bool LogToDevices { get; set; }
        [JsonProperty("warningTimeout")]
        public uint WarningTimeout { get; set; }
        [JsonProperty("errorTimeout")]
        public uint ErrorTimeout { get; set; }
        [JsonProperty("deviceKey")]
		public string DeviceKey { get; set; }
    }


}