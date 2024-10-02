namespace AddressRegistry.Infrastructure.Elastic
{
    using System.Collections.Generic;

    public static class ElasticHelpers
    {
        public static readonly List<string> DutchAbbreviationSynonyms =
        [
            "h, heilig, heilige",
            "onze lieve vrouw, ol vrouw, olv",
            "onze lieve heer, ol heer, olh",
            "onze-lieve-, ol ",
            "sint, st",
            "dr, dokter",
            "stwg, stw, steenweg",
            "burg, burgemeester",
            "str => straat",
            "k, koning",
            "kon, koning, koningin",
            "a, albert",
            "j, jean, julius, jan",
            "b, baptiste",
            "p, peter",
            "m, maurits",
            "w, winston",
            "monseigneur, mgr, msg",
            "bte => boite"
        ];

        public static readonly List<string> DutchNumeralSynonyms =
        [
            "1, een, i",
            "2, twee, ii",
            "3, drie, iii",
            "4, vier, iv",
            "5, vijf, v",
            "6, zes, vi",
            "7, zeven, vii",
            "8, acht, viii",
            "9, negen, ix",
            "10, tien, x",
        ];
    }
}
