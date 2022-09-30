using PepperDash.Essentials.Core;


namespace epi.utilities.deviceMonitor
{

    public class DeviceMonitorJoinMapAdvanced : JoinMapBaseAdvanced
    {
        [JoinName("MultipurposeJoin")] public JoinDataComplete MultipurposeJoin = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 1,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Utilized for all functions of plugin",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.DigitalAnalogSerial
            });



        public DeviceMonitorJoinMapAdvanced(uint joinStart)
            : base(joinStart, typeof(DeviceMonitorJoinMapAdvanced))
        {

        }
    }
}