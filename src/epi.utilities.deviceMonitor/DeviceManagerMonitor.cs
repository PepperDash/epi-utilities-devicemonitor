using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace epi.utilities.deviceMonitor
{
    public class DeviceManagerMonitor : IKeyName, IOnline
    {
        public DeviceManagerMonitor(string key, string name)
        {
            Key = key;
            Name = name;
            IsOnline = new BoolFeedback(() => Online);
        }

        public void LinkIOnline(IOnline iOnline)
        {
            if (iOnline == null) return;
            Online = iOnline.IsOnline.BoolValue;
            IsOnline.FireUpdate();
            iOnline.IsOnline.OutputChange += (s, a) =>
            {
                if (a == null) return;
                Online = a.BoolValue;
                IsOnline.FireUpdate();
            };
        }


        public string Key { get; private set; }
        public string Name { get; private set; }
        public bool Online { get; private set; }


        public BoolFeedback IsOnline { get; private set; }

    }
}