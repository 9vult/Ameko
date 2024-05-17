using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ameko.DataModels
{
    public class ScalePercentage(string text, double multiplier)
    {
        public string Text { get; } = text;
        public double Multiplier { get; } = multiplier;

        public static readonly ScalePercentage VS_12 = new("12.5%", 0.125);
        public static readonly ScalePercentage VS_25 = new("25%", 0.25);
        public static readonly ScalePercentage VS_37 = new("37.5%", 0.375);
        public static readonly ScalePercentage VS_50 = new("50%", 0.5);
        public static readonly ScalePercentage VS_62 = new("62.5%", 0.625);
        public static readonly ScalePercentage VS_75 = new("75%", 0.75);
        public static readonly ScalePercentage VS_87 = new("87.5%", 0.875);
        public static readonly ScalePercentage VS_100 = new("100%", 1);
        public static readonly ScalePercentage VS_112 = new("112.5%", 1.125);
        public static readonly ScalePercentage VS_125 = new("125%", 1.25);
        public static readonly ScalePercentage VS_137 = new("137.5%", 1.375);
        public static readonly ScalePercentage VS_150 = new("150%", 1.5);
        public static readonly ScalePercentage VS_162 = new("162.5%", 1.625);
        public static readonly ScalePercentage VS_175 = new("175%", 1.75);
        public static readonly ScalePercentage VS_187 = new("187.5%", 1.875);
        public static readonly ScalePercentage VS_200 = new("200%", 2);
        public static readonly ScalePercentage VS_212 = new("212.5%", 2.125);
        public static readonly ScalePercentage VS_225 = new("225%", 2.25);
        public static readonly ScalePercentage VS_237 = new("237.5%", 2.375);
        public static readonly ScalePercentage VS_250 = new("250%", 2.5);
        public static readonly ScalePercentage VS_262 = new("262.5%", 2.625);
        public static readonly ScalePercentage VS_275 = new("275%", 2.75);
        public static readonly ScalePercentage VS_287 = new("287.5%", 2.875);
        public static readonly ScalePercentage VS_300 = new("300%", 3);
        public static List<ScalePercentage> Scales => _scales;

        private static readonly List<ScalePercentage> _scales = [
            VS_12,
            VS_25,
            VS_37,
            VS_50,
            VS_62,
            VS_75,
            VS_87,
            VS_100,
            VS_112,
            VS_125,
            VS_137,
            VS_150,
            VS_162,
            VS_175,
            VS_187,
            VS_200,
            VS_212,
            VS_225,
            VS_237,
            VS_250,
            VS_262,
            VS_275,
            VS_287,
            VS_300
        ];
    }

}
