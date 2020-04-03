using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.DSP;
using System.Text.RegularExpressions;
using Crestron.SimplSharp.Reflection;
using Newtonsoft.Json;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Bridges;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.Diagnostics;

namespace epi.utilities.deviceMonitor
{
    public class DeviceMonitor : Device, IBridge
    {
        DeviceConfig _Dc;

        public event EventHandler<ErrorArgs> ErrorEvent;

        public DeviceMonitorProperties _Props;
        public List<MonitoredSimplDevice> MonitoredSimplDevices;
        public List<MonitoredEssentialsDevice> MonitoredEssentialsDevices;

        public static void LoadPlugin()
        {
            DeviceFactory.AddFactoryForType("devicemonitor", DeviceMonitor.BuildDevice);
        }

        public static string MinimumEssentialsFrameworkVersion = "1.4.32";


        public static DeviceMonitor BuildDevice(DeviceConfig dc)
        {
            var newMe = new DeviceMonitor(dc.Key, dc.Name, dc);
            return newMe;
        }

        public DeviceMonitor(string key, string name, DeviceConfig dc)
            : base(key)
        {
            _Dc = dc;
            _Props = JsonConvert.DeserializeObject<DeviceMonitorProperties>(dc.Properties.ToString());
            Debug.Console(2, this, "Made it to Device Constructor");
            MonitoredSimplDevices = new List<MonitoredSimplDevice>();

            foreach (var item in _Props.SimplDevices)
            {

                var monitoredDevice = new MonitoredSimplDevice(item);
                MonitoredSimplDevices.Add(monitoredDevice);
                monitoredDevice.StatusChangeEvent += new EventHandler<EventArgs>(monitoredDevice_StatusChangeEvent);
            }
            AddPostActivationAction(BuildEssentialsDevices);   
        }

        void StatusMonitor_StatusChange(object sender, MonitorStatusChangeEventArgs e)
        {
            MakeDeviceErrorString();
        }

        void monitoredDevice_StatusChangeEvent(object sender, EventArgs e)
        {
            MakeDeviceErrorString();
        }

        void BuildEssentialsDevices()
        {
            Debug.Console(2, this, "Linking To Essentials Devices");
            MonitoredEssentialsDevices = new List<MonitoredEssentialsDevice>();
            foreach (var item in _Props.EssentialsDevices)
            {
                Debug.Console(2, this, "Linking to Essentials Device : {0}", item.deviceKey);
                var monitoredDevice = new MonitoredEssentialsDevice(item);
                if (monitoredDevice.JoinNumber != int.MaxValue)
                {
                    MonitoredEssentialsDevices.Add(monitoredDevice);
                    monitoredDevice.StatusMonitor.StatusChange += new EventHandler<MonitorStatusChangeEventArgs>(StatusMonitor_StatusChange);
                }
            }
        }

        private void MakeDeviceErrorString()
        {
            int count = 0;
            var deviceString = "";
            foreach (var item in MonitoredSimplDevices)
            {
                if (item.UseInRoomHealth && item.Status == eDeviceStatus.error)
                {
                    if (count == 0)
                    {
                        deviceString = item.Name;
                    }
                    else
                    {
                        deviceString += string.Format(",{0}", item.Name);
                    }
                    count++;
                }
            }
            foreach (var item in MonitoredEssentialsDevices)
            {
                if (item.UseInRoomHealth && item.Status == eDeviceStatus.error)
                {
                    if (count == 0)
                    {
                        deviceString = item.Name;
                    }
                    else
                    {
                        deviceString += string.Format(",{0}", item.Name);
                    }
                    count++;
                }
            }
            
            var tempErrorMessage = "";
            if (count > 0)
            {
                tempErrorMessage = string.Format("2: Error! {0} offline.", deviceString);
            }
            else
            {
                tempErrorMessage = "0:Room OK";
            }
            var handler = ErrorEvent;
            if (handler != null)
            {
                handler(this, new ErrorArgs(tempErrorMessage));
            }
            
        }


        #region IBridge Members

        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            this.LinkToApiExt(trilist, joinStart, joinMapKey);
        }

        #endregion
    }
    public class ErrorArgs : EventArgs
    {
        public string DeviceError;
        public ErrorArgs(string data)
        {
            DeviceError = data;
        }
    }

    public enum eMonitoringType : int
    {
        serial = 0,
        digital
    };
    public enum eDeviceStatus : int
    {
        unknown = 0,
        warning = 1,
        error = 2,
        ok = 3
    };

    
    
}

