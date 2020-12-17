using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Core;


namespace epi.utilities.deviceMonitor
{
    public class MonitoredSimplDevice : IKeyed
    {
		
        public CTimer timer = null;
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
                IsOnline = value == eDeviceStatus.ok ? true : false;
                StatusFeedback.FireUpdate();
            }
        }
        public event EventHandler<EventArgs> StatusChangeEvent;
        private bool _IsOnline;
        public bool IsOnline
        {
            get
            {
                return _IsOnline;
            }
            set
            {
                _IsOnline = value;
                IsOnlineFeedback.FireUpdate();
            }
        }
        private uint WarningTimeout;
        private long WarningTimerTimeout
        {
            get
            {
                return (long)(WarningTimeout * 1000);
            }
        }

        private uint ErrorTimeout;
        private long ErrorTimerTimeout
        {
            get
            {
                return (long)(ErrorTimeout * 1000);
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

        public IntFeedback StatusFeedback;
        public StringFeedback NameFeedback;
        public BoolFeedback IsOnlineFeedback;


        public bool UseInRoomHealth { get; set; }

        /// <summary>
        /// Builds a new MonitoredSimplDevice
        /// </summary>
        /// <param name="name">Name of the device</param>
        /// <param name="joinNumber">Relative Join Number for the device</param>
        /// <param name="monitorType">Analog or Digital</param>
        /// <param name="timeout">Timeout in whole seconds to transition between states</param>
        public MonitoredSimplDevice(string name, uint joinNumber, string monitorType, uint warningTimeout, uint errorTimeout, bool useInRoomHealth)
        {
			
            StatusFeedback = new IntFeedback(() => (int)Status);
            NameFeedback = new StringFeedback(() => Name);
            IsOnlineFeedback = new BoolFeedback(() => IsOnline);
            Name = name;
            JoinNumber = joinNumber;
            Key = "MonitoredSimplDevice--" + Name;
            WarningTimeout = warningTimeout > 0 ? warningTimeout : 60;
            ErrorTimeout = errorTimeout > 0 && errorTimeout > WarningTimeout ? errorTimeout : 180;
            UseInRoomHealth = useInRoomHealth;
        }

        /// <summary>
        /// Builds a new MonitoredSimplDevice
        /// </summary>
        /// <param name="device">Full Object from Json</param>
        public MonitoredSimplDevice(DeviceMonitorDevice device)
        {
            StatusFeedback = new IntFeedback(() => (int)Status);
            NameFeedback = new StringFeedback(() => Name);
            IsOnlineFeedback = new BoolFeedback(() => IsOnline);
            Name = device.name;
            JoinNumber = device.joinNumber;
            Key = "MonitoredSimplDevice--" + Name;

            WarningTimeout = device.warningTimeout > 0 ? device.warningTimeout : 60;
            ErrorTimeout = device.errorTimeout > 0 && device.errorTimeout > WarningTimeout ? device.errorTimeout : 180;
            UseInRoomHealth = device.logToDevices;
			StartTimer();
        }

        public void DeviceOnline(bool Online)
        {
            try
            {
                if (Online == true)
                {
                    StopTimer();
                    Debug.Console(1, this, "Device Online");
                }
                else
                {
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
                if (timer != null)
                {
                    timer.Dispose();
                    timer = new CTimer(warningTimerExpired, WarningTimerTimeout);
                }
                else
                    timer = new CTimer(warningTimerExpired, WarningTimerTimeout);
            }
            catch (Exception ex)
            {
                Debug.Console(0, this, "Exception - {0}", ex.Message);
            }

        }

        private void StopTimer()
        {
            changeStatus(eDeviceStatus.ok);
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }

        public void StopTimerSerial()
        {
            changeStatus(eDeviceStatus.ok);
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
            timer = new CTimer(warningTimerExpired, WarningTimerTimeout);
        }

        private void warningTimerExpired(Object unused)
        {
            if (Status != eDeviceStatus.warning)
            {
                changeStatus(eDeviceStatus.warning);
                timer.Dispose();
                timer = new CTimer(warningTimerExpired, ErrorTimerTimeout - WarningTimerTimeout);
            }
            else
            {
                changeStatus(eDeviceStatus.error);
            }
            Debug.Console(2, this, "{0} Timer Expired", Status == eDeviceStatus.warning ? "Warning" : "Error" );
        }

        private void changeStatus(eDeviceStatus status)
        {
            try
            {
                Debug.Console(1, this, "ChangeStatus - {0}",  (int)status);

                if (Status != status && JoinNumber != int.MaxValue)
                {
                    Status = status;
                }
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
