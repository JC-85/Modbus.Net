using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     Mapping-layer between the protocol and the transport
    /// </summary>
    public abstract class ProtocalLinker : ProtocalLinker<byte[], byte[]>
    {
        
        [Obsolete("Calls unimplemented methods.")]
        public override async Task<byte[]> SendReceiveAsync(byte[] content)
        {
            var extBytes = BytesExtend(content);
            var receiveBytes = await SendReceiveWithoutExtAndDecAsync(extBytes);
            return receiveBytes == null ? null : receiveBytes.Length == 0 ? receiveBytes : BytesDecact(receiveBytes);
        }

        /// <summary>
        ///     发送并接收数据，不进行协议扩展和收缩，用于特殊协议
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public override async Task<byte[]> SendReceiveWithoutExtAndDecAsync(byte[] content)
        {
            
            var receiveBytes = await BaseConnector.SendMsgAsync(content);
            //seems to be protocol-specific but calls virtual method so it might be overridden
            var checkRight = CheckRight(receiveBytes);
            return checkRight == null ? new byte[0] : (!checkRight.Value ? null : receiveBytes);
            //返回字符
        }

        /// <summary>
        ///  Manually finds an extension class for the current subclass and and executes its BytesExtend method.
        /// </summary>
        public override byte[] BytesExtend(byte[] content)
        {

            //TODO: REmove, dont rely on string resultion to get a helper method. Especially since that helper seems protocol specific so it shouldn't be called in abstracts.

            var bytesExtend = Activator.CreateInstance(GetType().GetTypeInfo().Assembly.GetType(GetType().FullName + "BytesExtend"))
                    as IProtocalLinkerBytesExtend;
            return bytesExtend?.BytesExtend(content);
        }

        /// <summary>
        ///     协议内容缩减，接收时根据需要缩减
        /// </summary>
        /// <param name="content">缩减前的完整协议内容</param>
        /// <returns>缩减后的协议内容</returns>
        public override byte[] BytesDecact(byte[] content)
        {
            //自动查找相应的协议放缩类，命令规则为——当前的实际类名（注意是继承后的）+"BytesExtend"。
            var bytesExtend =
                Activator.CreateInstance(GetType().GetTypeInfo().Assembly.GetType(GetType().FullName + "BytesExtend"))
                    as
                    IProtocalLinkerBytesExtend;
            return bytesExtend?.BytesDecact(content);
        }
    }

    /// <summary>
    ///     基本的协议连接器
    /// </summary>
    public abstract class ProtocalLinker<TParamIn, TParamOut> : IProtocalLinker<TParamIn, TParamOut>
        where TParamOut : class
    {
        /// <summary>
        ///   Connector-instance to handle the communication 
        /// </summary>
        protected IConnector<TParamIn, TParamOut> BaseConnector;

        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>设备是否连接成功</returns>
        public bool Connect()
        {
            return BaseConnector.Connect();
        }

        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>设备是否连接成功</returns>
        public async Task<bool> ConnectAsync()
        {
            return await BaseConnector.ConnectAsync();
        }

        /// <summary>
        ///    Close transport. Will dispose the Connection object so any atempt to interact with BaseConnector will throw an exception.
        /// </summary>
        /// <returns>设备是否断开成功</returns>
        public bool Disconnect()
        {
            return BaseConnector.Disconnect();
        }

        /// <summary>
        ///     通讯字符串
        /// </summary>
        public string ConnectionToken => BaseConnector.ConnectionToken;

        /// <summary>
        ///     设备是否连接
        /// </summary>
        public bool IsConnected => BaseConnector != null && BaseConnector.IsConnected;

        /// <summary>
        ///     发送并接收数据
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public virtual TParamOut SendReceive(TParamIn content)
        {
            return AsyncHelper.RunSync(() => SendReceiveAsync(content));
        }

        public virtual async Task<TParamOut> SendReceiveAsync(TParamIn content)
        {
            var extBytes = BytesExtend(content);
            var receiveBytes = await SendReceiveWithoutExtAndDecAsync(extBytes);
            return BytesDecact(receiveBytes);
        }

        /// <summary>
        ///     发送并接收数据，不进行协议扩展和收缩，用于特殊协议
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public virtual TParamOut SendReceiveWithoutExtAndDec(TParamIn content)
        {
            return AsyncHelper.RunSync(() => SendReceiveWithoutExtAndDecAsync(content));
        }

        /// <summary>
        ///     Sends content to target
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public virtual async Task<TParamOut> SendReceiveWithoutExtAndDecAsync(TParamIn content)
        {
            var receiveBytes = await BaseConnector.SendMsgAsync(content);
            var checkRight = CheckRight(receiveBytes);
            return checkRight == true ? receiveBytes : null;
        }

        /// <summary>
        ///     Checks if content is set. Disconnects if content is null.
        /// </summary>
        public virtual bool? CheckRight(TParamOut content)
        {
            if (content != null) return true;
            Disconnect();
            return false;
        }

        /// <summary>
        ///     Dummy-method, replaced by the actual implementation of the subclass.
        /// </summary>
        [Obsolete("Not Implemented!")]
        public virtual TParamIn BytesExtend(TParamIn content)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///    NOT IMPLEMENTED
        /// </summary>
        /// <param name="content">缩减前的完整协议内容</param>
        /// <returns>缩减后的协议内容</returns>
        [Obsolete("Not Implemented!")]
        public virtual TParamOut BytesDecact(TParamOut content)
        {
            throw new NotImplementedException();
        }
    }
}