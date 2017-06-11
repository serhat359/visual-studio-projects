using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model.Data
{
    public class Radical : IEquatable<Radical>
    {
        public string kanji { get; set; }

        public UInt32 strokes { get; set; }

        public string radicals { get; set; }

        public bool Equals(Radical ex)
        {
            bool result = ex.kanji == this.kanji
                && ex.strokes == this.strokes
                && ex.radicals == this.radicals;

            return result;
        }
    }
}
