using System.Collections.Generic;

namespace PackageFormatConverter
{
    public class DigitalConfig
    {
        public int Id { get; set; }
        public string Name {get; set;}
        public List<BitConfig> Bits {get; set;}
    }
}