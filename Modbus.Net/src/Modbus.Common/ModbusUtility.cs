using System;
using System.Threading.Tasks;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Indicates which transport is used to connect with the device.
    /// </summary>
    public enum ModbusTransportType
    {
        /// <summary>
        ///     Rtu连接
        /// </summary>
        Rtu = 0,

        /// <summary>
        ///     Tcp连接
        /// </summary>
        Tcp = 1,

        /// <summary>
        ///     Ascii连接
        /// </summary>
        Ascii = 2,

        /// <summary>
        ///     Rtu连接Tcp透传
        /// </summary>
        RtuOverTcp = 3,

        /// <summary>
        ///     Ascii连接Tcp透传
        /// </summary>
        AsciiOverTcp = 4,
    }

    /// <summary>
    ///     写单个数据方法接口
    /// </summary>
    public interface IUtilityMethodWriteSingle : IUtilityMethod
    {
        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="setContent">需要设置的数据</param>
        /// <returns>设置是否成功</returns>
        Task<bool> SetSingleDataAsync(string startAddress, object setContent);
    }

    /// <summary>
    ///     Modbus基础Api入口
    /// </summary>
    public class ModbusUtility : BaseUtility<byte[], byte[], ProtocolUnit<byte[],byte[]>>, IUtilityMethodTime, IUtilityMethodWriteSingle
    {
        
        public ModbusUtility(int connectionType, byte slaveAddress, byte masterAddress,
            Endian endian = Endian.BigEndianLsb)
            : base(slaveAddress, masterAddress)
        {
            Endian = endian;
            ConnectionString = null;
            ModbusType = (ModbusTransportType) connectionType;
            InitModbusTransportType();

            AddressTranslator = new AddressTranslatorModbus();
        }

        public ModbusUtility(ModbusTransportType connectionType, string connectionString, byte slaveAddress, byte masterAddress,
            Endian endian = Endian.BigEndianLsb)
            : base(slaveAddress, masterAddress)
        {
            Endian = endian;
            ConnectionString = connectionString;
            ModbusType = connectionType;
            InitModbusTransportType();

            AddressTranslator = new AddressTranslatorModbus();
        }

        /// <summary>
        ///    Endian-ness for the connection.
        ///    (Should not be defined on the connection since different registers possibly can have different endian. 
        ///    Mark as obsolete and replace with DefaultEndian which can then be overridden by the field accessor.)
        /// </summary>
        public override Endian Endian { get; }

        /// <summary>
        ///     Ip地址
        /// </summary>
        protected string ConnectionStringIp
        {
            get
            {
                if (ConnectionString == null) return null;
                return ConnectionString.Contains(":") ? ConnectionString.Split(':')[0] : ConnectionString;
            }
        }

        /// <summary>
        ///     端口
        /// </summary>
        protected int? ConnectionStringPort
        {
            get
            {
                if (ConnectionString == null) return null;
                if (!ConnectionString.Contains(":")) return null;
                var connectionStringSplit = ConnectionString.Split(':');
                try
                {
                    return connectionStringSplit.Length < 2 ? (int?) null : int.Parse(connectionStringSplit[1]);
                }
                catch (Exception e)
                {
                    Log.Error(e, $"ModbusUtility: {ConnectionString} format error");
                    return null;
                }
            }
        }


        
        public readonly ModbusTransportType ModbusType;

        void InitModbusTransportType()
            {
            
                switch (ModbusType)
                {
                    //Rtu协议
                    case ModbusTransportType.Rtu:
                    {
                        ProtocolWrapper = ConnectionString == null
                            ? new ModbusRtuProtocal(SlaveAddress, MasterAddress)
                            : new ModbusRtuProtocal(ConnectionString, SlaveAddress, MasterAddress);
                        break;
                    }
                    //Tcp协议
                    case ModbusTransportType.Tcp:
                    {
                        ProtocolWrapper = ConnectionString == null
                            ? new ModbusTcpProtocol(SlaveAddress, MasterAddress)
                            : (ConnectionStringPort == null
                                ? new ModbusTcpProtocol(ConnectionString, SlaveAddress, MasterAddress)
                                : new ModbusTcpProtocol(ConnectionStringIp, ConnectionStringPort.Value, SlaveAddress,
                                    MasterAddress));
                        break;
                    }
                    //Ascii协议
                    case ModbusTransportType.Ascii:
                    {
                        ProtocolWrapper = ConnectionString == null
                            ? new ModbusAsciiProtocal(SlaveAddress, MasterAddress)
                            : new ModbusAsciiProtocal(ConnectionString, SlaveAddress, MasterAddress);
                        break;
                    }
                    //Rtu协议
                    case ModbusTransportType.RtuOverTcp:
                    {
                        ProtocolWrapper = ConnectionString == null
                            ? new ModbusRtuInTcpProtocal(SlaveAddress, MasterAddress)
                            : (ConnectionStringPort == null
                                ? new ModbusRtuInTcpProtocal(ConnectionString, SlaveAddress, MasterAddress)
                                : new ModbusRtuInTcpProtocal(ConnectionStringIp, ConnectionStringPort.Value, SlaveAddress,
                                    MasterAddress));
                        break;
                    }
                    //Ascii协议
                    case ModbusTransportType.AsciiOverTcp:
                    {
                        ProtocolWrapper = ConnectionString == null
                            ? new ModbusAsciiInTcpProtocal(SlaveAddress, MasterAddress)
                            : (ConnectionStringPort == null
                                ? new ModbusAsciiInTcpProtocal(ConnectionString, SlaveAddress, MasterAddress)
                                : new ModbusAsciiInTcpProtocal(ConnectionStringIp, ConnectionStringPort.Value, SlaveAddress,
                                    MasterAddress));
                        break;
                    }
                }
            }
        

        /// <summary>
        ///     读时间
        /// </summary>
        /// <returns>设备的时间</returns>
        public async Task<DateTime> GetTimeAsync()
        {
            try
            {
                var inputStruct = new GetSystemTimeModbusInputStruct(SlaveAddress);
                var outputStruct =
                    await ProtocolWrapper.SendReceiveAsync<GetSystemTimeModbusOutputStruct>(
                        ProtocolWrapper[typeof(GetSystemTimeModbusProtocal)], inputStruct);
                return outputStruct?.Time ?? DateTime.MinValue;
            }
            catch (Exception e)
            {
                Log.Error(e, $"ModbusUtility -> GetTime: {ConnectionString} error");
                return DateTime.MinValue;
            }
        }

        /// <summary>
        ///     写时间
        /// </summary>
        /// <param name="setTime">需要写入的时间</param>
        /// <returns>写入是否成功</returns>
        public async Task<bool> SetTimeAsync(DateTime setTime)
        {
            try
            {
                var inputStruct = new SetSystemTimeModbusInputStruct(SlaveAddress, setTime);
                var outputStruct =
                    await ProtocolWrapper.SendReceiveAsync<SetSystemTimeModbusOutputStruct>(
                        ProtocolWrapper[typeof(SetSystemTimeModbusProtocal)], inputStruct);
                return outputStruct?.WriteCount > 0;
            }
            catch (Exception e)
            {
                Log.Error(e, $"ModbusUtility -> SetTime: {ConnectionString} error");
                return false;
            }
        }


        /// <summary>
        ///     读数据
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="getByteCount">获取字节个数</param>
        /// <returns>获取的结果</returns>
        public override async Task<byte[]> GetDatasAsync(string startAddress, int getByteCount)
        {
            try
            {
                var inputStruct = new ReadDataModbusInputStruct(SlaveAddress, startAddress,
                    (ushort) getByteCount, AddressTranslator);
                var outputStruct = await
                    ProtocolWrapper.SendReceiveAsync<ReadDataModbusOutputStruct>(ProtocolWrapper[typeof(ReadDataModbusProtocal)],
                        inputStruct);
                return outputStruct?.DataValue;
            }
            catch (Exception e)
            {
                Log.Error(e, $"ModbusUtility -> GetDatas: {ConnectionString} error");
                return null;
            }
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="setContents">需要设置的数据</param>
        /// <returns>设置是否成功</returns>
        public override async Task<bool> SetDatasAsync(string startAddress, object[] setContents)
        {
            try
            {
                var inputStruct = new WriteDataModbusInputStruct(SlaveAddress, startAddress, setContents,
                    AddressTranslator, Endian);
                var outputStruct = await
                    ProtocolWrapper.SendReceiveAsync<WriteDataModbusOutputStruct>(ProtocolWrapper[typeof(WriteDataModbusProtocal)],
                        inputStruct);
                return outputStruct?.WriteCount == setContents.Length;
            }
            catch (Exception e)
            {
                Log.Error(e, $"ModbusUtility -> SetDatas: {ConnectionString} error");
                return false;
            }
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="setContent">需要设置的数据</param>
        /// <returns>设置是否成功</returns>
        public async Task<bool> SetSingleDataAsync(string startAddress, object setContent)
        {
            try
            {
                var inputStruct = new WriteSingleDataModbusInputStruct(SlaveAddress, startAddress, setContent,
                    (ModbusTranslatorBase)AddressTranslator, Endian);
                var outputStruct = await
                    ProtocolWrapper.SendReceiveAsync<WriteSingleDataModbusOutputStruct>(ProtocolWrapper[typeof(WriteSingleDataModbusProtocal)],
                        inputStruct);
                return outputStruct?.WriteValue.ToString() == setContent.ToString();
            }
            catch (Exception e)
            {
                Log.Error(e, $"ModbusUtility -> SetSingleDatas: {ConnectionString} error");
                return false;
            }
        }
    }
}