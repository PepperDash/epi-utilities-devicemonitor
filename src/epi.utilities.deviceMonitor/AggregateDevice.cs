using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace epi.utilities.deviceMonitor
{


    public class AggregateDevice : IKeyName, IOnline
    {
        private List<IOnline> onlineDevices = new List<IOnline>();
        private readonly IEnumerable<string> Devices;
        private readonly DeviceMonitor DeviceMonitor;
        public readonly uint JoinNumber;
        public AggregateDevice(string key, string name, IEnumerable<string> devices, uint joinNumber, DeviceMonitor deviceMonitor)
        {
            Key = key;
            Name = name;
            Devices = devices;
            DeviceMonitor = deviceMonitor;
            JoinNumber = joinNumber;
            NameFeedback = new StringFeedback(() => Name);
        }

        public void LinkToDevices()
        {
            foreach (var device in Devices)
            {
                var item = device;
                IOnline monitoredDevice;
                if (!DeviceMonitor.AllMonitoredDevices.TryGetValue(item, out monitoredDevice)) continue;
                onlineDevices.Add(monitoredDevice);
            }
            IsOnline = new BoolFeedback(() => onlineDevices.All(o => o.IsOnline.BoolValue));
            foreach (var item in onlineDevices)
            {
                var device = item;
                device.IsOnline.OutputChange += (s, a) => IsOnline.FireUpdate();
            }

        }


        public string Name { get; private set; }
        public string Key { get; private set; }
        public BoolFeedback IsOnline { get; private set; }
        public StringFeedback NameFeedback { get; set; }

    }
}