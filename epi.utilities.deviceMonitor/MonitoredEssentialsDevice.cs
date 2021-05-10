using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices;
using PepperDash.Essentials.DM;

namespace epi.utilities.deviceMonitor
{
    public class MonitoredEssentialsDevice	
    {
        public StatusMonitorBase StatusMonitor;

        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            private set
            {
                _Name = value;
                NameFeedback.FireUpdate();
            }
        }

        private uint _JoinNumber;
        public uint JoinNumber
        {
            get
            {
                return _JoinNumber > 0 ? _JoinNumber : uint.MaxValue;
            }
            private set
            {
                _JoinNumber = value;
            }
        }

        private eDeviceStatus _Status = eDeviceStatus.unknown;
        public eDeviceStatus Status
        {
            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
                StatusFeedback.FireUpdate();
            }
        }
        public bool UseInRoomHealth { get; private set; }

        public StringFeedback NameFeedback;
        public IntFeedback StatusFeedback;

		public MonitoredEssentialsDevice(DeviceMonitorDevice deviceConfig, Device device, ICommunicationMonitor newStatusMonitorBase)
        {
			NameFeedback = new StringFeedback(() => Name);
			StatusFeedback = new IntFeedback(() => (int)Status);
			StatusMonitor = newStatusMonitorBase.CommunicationMonitor;
			Name = deviceConfig.name;
			JoinNumber = deviceConfig.joinNumber;
			UseInRoomHealth = deviceConfig.logToDevices;

			// StatusMonitor = newStatusMonitorBase.CommunicationMonitor;
			newStatusMonitorBase.CommunicationMonitor.StatusChange += new EventHandler<MonitorStatusChangeEventArgs>(StatusMonitor_StatusChange);
        }

        void StatusMonitor_StatusChange(object sender, MonitorStatusChangeEventArgs e)
        {
            switch (e.Status)
            {
                case MonitorStatus.InError:
                    Status = eDeviceStatus.error;
                    break;
                case MonitorStatus.InWarning:
                    Status = eDeviceStatus.warning;
                    break;
                case MonitorStatus.IsOk:
                    Status = eDeviceStatus.ok;
                    break;
                case MonitorStatus.StatusUnknown:
                    Status = eDeviceStatus.unknown;
                    break;
                default:
                    break;
            }
        }
    }
}