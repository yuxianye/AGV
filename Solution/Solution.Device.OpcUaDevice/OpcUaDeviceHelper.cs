using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using Solution.Device.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Console;

namespace Solution.Device.OpcUaDevice
{
    public class OpcUaDeviceHelper : IDevice
    {
        public OpcUaDeviceHelper()
        {
            subscriptionDic = new Dictionary<int, Subscription>();
        }

        #region Public Methods

        /// <summary>
        /// 与OPC服务器建立连接
        /// </summary>
        /// <typeparam name="Tin"></typeparam>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="connectParam"></param>
        /// <returns></returns>
        public Task<Tout> Connect<Tin, Tout>(Tin connectParam) where Tin : IDeviceParam where Tout : IDeviceParam
        {
            return Task<Tout>.Factory.StartNew(() =>
            {
                OpcUaDeviceOutParamEntity opcUaDeviceOutParamEntity = new OpcUaDeviceOutParamEntity();

                try
                {
                    Task<bool> taskResult = Init();
                    if (taskResult.Result)
                    {
                        var param = connectParam as OpcUaDeviceConnectParamEntity;
                        Connect(param.DeviceUrl).Wait();
                        serverUrl = param.DeviceUrl;
                        if (Equals(session, null))
                        {
                            opcUaDeviceOutParamEntity.StatusCode = (uint)DeviceStatusCode.ConnFault;
                            opcUaDeviceOutParamEntity.Value = "Connect failed";
                        }
                        else
                        {
                            opcUaDeviceOutParamEntity.StatusCode = (uint)DeviceStatusCode.ConnOk;
                            opcUaDeviceOutParamEntity.Value = "Connect Success";
                        }
                    }
                    else
                    {
                        opcUaDeviceOutParamEntity.StatusCode = (uint)DeviceStatusCode.ConnFault;
                        opcUaDeviceOutParamEntity.Value = "Connect failed";
                    }
                }
                catch (ServiceResultException e)
                {
                    opcUaDeviceOutParamEntity.Value = e.LocalizedText;
                    opcUaDeviceOutParamEntity.StatusCode = e.StatusCode;
                }
                catch (Exception e)
                {
                    opcUaDeviceOutParamEntity.StatusCode = (uint)DeviceStatusCode.ConnFault;
                    opcUaDeviceOutParamEntity.Value = "Connect failed";
                }

                DeviceEventArgs<DeviceParamEntityBase> args = new DeviceEventArgs<DeviceParamEntityBase>(opcUaDeviceOutParamEntity);
                Notification?.Invoke(this, args);
                return (Tout)(opcUaDeviceOutParamEntity as object);
            });
        }

        /// <summary>
        /// 与OPC服务器断开连接
        /// </summary>
        /// <typeparam name="Tin"></typeparam>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="disConnectParam"></param>
        /// <returns></returns>
        public Task<Tout> DisConnect<Tin, Tout>(Tin disConnectParam) where Tin : IDeviceParam where Tout : IDeviceParam
        {
            return Task<Tout>.Factory.StartNew(() =>
            {
                OpcUaDeviceOutParamEntity opcUaDeviceOutParamEntity = new OpcUaDeviceOutParamEntity();

                if (session != null)
                {
                    try
                    {
                        Disconnect();
                        opcUaDeviceOutParamEntity.StatusCode = (uint)DeviceStatusCode.DisConnOk;
                        opcUaDeviceOutParamEntity.Value = "DisConnect Success";
                    }
                    catch (ServiceResultException e)
                    {
                        opcUaDeviceOutParamEntity.Value = e.LocalizedText;
                        opcUaDeviceOutParamEntity.StatusCode = e.StatusCode;
                    }
                    catch (Exception ex)
                    {
                        opcUaDeviceOutParamEntity.StatusCode = (uint)DeviceStatusCode.DisConnFault;
                        opcUaDeviceOutParamEntity.Value = "DisConnect failed";
                    }
                }

                DeviceEventArgs<DeviceParamEntityBase> args = new DeviceEventArgs<DeviceParamEntityBase>(opcUaDeviceOutParamEntity);
                Notification?.Invoke(this, args);
                return (Tout)(opcUaDeviceOutParamEntity as object);
            });
        }

