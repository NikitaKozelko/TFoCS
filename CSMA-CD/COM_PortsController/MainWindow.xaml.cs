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
        private int lastChar = 1;
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
            serialPort.Read(data, 0, data.Length);
            data = ByteStuffing.Reverse(data, ID);
            
            if (data != null)
            {
                string message_temp = "";
                
                data[0] = (byte) ' ';
                int j = 0;
               
                    if (data[1] == 36)
                    {
                        message_temp += "X";
                    }
                    else
                    {
                        if ((lastChar != 36) && (lastChar != 1))
                            message_temp += (char) lastChar;
                    }
                for (int i = 1; i < data.Length-1; i++)
                {
                    if (data[i] != 36)
                    {
                        if (data[i + 1] != 36)
                        {
                            message_temp += (char) data[i];
                        }
                        else
                        {
                            message_temp += "X";
                            i++;
                        }
                    }
                }
                if ((data[data.Length - 1] == 126)||(data[data.Length - 1])==125)
                    lastChar = 1;
                else lastChar = data[data.Length - 1]; 
                message_in.Dispatcher.Invoke(DispatcherPriority.Background,
                    new Action(() => { message_in.Text += message_temp; }));
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
            message_in.Text = "";
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
            message_in.Text = "";
            message_in.IsEnabled = false;
            message_out.IsEnabled = false; 
        }

        private void writeToSerialPort(byte a, byte IDto)
        {
            byte[] newData = new byte[1];
            newData[0] = a;
            newData = ByteStuffing.Direct(newData, IDto, ID);
            serialPort.RtsEnable = true;
            serialPort.Write(newData, 0, newData.Length);
            serialPort.RtsEnable = false;
            Thread.Sleep(100);
        }

        private void button_MessageSend_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show((" " + (byte)'}'));
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
                
                for (int i = 0; i < data.Length; i++)
                {
                    int countAttempt = 0;
                    byte[] newData = new byte[1];
                    newData[0] = data[i]; 
                    newData = ByteStuffing.Direct(newData, IDto, ID);

                    TimeSpan elapsedSpan = currentTimeSpan();
                    while (elapsedSpan.Seconds % 2 == 0)
                    {

                        countAttempt++;
                        Thread.Sleep(getDelayAttempt(countAttempt));
                        elapsedSpan = currentTimeSpan();
                    }
                    
                    elapsedSpan = currentTimeSpan();
                    
                    while (elapsedSpan.Milliseconds % 2 == 0)
                    {
                        writeToSerialPort(data[i], IDto);
                        writeToSerialPort(36, IDto);
                        elapsedSpan = currentTimeSpan();
                    }
                    writeToSerialPort(data[i], IDto);
                }
                writeToSerialPort(126, IDto);
            }
        }

        private TimeSpan currentTimeSpan()
        {
            DateTime centuryBegin = new DateTime(2001, 1, 1);
            DateTime currentDate = DateTime.Now;
            long elapsedTicks = currentDate.Ticks - centuryBegin.Ticks;
            TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
            return elapsedSpan;

        }
        private int getDelayAttempt(double countAttempt)
        {
            double maxCountAttempt = 10.0; 
            Random rand = new Random((int) Math.Pow(2, (double)Math.Min(maxCountAttempt, countAttempt)));
            int a = rand.Next();
            if (a > 500) a = 501;
            return a;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            message_in.Text = "";
        }
    }
}
