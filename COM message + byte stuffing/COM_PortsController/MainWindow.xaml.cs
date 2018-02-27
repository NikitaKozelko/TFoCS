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
        SerialPort serialPort;
        private byte ID;
        public MainWindow()
        {
            InitializeComponent();
        }

        void InitializePort(int speed)
        {
            string comSelection = (ComList.SelectedItem as ComboBoxItem).Content.ToString();
            if (comSelection == "COM1")
                ID = 1;
            else
                ID = 2;
            //Port_Enable.Text = comSelection; 
            serialPort = new SerialPort(comSelection, speed, Parity.None, 8, StopBits.One);
            serialPort.Open();
            serialPort.ErrorReceived += new SerialErrorReceivedEventHandler(serialPort_ErrorReceived);
            serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
        }

        void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] data = new byte[serialPort.BytesToRead];
            MessageBox.Show(data.Length.ToString());
            serialPort.Read(data, 0, data.Length);
            data = ByteStuffing.Reverse(data, ID);
            if (data != null)
            {
                string message_temp = "";
                if (data[0] == 1)
                    message_temp = "Message from COM1";
                else
                    message_temp = "Message from COM2";
                data[0] = (byte) ' ';
                foreach (var a in data)
                {
                    message_temp += (char) a;
                }
                message_in.Dispatcher.Invoke(DispatcherPriority.Background,
                    new Action(() => { message_in.Text = message_temp; }));
            }
        }

        void serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Port_Enable.Text = "ERROR!!!"; 
        }
       
        private int getSpeed()
        {
            int speed = (int)speed_slider.Value;
            int[] a = new int[]{ 50, 75, 150, 300, 600, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200}; 
            for (int i=0; i<a.Length; i++)
            {
                if (speed >= a[i]) return a[i];
            }
            return 115200;
        }

        private void button_Port_on_Click(object sender, RoutedEventArgs e)
        {
            
            int speed = getSpeed();
            InitializePort(speed);
            button_Port_on.IsEnabled = false;
            button_Port_off.IsEnabled = true;
            speed_slider.IsEnabled = false;
            Port_Enable.Text = "Port available";
            ComList.IsEnabled = false;
            message_out.Clear();
            message_in.Text = "Null";
            message_out.IsEnabled = true;
            message_in.IsEnabled = true; 
        }

        private void button_Port_off_Click(object sender, RoutedEventArgs e)
        {
            serialPort.Close();
            button_Port_off.IsEnabled = false;
            button_Port_on.IsEnabled = true;
            speed_slider.IsEnabled = true;
            Port_Enable.Text = "Port unavailable";
            ComList.IsEnabled = false;
            message_out.Clear();
            message_in.Text = "Null";
            message_in.IsEnabled = false;
            message_out.IsEnabled = false; 
        }

        private void button_MessageSend_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort.BytesToRead == 0)
            {
                string message = message_out.Text;
                byte[] data = new byte[message.Length];
                
                int index = 0;
                foreach (var ch in message)
                {
                    data[index++] = (byte)ch;
                }
                byte IDto;
                string comSelection = (MessageTo.SelectedItem as ComboBoxItem).Content.ToString();
                if (comSelection == "COM1")
                    IDto = 1;
                else
                    IDto = 2;

                byte[] newData = ByteStuffing.Direct(data, IDto, ID);
                MessageBox.Show(newData.Length.ToString());
                serialPort.RtsEnable = true;
                serialPort.Write(newData, 0, newData.Length);

                Thread.Sleep(100);
                serialPort.RtsEnable = false;
            }
        }

      
    }
}
