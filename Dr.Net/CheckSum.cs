using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dr.Net
{
    class CheckSum
    {
      



        public static UInt16 Check(byte[] SendDatas)
        {


            if (SendDatas.Length % 2 == 1)
            {
                byte[] newSendDatas = new byte[SendDatas.Length + 1];
                SendDatas.CopyTo(newSendDatas, 0);
                SendDatas = newSendDatas;
            }
            UInt16[] buffer = new UInt16[SendDatas.Length % 2 + SendDatas.Length / 2];
            int size = buffer.Length;
            for (int i = 0; i < size; i++)
            {          
                buffer[i] = BitConverter.ToUInt16(SendDatas,i*2);
            }

            

            Int32 cksum = 0;
            int counter;
            counter = 0;
            while (size > 0)
            {
                UInt16 val = buffer[counter];
                cksum += Convert.ToInt32(buffer[counter]);
                counter += 1;
                size -= 1;
            }
            cksum = (cksum >> 16) + (cksum & 0xffff);
            cksum += (cksum >> 16);
            return (UInt16)(~cksum);
















                //             byte CRCLo, CRCHi, SaveLo, SaveHi, ByteLo, ByteHi;
                //             int i, j;
                // 
                //             CRCLo = 0xff;
                //             CRCHi = 0xff;
                //             ByteLo = 0x1;
                //             ByteHi = 0xa0;
                //             for (i = 0; i < n; i++)
                //             {
                //                 CRCLo = Convert.ToByte((CRCLo ^ SendDatas[i]) % 256);
                //                 for (j = 0; j < 8; j++)
                //                 {
                //                     SaveLo = CRCLo;
                //                     SaveHi = CRCHi;
                //                     CRCLo = Convert.ToByte((CRCLo >> 1)%256);
                //                     CRCHi = Convert.ToByte((CRCHi >> 1) % 256);
                //                     if ((SaveHi & 0x1) == 1)
                //                         CRCLo = Convert.ToByte(CRCLo | 0x80);
                //                     if ((SaveLo & 0x1) == 1)
                //                     {
                //                         CRCLo = Convert.ToByte((CRCLo ^ ByteLo) % 256);
                //                         CRCHi = Convert.ToByte((CRCHi ^ ByteHi) % 256);
                //                     }
                //                 }
                //             }
                //             crcRet = Convert.ToByte((CRCLo | (CRCHi << 8) )% 256);
              //  return crcRet;
        }
    }
}
