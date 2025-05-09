using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Interfaces;
using PepperDash.Essentials.Core.Bridges;
using Newtonsoft.Json;
using PepperDash.Essentials.Core.Config;
using Crestron.SimplSharpPro.DeviceSupport;

namespace epi.utilities.deviceMonitor
{
	public class DeviceMonitor : EssentialsBridgeableDevice
    {
        public event EventHandler<ErrorArgs> ErrorEvent;
        private readonly DeviceMonitorProperties _props;
		public List<IKeyed> DevicesWithLogs;
		public Dictionary<string, MonitoredSimplDevice> MonitoredSimplDevices = new Dictionary<string, MonitoredSimplDevice>();
		public Dictionary<string, MonitoredEssentialsDevice> MonitoredEssentialsDevices = new Dictionary<string, MonitoredEssentialsDevice>();
		public const long WriteTimeout = 60000;
		public static CTimer WriteTimer;
	    private readonly bool _overrideDigitalOutput;

        public DeviceMonitor(string key, string name, DeviceConfig dc)
            : base(key, name)
        {
            _props = JsonConvert.DeserializeObject<DeviceMonitorProperties>(dc.Properties.ToString());
            _overrideDigitalOutput = _props.OverrideDigitalOutputToVisibility;
            Debug.LogInformation(this, "Made it to Device Constructor");
			DevicesWithLogs = new List<IKeyed>(); 
        }

        void StatusMonitor_StatusChange(object sender, EventArgs e)
		{			
            MakeDeviceErrorString();
        }

		public override bool CustomActivate()
        {
			
            Debug.LogInformation(this, "Creating DeviceMonitor Links");
			foreach (var item in _props.Devices)
			{
				try
				{
					if (item.Value.DeviceKey != null)
					{
						Debug.LogInformation(this, "Creating Essentials Device : {0}", item.Value.DeviceKey);
						var newDevice = DeviceManager.GetDeviceForKey(item.Value.DeviceKey) as Device;

						if (newDevice == null)
						{						
							Debug.LogError("DeviceMonitor -- Device with Key:{0} Does not exist", item.Value.DeviceKey);
                            continue;
						}
					    var commMonitor = newDevice as ICommunicationMonitor;

					    if (commMonitor != null)
                        {
                            Debug.LogVerbose(this, "{0} is an iCommunicationMonitor", newDevice.Key);

                            var monitoredDevice = new MonitoredEssentialsDevice(item.Value, commMonitor, item.Key);
                            Debug.LogVerbose(this, "{0} has been built as a monitoredEssentialsDevice", commMonitor.CommunicationMonitor.Key);

					        MonitoredEssentialsDevices.Add(item.Key, monitoredDevice);
                            Debug.LogVerbose(this, "{0} has been Added as a monitoredEssentialsDevice", commMonitor.CommunicationMonitor.Key);
                            monitoredDevice.StatusMonitor.StatusChange += StatusMonitor_StatusChange;
                            Debug.LogVerbose(this, "{0} has been registered", commMonitor.CommunicationMonitor.Key);
                            continue;
					    }
					    var commBasic = newDevice as IBasicCommunication;
                        if (commBasic != null)
					    {
                            Debug.LogVerbose(this, "{0} is an iBasicCommunication", commBasic.Key);
					        if (item.Value.CommunicationMonitor == null)
					        {
					            Debug.LogVerbose(this, "No Valid CommunicationMonitor for {0}", commBasic.Key);
					            continue;
					        }

                            var newDeviceMonitor = new GenericCommunicationMonitor(commBasic, commBasic,
					            item.Value.CommunicationMonitor);
                            if (newDeviceMonitor == null)
                            {
                                Debug.LogVerbose(this, "Failed to build CommunicationMonitor for {0}", commBasic.Key);
                                continue;
                            }                       
                            var monitoredDevice = new MonitoredEssentialsDevice(item.Value, new OnDemandCommunicationMonitorDevice(newDeviceMonitor), item.Key);
					        if (monitoredDevice == null)
					        {
                                Debug.LogVerbose(this, "Failed to build OnDemandCommunicationMonitorDevice for {0}", newDeviceMonitor.Key);
                                continue;

					        }
                            MonitoredEssentialsDevices.Add(newDeviceMonitor.Key, monitoredDevice);
					        monitoredDevice.StatusMonitor.StatusChange += StatusMonitor_StatusChange;
					        continue;
					    }
						Debug.LogVerbose(this, "DeviceMonitor -- Device {0} Does not support ICommunicationMonitor", item.Value.DeviceKey);
                    }
					else
					{
						Debug.LogInformation(this, "Creating Simpl Device : {0}", item.Value.Name);
						var monitoredDevice = new MonitoredSimplDevice(item.Value);
						MonitoredSimplDevices.Add(item.Key, monitoredDevice);
						monitoredDevice.StatusChangeEvent += StatusMonitor_StatusChange;
					}
				}
				catch (Exception ex)
				{					
					Debug.LogError(ex, "DeviceMonitor -- Device {0} Does not support ICommunicationMonitor", item.Value.DeviceKey);									
                }
			}

		    if (_props.LogToDeviceKeys != null)
		    {
		        foreach (var key in _props.LogToDeviceKeys)
		        {
		            Debug.LogInformation(this, "Looking For Device with Log: {0}", key);
		            var device = DeviceManager.GetDeviceForKey(key);
		            if (device == null) continue;
		            Debug.LogInformation(this, "Found Device: {0}", key);
		            DevicesWithLogs.Add(device);
		        }
		    }

		    AddPostActivationAction(MakeDeviceErrorString);
			return base.CustomActivate();
        }