        /// <summary>
        /// 从OPC服务器读取数据
        /// </summary>
        /// <typeparam name="Tin"></typeparam>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="readParam"></param>
        /// <returns></returns>
        public Task<Tout> Read<Tin, Tout>(Tin readParam) where Tin : IDeviceParam where Tout : IDeviceParam
        {
            return Task<Tout>.Factory.StartNew(() =>
            {
                var readNode = readParam as OpcUaDeviceInParamEntity;
                OpcUaDeviceOutParamEntity opcUaDeviceOutParamEntity = new OpcUaDeviceOutParamEntity();

                ReadValueId nodeToRead = new ReadValueId()
                {
                    NodeId = new NodeId(readNode.NodeId),
                    AttributeId = Attributes.Value
                };
                ReadValueIdCollection nodesToRead = new ReadValueIdCollection
                {
                    nodeToRead
                };
                try
                {
                    // read the current value
                    session.Read(
                        null,
                        0,
                        TimestampsToReturn.Neither,
                        nodesToRead,
                        out DataValueCollection results,
                        out DiagnosticInfoCollection diagnosticInfos);

                    ClientBase.ValidateResponse(results, nodesToRead);
                    ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);
                    //opcUaDeviceOutParamEntity.StatusCode = StatusCodes.Good;
                    opcUaDeviceOutParamEntity.StatusCode = (uint)DeviceStatusCode.ReadOk;
                    opcUaDeviceOutParamEntity.Value = results[0].Value;
                }
                catch (ServiceResultException e)
                {
                    opcUaDeviceOutParamEntity.Value = e.LocalizedText;
                    opcUaDeviceOutParamEntity.StatusCode = e.StatusCode;
                }
                catch (Exception ex)
                {
                    opcUaDeviceOutParamEntity.StatusCode = StatusCodes.Bad;
                    opcUaDeviceOutParamEntity.Value = null;
                    opcUaDeviceOutParamEntity.StatusCode = (uint)DeviceStatusCode.ReadFault;
                }

                DeviceEventArgs<DeviceParamEntityBase> args = new DeviceEventArgs<DeviceParamEntityBase>(opcUaDeviceOutParamEntity);
                Notification?.Invoke(this, args);

                return (Tout)(opcUaDeviceOutParamEntity as object);
            });
        }

        /// <summary>
        /// 向OPC服务器写入数据
        /// </summary>
        /// <typeparam name="Tin"></typeparam>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="writeParam"></param>
        /// <returns></returns>
        public Task<Tout> Write<Tin, Tout>(Tin writeParam) where Tin : IDeviceParam where Tout : IDeviceParam
        {
            return Task<Tout>.Factory.StartNew(() =>
            {
                var writeNode = writeParam as OpcUaDeviceInParamEntity;
                OpcUaDeviceOutParamEntity opcUaDeviceOutParamEntity = new OpcUaDeviceOutParamEntity();
                WriteValue valueToWrite = new WriteValue()
                {
                    NodeId = new NodeId(writeNode.NodeId),
                    AttributeId = Attributes.Value
                };
                valueToWrite.Value.Value = Convert.ChangeType(writeNode.Value, writeNode.ValueType);
                //Type type = valueToWrite.Value.Value.GetType();
                //valueToWrite.Value.Value = Convert.ToInt16(writeNode.Value);
                valueToWrite.Value.StatusCode = StatusCodes.Good;
                valueToWrite.Value.ServerTimestamp = DateTime.MinValue;
                valueToWrite.Value.SourceTimestamp = DateTime.MinValue;

                WriteValueCollection valuesToWrite = new WriteValueCollection
                {
                    valueToWrite
                };

                try
                {
                    session.Write(
                        null,
                        valuesToWrite,
                        out StatusCodeCollection results,
                        out DiagnosticInfoCollection diagnosticInfos);

                    ClientBase.ValidateResponse(results, valuesToWrite);
                    ClientBase.ValidateDiagnosticInfos(diagnosticInfos, valuesToWrite);
                    //opcUaDeviceOutParamEntity.StatusCode = StatusCodes.Good;
                    opcUaDeviceOutParamEntity.Value = !StatusCode.IsBad(results[0]);
                    if (!StatusCode.IsBad(results[0]))
                    {
                        opcUaDeviceOutParamEntity.StatusCode = (uint)DeviceStatusCode.WriteOk;
                    }
                    else
                    {
                        opcUaDeviceOutParamEntity.StatusCode = (uint)DeviceStatusCode.WriteFault;
                    }
                }
                catch (ServiceResultException e)
                {
                    opcUaDeviceOutParamEntity.Value = e.LocalizedText;
                    opcUaDeviceOutParamEntity.StatusCode = e.StatusCode;
                }
                catch (Exception ex)
                {
                    opcUaDeviceOutParamEntity.StatusCode = StatusCodes.Bad;
                    opcUaDeviceOutParamEntity.Value = null;
                    opcUaDeviceOutParamEntity.StatusCode = (uint)DeviceStatusCode.WriteFault;
                }
                DeviceEventArgs<DeviceParamEntityBase> args = new DeviceEventArgs<DeviceParamEntityBase>(opcUaDeviceOutParamEntity);
                Notification?.Invoke(this, args);

                return (Tout)(opcUaDeviceOutParamEntity as object);
            });
        }

