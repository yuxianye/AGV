using Solution.Device.Core;
using System;
using System.Collections.Generic;

namespace Solution.Device.OpcUaDevice
{

    /// <summary>
    /// OpcUa设备连接参数实体
    /// </summary>
    public class OpcUaDeviceConnectParamEntity : DeviceConnectParamEntityBase
    {
        ///// <summary>
        ///// 设备连接地址、连接字符串
        ///// </summary>
        //public string DeviceUrl { get; set; }

    }

    /// <summary>
    /// OpcUa设备操作输入参数实体基类
    /// </summary>
    //public class OpcUaDeviceInParamEntity : DeviceInParamEntityBase
    public class OpcUaDeviceInParamEntity : DeviceInputParamEntityBase
    {
        //DeviceInParamEntityBase

        ///// <summary>
        ///// 节点Id
        ///// </summary>
        //public string NodeId { get; set; }

        ///// <summary>
        ///// 服务器地址
        ///// </summary>
        //public string ServerUrl { get; set; } = @"opc.tcp://127.0.0.1:49320/";
    }

    /// <summary>
    /// OpcUa设备操作返回参数实体基类
    /// </summary>
    //public class OpcUaDeviceOutParamEntity : DeviceParamEntityBase
    public class OpcUaDeviceOutParamEntity : DeviceOutputParamEntityBase
    {
        //public object Value { get; set; }
        /// <summary>
        /// 订阅数据列表
        /// </summary>
        public List<Tuple<string, object>> SubScriptionValueList { get; set; }
    }

    /// <summary>
    /// Json实体，用于Socket通讯传输
    /// </summary>
    public class SocketJsonParamEntity : OpcUaDeviceOutParamEntity
    {
        public Guid KeyId { get; set; }

        public FuncCode FunctionCode { get; set; }

        /// <summary>
        /// 存储Business的ID
        /// </summary>
        public List<Guid> SubScriptionList { get; set; }

        public List<Tuple<Guid, string, object>> SendSubScriptionValueList { get; set; }
    }






}
