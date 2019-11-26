using ClientServerUsingNamedPipes.Client;
using ClientServerUsingNamedPipes.Interfaces;
using ClientServerUsingNamedPipes.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NamedPipeService
{
    public partial class Service1 : ServiceBase
    {
        string centralPipeName = "JsonPubSubServicePipeName";

        string serviceNameKey = "ServiceName";
        
        PipeServer centralPipeServer = null;

        Dictionary<string, PipeClient> clientsList = new Dictionary<string, PipeClient>();

        Dictionary<string, string> jsonDataList = new Dictionary<string, string>();

        Dictionary<string, List<string>> subscribersList = new Dictionary<string, List<string>>();

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var autoEvent = new AutoResetEvent(false);

            if (ConfigurationManager.AppSettings.AllKeys.Contains(serviceNameKey))
            {
                centralPipeName = ConfigurationManager.AppSettings[serviceNameKey];
            }

            centralPipeServer = new PipeServer(centralPipeName);

            centralPipeServer.ClientConnectedEvent += CentralPipeServer_ClientConnectedEvent;
            centralPipeServer.ClientDisconnectedEvent += CentralPipeServer_ClientDisconnectedEvent;
            centralPipeServer.MessageReceivedEvent += CentralPipeServer_MessageReceivedEvent;

            centralPipeServer.Start();
        }

        protected override void OnStop()
        {
            if(centralPipeServer != null)
            {
                centralPipeServer.Stop();
            }
        }

        private void CentralPipeServer_MessageReceivedEvent(object sender, ClientServerUsingNamedPipes.Interfaces.MessageReceivedEventArgs e)
        {
            if (e.Type == MessageType.Register)
            {
                clientsList[e.ReceiverId] = new PipeClient(e.ReceiverId);
                clientsList[e.ReceiverId].Start();

                clientsList[e.ReceiverId].SendMessage(new MessageReceivedEventArgs() { Type = MessageType.Registered });

            }
            else if (e.Type == MessageType.Unregister)
            {
                if (clientsList.ContainsKey(e.ReceiverId))
                {
                    clientsList[e.ReceiverId].Stop();
                }
            }
            else if (e.Type == MessageType.Publish)
            {
                jsonDataList[e.DataId] = e.JsonData;

                if (clientsList.ContainsKey(e.ReceiverId))
                {
                    clientsList[e.ReceiverId].SendMessage(new MessageReceivedEventArgs() { Type = MessageType.Published, DataId = e.DataId });
                }

                if (subscribersList.ContainsKey(e.DataId))
                {
                    var list = subscribersList[e.DataId];
                    foreach (var sub in list)
                    {
                        clientsList[sub].SendMessage(new MessageReceivedEventArgs() { DataId = e.DataId, JsonData = e.JsonData, Type = MessageType.PublishSubscribed });
                    }
                }
            }

            else if (e.Type == MessageType.Subscribe)
            {
                if (!subscribersList.ContainsKey(e.DataId))
                {
                    subscribersList[e.DataId] = new List<string>();
                }

                if (!subscribersList[e.DataId].Contains(e.ReceiverId))
                    subscribersList[e.DataId].Add(e.ReceiverId);

                if (clientsList.ContainsKey(e.ReceiverId))
                {
                    clientsList[e.ReceiverId].SendMessage(new MessageReceivedEventArgs() { DataId = e.DataId, Type = MessageType.Subscribed });
                }
            }
            else if (e.Type == MessageType.Unsubscribe)
            {
                if (subscribersList.ContainsKey(e.DataId))
                {
                    if (subscribersList[e.DataId].Contains(e.ReceiverId))
                    {
                        subscribersList[e.DataId].Remove(e.ReceiverId);
                    }
                }

                if (clientsList.ContainsKey(e.ReceiverId))
                {
                    clientsList[e.ReceiverId].SendMessage(new MessageReceivedEventArgs() { DataId = e.DataId, Type = MessageType.Unsubscribed });
                }
            }
        }

        private void CentralPipeServer_ClientDisconnectedEvent(object sender, ClientServerUsingNamedPipes.Interfaces.ClientDisconnectedEventArgs e)
        {

        }

        private void CentralPipeServer_ClientConnectedEvent(object sender, ClientServerUsingNamedPipes.Interfaces.ClientConnectedEventArgs e)
        {
        }
    }
}
