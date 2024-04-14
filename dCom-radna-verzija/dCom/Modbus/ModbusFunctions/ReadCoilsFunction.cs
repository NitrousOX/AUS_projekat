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
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc/>
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
            ModbusReadCommandParameters Read = this.CommandParameters as ModbusReadCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> recnik = new Dictionary<Tuple<PointType, ushort>, ushort>();
            int count = 0;
            ushort address = Read.StartAddress;
            byte mask = 1;



            for (int i = 0; i < response[8]; i++)
            {
                byte tempbyte = response[9 + i];
                
                for (int j = 0; j < 8; j++)
                {

                    ushort value = (ushort)(tempbyte & mask);
                    tempbyte >>= 1;
                    recnik.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, address), value);
                    
                    address++;
                    count++;
                    if (count == Read.Quantity)
                    {
                        break;
                    }
                }
            }

            return recnik;
        }
    }
}