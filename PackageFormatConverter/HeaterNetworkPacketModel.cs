using System.Collections.Generic;

namespace PackageFormatConverter
{
    public class HeaterNetworkPacketModel
    {
        public List<DigitalNetworkPacketModel> DigitalNetworkPacketModel { get; set; }
        public List<AnalogNetworkPacketModel> AnalogNetworkPacketModel { get; set; }
    }
}