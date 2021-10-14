using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Timers;
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
    public class StateObject : INotifyPropertyChanged
    {
        public int _index;
        // Size of receive buffer.  
        public const int BufferSize = 1024;

        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];

        // Received data string.
        public StringBuilder sb = new StringBuilder();

        // Client socket.
        public Socket client = null;

        public string dadosCliente
        {
            get { return $"Remote EndPoint: {client?.RemoteEndPoint?.ToString()}"; }
        }

        public int index
        {
            get { return _index; }
            set { _index = value; }
        }

        public string status
        {
            get 
            {
                if (client != null)
                {
                    return client.Connected ? "Conectado" : "Desconectado";
                }

                return " - ";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public partial class MainWindow : Window
    {
        public int countClient = 0;
        public Socket serverSocket;
        public StateObject state = new StateObject();
        public ObservableCollection<StateObject> listClient = new ObservableCollection<StateObject>();
        System.Timers.Timer timer = null;

        public MainWindow()
        {
            InitializeComponent();

            gridClient.ItemsSource = listClient;

            timer = new System.Timers.Timer(1000);
            timer.Elapsed += async (sender, e) => await AtualizaStatus();
            timer.Start();
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
            Socket cliente = state.client;

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
                Socket client = null;
                if (serverSocket != null && serverSocket.IsBound)
                {
                    client = serverSocket.EndAccept(result);

                    byte[] msg = Encoding.ASCII.GetBytes("Brancao na Escuta");

                    client.Send(msg);

                    StateObject state = new StateObject();
                    state.client = client;
                    state.index = ++countClient;
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);

                    App.Current.Dispatcher.Invoke((Action)delegate 
                    {
                        listClient.Add(state);
                    });
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

        private async Task AtualizaStatus()
        {
            foreach (var item in listClient)
            {
                try
                {
                    if (item.client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];
                        if (item.client.Receive(buff, SocketFlags.Peek) == 0)
                        {
                            item.client.Close();
                            // Client disconnected
                            item.OnPropertyChanged("status");
                        }
                    }
                }
                catch
                {
                    item.OnPropertyChanged("status");
                }
            }
        }

        private void btnEnviarMsg_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in gridClient.SelectedItems)
            {
                var clientAux = ((StateObject)item).client;

                byte[] msg = Encoding.ASCII.GetBytes(txtEnvio.Text);

                clientAux?.Send(msg);
            }
        }
    }
}
