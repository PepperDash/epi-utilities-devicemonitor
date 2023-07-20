using System;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using PepperDash.Core;


namespace epi.utilities.deviceMonitor
{
    public class MonitoredSimplDevice : IKeyed, IOnline
    {
		
        public CTimer Timer = null;
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
                Online = value == DeviceStatus.Ok;
                StatusFeedback.FireUpdate();
            }
        }
        public event EventHandler<EventArgs> StatusChangeEvent;
        private bool _online;
        public bool Online
        {
            get
            {
                return _online;
            }
            set
            {
                _online = value;
                IsOnline.FireUpdate();
            }
        }


        private readonly uint _warningTimeout;
        private long WarningTimerTimeout
        {
            get
            {
                return _warningTimeout * 1000;
            }
        }

        private readonly uint _errorTimeout;
        private long ErrorTimerTimeout
        {
            get
            {
                return _errorTimeout * 1000;
            }
        }
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

        public IntFeedback StatusFeedback;
        public StringFeedback NameFeedback;
        public BoolFeedback IsOnline { get; private set; }


        public bool UseInRoomHealth { get; set; }

        /// <summary>
        /// Builds a new MonitoredSimplDevice
        /// </summary>
        /// <param name="name">Name of the device</param>
        /// <param name="joinNumber">Relative Join Number for the device</param>
        /// <param name="warningTimeout">time in seconds before device enters warning status</param>
        /// <param name="errorTimeout">time in seconds before device enters error status</param>
        /// <param name="useInRoomHealth">show in overall room health metrics</param>
        public MonitoredSimplDevice(string key, string name, uint joinNumber, uint warningTimeout, uint errorTimeout, bool useInRoomHealth)
        {
			
            StatusFeedback = new IntFeedback(() => (int)Status);
            NameFeedback = new StringFeedback(() => Name);
            IsOnline = new BoolFeedback(() => Online);
            Name = name;
            JoinNumber = joinNumber;
            Key = key;
            _warningTimeout = warningTimeout > 0 ? warningTimeout : 60;
            _errorTimeout = errorTimeout > 0 && errorTimeout > _warningTimeout ? errorTimeout : 180;
            UseInRoomHealth = useInRoomHealth;
            StartTimer();
        }

        /// <summary>
        /// Builds a new MonitoredSimplDevice
        /// </summary>
        /// <param name="device">Full Object from Json</param>
        public MonitoredSimplDevice(string key, DeviceMonitorDevice device)
        {
            StatusFeedback = new IntFeedback(() => (int)Status);
            NameFeedback = new StringFeedback(() => Name);
            IsOnline = new BoolFeedback(() => Online);
            Name = device.Name;
            JoinNumber = device.JoinNumber;
            Key = key;
            if (device.CommunicationMonitor != null)
            {
                var monConfig = device.CommunicationMonitor;
                _warningTimeout = (uint)(monConfig.TimeToWarning > 0 ? monConfig.TimeToWarning : 60);
                _errorTimeout = (uint)(monConfig.TimeToError > 0 ? monConfig.TimeToError : 180);
            }
            _warningTimeout = device.WarningTimeout > 0 ? device.WarningTimeout : 60;
            _errorTimeout = device.ErrorTimeout > 0 && device.ErrorTimeout > _warningTimeout ? device.ErrorTimeout : 180;
            UseInRoomHealth = device.LogToDevices;
			StartTimer();
        }

        /// <summary>
        /// Set device Online Status
        /// </summary>
        /// <param name="online"></param>
        public void DeviceOnline(bool online)
        {
            try
            {
				Online = online;
                if (online)
                {
                    StopTimer();
                    Debug.Console(1, this, "Device Online");
                }
                else
                {
                    StartTimer();
                    Debug.Console(1, this, "Device Offline");
                }
            }
            catch (Exception ex)
            {
                Debug.Console(0, this, "Exception - {0}", ex.Message);
            }
        }

        private void StartTimer()
        {
            try
            {
                if (Timer != null)
                {
                    Timer.Dispose();
                    Timer = new CTimer(WarningTimerExpired, WarningTimerTimeout);
                }
                else
                    Timer = new CTimer(WarningTimerExpired, WarningTimerTimeout);
            }
            catch (Exception ex)
            {
                Debug.Console(0, this, "Exception - {0}", ex.Message);
            }

        }

        private void StopTimer()
        {
            ChangeStatus(DeviceStatus.Ok);
            if (Timer == null) return;
            Timer.Dispose();
            Timer = null;
        }

        public void StopTimerSerial()
        {
            ChangeStatus(DeviceStatus.Ok);
            if (Timer != null)
            {
                Timer.Dispose();
                Timer = null;
            }
            Timer = new CTimer(WarningTimerExpired, WarningTimerTimeout);
        }

        private void WarningTimerExpired(Object unused)
        {
            if (Status != DeviceStatus.Warning)
            {
                ChangeStatus(DeviceStatus.Warning);
                if(Timer!= null)
                    Timer.Dispose();
                Timer = new CTimer(WarningTimerExpired, ErrorTimerTimeout - WarningTimerTimeout);
            }
            else
            {
                ChangeStatus(DeviceStatus.Error);
            }
            Debug.Console(2, this, "{0} Timer Expired", Status == DeviceStatus.Warning ? "Warning" : "Error" );
        }

        private void ChangeStatus(DeviceStatus status)
        {
            try
            {
                Debug.Console(1, this, "ChangeStatus - {0}",  (int)status);

                if (Status == status || JoinNumber == int.MaxValue) return;
                Status = status;
                var handler = StatusChangeEvent;
                if (handler == null)
                {
                    return;
                }

                handler(this, new EventArgs());
            }
            catch (Exception ex)
            {
                Debug.Console(0, this, "Exception - {0}", ex.Message);
            }
        }

        #region IKeyed Members

        public string Key { get; set; }

        #endregion





	}
}
