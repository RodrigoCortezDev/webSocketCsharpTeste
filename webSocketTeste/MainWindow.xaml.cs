using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
    public partial class MainWindow : Window
    {
        public Socket serverSocket;

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
                    Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                    serverSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999));
                    serverSocket.Listen(128);
                    serverSocket.BeginAccept(null, 0, OnAccept, null);
                    //serverSocket.BeginReceive(null, 0, onReceive, null);
                }
                catch 
                {
                    Dispatcher.Invoke(() => btnLigaServer.IsEnabled = true);
                }
            });
        }


        private void OnAccept(IAsyncResult result)
        {
            try
            {
                Socket client = null;
                if (serverSocket != null && serverSocket.IsBound)
                {
                    client = serverSocket.EndAccept(result);
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


        //private void onReceive(IAsyncResult result)
        //{
        //    try
        //    {
        //        Socket client = null;
        //        if (serverSocket != null && serverSocket.IsBound)
        //        {
        //            client = serverSocket.EndAccept(result);
        //        }
        //        if (client != null)
        //        {
        //            /* Handshaking and managing ClientSocket */
        //        }
        //    }
        //    catch (SocketException exception)
        //    {
        //        MessageBox.Show("Erro", exception.Message);
        //    }
        //    finally
        //    {
        //        if (serverSocket != null && serverSocket.IsBound)
        //        {
        //            serverSocket.BeginAccept(null, 0, OnAccept, null);
        //        }
        //    }
        //}
    }
}
