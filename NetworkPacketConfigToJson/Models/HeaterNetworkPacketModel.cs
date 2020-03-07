using System.Collections.Generic;

namespace NetworkPacketConfigToJson.Models
{
    public class HeaterNetworkPacketModel
    {
        public List<AnalogNetworkPacketModel> AnalogNetworkPacketModel { get; set; }
        public List<DigitalNetworkPacketModel> DigitalNetworkPacketModel { get; set; }
    }
}