using System;
using System.Collections.Generic;
using System.Text;
using ModbusOld = Modbus.Net;
namespace Modbus.Protocol
{
    class TcpTransport : Modbus.Net.TcpConnector
    {
        public TcpTransport(string ipaddress, int port, int timeoutTime) : base(ipaddress, port, timeoutTime)
        {
        }
    }
}
