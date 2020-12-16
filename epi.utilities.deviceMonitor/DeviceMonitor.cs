using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Interfaces;
using PepperDash.Essentials.Core.Bridges;
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
	public class DeviceMonitor : EssentialsBridgeableDevice
    {
        DeviceConfig _Dc;

        public event EventHandler<ErrorArgs> ErrorEvent;
		private Action SendToDynFusion; 
        public DeviceMonitorProperties _Props;
		public List<IKeyed> DevicesWithLogs; 
        public List<MonitoredSimplDevice> MonitoredSimplDevices;
        public List<MonitoredEssentialsDevice> MonitoredEssentialsDevices;

        public static void LoadPlugin()
        {
            DeviceFactory.AddFactoryForType("devicemonitor", DeviceMonitor.BuildDevice);
        }

        public static string MinimumEssentialsFrameworkVersion = "1.6.10";


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
			DevicesWithLogs = new List<IKeyed>();
            foreach (var item in _Props.SimplDevices)
            {

                var monitoredDevice = new MonitoredSimplDevice(item);
                MonitoredSimplDevices.Add(monitoredDevice);
                monitoredDevice.StatusChangeEvent += new EventHandler<EventArgs>(monitoredDevice_StatusChangeEvent);
            }
            //AddPostActivationAction(BuildEssentialsDevices);   
        }

        void StatusMonitor_StatusChange(object sender, MonitorStatusChangeEventArgs e)
        {
            MakeDeviceErrorString();
        }

        void monitoredDevice_StatusChangeEvent(object sender, EventArgs e)
        {
            MakeDeviceErrorString();
        }

		public override bool CustomActivate()
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
			foreach (var key in _Props.LogToDeviceKeys)
			{
				Debug.Console(2, this, "Looking For Device with Log: {0}", key);
				var device = DeviceManager.GetDeviceForKey(key);
				if (device != null)
				{
					Debug.Console(2, this, "Found Device: {0}", key);
					DevicesWithLogs.Add(device);
				}
				
			}

			return base.CustomActivate();
        }

        private void MakeDeviceErrorString()
        {
            int count = 0;
			var deviceString = "Room OK";
			Debug.ErrorLogLevel status = Debug.ErrorLogLevel.None;
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
				status = Debug.ErrorLogLevel.Error;
                tempErrorMessage = string.Format("Error! {1} offline.", status, deviceString);
            }
            else
            {
                tempErrorMessage = "Room OK";
            }
			Debug.Console(2, this, tempErrorMessage);
			
			if (DevicesWithLogs.Count > 0)
			{
				foreach (var device in DevicesWithLogs)
				{
					var logCapableDevice = device as ILogStringsWithLevel;
					if (logCapableDevice != null)
					{
						logCapableDevice.SendToLog(this, status, tempErrorMessage);
					}
				}
			}
			 
            var handler = ErrorEvent;
            if (handler != null)
            {
                handler(this, new ErrorArgs(tempErrorMessage));
            }
            
        }


		public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
		{
			DeviceMonitorJoinMap joinMap = new DeviceMonitorJoinMap();
			var joinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

			if (!string.IsNullOrEmpty(joinMapSerialized))
			{
				joinMap = JsonConvert.DeserializeObject<DeviceMonitorJoinMap>(joinMapSerialized);
			}

			joinMap.OffsetJoinNumbers(joinStart);

			Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
			Debug.Console(0, "Linking to DeviceMonitor: {0}", this.Name);

			foreach (var item in this.MonitoredSimplDevices)
			{
				var device = item;
				var join = joinMap.MultipurposeJoin + device.JoinNumber;

				if (item.JoinNumber != uint.MaxValue)
				{
					if (item.MonitorType == eMonitoringType.serial)
					{
						Debug.Console(2, this, "{0} is Serial Monitored on join {1}", device.Name, join);
						trilist.SetStringSigAction(join, (s) => device.StopTimerSerial());
					}
					else if (item.MonitorType == eMonitoringType.digital)
					{
						Debug.Console(2, this, "{0} is Digital Monitored on join {1}", device.Name, join);
						trilist.SetBoolSigAction(join, device.DeviceOnline);
					}

					device.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[join]);
					device.StatusFeedback.LinkInputSig(trilist.UShortInput[join]);
					device.NameFeedback.LinkInputSig(trilist.StringInput[join]);
				}
			}
			foreach (var item in this.MonitoredEssentialsDevices)
			{
				Debug.Console(2, this, "Linking Bridge to Essentials Device : {0}", item.Name);
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
					foreach (var device in this.MonitoredSimplDevices)
					{
						var join = joinMap.MultipurposeJoin + device.JoinNumber;

						device.IsOnlineFeedback.FireUpdate();
						device.StatusFeedback.FireUpdate();
						device.NameFeedback.FireUpdate();
					}

					foreach (var device in this.MonitoredEssentialsDevices)
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

