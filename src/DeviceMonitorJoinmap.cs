using System;
using System.Collections.Generic;
using PepperDash.Essentials.Core;

namespace epi.utilities.deviceMonitor
{
    public class DeviceMonitorJoinMapAdvanced : JoinMapBaseAdvanced
    {
        public DeviceMonitorJoinMapAdvanced(uint joinStart, Dictionary<string, MonitoredSimplDevice> simplDevices, Dictionary<string, MonitoredEssentialsDevice> essentialsDevices)
            : base(joinStart, typeof(DeviceMonitorJoinMapAdvanced))
        {
            var simplMonitorDevices = simplDevices;
            var essentialsMonitorDevices = essentialsDevices;
            foreach (var device in simplMonitorDevices)
            {
                var item = device.Value;
                var key = device.Key;
                var name = String.Format("DeviceMonitor--{0}", key);

                var joinData = new JoinData()
                {
                    JoinNumber = item.JoinNumber + joinStart - 1,
                    JoinSpan = 1
                };

                var joinMetaData = new JoinMetadata
                {
                    Description = String.Format("{0} :: Device Monitor Join :: SIMPL", item.Name),
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.DigitalAnalogSerial
                };

                Joins.Add(name, new JoinDataComplete(joinData, joinMetaData));
            }
            foreach (var device in essentialsMonitorDevices)
            {
                var item = device.Value;
                var key = device.Key;
                var name = String.Format("DeviceMonitor--{0}", key);

                var joinData = new JoinData()
                {
                    JoinNumber = item.JoinNumber + joinStart - 1,
                    JoinSpan = 1
                };

                var joinMetaData = new JoinMetadata
                {
                    Description = String.Format("{0} :: Device Monitor Join :: ESSENTIALS", item.Name),
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.DigitalAnalogSerial
                };

                Joins.Add(name, new JoinDataComplete(joinData, joinMetaData));
            }
        }
    }
}