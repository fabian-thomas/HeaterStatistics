using System.Collections.Generic;

namespace PackageFormatConverter
{
    public class DigitalNetworkPacketModel : NetworkPacketModel
    {
        public List<BitModel> Bits { get; set; }

        public DigitalNetworkPacketModel()
        {
            Bits = new List<BitModel>();
        }
    }
}