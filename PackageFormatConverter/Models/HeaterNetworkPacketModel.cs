using System.Collections.Generic;

namespace PackageFormatConverter
{
    public class HeaterNetworkPacketModel
    {
        public List<AnalogNetworkPacketModel> AnalogNetworkPacketModel { get; set; }
        public List<DigitalNetworkPacketModel> DigitalNetworkPacketModel { get; set; }
    }
}