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

namespace CRC
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private int[] key = { 1, 1, 0, 1};
        private int[] AddComboBoxInfo(int a)
        {
            string selectionBits = (BitsList.SelectedItem as ComboBoxItem).Content.ToString();
            int[] res = new int[4];
            string temp = a + "";
            if (temp.Length == 7) temp = "0" + temp;
            if (selectionBits.Equals("0-3"))
                for (int i = 0; i < 4; i++)               
                    res[i] = (byte) temp[i] - 48;
            if (selectionBits.Equals("4-7"))
                for (int i = 4; i < 8; i++)
                    res[i-4] = (byte) temp[i] - 48;
            if (selectionBits.Equals("3-0"))
                for (int i = 3; i >=0 ; i--)
                    res[i] = (byte)temp[i] - 48;
            if (selectionBits.Equals("7-4"))
                for (int i = 7; i >=4; i--)
                    res[i-4] = (byte)temp[i] - 48;
            return res;
        }

        private int CharToBite(int a)
        {
            int res = 0;
            int step = 1;
            while (a > 0)
            {
                res += (a % 2) * step;
                a /= 2;
                step *= 10;
            }
            return res;
        }

        //будет работать только если на вход подавать массив из 4ех битов
        private int[] Encode_data(int[] inData)
        {
            int[] tempData = new int[7];
            for (int i = 0; i < 7; i++)
            {
                if (i < 4) tempData[i] = inData[i];
                else tempData[i] = 0; 
            }

            for (int i = 0; i < 4; i++)
            {
                if (tempData[i] != 0)
                {   
                    for (int j = 0; j < 4; j++)
                        tempData[i + j] = (tempData[i + j] + key[j]) % 2;
                }
            }
            for (int i = 0; i < 4; i++)
            {
                tempData[i] = inData[i];
            }
            return tempData;
        }
        private void Encode_Click(object sender, RoutedEventArgs e)
        {
            String inStr = in_box_dir.Text;
            int temp = CharToBite((byte)inStr[0]);
            string tempstr = temp + "";
            if (tempstr.Length == 7) tempstr ="0"+tempstr; 
            char_bite_format.Text = tempstr;
            int[] aa = AddComboBoxInfo(temp);
            aa = Encode_data(aa);
            encode_data.Text = String.Join("", aa);
        }

        private void Decode_Click(object sender, RoutedEventArgs e)
        {
            String inStr = in_st_rvrs.Text;
            int[] codeData = new int[7];
            for (int i = 0; i < 7; i++)
            {
                codeData[i] = (byte) inStr[i] - 48;
            }

            int shift = 0;
            int check = Decode_data(codeData);
            if (check != 0)
            {
                MessageBox.Show("Data has error. \nNow try to correct it");
            }

            while (true)
            {
                check = Decode_data(codeData);
                
                if (check <= 1)
                {
                    codeData[6] += check;
                    for (int i = 6; i > 0; i--)
                    {
                        codeData[i-1] = codeData[i-1] + codeData[i] / 2;
                        codeData[i] = codeData[i] % 2;
                    }
                    if (codeData[0] > 1)
                    {
                        MessageBox.Show("It's impossible to decode the data");
                        decode_data.Text = "Error";
                        break;
                    }
                    for (int i = 0; i < shift; i++)
                    {
                        codeData = ShiftRight(codeData);
                    }
                    int[] ans = new int[4];
                    for (int i = 0; i < 4; i++) ans[i] = codeData[i];

                    decode_data.Text = String.Join("", ans);
                    break;
                }
                if (shift > 7)
                {
                    MessageBox.Show("It's impossible to decode the data");
                    decode_data.Text = "ERROR" ;
                    break;
                }
                shift++;
                codeData = ShiftLeft(codeData);
            }
        }
        
        //toDO что возвращает 
        private int[] ShiftRight(int[] arr)
        {
            int temp = arr[6];
            for (int i = 6; i > 0; i--)
            {
                arr[i] = arr[i - 1];
            }
            arr[0] = temp;
            return arr;
        }

        private int[] ShiftLeft(int[] arr)
        {
            int temp = arr[0];
            for (int i = 0; i < 6; i++)
            {
                arr[i] = arr[i + 1];
            }
            arr[6] = temp;
            return arr;
        }

        private int Decode_data(int[] arr)
        {
            int[] tempArr = new int[7];
            for (int i = 0; i < 7; i++)
            {
                tempArr[i] = arr[i];
            }
            for (int i = 0; i < 4; i++)
            {
                if (tempArr[i] != 0)
                {
                    for (int j = 0; j < 4; j++)
                        tempArr[i + j] = (tempArr[i + j] + key[j]) % 2;
                }
            }
            return (tempArr[4]+tempArr[5]+tempArr[6]);
        }
    }
}