        /// <summary>
        /// 创建订阅
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public Task<bool> CreateSubScriptions(List<NodeEntity> nodes)
        {
            return Task.Run(() =>
            {
                if (session == null)
                {
                    return false; ;
                }

                OpcUaDeviceOutParamEntity opcUaDeviceOutParamEntity = new OpcUaDeviceOutParamEntity();
                try
                {
                    List<int> intervals = nodes.Select(data => data.Interval).ToList<int>().Distinct().ToList<int>();
                    intervals.Sort();
                    foreach (int interval in intervals)
                    {
                        List<NodeEntity> subNodes = nodes.FindAll((s) => s.Interval == interval);

                        Subscription subscription;
                        if (!subscriptionDic.ContainsKey(interval))
                        {
                            subscription = new Subscription(session.DefaultSubscription);
                            session.AddSubscription(subscription);
                            subscription.Create();
                            subscriptionDic.Add(interval, subscription);
                        }
                        else
                        {
                            subscription = subscriptionDic[interval];
                        }

                        subscription.PublishingInterval = interval;
                        var monitoredItems = new List<MonitoredItem>();
                        foreach (NodeEntity entity in subNodes)
                        {
                            monitoredItems.Add(
                                new MonitoredItem(subscription.DefaultItem)
                                {
                                    StartNodeId = entity.NodeId
                                }
                            );
                        };
                        //monitoredItems.ForEach(i => i.Notification += OnMonitoredNotification);
                        if (subscription.MonitoredItemCount == 0)
                        {
                            subscription.AddItems(monitoredItems);
                            subscription.ApplyChanges();
                        }
                        else
                        {
                            subscription.RemoveItems(subscription.MonitoredItems);
                            subscription.AddItems(monitoredItems);
                            subscription.ApplyChanges();
                        }
                        subscription.FastDataChangeCallback += OnFastDataChange;
                        subscriptionDic[interval] = subscription;
                    }

                    //delete subscription
                    foreach (var data in subscriptionDic)
                    {
                        if (intervals.FindAll((s) => s == data.Key).Count() <= 0)
                        {
                            session.RemoveSubscription(subscriptionDic[data.Key]);
                        }
                    }
                }
                catch (ServiceResultException e)
                {
                    opcUaDeviceOutParamEntity.Value = e.LocalizedText;
                    opcUaDeviceOutParamEntity.StatusCode = e.StatusCode;
                    return false;
                }
                DeviceEventArgs<DeviceParamEntityBase> args = new DeviceEventArgs<DeviceParamEntityBase>(opcUaDeviceOutParamEntity);
                Notification?.Invoke(this, args);
                return true;
            });
        }

        //public int GetSubscriptionGroupIdsCount()
        //{
        //    return subscriptionDic.Count;
        //}

        public Task<bool> ClearSubscriptionGroups()
        {
            return Task.Run<bool>(() =>
            {

                bool result = false;

                try
                {
                    result = session.RemoveSubscriptions(session.Subscriptions);
                }
                catch (Exception e)
                {
                    result = false;
                }

                return result;
            });
        }

