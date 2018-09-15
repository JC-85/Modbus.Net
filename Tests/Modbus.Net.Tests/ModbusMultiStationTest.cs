using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Modbus.Net.Modbus;

namespace Modbus.Net.Tests
{
    
    public class ModbusMultiStationTest
    {
        private BaseMachine _modbusRtuMachine1;

        private BaseMachine _modbusRtuMachine2;

        
        public ModbusMultiStationTest()
        {
            _modbusRtuMachine1 = new ModbusMachine(ModbusTransportType.Rtu, "COM1", null, true, 1, 0);
            _modbusRtuMachine2 = new ModbusMachine(ModbusTransportType.Rtu, "COM1", null, true, 2, 0);
        }

        [Fact]
        public async Task MultiStation()
        {
            var addresses = new List<AddressUnit<string>>
            {
                new AddressUnit<string>
                {
                    Id = "0",
                    Area = "4X",
                    Address = 2,
                    SubAddress = 0,
                    CommunicationTag = "A1",
                    DataType = typeof(ushort)
                },
                new AddressUnit<string>
                {
                    Id = "1",
                    Area = "4X",
                    Address = 3,
                    SubAddress = 0,
                    CommunicationTag = "A2",
                    DataType = typeof(ushort)
                },
                new AddressUnit<string>
                {
                    Id = "2",
                    Area = "4X",
                    Address = 4,
                    SubAddress = 0,
                    CommunicationTag = "A3",
                    DataType = typeof(ushort)
                },
                new AddressUnit<string>
                {
                    Id = "3",
                    Area = "4X",
                    Address = 5,
                    SubAddress = 0,
                    CommunicationTag = "A4",
                    DataType = typeof(ushort)
                },
                new AddressUnit<string>
                {
                    Id = "4",
                    Area = "4X",
                    Address = 6,
                    SubAddress = 0,
                    CommunicationTag = "A5",
                    DataType = typeof(uint)
                },
                new AddressUnit<string>
                {
                    Id = "5",
                    Area = "4X",
                    Address = 8,
                    SubAddress = 0,
                    CommunicationTag = "A6",
                    DataType = typeof(uint)
                }
            };

            _modbusRtuMachine1.GetAddresses = addresses.ToList();
            _modbusRtuMachine2.GetAddresses = addresses.ToList();

            Random r = new Random();
            var dic1 = new Dictionary<string, double>()
            {
                {
                    "A1", r.Next(0, UInt16.MaxValue)
                },
                {
                    "A2", r.Next(0, UInt16.MaxValue)
                },
                {
                    "A3", r.Next(0, UInt16.MaxValue)
                },
                {
                    "A4", r.Next(0, UInt16.MaxValue)
                },
                {
                    "A5", r.Next()
                },
                {
                    "A6", r.Next()
                },
            };

            var dic2 = new Dictionary<string, double>()
            {
                {
                    "A1", r.Next(0, UInt16.MaxValue)
                },
                {
                    "A2", r.Next(0, UInt16.MaxValue)
                },
                {
                    "A3", r.Next(0, UInt16.MaxValue)
                },
                {
                    "A4", r.Next(0, UInt16.MaxValue)
                },
                {
                    "A5", r.Next()
                },
                {
                    "A6", r.Next()
                },
            };

            await _modbusRtuMachine1.SetDatasAsync(MachineSetDataType.CommunicationTag, dic1);
            await _modbusRtuMachine2.SetDatasAsync(MachineSetDataType.CommunicationTag, dic2);

            var ans = await _modbusRtuMachine1.GetDataAsync(MachineGetDataType.CommunicationTag);
            var ans2 = await _modbusRtuMachine2.GetDataAsync(MachineGetDataType.CommunicationTag);

            _modbusRtuMachine1.Disconnect();
            _modbusRtuMachine2.Disconnect();

            Assert.Equal(ans["A1"].PlcValue, dic1["A1"]);
            Assert.Equal(ans2["A1"].PlcValue, dic2["A1"]);
            Assert.Equal(ans["A2"].PlcValue, dic1["A2"]);
            Assert.Equal(ans2["A2"].PlcValue, dic2["A2"]);
            Assert.Equal(ans["A3"].PlcValue, dic1["A3"]);
            Assert.Equal(ans2["A3"].PlcValue, dic2["A3"]);
            Assert.Equal(ans["A4"].PlcValue, dic1["A4"]);
            Assert.Equal(ans2["A4"].PlcValue, dic2["A4"]);
            Assert.Equal(ans["A5"].PlcValue, dic1["A5"]);
            Assert.Equal(ans2["A5"].PlcValue, dic2["A5"]);
            Assert.Equal(ans["A6"].PlcValue, dic1["A6"]);
            Assert.Equal(ans2["A6"].PlcValue, dic2["A6"]);
        }
    }
}
