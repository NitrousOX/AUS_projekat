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
    /// Class containing logic for parsing and packing modbus read holding registers functions/requests.
    /// </summary>
    public class ReadHoldingRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadHoldingRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadHoldingRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            byte[] request = new byte[12];
            ModbusReadCommandParameters Read = this.CommandParameters as ModbusReadCommandParameters;

            short transactionId = IPAddress.HostToNetworkOrder((short)Read.TransactionId);
            short protocolId = IPAddress.HostToNetworkOrder((short)Read.ProtocolId);
            short length = IPAddress.HostToNetworkOrder((short)Read.Length);
            short startAddress = IPAddress.HostToNetworkOrder((short)Read.StartAddress);
            short quantity = IPAddress.HostToNetworkOrder((short)Read.Quantity);

            Buffer.BlockCopy(BitConverter.GetBytes(transactionId), 0, request, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(protocolId), 0, request, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(length), 0, request, 4, 2);

            request[6] = Read.UnitId;
            request[7] = Read.FunctionCode;

            Buffer.BlockCopy(BitConverter.GetBytes(startAddress), 0, request, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(quantity), 0, request, 10, 2);

            return request;
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

                recnik.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, (ushort)(ModbusRead.StartAddress + i)), value);
            }

            return recnik;

        }
    }
}