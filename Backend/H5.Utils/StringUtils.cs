using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5.Lib.StringUtils;
public static partial class Utils {
    private readonly struct EscapeSequence {
        public readonly string Find;
        public readonly string Replace;
        public EscapeSequence(string find, string replace) {
            this.Find = find;
            this.Replace = replace;
        }
        public string Escape(string s) { return s.Replace(this.Find, this.Replace); }
    }
    private static readonly EscapeSequence[] WhitespaceEscapes = new EscapeSequence[] {
        new EscapeSequence("\r", @"\r"),
        new EscapeSequence("\n", @"\n"),
        new EscapeSequence("\t", @"\t"),
        new EscapeSequence("\f", @"\f"),
        new EscapeSequence("\v", @"\v"),
    };

    public static string EscapeWhitespace(this string str) {
        string r = str;
        foreach (EscapeSequence e in WhitespaceEscapes) {
            r = e.Escape(r);
        }
        return r;
    }
}
