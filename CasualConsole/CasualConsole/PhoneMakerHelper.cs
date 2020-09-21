using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CasualConsole
{
    public class PhoneMakerHelper
    {
    }
    
    class PhoneMaker
    {
        public int Maker { get; set; }
        public string[] PhoneLinks { get; set; }
    }

    class Link
    {
        public string Url { get; set; }
        public bool IsChecked { get; set; }
        public bool IsSuccess { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsNotAndroid { get; set; }
        public bool IsNoResolution { get; set; }
        public double? ScreenSize { get; set; }
        public string OSInfo { get; set; }
    }

}
