using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Plugins
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