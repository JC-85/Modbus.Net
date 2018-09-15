using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Modbus.Net.Modbus;

namespace Modbus.Net.Tests
{
    
    public class MachineMethodTest
    {

        const string LOCAL_ADDR_10 = "127.0.0.10";
        const string LOCAL_ADDR_12 = "127.0.0.12";
        const string LOCAL_ADDR_1 = "127.0.0.1";

        [Fact]
        public void GetUtility()
        {
            BaseMachine<int, int> baseMachine = new ModbusMachine<int, int>(ModbusTransportType.Tcp, LOCAL_ADDR_12, null, true, 2, 0);
            var utility = baseMachine.GetUtility<IUtilityMethodTime>();
            var methods = utility.GetType().GetRuntimeMethods();
            Assert.Equal(methods.FirstOrDefault(method => method.Name == "GetTimeAsync") != null, true);
            Assert.Equal(methods.FirstOrDefault(method => method.Name == "SetTimeAsync") != null, true);
            baseMachine.Disconnect();
        }

        [Fact]
        public async Task InvokeUtility()
        {
            BaseMachine<int, int> baseMachine = new ModbusMachine<int, int>(ModbusTransportType.Tcp, LOCAL_ADDR_12, null, true, 2, 0);
            bool success = await baseMachine.BaseUtility.GetUtilityMethods<IUtilityMethodTime>().SetTimeAsync(DateTime.Now);
            Assert.True(success);

            //Casuses
            var time = await baseMachine.BaseUtility.GetUtilityMethods<IUtilityMethodTime>().GetTimeAsync();
            Assert.True((time.ToUniversalTime() - DateTime.Now.ToUniversalTime()).Seconds < 10);
            baseMachine.Disconnect();
        }

        [Fact]
        public async Task InvokeMachine()
        {
            BaseMachine<int, int> baseMachine = new ModbusMachine<int, int>(ModbusTransportType.Tcp, LOCAL_ADDR_10, new List<AddressUnit<int>>
            {
                new AddressUnit<int>
                {
                    Id = 0,
                    Area = "0X",
                    Address = 1,
                    SubAddress = 0,
                    CommunicationTag = "A1",
                    DataType = typeof(bool)
                }
            }, true, 2, 0);
            var success = await baseMachine.GetMachineMethods<IMachineMethodData>().SetDatasAsync(
                MachineSetDataType.Address,
                new Dictionary<string, double>
                {
                    {
                        "0X 1.0", 1
                    }
                });
            Assert.Equal(success, true);
            var datas = await baseMachine.GetMachineMethods<IMachineMethodData>().GetDataAsync(MachineGetDataType.Address);
            Assert.Equal(datas["0X 1.0"].PlcValue, 1);
            success = await baseMachine.GetMachineMethods<IMachineMethodData>().SetDatasAsync(
                MachineSetDataType.Address,
                new Dictionary<string, double>
                {
                    {
                        "0X 1.0", 0
                    }
                });
            Assert.Equal(success, true);
            baseMachine.Disconnect();
        }
    }
}