        private void MakeDeviceErrorString()
        
        {
            if (WriteTimer == null)
                WriteTimer = new CTimer(WriteLog, WriteTimeout);

            WriteTimer.Reset(WriteTimeout);

            Debug.LogDebug(this, "Log timer has been reset.");
        }


		private void WriteLog(object o)
		{
			var count = 0;
			var deviceString = "Room OK";
			var status = Debug.ErrorLogLevel.None;
			foreach (var item in MonitoredSimplDevices.Values.Where(item => item.UseInRoomHealth && item.Status == DeviceStatus.Error))
			{
                deviceString = count == 0 ? item.Name : string.Format(", {0}", item.Name);
                count++;
            }
			foreach (var item in MonitoredEssentialsDevices.Values.Where(item => item.UseInRoomHealth && item.Status == DeviceStatus.Error))
			{
			    deviceString = count == 0 ? item.Name : string.Format(", {0}", item.Name);
			    count++;
			}
		    var tempErrorMessage = "Room OK";
			if (count > 0)
			{
				status = Debug.ErrorLogLevel.Error;
				tempErrorMessage = string.Format("Error! {0} offline.", deviceString);
			}			
			Debug.LogInformation(this, tempErrorMessage);

            if (DevicesWithLogs.Count > 0)
			{
				foreach (var logCapableDevice in DevicesWithLogs.OfType<ILogStringsWithLevel>())
				{
				    logCapableDevice.SendToLog(this, status, tempErrorMessage);
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
            var joinMap = new DeviceMonitorJoinMapAdvanced(joinStart, MonitoredSimplDevices, MonitoredEssentialsDevices);
			var joinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

			if (!string.IsNullOrEmpty(joinMapSerialized))
			{
                joinMap = JsonConvert.DeserializeObject<DeviceMonitorJoinMapAdvanced>(joinMapSerialized);
			}

            bridge.AddJoinMap(Key, joinMap);

			Debug.LogDebug("Linking to Trilist '{0}'", trilist.ID.ToString("X"));
			Debug.LogVerbose("Linking to DeviceMonitor: {0}", Name);
			
            foreach (var item in MonitoredSimplDevices)
            {
                var key = item.Key;
	            var device = item.Value;

                //var join = joinMap.Joins[String.Format(String.Format("DeviceMonitor--{0}", device.Key))];
                JoinDataComplete joinData;

                if (!joinMap.Joins.TryGetValue(String.Format("DeviceMonitor--{0}", key), out joinData))
                {
                    Debug.LogVerbose(this, "could not get joinmap data for {0}", device.Key);
                    continue;
                }
                if (!joinData.Metadata.Description.EndsWith(":: SIMPL"))
                {
                    Debug.LogVerbose(this, "join data did not end with :: SIMPL ", joinData.Metadata.Description);
                    continue;
                }

                Debug.LogVerbose(this, "Linking Bridge to Simpl Device : {0} join: {1}", device.Name, joinData.JoinNumber);
                trilist.SetStringSigAction(joinData.JoinNumber, s => device.StopTimerSerial());

                trilist.SetBoolSigAction(joinData.JoinNumber, device.DeviceOnline);
                if (trilist.BooleanOutput[joinData.JoinNumber].BoolValue)
                {
                    device.DeviceOnline(true); 
                }
                if (_overrideDigitalOutput) trilist.BooleanInput[joinData.JoinNumber].BoolValue = !String.IsNullOrEmpty(device.Name);
                else device.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinData.JoinNumber]);

                device.StatusFeedback.LinkInputSig(trilist.UShortInput[joinData.JoinNumber]);
                device.NameFeedback.LinkInputSig(trilist.StringInput[joinData.JoinNumber]);
            }
            foreach (var item in MonitoredEssentialsDevices.Values)
            {
                var device = item;
                
                JoinDataComplete joinData;

                if (!joinMap.Joins.TryGetValue(String.Format("DeviceMonitor--{0}", device.Key), out joinData))
                    continue;
                if (!joinData.Metadata.Description.EndsWith(":: ESSENTIALS"))
                    continue;

                Debug.LogInformation(this, "Linking Bridge to Essentials Device : {0} join: {1}", item.Name, joinData.JoinNumber);

                device.StatusMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinData.JoinNumber]);
                device.StatusFeedback.LinkInputSig(trilist.UShortInput[joinData.JoinNumber]);
                device.NameFeedback.LinkInputSig(trilist.StringInput[joinData.JoinNumber]);
            }

		    trilist.OnlineStatusChange += (d, args) =>
		    {
		        if (!args.DeviceOnLine) return;
		        foreach (var device in MonitoredSimplDevices.Values)
		        {
		            device.IsOnlineFeedback.FireUpdate();
		            device.StatusFeedback.FireUpdate();
		            device.NameFeedback.FireUpdate();
		        }

		        foreach (var device in MonitoredEssentialsDevices.Values)
		        {
		            device.StatusMonitor.IsOnlineFeedback.FireUpdate();
		            device.StatusFeedback.FireUpdate();
		            device.NameFeedback.FireUpdate();
		        }
		    };
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

    public enum MonitoringType
    {
        Serial,
        Digital
    };

    public enum DeviceStatus
    {
        Unknown,
        Warning,
        Error,
        Ok
    };

    
    
}

