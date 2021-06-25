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
        public DeviceMonitorProperties _Props;
		public List<IKeyed> DevicesWithLogs;
		public Dictionary<string, MonitoredSimplDevice> MonitoredSimplDevices = new Dictionary<string, MonitoredSimplDevice>();
		public Dictionary<string, MonitoredEssentialsDevice> MonitoredEssentialsDevices = new Dictionary<string, MonitoredEssentialsDevice>();
        private CMutex writeWait = new CMutex();
        private bool writeLock = false;
        private bool activateComplete = false;

        public DeviceMonitor(string key, string name, DeviceConfig dc)
            : base(key)
        {
            _Dc = dc;
            _Props = JsonConvert.DeserializeObject<DeviceMonitorProperties>(dc.Properties.ToString());
            Debug.Console(2, this, "Made it to Device Constructor");
			DevicesWithLogs = new List<IKeyed>(); 
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

            Debug.Console(2, this, "Creating DeviceMonitor Links");
			foreach (var item in _Props.Devices)
			{
				if (item.Value.deviceKey != null)
				{
					Debug.Console(2, this, "Creating Essentials Device : {0}", item.Value.deviceKey);
					Device newDevice = DeviceManager.GetDeviceForKey(item.Value.deviceKey) as Device; 

					if (newDevice == null)
					{
						Debug.Console(0, Debug.ErrorLogLevel.Error, "DeviceMonitor -- Device with Key:{0} Does not exists", item.Value.deviceKey);
						continue;
					}
					var newStatusMonitorBase = newDevice as ICommunicationMonitor;
					var StatusMonitor = newStatusMonitorBase.CommunicationMonitor;
					if (newStatusMonitorBase == null)
					{
						Debug.Console(0, Debug.ErrorLogLevel.Error, "DeviceMonitor -- Device {0} Does not support ICommunicationMonitor", item.Value.deviceKey);
						continue;
					}
					var monitoredDevice = new MonitoredEssentialsDevice(item.Value, newDevice, newStatusMonitorBase);
					MonitoredEssentialsDevices.Add(item.Key, monitoredDevice);
					monitoredDevice.StatusMonitor.StatusChange += new EventHandler<MonitorStatusChangeEventArgs>(StatusMonitor_StatusChange);
		
				}
				else
				{
					Debug.Console(2, this, "Creating Simpl Device : {0}", item.Value.name);
					var monitoredDevice = new MonitoredSimplDevice(item.Value);
					MonitoredSimplDevices.Add(item.Key, monitoredDevice);
					monitoredDevice.StatusChangeEvent += new EventHandler<EventArgs>(monitoredDevice_StatusChangeEvent);
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

            AddPostActivationAction(() =>
            {
                activateComplete = true;
                MakeDeviceErrorString();
            });

			return base.CustomActivate();
        }

        private void MakeDeviceErrorString()
        {
            try
            {
                if (!writeLock && activateComplete)
                {
                    writeLock = true;
                    if (writeWait.WaitForMutex())
                    {
                        writeLock = false;
                        WriteLog(null);
                        CrestronEnvironment.Sleep(30000);
                        writeWait.ReleaseMutex();
                    }
                }
            }
            catch
            {
                writeWait.ReleaseMutex();
                writeLock = false;
            }
        }

		private void WriteLog(object o)
		{
			int count = 0;
			var deviceString = "Room OK";
			Debug.ErrorLogLevel status = Debug.ErrorLogLevel.None;
			foreach (var item in MonitoredSimplDevices.Values)
			{
				if (item.UseInRoomHealth && item.Status == eDeviceStatus.error)
				{
					if (count == 0)
					{
						deviceString = item.Name;
					}
					else
					{
						deviceString += string.Format(", {0}", item.Name);
					}
					count++;
				}
			}
			foreach (var item in MonitoredEssentialsDevices.Values)
			{
				if (item.UseInRoomHealth && item.Status == eDeviceStatus.error)
				{
					if (count == 0)
					{
						deviceString = item.Name;
					}
					else
					{
						deviceString += string.Format(", {0}", item.Name);
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
			
			foreach (var item in this.MonitoredSimplDevices.Values)
			{
				var device = item;
				var join = joinMap.MultipurposeJoin + device.JoinNumber;

				if (item.JoinNumber != uint.MaxValue)
				{
					Debug.Console(2, this, "Linking Bridge to Simpl Device : {0}", item.Name);
					trilist.SetStringSigAction(join, (s) => device.StopTimerSerial());
					trilist.SetBoolSigAction(join, device.DeviceOnline);
					device.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[join]);
					device.StatusFeedback.LinkInputSig(trilist.UShortInput[join]);
					device.NameFeedback.LinkInputSig(trilist.StringInput[join]);
				}
			}
			foreach (var item in this.MonitoredEssentialsDevices.Values)
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
					foreach (var device in this.MonitoredSimplDevices.Values)
					{
						var join = joinMap.MultipurposeJoin + device.JoinNumber;

						device.IsOnlineFeedback.FireUpdate();
						device.StatusFeedback.FireUpdate();
						device.NameFeedback.FireUpdate();
					}

					foreach (var device in this.MonitoredEssentialsDevices.Values)
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


		#region IPower Members

		public BoolFeedback PowerIsOnFeedback
		{
			get { throw new NotImplementedException(); }
		}

		public void PowerOff()
		{
			throw new NotImplementedException();
		}

		public void PowerOn()
		{
			throw new NotImplementedException();
		}

		public void PowerToggle()
		{
			throw new NotImplementedException();
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

