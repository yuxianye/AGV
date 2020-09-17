using System;
using System.Collections.Generic;
using System.Text;

namespace Solution.Device.OpcUaDevice
{
    public class NodeEntity
    {
        public NodeEntity() { }

        public String NodeId{ get;set; }

        public int Interval { get; set; } = 1000;
    }
}
