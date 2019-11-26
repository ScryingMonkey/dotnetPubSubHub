using ClientServerUsingNamedPipes.Client;
using ClientServerUsingNamedPipes.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            this.FormClosed += Form1_FormClosed;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                foreach (var item in lstSubscribedChannels.Items)
                {
                    var dataId = item as string;

                    var result = _client.SendMessage(new MessageReceivedEventArgs() { DataId = dataId, Type = MessageType.Unsubscribe });
                }
            }
            catch(Exception ex)
            {

            }
        }

        PipeClient _client = null;

        private string serviceNameKey = "ServiceName";
        
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                var serviceName = "JsonPubSubServicePipeName";
                if (ConfigurationManager.AppSettings.AllKeys.Contains(serviceNameKey))
                {
                    serviceName = ConfigurationManager.AppSettings[serviceNameKey];
                }

                _client = new PipeClient(serviceName, true);

                _client.MessageReceivedEvent += _client_MessageReceivedEvent;

                _client.Start();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Cannot connect to the service");
            }
        }

        private void _client_MessageReceivedEvent(object sender, ClientServerUsingNamedPipes.Interfaces.MessageReceivedEventArgs e)
        {
            switch (e.Type)
            {
                case MessageType.Registered:
                    txtStatus.Text = "Connected";
                    
                    break;

                case MessageType.Published:
                    txtStatus.Text = "Data has been published to data channel - " + e.DataId;

                    break;

                case MessageType.Subscribed:
                    txtStatus.Text = "You have been subscribed to data channel - " + e.DataId;

                    if(!lstSubscribedChannels.Items.Contains(e.DataId))
                    {
                        lstSubscribedChannels.Items.Add(e.DataId);
                    }

                    break;

                case MessageType.Unsubscribed:
                    txtStatus.Text = "You have been unsubscribed from data channel - " + e.DataId;

                    if (lstSubscribedChannels.Items.Contains(e.DataId))
                    {
                        lstSubscribedChannels.Items.Remove(e.DataId);
                    }

                    break;

                case MessageType.PublishSubscribed:
                    txtStatus.Text = "You received published data from channel - " + e.DataId;

                    lstReceivedData.Items.Add(new SubscribedData() { DataId = e.DataId, DateTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") , Data = e.JsonData });

                    break;
            }
        }

        private void btnPublish_Click(object sender, EventArgs e)
        {
            var result = _client.SendMessage(new MessageReceivedEventArgs() { DataId = txtPublishDataID.Text, JsonData = txtPublishData.Text, Type = MessageType.Publish });
            if (result.IsFaulted)
            {
                MessageBox.Show("Cannot publish now");
            }

            txtPublishDataID.Text = string.Empty;
            txtPublishData.Text = string.Empty;
        }

        private void btnSubscribed_Click(object sender, EventArgs e)
        {
            var result = _client.SendMessage(new MessageReceivedEventArgs() { DataId = txtSubscribeDataID.Text, Type = MessageType.Subscribe });
            if(result.IsFaulted)
            {
                MessageBox.Show("Cannot subscribe now");
            }

            txtSubscribeDataID.Text = string.Empty;
        }

        private void btnUnsubscribe_Click(object sender, EventArgs e)
        {
            if(lstSubscribedChannels.SelectedItem != null)
            {
                var dataId = lstSubscribedChannels.SelectedItem as string;

                var result = _client.SendMessage(new MessageReceivedEventArgs() { DataId = dataId, Type = MessageType.Unsubscribe });
                if (result.IsFaulted)
                {
                    MessageBox.Show("Cannot unsubscribe now");
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(lstReceivedData.SelectedItem != null)
            {
                var receivedData = lstReceivedData.SelectedItem as SubscribedData;
                if(receivedData != null)
                {
                    txtReceivedDataID.Text = receivedData.DataId;
                    txtReceivedData.Text = receivedData.Data;
                    txtReceivedTime.Text = receivedData.DateTime;
                }
            }
        }
    }

    class SubscribedData
    {
        public override string ToString()
        {
            return "From channel - '" + DataId + "' at " + DateTime;
        }

        public string DataId { get; set; }

        public string DateTime { get; set; }

        public string Data { get; set; }
    }
}
