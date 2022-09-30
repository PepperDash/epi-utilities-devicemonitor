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
}