using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Modbus.Net.Modbus;

namespace Modbus.Net.Tests
{
    
    public class TaskManagerTest
    {
        const string LOCAL_ADDR_10 = "127.0.0.10";
        const string LOCAL_ADDR_12 = "127.0.0.12";
        const string LOCAL_ADDR_1 = "127.0.0.1";

        private TaskManager _taskManager;

        private Dictionary<string, double> _valueDic = new Dictionary<string, double>();

        private Timer _timer;

        
        public TaskManagerTest()
        {
            _taskManager = new TaskManager(20, true);

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

            BaseMachine machine = new ModbusMachine(ModbusTransportType.Tcp, LOCAL_ADDR_10, addresses, true, 2, 0)
            {
                Id = "1"
            };

            _taskManager.AddMachine(machine);

            var r = new Random();

            _timer = new Timer(async state =>
            {
                lock (_valueDic)
                {
                    _valueDic = new Dictionary<string, double>
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
                        }
                    };
                }
                await _taskManager.InvokeOnceAll(new TaskItemSetData(() => _valueDic, MachineSetDataType.CommunicationTag));
            }, null, 0, 5000);
        }

        [Fact]
        public async Task TaskManagerValueReadWriteTest()
        {
            Thread.Sleep(2000);

            var i = 5;
            while (i > 0)
            {
                Thread.Sleep(5000);
                await _taskManager.InvokeOnceAll(new TaskItemGetData(
                    def =>
                    {
                        var dicans = def.ReturnValues.ToDictionary(p => p.Key, p => p.Value.PlcValue);
                        Assert.Equal(dicans["A1"], _valueDic["A1"]);
                        Assert.Equal(dicans["A2"], _valueDic["A2"]);
                        Assert.Equal(dicans["A3"], _valueDic["A3"]);
                        Assert.Equal(dicans["A4"], _valueDic["A4"]);
                        Assert.Equal(dicans["A5"], _valueDic["A5"]);
                        Assert.Equal(dicans["A6"], _valueDic["A6"]);
                    }, MachineGetDataType.CommunicationTag));                
                i--;
            }
        }

        
    }
}