        /// <summary>
        /// 获取OPC UA服务器URL
        /// </summary>
        /// <returns></returns>
        public String GetServerUrl()
        {
            return serverUrl;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// 处理心跳事件
        /// </summary>
        private void OnKeepAliveNotification(Session session, KeepAliveEventArgs e)
        {
            try
            {
                if (!ReferenceEquals(session, session))
                {
                    return;
                }

                OpcUaDeviceOutParamEntity opcUaDeviceOutParamEntity = new OpcUaDeviceOutParamEntity();

                if (ServiceResult.IsBad(e.Status))
                {
                    if (reconnectPeriod <= 0)
                    {
                        opcUaDeviceOutParamEntity.StatusCode = (uint)DeviceStatusCode.ReConnOk;
                        DeviceEventArgs<DeviceParamEntityBase> args = new DeviceEventArgs<DeviceParamEntityBase>(opcUaDeviceOutParamEntity);
                        Notification?.Invoke(this, args);
                        return;
                    }

                    if (reconnectHandler == null)
                    {
                        reconnectHandler = new SessionReconnectHandler();
                        reconnectHandler.BeginReconnect(session, reconnectPeriod * 1000, Server_ReconnectComplete);
                    }

                    return;
                }
                isConnected = true;
            }
            catch (Exception exception)
            {
                WriteLine(exception.ToString());
            }
        }

        /// <summary>
        /// 处理订阅事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        private void OnMonitoredNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            OpcUaDeviceOutParamEntity opcUaDeviceOutParamEntity = new OpcUaDeviceOutParamEntity();
            opcUaDeviceOutParamEntity.StatusCode = (uint)DeviceStatusCode.SubscriptionOK;
            opcUaDeviceOutParamEntity.SubScriptionValueList = new List<Tuple<string, object>>();
            foreach (var value in item.DequeueValues())
            {
                opcUaDeviceOutParamEntity.SubScriptionValueList.Add(Tuple.Create(item.ResolvedNodeId.ToString(), value.Value));
            }
            DeviceEventArgs<DeviceParamEntityBase> args = new DeviceEventArgs<DeviceParamEntityBase>(opcUaDeviceOutParamEntity);
            Notification.Invoke(this, args);
        }

        private void OnFastDataChange(Subscription subscription, DataChangeNotification notification, IList<string> stringTable)
        {
            OpcUaDeviceOutParamEntity opcUaDeviceOutParamEntity = new OpcUaDeviceOutParamEntity();
            opcUaDeviceOutParamEntity.StatusCode = (uint)DeviceStatusCode.SubscriptionOK;
            opcUaDeviceOutParamEntity.SubScriptionValueList = new List<Tuple<string, object>>();

            foreach (MonitoredItemNotification itemNotification in notification.MonitoredItems)
            {
                MonitoredItem item = subscription.FindItemByClientHandle(itemNotification.ClientHandle);
                if (item == null)
                {
                    continue;
                }
                foreach (var value in item.DequeueValues())
                {
                    opcUaDeviceOutParamEntity.SubScriptionValueList.Add(Tuple.Create(item.ResolvedNodeId.ToString(), value.Value));
                }
            }

            DeviceEventArgs<DeviceParamEntityBase> args = new DeviceEventArgs<DeviceParamEntityBase>(opcUaDeviceOutParamEntity);
            Notification.Invoke(this, args);
        }

        /// <summary>
        /// 处理证书校验事件
        /// </summary>
        /// <param name="validator"></param>
        /// <param name="e"></param>
        private void OnCertificateValidatorNotification(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
            {
                e.Accept = autoAccept;
                if (autoAccept)
                {
                    WriteLine("Accepted Certificate: {0}", e.Certificate.Subject);
                }
                else
                {
                    WriteLine("Rejected Certificate: {0}", e.Certificate.Subject);
                }
            }
        }

