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
            

            Modbus.Net.Modbus.ModbusMachine machine = new Modbus.Net.Modbus.ModbusMachine(Modbus.Net.Modbus.ModbusType.Tcp,
                "192.168.96.62",
                new List<AddressUnit>
                {
                    new AddressUnit()
                    {
                        Address = 0,
                        Area = "0X"
                    }
                },
                0,
                10,
                Endian.LittleEndianLsb);
            machine.Connect();

            var data = machine.GetDatas(MachineGetDataType.CommunicationTag);

            Console.ReadKey();
        }
    }
}
