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
    /// Class containing logic for parsing and packing modbus write single register functions/requests.
    /// </summary>
    public class WriteSingleRegisterFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleRegisterFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleRegisterFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters Write = this.CommandParameters as ModbusWriteCommandParameters;
            byte[] request = new byte[12];

            short transactionId = IPAddress.HostToNetworkOrder((short)Write.TransactionId);
            short protocolId = IPAddress.HostToNetworkOrder((short)Write.ProtocolId);
            short length = IPAddress.HostToNetworkOrder((short)Write.Length);
            short outputAddress = IPAddress.HostToNetworkOrder((short)Write.OutputAddress);
            short value = IPAddress.HostToNetworkOrder((short)Write.Value);

            Buffer.BlockCopy(BitConverter.GetBytes(transactionId), 0, request, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(protocolId), 0, request, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(length), 0, request, 4, 2);

            request[6] = Write.UnitId;
            request[7] = Write.FunctionCode;

            Buffer.BlockCopy(BitConverter.GetBytes(outputAddress), 0, request, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, request, 10, 2);

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusWriteCommandParameters Write = this.CommandParameters as ModbusWriteCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> recnik = new Dictionary<Tuple<PointType, ushort>, ushort>();

            ushort value = (ushort)(response[11] + (response[10] << 8));
            recnik.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, (ushort)(Write.OutputAddress)), value);

            return recnik;
        }
    }
}