using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpPcap;

namespace Dr.Net
{
    class Device
    {
        string displayName; 
        public string DisplayName
        {
            get { return displayName; }
            set { displayName = value; }
        }

        ICaptureDevice captureDevice;

        public ICaptureDevice CaptureDevice
        {
            get { return captureDevice; }
            set { captureDevice = value; }
        }

        public override string ToString()
        {
            return displayName;
            //return base.ToString();
        }
    }
}