        /// <summary>
        /// 处理OPC UA服务器重连事件
        /// </summary>
        private void Server_ReconnectComplete(object sender, EventArgs e)
        {
            OpcUaDeviceOutParamEntity opcUaDeviceOutParamEntity = new OpcUaDeviceOutParamEntity();
            try
            {
                if (!ReferenceEquals(sender, reconnectHandler))
                {
                    return;
                }

                session = reconnectHandler.Session;
                reconnectHandler.Dispose();
                reconnectHandler = null;

                opcUaDeviceOutParamEntity.StatusCode = (uint)DeviceStatusCode.ReConnOk;

            }
            catch (Exception exception)
            {
                opcUaDeviceOutParamEntity.StatusCode = (uint)DeviceStatusCode.ReConnFault;
            }

            DeviceEventArgs<DeviceParamEntityBase> args = new DeviceEventArgs<DeviceParamEntityBase>(opcUaDeviceOutParamEntity);
            Notification?.Invoke(this, args);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// OPC UA通信初始化
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Init()
        {
            application = new ApplicationInstance
            {
                ApplicationName = "OpcUaDeviceHelper",
                ApplicationType = ApplicationType.Client,
                ConfigSectionName = "Solution.OpcUaClient"
            };

            try
            {
                // load the application configuration.
                //configuration = await application.LoadApplicationConfiguration(false);
                string configFilePath = System.AppDomain.CurrentDomain.BaseDirectory + @"bin\Solution.OpcUaClient.Config.xml";
                configuration = await application.LoadApplicationConfiguration(configFilePath, false);

                // check the application certificate.
                haveAppCertificate = await application.CheckApplicationInstanceCertificate(false, 0);
                if (!haveAppCertificate)
                {
                    //throw new Exception("Application instance certificate invalid!");
                    return false;
                }

                if (haveAppCertificate)
                {
                    configuration.ApplicationUri = Utils.GetApplicationUriFromCertificate(configuration.SecurityConfiguration.ApplicationCertificate.Certificate);
                    if (configuration.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                    {
                        autoAccept = true;
                    }
                    configuration.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(OnCertificateValidatorNotification);
                }
                else
                {
                    Console.WriteLine("    WARN: missing application certificate, using unsecure connection.");
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 与OPC UA服务器建立连接，创建一个新的 session.
        /// </summary>
        /// <returns>The new session object.</returns>
        private async Task Connect(string serverUrl)
        {
            // disconnect from existing session.
            Disconnect();

            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            try
            {
                var selectedEndpoint = CoreClientUtils.SelectEndpoint(serverUrl, haveAppCertificate, 15000);
                var endpointConfiguration = EndpointConfiguration.Create(configuration);
                var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);
                session = await Session.Create(configuration, endpoint, false, "OPC UA Console Client", 60000, new UserIdentity(new AnonymousIdentityToken()), null);
            }
            catch (Exception e)
            {
            }

            // register keep alive handler
            session.KeepAlive += new KeepAliveEventHandler(OnKeepAliveNotification);

            // update the client status
            isConnected = true;
        }

        /// <summary>
        /// 与OPC UA服务器断开连接.
        /// </summary>
        private void Disconnect()
        {
            // disconnect any existing session.
            if (session != null)
            {
                session.Close(10000);
                session = null;
            }

            // update the client status
            isConnected = false;
        }

        /// <summary>
        /// 检测返回结果
        /// </summary>
        /// <param name="status"></param>
        private void CheckReturnValue(StatusCode status)
        {
            if (!StatusCode.IsGood(status))
                throw new Exception(string.Format("Invalid response from the server. (Response Status: {0})", status));
        }

        #endregion

        #region Public Fields

        public event DeviceNotificationEventHandler Notification;
        /// <summary>
        /// Indicate the connect status
        /// </summary>
        public bool Connected
        {
            get { return isConnected; }
        }

        #endregion

        #region Private Fields
        private ApplicationInstance application;
        private ApplicationConfiguration configuration;
        private Session session;
        private Dictionary<int, Subscription> subscriptionDic;
        private bool isConnected;            //是否已经连接过
        private int reconnectPeriod = 10;   //重连状态
        private static bool autoAccept = false;
        private bool haveAppCertificate;
        private SessionReconnectHandler reconnectHandler;
        private String serverUrl;
        #endregion
    }
}
