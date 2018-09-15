using Modbus.Net.Modbus;
using System;
using System.Collections.Generic;
using Xunit;

namespace Modbus.Net.Tests
{
    public class UnitTest1
    {
        private List<AddressUnit<int>> _addressUnits;
        private ModbusMachine<int, int> _baseMachine;

        const string LOCAL_ADDR_10 = "127.0.0.10";
        const string LOCAL_ADDR_12 = "127.0.0.12";
        const string LOCAL_ADDR_1 = "127.0.0.1";

        public UnitTest1()
        {
            _addressUnits = new List<AddressUnit<int>>
            {
                new AddressUnit<int>
                {
                    Id = 1,
                    Area = "3X",
                    Address = 1,
                    SubAddress = 0,
                    DataType = typeof(bool)
                },
                new AddressUnit<int>
                {
                    Id = 2,
                    Area = "3X",
                    Address = 1,
                    SubAddress = 1,
                    DataType = typeof(bool)
                },
                new AddressUnit<int>
                {
                    Id = 3,
                    Area = "3X",
                    Address = 1,
                    SubAddress = 2,
                    DataType = typeof(bool)
                },
                new AddressUnit<int>
                {
                    Id = 4,
                    Area = "3X",
                    Address = 2,
                    SubAddress = 0,
                    DataType = typeof(byte)
                },
                new AddressUnit<int>
                {
                    Id = 5,
                    Area = "3X",
                    Address = 2,
                    SubAddress = 8,
                    DataType = typeof(byte)
                },
                new AddressUnit<int>
                {
                    Id = 6,
                    Area = "3X",
                    Address = 3,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
                new AddressUnit<int>
                {
                    Id = 7,
                    Area = "3X",
                    Address = 4,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
                new AddressUnit<int>
                {
                    Id = 8,
                    Area = "3X",
                    Address = 6,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
                new AddressUnit<int>
                {
                    Id = 9,
                    Area = "3X",
                    Address = 9,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
                new AddressUnit<int>
                {
                    Id = 10,
                    Area = "3X",
                    Address = 10,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
                new AddressUnit<int>
                {
                    Id = 11,
                    Area = "3X",
                    Address = 100,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
                new AddressUnit<int>
                {
                    Id = 12,
                    Area = "4X",
                    Address = 1,
                    SubAddress = 0,
                    DataType = typeof(uint)
                },
                new AddressUnit<int>
                {
                    Id = 13,
                    Area = "4X",
                    Address = 4,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
            };

            _baseMachine = new ModbusMachine<int, int>(ModbusTransportType.Tcp, LOCAL_ADDR_1, _addressUnits, true, 2, 0)
            {
                Id = 1,
                ProjectName = "Project 1",
                MachineName = "Test 1"
            };
            
            /*
            _taskManager = new TaskManager<int>(10, true);

            _taskManager.AddMachine(_baseMachine);
            */
        }


        [Fact]
        public void BaseMachineGetAddressTest()
        {
            var addressUnit = _baseMachine.GetAddressUnitById(1);
            Assert.Equal(addressUnit.Area, "3X");
            Assert.Equal(addressUnit.Address, 1);
        }
    }
}
