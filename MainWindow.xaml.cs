using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Messaging;
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

namespace COM_communication
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker backgroundWorker1 = new BackgroundWorker();
        public MainWindow()
        {
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            InitializeComponent();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.comboBox1.Items.Clear();
            string[] portList = System.IO.Ports.SerialPort.GetPortNames();
            for (int i = 0; i < portList.Length; i++)
            {
                string name = portList[i];
                this.comboBox1.Items.Add(name);
            }
        }

        SerialPort _serialPort;
        Thread readThread;


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            toolStripStatusLabel1.Text = "打开选择的串口:" + this.comboBox1.SelectedItem.ToString();
            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort(this.comboBox1.SelectedItem.ToString());//端口

            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            readThread = new Thread(Read);

            // Allow the user to set the appropriate properties.
            //_serialPort.PortName = SetPortName(_serialPort.PortName);
            _serialPort.BaudRate = 9600;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.Encoding = Encoding.GetEncoding("GB2312");
            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();
            readThread.Start();
            //_serialPort.WriteLine(richTextBox1.Text);
            //readThread.Join();
            //_serialPort.Close();
            toolStripStatusLabel1.Text = "打开串口成功";
            this.dakai_button.IsEnabled = false;

        }



        private void Read()
        {
            bool? temp_checkBox1=false;
            while (true)
            {
                try
                {                    
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        temp_checkBox1 = checkBox1.IsChecked;
                    }));
                    string message = _serialPort.ReadLine();
                    Dispatcher.Invoke(new Action(delegate { richTextBox2.Text = message; }));
                    if (message != null)
                    {
                        if (temp_checkBox1 == true)//判断是否开启消息队列
                        {
                            Dispatcher.Invoke(new Action(delegate { toolStripStatusLabel2.Text = "准备写入消息队列..."; }));
                            try
                            {
                                MessageQueue myQueue1 = MessageQueue.Exists(".\\Private$\\myQueue") ? new MessageQueue(".\\Private$\\myQueue") : MessageQueue.Create(".\\Private$\\myQueue");
                                myQueue1.Send(message);
                                Dispatcher.Invoke(new Action(delegate { toolStripStatusLabel2.Text = "成功写入消息队列"; }));
                            }
                            catch (Exception)
                            {

                                throw;
                            }
                        }
                    }
                }
                catch (TimeoutException) { }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            toolStripStatusLabel1.Text = "准备发送...";
            _serialPort.WriteLine(richTextBox1.Text);
            toolStripStatusLabel1.Text = "发送成功";
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (readThread.IsAlive)
            {
                readThread.Abort();
            }
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }


        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
            {
                backgroundWorker1.RunWorkerAsync();
                //打开后台读取服务
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            MessageQueue mq = new MessageQueue(".\\Private$\\myQueue");
            while (true)
            {
                Message message = mq.Receive();
                message.Formatter = new XmlMessageFormatter(new Type[] { typeof(String) });
                MessageBox.Show((string)message.Body);
                // Process the journal message.
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageQueue myQueue1 = MessageQueue.Exists(".\\Private$\\myQueue") ? new MessageQueue(".\\Private$\\myQueue") : MessageQueue.Create(".\\Private$\\myQueue");
                myQueue1.Send("本条信息非com口读取到，而是模板自带");
                toolStripStatusLabel2.Text = "成功写入消息队列";
            }
            catch
            {
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
