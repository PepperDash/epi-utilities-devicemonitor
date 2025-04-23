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

        public MonitoredEssentialsDevice(DeviceMonitorDevice deviceConfig, ICommunicationMonitor newStatusMonitorBase, string key)
        {
            Debug.LogVerbose("{0} Entered Constructor", deviceConfig.DeviceKey);
            Key = key;
            NameFeedback = new StringFeedback(() => Name);
            StatusFeedback = new IntFeedback(() => (int)Status);
            StatusMonitor = newStatusMonitorBase.CommunicationMonitor;
            Name = deviceConfig.Name;
            JoinNumber = deviceConfig.JoinNumber;
            UseInRoomHealth = deviceConfig.LogToDevices;
            StatusMonitor.StatusChange += StatusMonitor_StatusChange;
            Debug.LogVerbose("{0} Exited Constructor", deviceConfig.DeviceKey);

            TranslateStatus(newStatusMonitorBase.CommunicationMonitor.Status);
        }

        void TranslateStatus(MonitorStatus status)
        {
            switch (status)
            {
                case MonitorStatus.InError:
                    Status = DeviceStatus.Error;
                    break;
                case MonitorStatus.InWarning:
                    Status = DeviceStatus.Warning;
                    break;
                case MonitorStatus.IsOk:
                    Status = DeviceStatus.Ok;
                    break;
                case MonitorStatus.StatusUnknown:
                    Status = DeviceStatus.Unknown;
                    break;
            }
        }

        void StatusMonitor_StatusChange(object sender, MonitorStatusChangeEventArgs e)
        {
            if (e == null) return;

            TranslateStatus(e.Status);
        }
    }
}
