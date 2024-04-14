using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read input registers functions/requests.
    /// </summary>
    public class ReadInputRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadInputRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadInputRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            byte[] recVal = new byte[12];
            // ModbusReadCommandParameters nam treba
            ModbusReadCommandParameters Read = this.CommandParameters as ModbusReadCommandParameters;
            //Head message

            short transactionId = IPAddress.HostToNetworkOrder((short)Read.TransactionId);
            short protocolId = IPAddress.HostToNetworkOrder((short)Read.ProtocolId);
            short length = IPAddress.HostToNetworkOrder((short)Read.Length);
            short startAddress = IPAddress.HostToNetworkOrder((short)Read.StartAddress);
            short quantity = IPAddress.HostToNetworkOrder((short)Read.Quantity);

            Buffer.BlockCopy(BitConverter.GetBytes(transactionId), 0, recVal, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(protocolId), 0, recVal, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(length), 0, recVal, 4, 2);

            recVal[6] = Read.UnitId;
            recVal[7] = Read.FunctionCode;

            // Data message
            Buffer.BlockCopy(BitConverter.GetBytes(startAddress), 0, recVal, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(quantity), 0, recVal, 10, 2);

            Console.WriteLine("Request ended");
            return recVal;


        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusReadCommandParameters ModbusRead = this.CommandParameters as ModbusReadCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> recnik = new Dictionary<Tuple<PointType, ushort>, ushort>();

            ushort count = response[8];
            ushort value;

            int start1 = 8;
            int start2 = 7;

            for (int i = 0; i < count / 2; i++)
            {
                byte first_byte = response[start1 += 2];
                byte second_byte = response[start2 += 2];

                value = (ushort)(first_byte + (second_byte << 8));

                recnik.Add(new Tuple<PointType, ushort>(PointType.ANALOG_INPUT, (ushort)(ModbusRead.StartAddress + i)), value);
            }

            return recnik;
        }
    }
}