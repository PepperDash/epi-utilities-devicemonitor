using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common;
using PepperDash.Essentials.Bridges;

using Newtonsoft.Json;



namespace epi.utilities.deviceMonitor
{
    public static class DeviceMonitorApiExtensions
    {


    }

    public class DeviceMonitorJoinMap : JoinMapBase
    {
        #region Digitals

        public uint MultipurposeJoin { get; set; }

        #endregion

        public DeviceMonitorJoinMap()
        {
            MultipurposeJoin = 0;
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;
            MultipurposeJoin += joinOffset;
        }

    }
}