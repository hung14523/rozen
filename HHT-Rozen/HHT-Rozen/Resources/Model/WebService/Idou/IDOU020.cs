
using System.Collections.Generic;

namespace HHT.Resources.Model
{
    public class IDOU020
    {
        public string syukaDate { get; set; }
        public string matehan { get; set; }
        public int oriconSu { get; set; }
        public int caseSu { get; set; }
        public int hutekeiSu { get; set; }
        public int haisodaikoSu { get; set; }
        public int hansokuSu { get; set; }
        public int mailSu { get; set; }
        public int tenidoSu { get; set; }

        public string errorMsg { get; set; }
        

        public int GetTotal() {
           return this.oriconSu+ this.caseSu + this.hutekeiSu + this.haisodaikoSu + this.hansokuSu + this.mailSu + this.tenidoSu;
        }

        public List<string> kamotsuList { get; set; }
    }
}