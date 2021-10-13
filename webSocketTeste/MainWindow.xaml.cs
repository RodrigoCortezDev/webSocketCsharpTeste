using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace webSocketTeste
{
    public class StateObject
    {
        // Size of receive buffer.  
        public const int BufferSize = 1024;

        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];

        // Received data string.
        public StringBuilder sb = new StringBuilder();

        // Client socket.
        public Socket workSocket = null;
    }

    public partial class MainWindow : Window
    {
        public Socket serverSocket;
        public Socket client;
        public StateObject state = new StateObject();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLigaServer_Click(object sender, RoutedEventArgs e)
        {
            btnLigaServer.IsEnabled = false;
            Task.Run(() => 
            {
                try
                {
                    serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                    serverSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999));
                    serverSocket.Listen(128);
                    serverSocket.BeginAccept(null, 0, OnAccept, null);
                }
                catch (Exception e)
                {
                    Dispatcher.Invoke(() => MessageBox.Show("Erro: "+ e.Message));
                    Dispatcher.Invoke(() => btnLigaServer.IsEnabled = true);
                }
            });
        }


        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket cliente = state.workSocket;

            // Read data from the client socket.
            int bytesRead = cliente.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read
                // more data.  
                content = state.sb.ToString();

                Dispatcher.Invoke(new Action(() =>
                {
                    txtMsg.Text = DateTime.Now+" Cliente: "+ content + Environment.NewLine + txtMsg.Text;
                }));

                //for (int i = 0; i < 100; i++)
                {
                    cliente.Send(Encoding.ASCII.GetBytes(DateTime.Now + " Recebido: " + content));
                    //Thread.Sleep(1000);
                }

                // Not all data received. Get more.  
                cliente.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);  
            }
        }


        private void OnAccept(IAsyncResult result)
        {
            try
            {
                
                if (serverSocket != null && serverSocket.IsBound)
                {
                    client = serverSocket.EndAccept(result);

                    byte[] msg = Encoding.ASCII.GetBytes("Brancao na Escuta");

                    client.Send(msg);

                    StateObject state = new StateObject();
                    state.workSocket = client;
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                }
                if (client != null)
                {
                    /* Handshaking and managing ClientSocket */
                }
            }
            catch (SocketException exception)
            {
                MessageBox.Show("Erro",exception.Message);
            }
            finally
            {
                if (serverSocket != null && serverSocket.IsBound)
                {
                    serverSocket.BeginAccept(null, 0, OnAccept, null);
                }
            }
        }




        private void onReceive(IAsyncResult result)
        {
            try
            {
                if (serverSocket != null && serverSocket.IsBound)
                {
                    int bytesRead = serverSocket.EndReceive(result);

                    byte[] msg = Encoding.ASCII.GetBytes("Buuuurro");

                    client.Send(msg);
                }
                if (client != null)
                {
                    /* Handshaking and managing ClientSocket */
                }
            }
            catch (SocketException exception)
            {
                MessageBox.Show("Erro", exception.Message);
            }
            finally
            {
                if (serverSocket != null && serverSocket.IsBound)
                {
                    serverSocket.BeginAccept(null, 0, onReceive, serverSocket);
                }
            }
        }

        private void btnEnviarMsg_Click(object sender, RoutedEventArgs e)
        {
            if (client != null)
            {
                byte[] msg = Encoding.ASCII.GetBytes(txtEnvio.Text);

                client.Send(msg);
            }
        }
    }
}
