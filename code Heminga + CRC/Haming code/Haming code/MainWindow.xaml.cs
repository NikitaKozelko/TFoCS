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

namespace Haming_code
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void direct_coding_Click(object sender, RoutedEventArgs e)
        {
            String str = in_str_dir.Text;
            int[] a = new int[str.Length*8];
            a = Str_to_Bit(str, a);
            str_bit_dir.Text = String.Join("", a);
            a = addHammingCode(a);
            out_str_dir.Text = String.Join("", a);
        }

        private int[] Str_to_Bit(string str, int[] a)
        {
            for (int i = 0; i < str.Length * 8 - 1; i++)
            {
                a[i] = 0;
            }
            for (int i = 0; i < str.Length; i++)
            {
                int temp = (int) str[i];
                int j = (i+1) * 8 - 1;
                
                while (temp > 0)
                {
                    a[j--] = temp % 2;
                    temp = temp / 2; 
                }

            }
            return a;
        }

        private int[] addHammingCode(int[] a)
        {
            int[] temp = new int[a.Length+a.Length / 16 * 5];
            for (int i = 0; i < (a.Length + a.Length / 16 * 5); i++)
                temp[i] = 0;

            int count = a.Length / 16;

            while (count > 0)
            {
                temp = addHammingCodePart(a, temp, a.Length / 16 - count);
                count--;
            }
            return temp;
        }

        private int[] addHammingCodePart(int[] a, int[] temp, int part)
        {
            int j = part * 16;
            part = part * 21;
            temp[2 + part] = a[j++];
            for (int i = 4; i <= 6; i++)
                temp[i + part] = a[j++];
            for (int i = 8; i <= 14; i++)
                temp[i + part] = a[j++];
            for (int i = 16; i <= 20; i++)
                temp[i + part] = a[j++];

            for (int i = 0; i <= 20; i+=2)
                temp[0 + part] = (temp[0 + part] + temp[i+part]) % 2;
            for (int i = 1; i <= 20; i += 4)
                temp[1 + part] = (temp[1 + part] + temp[i + part] + temp[i + 1 + part]) % 2;
            for (int i = 3; i <= 18; i += 8)
                temp[3 + part] = (temp[3 + part] + temp[i + part] + temp[i + 1 + part] + temp[i + 2 + part] + temp[i + 3 + part]) % 2;
            temp[3 + part] = (temp[3 + part] + temp[19 + part] + temp[20 + part]) % 2; 
            for (int i = 7; i < 15; i++)
                temp[7 + part] = (temp[7 + part] + temp[i + part]);
            temp[7 + part] %= 2;
            for (int i = 16; i <= 20; i++)
                temp[15 + part] = (temp[15 + part] + temp[i + part]);
            temp[15 + part] %= 2;
            return temp;
        }
             
        private void reverse_coding_Click(object sender, RoutedEventArgs e)
        {
            String str = in_str_rvrs.Text;
            int[] a = new int[str.Length];
            for (int i = 0; i < str.Length; i++)
                a[i] = (byte)str[i]-48;
            a = deleteHammingCode(a);
            if (a == null)
            {
                str_bit_rvrs.Text = "Была обнаружена ошибка";
                out_str_rvrs.Text = "";
            }
            else
            {
                str_bit_rvrs.Text = String.Join("", a);
                str = "";
                for (int i = 0; i < a.Length / 8; i++)
                {
                    int j = (i + 1) * 8 - 1;
                    int pow2 = 1;
                    int ans=0;
                    while (j >= i * 8)
                    {
                        ans =ans + a[j--] * pow2;
                        pow2 *= 2;
                    }
                    str += "" + (char)ans;
                }
                out_str_rvrs.Text = str;
            }
        }

        private int[] deleteHammingCode(int[] a)
        {
            int[] temp = new int[a.Length - a.Length / 21 * 5];
            
            int count = a.Length / 21;

            while (count > 0)
            {
                temp = deleteHammingCodePart(temp, a, a.Length / 21 - count);
                if (temp == null) return null;
                count--;
            }
            return temp;
        }

        private int[] deleteHammingCodePart(int[] a,int[] temp, int part)
        {
            part = part * 21;
            int temp0 = temp[0 + part];
            int temp1 = temp[1 + part];
            int temp3 = temp[3 + part];
            int temp7 = temp[7 + part];
            int temp15 = temp[15 + part];

            for (int i = 0; i <= 20; i += 2)
                temp0 = (temp0 + temp[i + part]) % 2;
            for (int i = 1; i <= 20; i += 4)
                temp1 = (temp1 + temp[i + part] + temp[i + 1 + part]) % 2;
            for (int i = 3; i <= 18; i += 8)
                temp3 = (temp3 + temp[i + part] + temp[i + 1 + part] + temp[i + 2 + part] + temp[i + 3 + part]) % 2;
            temp3 = (temp3 + temp[19 + part] + temp[20 + part]) % 2;
            for (int i = 7; i < 15; i++)
                temp7 = (temp7 + temp[i + part]);
            temp7 %= 2;
            for (int i = 15; i <= 20; i++)
                temp15 = (temp15 + temp[i + part]);
            temp15 %= 2;

            if ((temp0 != temp[0 + part]) || (temp1 != temp[1 + part]) || (temp3 != temp[3 + part]) ||
                  (temp7 != temp[7 + part]) || (temp15 != temp[15 + part]))
            {
                MessageBox.Show("В сообщении обнаружена ошибка");
                int error = 0;
                if (temp0 != temp[0 + part]) error += 1;
                if (temp1 != temp[1 + part]) error += 2;
                if (temp3 != temp[3 + part]) error += 4;
                if (temp7 != temp[7 + part]) error += 8;
                if (temp15 != temp[15 + part]) error += 16;
                if (--error >= 20)
                {
                    MessageBox.Show("Исправить ошибку не удалось.\nНеобходимо запросить пакет заново");
                    return null; 
                }
                else
                {
                    MessageBox.Show("Одиночная ошибка обнаружена и исправлена в разряде "+error+"\nВ пакете "+ part/21);
                    temp[error + part] = (temp[error + part] + 1) % 2;
                }
            }
            int j = part / 21 * 16;
            a[j++] = temp[part + 2];
            for (int i = 4; i <= 6; i++)
                a[j++] = temp[part + i];
            for (int i = 8; i <= 14; i++)
                a[j++] = temp[part + i];
            for (int i = 16; i <= 20; i++)
                a[j++] = temp[part + i];
            return a;
        }
    }
}
