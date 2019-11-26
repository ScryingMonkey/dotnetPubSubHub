using System;

namespace ClientServerUsingNamedPipes.Interfaces
{
    public interface ICommunicationServer : ICommunication
    {
        /// <summary>
        /// The server id
        /// </summary>
        string ServerId { get; }

        /// <summary>
        /// This event is fired when a message is received 
        /// </summary>
        event EventHandler<MessageReceivedEventArgs> MessageReceivedEvent;

        /// <summary>
        /// This event is fired when a client connects 
        /// </summary>
        event EventHandler<ClientConnectedEventArgs> ClientConnectedEvent;

        /// <summary>
        /// This event is fired when a client disconnects 
        /// </summary>
        event EventHandler<ClientDisconnectedEventArgs> ClientDisconnectedEvent;
    }

    public class ClientConnectedEventArgs : EventArgs
    {
        public string ClientId { get; set; }
    }

    public class ClientDisconnectedEventArgs : EventArgs
    {
        public string ClientId { get; set; }
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        public string ReceiverId { get; set; }
        public MessageType Type { get; set; }
        public string DataId { get; set; }
        public string JsonData { get; set; }
    }

    public enum MessageType
    {
        Register,
        Unregister,
        Publish,
        Subscribe,
        Unsubscribe,
        Registered,
        Published,
        Subscribed,
        Unsubscribed,
        PublishSubscribed
    }
}
