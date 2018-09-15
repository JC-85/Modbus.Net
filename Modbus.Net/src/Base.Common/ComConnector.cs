using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     SerialPort with lock-object for synchronization.
    /// </summary>
    public class SerialPortLock : SerialPort
    {
        
        public object LockObject { get; set; } = new object();
    }

    /// <summary>
    ///     串口通讯类
    /// </summary>
    public class ComConnector : BaseConnector, IDisposable
    {
        /// <summary>
        ///     波特率
        /// </summary>
        private readonly int _baudRate;

        /// <summary>
        ///     串口地址
        /// </summary>
        private readonly string _com;

        /// <summary>
        ///     数据位
        /// </summary>
        private readonly int _dataBits;

        /// <summary>
        ///     奇偶校验
        /// </summary>
        private readonly Parity _parity;

        /// <summary>
        ///     从站号
        /// </summary>
        private readonly string _slave;

        /// <summary>
        ///     停止位
        /// </summary>
        private readonly StopBits _stopBits;

        /// <summary>
        ///     超时时间
        /// </summary>
        private readonly int _timeoutTime;

        private int _errorCount;
        private int _receiveCount;

        private int _sendCount;

        /// <summary>
        ///     Dispose是否执行
        /// </summary>
        private bool m_disposed;

        /// <summary>
        ///     Creates a new serial-transport to a specific slave. Reuses existing com-port if altready connected.
        /// </summary>
        /// <param name="com">COM-Port and slave numer. "COM1:1"</param>
        public ComConnector(string com, int baudRate, Parity parity, StopBits stopBits, int dataBits, int timeoutTime)
        {
            /// Here com argument is defined as "COM-PORT:SLAVE" but ConnectionToken is defined in reverse order.
            /// Which is correct?

            _com = com.Split(':')[0];
            _timeoutTime = timeoutTime;
            _baudRate = baudRate;
            _parity = parity;
            _stopBits = stopBits;
            _dataBits = dataBits;
            _slave = com.Split(':')[1];
        }

        /// <summary>
        ///     List of used Serial-Ports.
        /// </summary>
        private static Dictionary<string, SerialPortLock> Connectors { get; } = new Dictionary<string, SerialPortLock>();

        /// <summary>
        ///    List of slaves and their corresponding serial port.
        /// </summary>
        private static Dictionary<string, string> Linkers { get; } = new Dictionary<string, string>();

        /// <summary>
        ///     String representation of com-port/slaveID pair. (Might be in wrong order.)
        /// </summary>
        public override string ConnectionToken => _slave + ":" + _com;

        private SerialPortLock SerialPort
        {
            get
            {
                if (Connectors.ContainsKey(_com))
                    return Connectors[_com];
                return null;
            }
        }

        /// <summary>
        ///     实现IDisposable接口
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     串口读(非阻塞方式读串口，直到串口缓冲区中没有数据
        /// </summary>
        /// <param name="readBuf">串口数据缓冲 </param>
        /// <param name="bufRoom">串口数据缓冲空间大小 </param>
        /// <param name="howTime">设置串口读放弃时间 </param>
        /// <param name="byteTime">字节间隔最大时间 </param>
        /// <returns>串口实际读入数据个数 </returns>
        public int ReadComm(out byte[] readBuf, int bufRoom, int howTime, int byteTime)
        {
            readBuf = new byte[1023];
            Array.Clear(readBuf, 0, readBuf.Length);

            if (SerialPort.IsOpen == false)
                return -1;
            var nBytelen = 0;
            SerialPort.ReadTimeout = howTime;

            while (SerialPort.BytesToRead > 0)
            {
                readBuf[nBytelen] = (byte) SerialPort.ReadByte();
                var bTmp = new byte[bufRoom];
                Array.Clear(bTmp, 0, bTmp.Length);

                var nReadLen = ReadBlock(bTmp, bufRoom, byteTime);

                if (nReadLen > 0)
                {
                    Array.Copy(bTmp, 0, readBuf, nBytelen + 1, nReadLen);
                    nBytelen += 1 + nReadLen;
                }

                else if (nReadLen == 0)
                {
                    nBytelen += 1;
                }
            }

            return nBytelen;
        }

        /// <summary>
        ///     Reads from SerialPort stream until end or readMaxCount reached. This is a blocking operation.
        /// </summary>
        /// <param name="readBuf">Buffer to fill from stream.</param>
        /// <param name="readMaxCount">Max number of bytes to read.</param>
        /// <param name="readTimeout">Timeout in milliseconds </param>
        /// <returns>Number of bytes read.</returns>
        public int ReadBlock(byte[] readBuf, int readMaxCount, int readTimeout)
        {
            if (readBuf.Length < readMaxCount) throw new InvalidOperationException("readBuf is too small.");

            if (SerialPort.IsOpen == false)
                return 0;

            sbyte count = 0;
            SerialPort.ReadTimeout = readTimeout;

            while (count < readMaxCount - 1 && SerialPort.BytesToRead > 0)
            {
                readBuf[count] = (byte) SerialPort.ReadByte();
                count++; // add one 
            }
            readBuf[count] = 0x00;
            return count;
        }


        /// <summary>
        ///     Returns byte-array as hex-string representation.
        /// </summary>
        /// <param name="inBytes">Array of bytes. </param>
        /// <returns>Returns in the format "01 02 0F" </returns>
        public static string ByteToString(byte[] inBytes)
        {
            var stringOut = "";
            foreach (var inByte in inBytes)
                stringOut = stringOut + $"{inByte:X2}" + " ";

            return stringOut.Trim();
        }

        /// <summary>
        ///     Hex-string to byte-array
        /// </summary>
        /// <param name="inString">"01 02 0F"</param>
        /// <returns> </returns>
        public static byte[] StringToByte(string inString)
        {
            var byteStrings = inString.Split(" ".ToCharArray());
            var byteOut = new byte[byteStrings.Length];
            for (var i = 0; i <= byteStrings.Length - 1; i++)
                byteOut[i] = byte.Parse(byteStrings[i], NumberStyles.HexNumber);
            return byteOut;
        }

        /// <summary>
        ///     strhex 转字节数组
        /// </summary>
        /// <param name="inString">类似"01 02 0F" 中间无空格 </param>
        /// <returns> </returns>
        public static byte[] StringToByte_2(string inString)
        {
            inString = inString.Replace(" ", "");

            var byteStrings = new string[inString.Length / 2];
            var j = 0;
            for (var i = 0; i < byteStrings.Length; i++)
            {
                byteStrings[i] = inString.Substring(j, 2);
                j += 2;
            }

            var byteOut = new byte[byteStrings.Length];
            for (var i = 0; i <= byteStrings.Length - 1; i++)
                byteOut[i] = byte.Parse(byteStrings[i], NumberStyles.HexNumber);

            return byteOut;
        }

        /// <summary>
        ///     Takes a string and returns hex-formatted string.
        /// </summary>
        /// <returns>"00 01 02"</returns>
        public static string Str_To_0X(string inString)
        {
            return ByteToString(Encoding.Default.GetBytes(inString));
        }

        /// <summary>
        ///     虚方法，可供子类重写
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    // Release managed resources
                }
                // Release unmanaged resources
                if (SerialPort != null)
                {
                    if (Linkers.Values.Count(p => p == _com) <= 1)
                    {
                        try
                        {

                            SerialPort.Close();
                        }
                        catch
                        {
                            //ignore
                        }
                        SerialPort.Dispose();
                        Log.Information("Com interface {Com} Disposed", _com);
                        Connectors[_com] = null;
                        Connectors.Remove(_com);
                    }
                    Linkers.Remove(_slave);
                    Log.Information("Com connector {ConnectionToken} Removed", ConnectionToken);
                }
                m_disposed = true;
            }
        }

        /// <summary>
        ///    Destructor. 
        /// </summary>
        ~ComConnector()
        {
            Dispose(false);
        }

        private void RefreshSendCount()
        {
            _sendCount++;
            Log.Verbose("Com client {ConnectionToken} send count: {SendCount}", ConnectionToken, _sendCount);
        }

        private void RefreshReceiveCount()
        {
            _receiveCount++;
            Log.Verbose("Com client {ConnectionToken} receive count: {SendCount}", ConnectionToken, _receiveCount);
        }

        private void RefreshErrorCount()
        {
            _errorCount++;
            Log.Verbose("Com client {ConnectionToken} error count: {ErrorCount}", ConnectionToken, _errorCount);
        }

        #region 发送接收数据

        /// <summary>
        ///     是否正在连接
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                if (SerialPort != null && !SerialPort.IsOpen)
                    SerialPort.Dispose();
                return SerialPort != null && SerialPort.IsOpen && Linkers.ContainsKey(_slave);
            }
        }

        /// <summary>
        ///     Creates a serial port transport and opens it.
        /// </summary>
        /// <returns>true if port opened succesfully.</returns>
        public override bool Connect()
        {
            try
            {
                // Creates new serial port if it doesnt already exist.
                if (!Connectors.ContainsKey(_com))
                    Connectors.Add(_com, new SerialPortLock
                    {
                        PortName = _com,
                        BaudRate = _baudRate,
                        Parity = _parity,
                        StopBits = _stopBits,
                        DataBits = _dataBits,
                        ReadTimeout = _timeoutTime
                    });

                if (!Linkers.ContainsKey(_slave))
                    Linkers.Add(_slave, _com);

                SerialPort.Open();
                Log.Information($"Com client {ConnectionToken} connect success");
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e, $"Com client {ConnectionToken} connect error");
                return false;
            }
        }

        /// <summary>
        ///     连接串口
        /// </summary>
        /// <returns>是否连接成功</returns>
        public override Task<bool> ConnectAsync()
        {
            return Task.FromResult(Connect());
        }

        /// <summary>
        ///     断开串口
        /// </summary>
        /// <returns>是否断开成功</returns>
        public override bool Disconnect()
        {
            if (Linkers.ContainsKey(_slave) && Connectors.ContainsKey(_com))
                try
                {
                    Dispose();
                    Log.Information($"Com client {ConnectionToken} disconnect success");
                    return true;
                }
                catch (Exception e)
                {
                    Log.Error(e, $"Com client {ConnectionToken} disconnect error");
                    return false;
                }
            Log.Error(new Exception("Linkers or Connectors Dictionary not found"),$"Com client {ConnectionToken} disconnect error");
            return false;
        }

        /// <summary>
        ///     带返回发送数据
        /// </summary>
        /// <param name="sendStr">需要发送的数据</param>
        /// <returns>是否发送成功</returns>
        public string SendMsg(string sendStr)
        {
            var myByte = StringToByte_2(sendStr);

            var returnBytes = SendMsg(myByte);

            return ByteToString(returnBytes);
        }

        /// <summary>
        ///     无返回发送数据
        /// </summary>
        /// <param name="message">需要发送的数据</param>
        /// <returns>是否发送成功</returns>
        public override Task<bool> SendMsgWithoutReturnAsync(byte[] message)
        {
            return Task.FromResult(SendMsgWithoutReturn(message));
        }

        /// <summary>
        ///     带返回发送数据
        /// </summary>
        /// <param name="sendbytes">需要发送的数据</param>
        /// <returns>是否发送成功</returns>
        public override byte[] SendMsg(byte[] sendbytes)
        {
            try
            {
                if (!SerialPort.IsOpen)
                    try
                    {
                        SerialPort.Open();
                    }
                    catch (Exception err)
                    {
                        Log.Error(err, $"Com client {ConnectionToken} open error");
                        Dispose();
                        SerialPort.Open();
                    }

                byte[] returnBytes;

                lock (SerialPort.LockObject)
                {
                    try
                    {
                        Log.Verbose($"Com client {ConnectionToken} send msg length: {sendbytes.Length}");
                        Log.Verbose($"Com client {ConnectionToken} send msg: {String.Concat(sendbytes.Select(p => " " + p))}");
                        SerialPort.Write(sendbytes, 0, sendbytes.Length);
                    }
                    catch (Exception err)
                    {
                        Log.Error(err, $"Com client {ConnectionToken} send msg error");
                        return null;
                    }
                    RefreshSendCount();

                    try
                    {
                        returnBytes = ReadMsg();
                        Log.Verbose("Com client {ConnectionToken} receive msg length: {Length}", ConnectionToken,returnBytes.Length);
                        Log.Verbose($"Com client {ConnectionToken} receive msg: {String.Concat(returnBytes.Select(p => " " + p))}");
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, $"Com client {ConnectionToken} read msg error");
                        return null;
                    }
                    RefreshReceiveCount();
                }
                return returnBytes;
            }
            catch (Exception err)
            {
                Log.Error(err, $"Com client {ConnectionToken} read error");
                Dispose();
                return null;
            }
        }

        /// <summary>
        ///     带返回发送数据
        /// </summary>
        /// <param name="message">需要发送的数据</param>
        /// <returns>是否发送成功</returns>
        public override Task<byte[]> SendMsgAsync(byte[] message)
        {
            return Task.FromResult(SendMsg(message));
        }

        /// <summary>
        ///     无返回发送数据
        /// </summary>
        /// <param name="sendbytes">需要发送的数据</param>
        /// <returns>是否发送成功</returns>
        public override bool SendMsgWithoutReturn(byte[] sendbytes)
        {
            try
            {
                if (!SerialPort.IsOpen)
                    try
                    {
                        SerialPort.Open();
                    }
                    catch (Exception err)
                    {
                        Log.Error(err, $"Com client {ConnectionToken} open error");
                        Dispose();
                        SerialPort.Open();
                    }
                lock (SerialPort.LockObject)
                {
                    try
                    {
                        Log.Verbose($"Com client {ConnectionToken} send msg length: {sendbytes.Length}");
                        Log.Verbose($"Com client {ConnectionToken} send msg: {string.Concat(sendbytes.Select(p => " " + p))}");
                        SerialPort.Write(sendbytes, 0, sendbytes.Length);
                    }
                    catch (Exception err)
                    {
                        Log.Error(err, $"Com client {ConnectionToken} send msg error");
                        Dispose();
                        return false;
                    }
                    RefreshSendCount();
                }
                return true;
            }
            catch (Exception err)
            {
                Log.Error(err, $"Com client {ConnectionToken} reopen error");
                return false;
            }
        }

        private byte[] ReadMsg()
        {
            try
            {
                if (!SerialPort.IsOpen)
                    SerialPort.Open();

                byte[] data;
                Thread.Sleep(100);
                var i = ReadComm(out data, 10, 5000, 1000);
                var returndata = new byte[i];
                Array.Copy(data, 0, returndata, 0, i);
                return returndata;
            }
            catch (Exception e)
            {
                Log.Error(e, $"Com client {ConnectionToken} read error");
                RefreshErrorCount();
                Dispose();
                return null;
            }
        }

        #endregion
    }
}