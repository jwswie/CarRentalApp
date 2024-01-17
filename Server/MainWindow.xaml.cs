using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static private int messageCount = 0;
        static private string[] messageColors = { "#FFFCA590", "#FFFFFFA0", "#FFCDEFF1", "#FFF8E7B4" };

        private TcpListener server;
        private TcpClient client;
        private NetworkStream stream;

        public MainWindow()
        {
            InitializeComponent();
            Closed += Window_Closed;
            StartServer();
        }

        private async void StartServer()
        {
            server = new TcpListener(IPAddress.Any, 8888);
            server.Start();

            client = await server.AcceptTcpClientAsync();
            stream = client.GetStream();

            User user = new User() { Username = "admin" };
            UserMessageViewModel userMessageViewModel = new UserMessageViewModel(user);
            
            // Start a task to continuously read from the server
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        byte[] buffer = new byte[1024];
                        int byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);
                        string responseData = Encoding.UTF8.GetString(buffer, 0, byteCount);

                        // Dispatcher.Invoke(() => receivedTextBox.AppendText(responseData + Environment.NewLine));
                        Dispatcher.Invoke(() => userMessageViewModel.AddMessage(user, responseData, MessageContainer, MyScrollViewer, "client"));
                    }
                }
                catch (IOException ex)
                {
                    // Handle IOException when the server disconnects
                    MessageBox.Show("Server disconnected: " + ex);
                }
            });
        }


        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                User user = new User() { Username = "admin" };
                UserMessageViewModel userMessageViewModel = new UserMessageViewModel(user);
                string dataString = EnterTextBox.Text;
                byte[] data = Encoding.UTF8.GetBytes(dataString);
                await stream.WriteAsync(data, 0, data.Length);
                EnterTextBox.Clear();
                await Dispatcher.InvokeAsync(() =>
                {
                    userMessageViewModel.AddMessage(user, dataString, MessageContainer, MyScrollViewer, "server");
                    DataContext = userMessageViewModel;
                });
            }
            catch (IOException ex)
            {
                // Handle IOException when the server disconnects
                MessageBox.Show("Server disconnected: " + ex);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            client.Close();
        }
    }
}
