
namespace HHT.Resources.Model
{
    public class Nohin030
    {
        public string matehanCd { get; set; }
        public int matehanKosu { get; set; }
        public int mateCount { get; set; }
        public int sumiMateCount { get; set; }
        public int kamotsuCount { get; set; }
        public int sumiCount { get; set; }

        public int category0 { get; set; }  //０：オリコン　１：ケース　２：不定形
        public int category1 { get; set; }
        public int category2 { get; set; }
        public int category5 { get; set; }  //５：配送代行　6：販促品　７：メール便　８：店間移動
        public int category6 { get; set; }
        public int category7 { get; set; }
        public int category8 { get; set; }

        public string message { get; set; }

    }
}