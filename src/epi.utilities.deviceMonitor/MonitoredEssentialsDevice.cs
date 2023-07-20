using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace epi.utilities.deviceMonitor
{
    public class MonitoredEssentialsDevice : IKeyed
    {
        public Device Device { get; set; }
        public StatusMonitorBase StatusMonitor;

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            private set
            {
                _name = value;
                NameFeedback.FireUpdate();
            }
        }

        public string Key { get; private set; }

        private uint _joinNumber;
        public uint JoinNumber
        {
            get
            {
                return _joinNumber > 0 ? _joinNumber : uint.MaxValue;
            }
            private set
            {
                _joinNumber = value;
            }
        }

        private DeviceStatus _status = DeviceStatus.Unknown;
        public DeviceStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                StatusFeedback.FireUpdate();
            }
        }
        public bool UseInRoomHealth { get; private set; }

        public StringFeedback NameFeedback;
        public IntFeedback StatusFeedback;

        public MonitoredEssentialsDevice(DeviceMonitorDevice deviceConfig, StatusMonitorBase newStatusMonitorBase, string key)
        {
            Key = key;
            NameFeedback = new StringFeedback(() => Name);
            StatusFeedback = new IntFeedback(() => (int)Status);
            StatusMonitor = newStatusMonitorBase;
            Name = deviceConfig.Name;
            JoinNumber = deviceConfig.JoinNumber;
            UseInRoomHealth = deviceConfig.LogToDevices;

            // StatusMonitor = newStatusMonitorBase.CommunicationMonitor;
            StatusMonitor.StatusChange += StatusMonitor_StatusChange;
            Status = StatusMonitorTranslate(StatusMonitor.Status);
        }

        public MonitoredEssentialsDevice(DeviceMonitorDevice deviceConfig, ICommunicationMonitor newStatusMonitorBase, string key)
        {
            Key = key;
            NameFeedback = new StringFeedback(() => Name);
            StatusFeedback = new IntFeedback(() => (int)Status);
            StatusMonitor = newStatusMonitorBase.CommunicationMonitor;
            Name = deviceConfig.Name;
            JoinNumber = deviceConfig.JoinNumber;
            UseInRoomHealth = deviceConfig.LogToDevices;

            // StatusMonitor = newStatusMonitorBase.CommunicationMonitor;
            StatusMonitor.StatusChange += StatusMonitor_StatusChange;
            Status = StatusMonitorTranslate(StatusMonitor.Status);
        }

        void StatusMonitor_StatusChange(object sender, MonitorStatusChangeEventArgs e)
        {
            if (e == null) return;
            Status = StatusMonitorTranslate(e.Status);
        }

        private static DeviceStatus StatusMonitorTranslate(MonitorStatus status)
        {
            DeviceStatus localStatus;
            switch (status)
            {
                case MonitorStatus.InError:
                    localStatus = DeviceStatus.Error;
                    break;
                case MonitorStatus.InWarning:
                    localStatus = DeviceStatus.Warning;
                    break;
                case MonitorStatus.IsOk:
                    localStatus = DeviceStatus.Ok;
                    break;
                case MonitorStatus.StatusUnknown:
                    localStatus = DeviceStatus.Unknown;
                    break;
                default:
                    localStatus = DeviceStatus.Unknown;
                    break;
            }
            return localStatus;

        }
    }
}