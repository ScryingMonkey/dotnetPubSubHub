using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using ClientServerUsingNamedPipes.Interfaces;
using ClientServerUsingNamedPipes.Server;
using ClientServerUsingNamedPipes.Utilities;
using Newtonsoft.Json;

namespace ClientServerUsingNamedPipes.Client
{
    public class PipeClient : ICommunicationClient
    {
        #region private fields

        private readonly NamedPipeClientStream _pipeClient;

        #endregion

        public string ReceiverId { get; set; }

        public bool IsDuplex { get; set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceivedEvent;

        private PipeServer _receiveServer { get; set; }

        #region c'tor

        public PipeClient(string serverId, bool isDuplex = false)
        {
            _pipeClient = new NamedPipeClientStream(".", serverId, PipeDirection.InOut, PipeOptions.Asynchronous);

            IsDuplex = isDuplex;

            if(IsDuplex)
                ReceiverId = Guid.NewGuid().ToString();
        }

        #endregion

        #region ICommunicationClient implementation

        /// <summary>
        /// Starts the client. Connects to the server.
        /// </summary>
        public void Start()
        {
            const int tryConnectTimeout = 5 * 1000; // 5 minutes
            _pipeClient.Connect(tryConnectTimeout);

            if (IsDuplex)
            {
                _receiveServer = new PipeServer(ReceiverId);

                _receiveServer.MessageReceivedEvent += _receiveServer_MessageReceivedEvent;

                _receiveServer.Start();

                SendMessage(new ClientServerUsingNamedPipes.Interfaces.MessageReceivedEventArgs() { ReceiverId = ReceiverId, Type = MessageType.Register });
            }
        }

        private void _receiveServer_MessageReceivedEvent(object sender, MessageReceivedEventArgs e)
        {
            if (MessageReceivedEvent != null)
            {
                MessageReceivedEvent(sender, e);
            }
        }

        /// <summary>
        /// Stops the client. Waits for pipe drain, closes and disposes it.
        /// </summary>
        public void Stop()
        {
            try
            {
                if (IsDuplex)
                {
                    SendMessage(new ClientServerUsingNamedPipes.Interfaces.MessageReceivedEventArgs() { ReceiverId = ReceiverId, Type = MessageType.Unregister });
                    if(_receiveServer != null)
                    {
                        _receiveServer.Stop();
                    }
                }

                _pipeClient.WaitForPipeDrain();
            }
            finally
            {
                _pipeClient.Close();
                _pipeClient.Dispose();
            }
        }

        public Task<TaskResult> SendMessage(MessageReceivedEventArgs args)
        {
            var taskCompletionSource = new TaskCompletionSource<TaskResult>();

            try
            {
                if (_pipeClient.IsConnected)
                {
                    args.ReceiverId = this.ReceiverId;

                    var message = JsonConvert.SerializeObject(args);
                    var buffer = Encoding.UTF8.GetBytes(message);
                    _pipeClient.BeginWrite(buffer, 0, buffer.Length, asyncResult =>
                    {
                        try
                        {
                            taskCompletionSource.SetResult(EndWriteCallBack(asyncResult));
                        }
                        catch (Exception ex)
                        {
                            taskCompletionSource.SetException(ex);
                        }

                    }, null);
                }
                else
                {
                    Logger.Error("Cannot send message, pipe is not connected");
                    throw new IOException("pipe is not connected");
                }
            }
            catch(Exception ex)
            {
                taskCompletionSource.SetException(ex);
            }

            return taskCompletionSource.Task;
        }

        #endregion


        #region private methods

        /// <summary>
        /// This callback is called when the BeginWrite operation is completed.
        /// It can be called whether the connection is valid or not.
        /// </summary>
        /// <param name="asyncResult"></param>
        private TaskResult EndWriteCallBack(IAsyncResult asyncResult)
        {
            _pipeClient.EndWrite(asyncResult);
            _pipeClient.Flush();

            return new TaskResult {IsSuccess = true};
        }

        #endregion
    }
}
