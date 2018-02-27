using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COM_PortsController
{
    class ByteStuffing
    {
        public static byte[] Reverse(byte[] data, byte destinationAdress)
        {
            int dataLength = data.Length;
            int count = 2;
            for (int i=0; i<dataLength; i++)
                if (data[i] == 125)
                    count++;
            int newDataLength = dataLength - 2 - count;
            if (newDataLength < 0) newDataLength = 0;
            byte[] newData = new byte[newDataLength+1];
            int j = 0; // iterator for newData
            if (data[1] != destinationAdress)
            {
                return null;
            }
            else
            {
                newData[j++] = data[2];
                for (int i = 3; i < dataLength - 1; i++)
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

        public static byte[] Direct(byte[] data, byte destinationAdress, byte sourceAdress)
        {
            int dataLength = data.Length;
            int count = 0;
            for (int i =0; i<dataLength; i++) 
                if (( data[i] == 126) || (data[i] == 125))  // 7D = 125, 7E = 126, 5E = 94
                     count++;
            int newDataLength = dataLength + count + 2 + 2;
            byte[] newData = new byte[newDataLength];
            newData[0] = 126;
            newData[1] = destinationAdress;
            newData[2] = sourceAdress;
            newData[newDataLength-1] = 126;
            int j = 3; // iterator for newData
            for (int i = 0; i < dataLength; i++)
            {
                switch (data[i])
                {
                    case 126:
                        newData[j++] = 125;
                        newData[j++] = 94; 
                        break;
                    case 156:
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
    }
}
