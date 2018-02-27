using System;
using System.Collections.Generic;
using System.Linq;
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
using System.IO.Ports;
using System.Threading;
using System.Windows.Threading;

namespace COM_PortsController
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        private SerialPort _inSerialPort;

        private SerialPort _outSerialPort;

        private byte _id; 

        private byte _priority;

        private bool isMonitor = false;

        private bool isWrite = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        void InitializePort(int speed)
        {
            string comSelection = (InComList.SelectedItem as ComboBoxItem).Content.ToString();
            //Port_Enable.Text = comSelection; 
            _inSerialPort = new SerialPort(comSelection, speed, Parity.None, 8, StopBits.One);
            _inSerialPort.Open();
            _inSerialPort.ErrorReceived += new SerialErrorReceivedEventHandler(serialPort_ErrorReceived);
            _inSerialPort.DataReceived += new SerialDataReceivedEventHandler(inSerialPort_DataReceived);

            comSelection = (OutComList.SelectedItem as ComboBoxItem).Content.ToString();
            _outSerialPort = new SerialPort(comSelection, speed, Parity.None, 8, StopBits.One);
            _outSerialPort.Open();
            _outSerialPort.ErrorReceived += new SerialErrorReceivedEventHandler(serialPort_ErrorReceived);
            _outSerialPort.DataReceived += new SerialDataReceivedEventHandler(outSerialPort_DataReceived);
        }

        void outSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
        }

        void inSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] data = new byte[_inSerialPort.BytesToRead];
            _inSerialPort.Read(data, 0, data.Length);
            string message = "";
            Dispatcher.Invoke(() => message = message_out.Text);


            if (DataWrapper.isToken(data))
            {
                byte reservPr = DataWrapper.getReservPriority(data);
                if ((_priority > reservPr)&&(isWrite == true))
                {
                    data = DataWrapper.setReservPriority(data, _priority);
                }
                if ((_priority == reservPr)&&(isWrite == true))
                {
                    Dispatcher.Invoke(() => message = message_out.Text);
                    data = new byte[message.Length];

                    int index = 0;
                    foreach (var ch in message)
                    {
                        data[index++] = (byte)ch;
                    }
                    byte DA = ComToSendToId();
                    data = DataWrapper.createMessage(data, DA, _id, _priority);
                    isWrite = false;
                }
                MessageSend(data);
            }
            else
            {
                byte DA = DataWrapper.getDestAdress(data);
                if (DA == _id)
                {
                    byte[] temp = DataWrapper.getInfo(data);
                    string message_temp = "";
                    foreach (var a in temp)
                    {
                        message_temp += (char)a;
                    }
                    message_in.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { message_in.Text = message_temp; }));
                    MessageSend(data);
                }
                else
                {
                    byte SA = DataWrapper.getSourceAdress(data);
                    if (SA == _id)
                    {
                        byte reservPr = DataWrapper.getReservPriority(data);
                        byte[] message1 = DataWrapper.getToken(reservPr);
                        MessageSend(message1);
                    }
                    else
                    {
                        MessageSend(data);
                    }
                    
                }
            }
            Thread.Sleep(5000);
        }

        void serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Port_Enable.Text = "ERROR!!!"; 
        }
       
        private int getSpeed()
        {
            return 9600;
        }

        private void button_Port_on_Click(object sender, RoutedEventArgs e)
        {
            _id = InComListtoId();
            _priority = SetPriority();
            int speed = getSpeed();
            InitializePort(speed);
            button_Port_on.IsEnabled = false;
            button_Port_off.IsEnabled = true;
            if (IsMonitorMode.IsChecked == true)
            {
                isMonitor = true;
                SendToken.IsEnabled = true;
            }
            Port_Enable.Text = "Port available";
            InComList.IsEnabled = false;
            OutComList.IsEnabled = false;
            message_out.Clear();
            message_in.Text = "Null";
            message_out.IsEnabled = true;
            message_in.IsEnabled = true;
            ComListToSend.IsEnabled = true;
        }

        private void button_Port_off_Click(object sender, RoutedEventArgs e)
        {
            _inSerialPort.Close();
            button_Port_off.IsEnabled = false;
            button_Port_on.IsEnabled = true;
            SendToken.IsEnabled = false;
            isMonitor = false;
            Port_Enable.Text = "Port unavailable";
            InComList.IsEnabled = true;
            OutComList.IsEnabled = true;
            message_out.Clear();
            message_in.Text = "Null";
            message_in.IsEnabled = false;
            message_out.IsEnabled = false;
            ComListToSend.IsEnabled = false;
        }

        private void MessageSend(byte[] data)
        {
            _outSerialPort.RtsEnable = true;
            _outSerialPort.Write(data, 0, data.Length);
            Thread.Sleep(100);
            _outSerialPort.RtsEnable = false;
        }

        private void button_MessageSend_Click(object sender, RoutedEventArgs e)
        {
            isWrite = true;
        }
        
        private void SendToken_Click(object sender, RoutedEventArgs e)
        {
            if ((_outSerialPort.BytesToRead == 0)&&(isMonitor == true))
            {
                byte[] token = DataWrapper.getToken();

                _outSerialPort.RtsEnable = true;
                _outSerialPort.Write(token, 0, token.Length);

                Thread.Sleep(100);
                _outSerialPort.RtsEnable = false;
                isMonitor = false;
                SendToken.IsEnabled = false;
            }
        }

        private byte ComToSendToId()
        {
            string comSelection = "";
            ComListToSend.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { comSelection = (ComListToSend.SelectedItem as ComboBoxItem).Content.ToString(); }));
            byte sel =(byte)((byte)comSelection[0] - 48);
            return sel;
        }

        private byte InComListtoId()
        {
            string comSelection = (IDIDComList.SelectedItem as ComboBoxItem).Content.ToString();
            byte sel = (byte)((byte)comSelection[0] - 48);
            return sel;
        }

        private byte SetPriority()
        {
            string priority = Priority_TextBox.Text;
            int pr = 0;
            foreach (var ch in priority)
            {
                pr *= 10;
                pr += (byte) ch - 48;
            }
            byte res = (byte)pr;
            return res; 
        }
    }
}
