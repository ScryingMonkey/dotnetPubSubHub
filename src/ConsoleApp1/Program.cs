using ClientServerUsingNamedPipes.Client;
using ClientServerUsingNamedPipes.Interfaces;
using ClientServerUsingNamedPipes.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args2)
        {

            
            Console.ReadLine();
        }

        private static void _client_MessageReceivedEvent(object sender, MessageReceivedEventArgs e)
        {
            switch(e.Type)
            {
                case MessageType.Registered:


                    break;

                case MessageType.Published:

                    break;

                case MessageType.PublishSubscribed:


                    break;
            }
        }
    }
}
