using Modbus.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var entities = new List<AddressUnit<string>>
            {
                new AddressUnit<string>()
                    {
                        Id = "Identifier",
                        Address = 0,
                        Area = "0X"
                    }
            };
            

            Modbus.Net.Modbus.ModbusMachine machine = new Modbus.Net.Modbus.ModbusMachine(Modbus.Net.Modbus.ModbusTransportType.Tcp,
                "192.168.96.62",
                entities,
                0,
                10,
                Endian.LittleEndianLsb);
            machine.Connect();
            
            var dataAsyncTask = machine.GetDataAsync(MachineGetDataType.Address);
            dataAsyncTask.Wait();
            var asyncResult = dataAsyncTask.Result;

            //var data = machine.GetData(MachineGetDataType.CommunicationTag);

            Console.ReadKey();
        }
    }
}
