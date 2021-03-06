﻿using System.Configuration;

namespace Modbus.Net
{
    /// <summary>
    ///     Wraps a TcpConnector and provides methods for reading and writing to the stream.
    /// </summary>
    public abstract class TcpProtocolLinker : ProtocalLinker
    {
        /// <summary>
        ///     构造器
        /// </summary>
        protected TcpProtocolLinker(int port)
            : this(ConfigurationManager.AppSettings["IP"], port)
        {
        }

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="ip">Ip地址</param>
        /// <param name="port">端口</param>
        protected TcpProtocolLinker(string ip, int port)
            : this(ip, port, int.Parse(ConfigurationManager.AppSettings["IPConnectionTimeout"] ?? "5000"))
        {
        }

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="ip">Ip地址</param>
        /// <param name="port">端口</param>
        /// <param name="connectionTimeout">超时时间</param>
        protected TcpProtocolLinker(string ip, int port, int connectionTimeout)
        {
            //初始化连接对象
            BaseConnector = new TcpConnector(ip, port, connectionTimeout);
        }
    }
}