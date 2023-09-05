using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus_Relay
{
    class Class_Modbus_Relay
    {
        SerialPort _serialPort = new SerialPort();
        public bool connect = false;
        public string PortName = "";

        public void ConnectModbusRelay(string portName, int baudRate = 96000)
        {
            
            try
            {
                this.PortName = portName;
                _serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open();
                    connect = true;
                }
            }
            catch
            {
                connect = false;
            }

        }

        UInt16 CalculateCRC(Byte dchar, UInt16 crc16)
        {
            UInt16 mask = (UInt16)(dchar & 0x00FF);
            crc16 = (UInt16)(crc16 ^ mask);
            for (int i = 0; i < 8; i++)
            {
                if ((UInt16)(crc16 & 0x0001) == 1)
                {
                    mask = (UInt16)(crc16 / 2);
                    crc16 = (UInt16)(mask ^ 0xA001);
                }
                else
                {
                    mask = (UInt16)(crc16 / 2);
                    crc16 = mask;
                }
            }
            return crc16;
        }

        public void sendMessage(ArrayList alToSend)
        {
            if (_serialPort != null)
            {
                if (!_serialPort.IsOpen)
                {
                    ConnectModbusRelay(this.PortName);
                }
                if (alToSend.Count > 0)
                {
                    byte[] bytesToSend = new byte[alToSend.Count + 2]; // the 2 is for the CRC we'll add at the end
                    String sMessageSent = "";
                    UInt16 crc16 = 0xFFFF;
                    for (int i = 0; i < alToSend.Count; i++)
                    {
                        Byte byteFromArray = (Byte)alToSend[i];
                        bytesToSend[i] = byteFromArray;
                        crc16 = CalculateCRC(byteFromArray, crc16);
                        sMessageSent += bytesToSend[i].ToString("X").PadLeft(2, '0') + " ";
                    }

                    bytesToSend[bytesToSend.Count() - 2] = (Byte)(crc16 % 0x100);
                    sMessageSent += bytesToSend[bytesToSend.Count() - 2].ToString("X").PadLeft(2, '0') + " ";

                    bytesToSend[bytesToSend.Count() - 1] = (Byte)(crc16 / 0x100);
                    sMessageSent += bytesToSend[bytesToSend.Count() - 1].ToString("X").PadLeft(2, '0') + " ";

                    //MessageSent.Text = sMessageSent;
                    try
                    {
                        _serialPort.Write(bytesToSend, 0, bytesToSend.Length);
                    }
                    catch
                    {
                    }
                }
            }
        }


        public void WriteRelay(int iValue)
        {
            ArrayList alReturn = new ArrayList();
            alReturn.Add((byte)0xFF); // PLC ID# in this example we set it to 1
            alReturn.Add((byte)0x05); // Write Bit (Modbus Write Single Coil)

            //In this example we're just sending this to address 0.
            // **Note: these are offset by -1 from the # you setup in vBuilder.
            alReturn.Add((byte)0x00);               // Starting Address Hi
            alReturn.Add((byte)0x00);               // Starting Address Lo. 

            if (iValue == 1)
            {
                alReturn.Add((byte)0xFF);            // Quantity of Outputs Hi
            }
            else
            {
                alReturn.Add((byte)0x00);            // Quantity of Outputs Hi
            }
            alReturn.Add((byte)0x00);                // Quantity of Outputs Lo

            sendMessage(alReturn);
        }

    }
}
