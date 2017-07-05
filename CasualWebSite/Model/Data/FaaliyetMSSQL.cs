using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model.Data
{
    public class FaaliyetMSSQL : IEquatable<FaaliyetMSSQL>
    {
        public decimal FaaliyetID { get; set; }
        public decimal AskerID { get; set; }
        public int BirlikID { get; set; }
        public DateTime GelisTarih { get; set; }
        public DateTime? CikisTarih { get; set; }
        public DateTime? BelgeCikisTarih { get; set; }
        public int DurumID { get; set; }
        public string Aciklama { get; set; }
        public int GeldigiMerkez { get; set; }
        public int GidecegiMerkez { get; set; }
        public string SorumluTel { get; set; }
        public int IsKonvoy { get; set; }
        public int? KonvoyID { get; set; }

        public bool Equals(FaaliyetMSSQL ex)
        {
            bool result = ex.FaaliyetID == this.FaaliyetID
                && ex.AskerID == this.AskerID
                && ex.BirlikID == this.BirlikID
                && ex.GelisTarih == this.GelisTarih
                && ex.CikisTarih == this.CikisTarih
                && ex.BelgeCikisTarih == this.BelgeCikisTarih
                && ex.DurumID == this.DurumID
                && ex.Aciklama == this.Aciklama
                && ex.GeldigiMerkez == this.GeldigiMerkez
                && ex.GidecegiMerkez == this.GidecegiMerkez
                && ex.SorumluTel == this.SorumluTel
                && ex.IsKonvoy == this.IsKonvoy
                && ex.KonvoyID == this.KonvoyID;

            return result;
        }
    }
}
