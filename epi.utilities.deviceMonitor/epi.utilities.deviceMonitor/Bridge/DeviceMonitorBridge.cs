using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common;
using PepperDash.Essentials.Bridges;

using Newtonsoft.Json;



namespace epi.utilities.deviceMonitor
{
    public static class DeviceMonitorApiExtensions
    {
        public static void LinkToApiExt(this DeviceMonitor monitorDevice, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            DeviceMonitorJoinMap joinMap = new DeviceMonitorJoinMap();
            var joinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
            {
                joinMap = JsonConvert.DeserializeObject<DeviceMonitorJoinMap>(joinMapSerialized);
            }

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.Console(0, "Linking to DeviceMonitor: {0}", monitorDevice.Name);

            foreach (var item in monitorDevice.MonitoredSimplDevices)
            {
                var device = item;
                var join = joinMap.MultipurposeJoin + device.JoinNumber;

                if(item.JoinNumber != uint.MaxValue) {
                    if (item.MonitorType == eMonitoringType.serial)
                    {
                        Debug.Console(2, monitorDevice, "{0} is Serial Monitored on join {1}", device.Name, join);
                        trilist.SetStringSigAction(join, (s) => device.StopTimerSerial());
                    }
                    else if (item.MonitorType == eMonitoringType.digital)
                    {
                        Debug.Console(2, monitorDevice, "{0} is Digital Monitored on join {1}", device.Name, join);
                        trilist.SetBoolSigAction(join, device.DeviceOnline);
                    }

                    device.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[join]);
                    device.StatusFeedback.LinkInputSig(trilist.UShortInput[join]);
                    device.NameFeedback.LinkInputSig(trilist.StringInput[join]);
                }
            }
            foreach (var item in monitorDevice.MonitoredEssentialsDevices)
            {
                Debug.Console(2, monitorDevice, "Linking Bridge to Essentials Device : {0}", item.StatusMonitor.Parent.Key);
                var device = item;
                var join = joinMap.MultipurposeJoin + device.JoinNumber;

                device.StatusMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[join]);
                device.StatusFeedback.LinkInputSig(trilist.UShortInput[join]);
                device.NameFeedback.LinkInputSig(trilist.StringInput[join]);
            }
            trilist.OnlineStatusChange += new Crestron.SimplSharpPro.OnlineStatusChangeEventHandler((d, args) =>
                {
                    if (args.DeviceOnLine)
                    {
                        foreach (var device in monitorDevice.MonitoredSimplDevices)
                        {
                            var join = joinMap.MultipurposeJoin + device.JoinNumber;

                            device.IsOnlineFeedback.FireUpdate();
                            device.StatusFeedback.FireUpdate();
                            device.NameFeedback.FireUpdate();
                        }

                        foreach (var device in monitorDevice.MonitoredEssentialsDevices)
                        {
                            var join = joinMap.MultipurposeJoin + device.JoinNumber;

                            device.StatusMonitor.IsOnlineFeedback.FireUpdate();
                            device.StatusFeedback.FireUpdate();
                            device.NameFeedback.FireUpdate();
                        }

                    }
                }
            );

        }

    }

    public class DeviceMonitorJoinMap : JoinMapBase
    {
        #region Digitals

        public uint MultipurposeJoin { get; set; }

        #endregion

        public DeviceMonitorJoinMap()
        {
            MultipurposeJoin = 1;
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;
            MultipurposeJoin += joinOffset;
        }

    }
}