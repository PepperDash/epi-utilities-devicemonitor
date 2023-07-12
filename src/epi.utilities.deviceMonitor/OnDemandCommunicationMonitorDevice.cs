using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace epi.utilities.deviceMonitor
{
    public class OnDemandCommunicationMonitorDevice : ICommunicationMonitor
    {
        #region ICommunicationMonitor Members

        public StatusMonitorBase CommunicationMonitor { get; private set; }

        #endregion

        public OnDemandCommunicationMonitorDevice(StatusMonitorBase monitor)
        {
            CommunicationMonitor = monitor;
            CommunicationMonitor.Start();
        }
    }

    public class OnDemandIOnlineCommunicationMonitorDevice : StatusMonitorBase
    {
        public IOnline Device { get; private set; }

// ReSharper disable once NotAccessedField.Local
        private CTimer _checkStatus;

        public OnDemandIOnlineCommunicationMonitorDevice(IKeyed parent, long warningTime, long errorTime) :
            base(parent, ((errorTime == 0) || ((errorTime != 0) && (errorTime < 5000))) ? 60000 : errorTime, ((warningTime == 0) || ((warningTime != 0) && (warningTime < 5000))) ? 180000 : warningTime)
        {
            Debug.Console(2, this, "Attempting to Register IOnlineCommunicationMonitorDevice");

            var iOnline = parent as IOnline;
            if (iOnline == null)
            {
                Debug.Console(0, this,"Unable to register IOnline Events for device {0}", parent.Key);
                return;
            }
            Device = iOnline;

            Status = Device.IsOnline.BoolValue ? MonitorStatus.IsOk : MonitorStatus.StatusUnknown;

            ConstructorStart();
        }

        void IsOnline_OutputChange(object sender, FeedbackEventArgs e)
        {
            if (e == null) return;
            if (!e.BoolValue)
            {
                StartErrorTimers();
                return;
            }
            StopErrorTimers();
            Status = MonitorStatus.IsOk;
        }

        void Poll()
        {
            _checkStatus = null;
            if (!Device.IsOnline.BoolValue)
            {
                StartErrorTimers();
            }
            else
            {
                StopErrorTimers();
                Status = MonitorStatus.IsOk;
            }
            _checkStatus = new CTimer(o => Poll(), 15000);
        }

        void ConstructorStart()
        {
            Start();
        }

        #region IStatusMonitor Members


        public override void Start()
        {
            Device.IsOnline.OutputChange += IsOnline_OutputChange;
            _checkStatus = new CTimer(o => Poll(), 15000);

        }

        public override void Stop()
        {
            Device.IsOnline.OutputChange -= IsOnline_OutputChange;
        }
        #endregion

    }

}