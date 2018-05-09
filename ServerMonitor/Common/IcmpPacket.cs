using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.Controls
{
    class IcmpPacket
    {
        //此类用于创建一个icmppacket报文  ipv4格式
        private Byte _type;//类型
        private Byte _subCode;//代码
        private UInt16 _checkSum;//校验和
        private UInt16 _identifier;//识别符
        private UInt16 _sequenceNumber;//序列号
        private Byte[] _data;//选项数据
        public IcmpPacket(Byte type, Byte subCode, UInt16 checkSum, UInt16 identifier, UInt16 sequenceNumber, int dataSize)
        {
            _type = type;
            _subCode = subCode;
            _checkSum = checkSum;
            _identifier = identifier;
            _sequenceNumber = sequenceNumber;
            _data = new Byte[dataSize];
            //在数据中，写入指定的数据大小
            for (int i = 0; i < dataSize; i++)
            {
                //由于选项数据在此命令中并不重要，所以你可以改换任何你喜欢的字符 
                _data[i] = (byte)'#';
            }
        }
        public UInt16 CheckSum
        {
            get
            {
                return _checkSum;
            }
            set
            {
                _checkSum = value;
            }
        }
        //初始化ICMP报文
        public int CountByte(Byte[] buffer)
        {
            Byte[] b_type = new Byte[1] { _type };
            Byte[] b_code = new Byte[1] { _subCode };
            Byte[] b_cksum = BitConverter.GetBytes(_checkSum);
            Byte[] b_id = BitConverter.GetBytes(_identifier);
            Byte[] b_seq = BitConverter.GetBytes(_sequenceNumber);
            int i = 0;
            Array.Copy(b_type, 0, buffer, i, b_type.Length);
            i += b_type.Length;
            Array.Copy(b_code, 0, buffer, i, b_code.Length);
            i += b_code.Length;
            Array.Copy(b_cksum, 0, buffer, i, b_cksum.Length);
            i += b_cksum.Length;
            Array.Copy(b_id, 0, buffer, i, b_id.Length);
            i += b_id.Length;
            Array.Copy(b_seq, 0, buffer, i, b_seq.Length);
            i += b_seq.Length;
            Array.Copy(_data, 0, buffer, i, _data.Length);
            i += _data.Length;
            return i;
        }
        //将整个ICMP报文信息转化为byte数据包
        
        public static UInt16 SumOfCheck(UInt16[] buffer)
        {
            int cksum = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                cksum += (int)buffer[i];
                cksum = (cksum >> 16) + (cksum & 0xffff);
                cksum += (cksum >> 16);
            }
            return (UInt16)(~cksum);
        }
        //检验和
    }
}
