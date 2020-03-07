using System.Collections.Generic;

namespace NetworkPacketConfigToJson.Models
{
    public class HeaterNetworkPacketConfig
    {
        public List<AnalogNetworkPacketModel> AnalogNetworkPacketConfig { get; set; }
        public List<DigitalNetworkPacketModel> DigitalNetworkPacketConfig { get; set; }
    }
}