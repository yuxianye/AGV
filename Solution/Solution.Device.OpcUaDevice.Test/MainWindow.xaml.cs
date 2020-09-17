using Newtonsoft.Json;
using Solution.Device.Core;
using Solution.Utility.Socket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Solution.Device.OpcUaDevice.Test
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool connectResult = false;
        private SocketClientHelper client = new SocketClientHelper("192.168.1.159", 13805);
        //private SocketClientHelper client = new SocketClientHelper("192.168.1.159", 2004);
        public MainWindow()
        {
            InitializeComponent();
            client.Connected += OnConnect;
            client.Closed += OnClose;
            client.OnDataReceived += OnReceive;
            client.Error += OnError;
        }

        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            

            if (connectResult ^= true)
            {
                Task<bool> resultTask = client.ConnectAsync();
                bool result = resultTask.Result;
                ResultTextBlock.Text += Environment.NewLine;
                if (result)
                {
                    ResultTextBlock.Text += "Connect OK!";
                }
                else
                {
                    ResultTextBlock.Text += "Connect Fault!";
                }
                ResultTextBlock.Text += Environment.NewLine;
                ConnectBtn.Content = "断开";
            }
            else
            {
                Task<bool> resultTask = client.CloseAsync();
                bool result = resultTask.Result;
                ResultTextBlock.Text += Environment.NewLine;
                ResultTextBlock.Text += "Closed!";
                ResultTextBlock.Text += Environment.NewLine;
                ConnectBtn.Content = "连接";
            }
           
        }

        private void ReadBtn_Click(object sender, RoutedEventArgs e)
        {
            SocketJsonParamEntity socketJsonParamEntity = new SocketJsonParamEntity();
            socketJsonParamEntity.KeyId = Guid.Parse("2B68E863-6F5D-E811-8BA9-F8A005475E49");
            socketJsonParamEntity.NodeId = "ns=2;s=TestChannel.TestDevice.word_1";
            socketJsonParamEntity.ValueType = typeof(short);
            socketJsonParamEntity.FunctionCode = Core.FuncCode.Read;
            client.Send(GetMessage(JsonConvert.SerializeObject(socketJsonParamEntity))); 
        }

        private void WriteBtn_Click(object sender, RoutedEventArgs e)
        {
            SocketJsonParamEntity socketJsonParamEntity = new SocketJsonParamEntity();
            socketJsonParamEntity.KeyId = Guid.Parse("2B68E863-6F5D-E811-8BA9-F8A005475E49");
            socketJsonParamEntity.NodeId = "ns=2;s=TestChannel.TestDevice.word_1";
            socketJsonParamEntity.Value = WriteTextBox.Text.Trim();
            socketJsonParamEntity.ValueType = typeof(short);
            socketJsonParamEntity.FunctionCode = Core.FuncCode.Write;
            client.Send(GetMessage(JsonConvert.SerializeObject(socketJsonParamEntity)));
        }

        private void SubScriptionBtn_Click(object sender, RoutedEventArgs e)
        {
            SocketJsonParamEntity socketJsonParamEntity = new SocketJsonParamEntity();
            //socketJsonParamEntity.KeyId = Guid.Parse("2B68E863-6F5D-E811-8BA9-F8A005475E49");
            //socketJsonParamEntity.NodeId = "ns=2;s=TestChannel.TestDevice.word_1";
            socketJsonParamEntity.FunctionCode = Core.FuncCode.SubScription;
            List<Guid> subIdList = new List<Guid>();
            subIdList.Add(Guid.Parse("49ad2434-7964-e811-8bad-973b85bbda56"));
            socketJsonParamEntity.SubScriptionList = subIdList;

            client.Send(GetMessage(JsonConvert.SerializeObject(socketJsonParamEntity)));
        }

        private String GetMessage(String msgBody)
        {
            return "[STX]" + msgBody + "[ETX]";
        }

        private void OnError(object sender, Exception e)
        {
            ResultTextBlock.Dispatcher.Invoke(
                           new Action(
                              delegate {
                                  ResultTextBlock.Text += Environment.NewLine;
                                  ResultTextBlock.Text += $"错误{e.ToString()}";
                                  ResultTextBlock.Text += Environment.NewLine;
                              }
                           ));
        }

        private void OnConnect(object sender, EventArgs args)
        {
            ResultTextBlock.Dispatcher.Invoke(
                            new Action(
                               delegate {
                                   ResultTextBlock.Text += Environment.NewLine;
                                   ResultTextBlock.Text += $"连接成功";
                                   ResultTextBlock.Text += Environment.NewLine;
                               }
                            ));
        }

        private void OnClose(object sender, EventArgs args)
        {
            ResultTextBlock.Dispatcher.Invoke(
                           new Action(
                              delegate {
                                  ResultTextBlock.Text += Environment.NewLine;
                                  ResultTextBlock.Text += $"连接断开";
                                  ResultTextBlock.Text += Environment.NewLine;
                              }
                           ));
        }

        private void OnReceive(object sender, DataEventArgs args)
        {
            String jsonStr = args.PackageInfo.Data;
            SocketJsonParamEntity socketJsonParamEntity = JsonConvert.DeserializeObject<SocketJsonParamEntity>(jsonStr);
            switch (socketJsonParamEntity.FunctionCode)
            {
                case FuncCode.Read:
                    {
                        if (socketJsonParamEntity.StatusCode == (uint)DeviceStatusCode.ReadOk)
                        {
                            ResultTextBlock.Dispatcher.Invoke(
                            new Action(
                               delegate {
                                   ResultTextBlock.Text += Environment.NewLine;
                                   ResultTextBlock.Text += $"读取成功！值为：{socketJsonParamEntity.Value}!";
                                   ResultTextBlock.Text += Environment.NewLine;
                               }
                            ));
                        }
                        else
                        {
                            ResultTextBlock.Dispatcher.Invoke(
                            new Action(
                              delegate {
                                  ResultTextBlock.Text += $"读取失败!";
                                  ResultTextBlock.Text += Environment.NewLine;
                              }
                            ));
                        }
                    }
                    break;
                case FuncCode.Write:
                    {
                        if (socketJsonParamEntity.StatusCode == (uint)DeviceStatusCode.WriteOk)
                        {
                            ResultTextBlock.Dispatcher.Invoke(
                            new Action(
                              delegate {
                                  ResultTextBlock.Text += $"写入成功！值为：{socketJsonParamEntity.Value}!";
                                  ResultTextBlock.Text += Environment.NewLine;
                              }
                            ));
                        }
                        else
                        {
                            ResultTextBlock.Dispatcher.Invoke(
                            new Action(
                                delegate {
                                    ResultTextBlock.Text += $"写入失败！";
                                    ResultTextBlock.Text += Environment.NewLine;
                                }
                           ));
                        }
                    }
                    break;
                case FuncCode.SubScription:
                    {
                        if (socketJsonParamEntity.StatusCode == (uint)DeviceStatusCode.SubscriptionOK)
                        {
                            ResultTextBlock.Dispatcher.Invoke(
                            new Action(
                                delegate
                                {
                                    ResultTextBlock.Text += $"{DateTime.Now}   订阅成功!";
                                    ResultTextBlock.Text += Environment.NewLine;
                                }
                           ));
                        }
                        else
                        {
                            subscriptionValue.Dispatcher.Invoke(
                            new Action(
                                delegate
                                {
                                    subscriptionValue.Content = $"{DateTime.Now}   订阅值：{socketJsonParamEntity.Value}!";
                                }
                            ));
                           // ResultTextBlock.Dispatcher.Invoke(
                           // new Action(
                           //     delegate
                           //     {
                           //         ResultTextBlock.Text += $"{DateTime.Now}   订阅值：{socketJsonParamEntity.Value}!";
                           //         ResultTextBlock.Text += Environment.NewLine;
                           //     }
                           //));
                        }
                    }
                    break;
                default:
                    break;
            }
            

        }
    }
}
