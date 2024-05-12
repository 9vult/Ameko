using Ameko.Services;
using AssCS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ameko.DataModels
{
    public class AmekoInfo : ConsumerInfo
    {
        public AmekoInfo(string name, string version, string website) : base(name, version, website)
        {}

        public static AmekoInfo Instance { get; } = new AmekoInfo(
            "Ameko",
            AmekoService.VERSION_BUG,
            "https://ameko.moe"
        );

        
    }
}
