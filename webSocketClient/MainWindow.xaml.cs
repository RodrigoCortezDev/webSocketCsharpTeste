using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace webSocketClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnConnectServer_Click(object sender, RoutedEventArgs e)
        {
            var host = "127.0.0.1";
            var port = 9999;

            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Console.WriteLine("Establishing Connection to {0}", host);
            s.Connect(host, port);
            Console.WriteLine("Connection established");
            s.Send(new byte[0]);
        }


        public async void WebSocket()
        {
            try
            {
                Console.WriteLine("Starting EKE WSS connection");

                ClientWebSocket socket = new ClientWebSocket();
                Uri uri = new Uri("https://websocketstest.com/");
                var cts = new CancellationTokenSource();
                await socket.ConnectAsync(uri, cts.Token);

                Console.WriteLine(socket.State);

                _ = Task.Factory.StartNew(
                    async () =>
                    {
                        var rcvBytes = new byte[128];
                        var rcvBuffer = new ArraySegment<byte>(rcvBytes);
                        while (true)
                        {
                            WebSocketReceiveResult rcvResult = await socket.ReceiveAsync(rcvBuffer, cts.Token);
                            byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();
                            string rcvMsg = Encoding.UTF8.GetString(msgBytes);
                            Console.WriteLine("Received: {0}", rcvMsg);
                        }
                    }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                while (true)
                {
                    //var message = Console.ReadLine();
                    //if (message == "Bye")
                    //{
                    //    cts.Cancel();
                    //    return;
                    //}
                    byte[] sendBytes = Encoding.UTF8.GetBytes("asdasdasd");
                    var sendBuffer = new ArraySegment<byte>(sendBytes);
                    await socket.SendAsync(sendBuffer, WebSocketMessageType.Text, endOfMessage: true, cancellationToken: cts.Token);
                }

            }
            catch
            {

            }
        }
    }
}
