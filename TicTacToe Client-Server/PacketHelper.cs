using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TicTacToe_Client_Server
{
    public static class PacketHelper
    {
        public static string Serialize(object packet)
        {
            //    TcpPacketBoard|JSON
            string typePrefix = packet.GetType().Name + "|"; // Dodanie prefixu z nazwą klasy
            string serialized = JsonConvert.SerializeObject(packet);
            return typePrefix + serialized;
        }

        public static object Deserialize(string packetData)
        {
            string[] parts = packetData.Split(new char[] { '|' }, 2);
            string packetType = parts[0];
            string jsonData = parts[1];

            switch (packetType)
            {
                case nameof(TcpPacketMove):
                    return JsonConvert.DeserializeObject<TcpPacketMove>(jsonData);
                case nameof(TcpPacketBoard):
                    return JsonConvert.DeserializeObject<TcpPacketBoard>(jsonData);
                case nameof(TcpPacketWin):
                    return JsonConvert.DeserializeObject<TcpPacketWin>(jsonData);
                default:
                    throw new Exception("Nieznany typ pakietu");
            }
        }
    }
}
