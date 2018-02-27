using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COM_PortsController
{
    public static class DataWrapper
    {
        private static int _priorityByte = 1;

        private static int _tokenByte = 2;

        private static int _reservPriority = 3;

        private static int _destAdress = 4;

        private static int _sourceAdress = 5;

        private static int _startInfo = 6;

        //add start delimeter and end delimeter
        public static byte[] AddSDandED(byte[] data)   
        {
            byte[] newData = new byte[data.Length+2];
            newData[0] = 126;
            newData[data.Length + 1] = 126;
            for (int i = 0; i < data.Length; i++)
            {
                newData[i + 1] = data[i];
            }
            return newData; 
        }

        //add access control byte(priotiy, token byte(1 - frame, 0 - token), reserv priority)
        public static byte[] AddAC(byte[] data, byte pr, byte tokenByte, byte reservPr = 0 )
        {
            byte[] newData = new byte[data.Length + 3];
            newData[0] = pr;
            newData[1] = tokenByte;
            newData[2] = reservPr;
            for (int i = 0; i < data.Length; i++)
            {
                newData[i + 3] = data[i];
            }
            return newData;
        }

        //add destination adress and soucre adress
        public static byte[] AddDAandSA(byte[] data, byte DA, byte SA) 
        {
            byte[] newData = new byte[data.Length + 2];
            newData[0] = DA;
            newData[1] = SA;
            for (int i = 0; i < data.Length; i++)
            {
                newData[i + 2] = data[i];
            }
            return newData;
        }

        public static byte[] codeInfo(byte[] data)
        {
            int dataLength = data.Length;
            int count = 0;
            for (int i = 0; i < dataLength; i++)
                if ((data[i] == 126) || (data[i] == 125))  // 7D = 125, 7E = 126
                    count++;
            int newDataLength = dataLength + count;
            byte[] newData = new byte[newDataLength];
            int j = 0; // iterator for newData
            for (int i = 0; i < dataLength; i++)
            {
                switch (data[i])
                {
                    case 126:
                        newData[j++] = 125;
                        newData[j++] = 126;
                        break;
                    case 125:
                        newData[j++] = 125;
                        newData[j++] = 125;
                        break;
                    default:
                        newData[j++] = data[i];
                        break;
                }
            }
            return newData;
        }

        public static byte[] createMessage(byte[] info, byte DA, byte SA, byte priority)
        {
            byte[] message = codeInfo(info);
            message = AddDAandSA(message, DA, SA);
            message = AddAC(message, priority, 1);
            message = AddSDandED(message);
            return message;
        }

        public static byte[] getToken(byte reservPr = 0)
        {
            byte[] token = new byte[0];
            token = AddAC(token, 0, 0, reservPr);
            token = AddSDandED(token);
            return token; 
        }

        public static Boolean isToken(byte[] data)
        {
            if (data[_tokenByte] == 0)
                return true;
            return false;
        }

        public static byte[] setReservPriority(byte[] data, byte pr)
        {
            data[_reservPriority] = pr;
            return data;
        }

        public static byte getPriority(byte[] data)
        {
            return data[_priorityByte];
        }

        public static byte getReservPriority(byte[] data)
        {
            return data[_reservPriority];
        }

        public static byte getDestAdress(byte[] data)
        {
            return data[_destAdress];
        }

        public static byte getSourceAdress(byte[] data)
        {
            return data[_sourceAdress];
        }

        public static byte[] getInfo(byte[] data)
        {
            int dataLength = data.Length;
            int count = 0;
            for (int i = _startInfo; i < dataLength - 1; i++)
                if (data[i] == 125) 
                    count++;
            int newDataLength = dataLength - count - 7;
            if (newDataLength < 0) newDataLength = 0;
            byte[] newData = new byte[newDataLength];
            int j = 0; // iterator for newData
            for (int i = _startInfo; i < dataLength - 1; i++)
            {
                if (data[i] == 125)
                {
                    if (data[i++] == 125)
                        newData[j++] = 125;
                    else
                        newData[j++] = 126;
                }
                else
                {
                    newData[j++] = data[i];
                }
            }
            return newData;    
        }
    }
}
