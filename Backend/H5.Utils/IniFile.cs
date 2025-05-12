using H5.Lib.CollectionUtils;
using H5.Lib.StreamUtils;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace H5.Lib;
public sealed class IniFile {
    private const string DefaultSectionName = "";

    private static readonly Encoding DefaultEncoding = Encoding.UTF8;

    private readonly SortedDictionary<string, StringDictionary> Sections = new(StringComparer.InvariantCultureIgnoreCase);

    public void Save(FileInfo file) {
        throw new NotImplementedException();
    }
    public void Save(string filePath) { this.Save(new FileInfo(filePath)); }

    public static IniFile Load(Stream stream, Encoding encoding) {
        IniFile result = new();

        using StreamReader sr = new(stream, encoding);
        string currentSection = DefaultSectionName;

        int lineIndex = 0;
        foreach (string l in sr.ReadLines()) {
            lineIndex++;
            string line = l.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            ReadOnlySpan<char> span = line;
            int lineCommentIdx = -1;
            int sectionHeadOpenIdx = -1;
            int sectionHeadCloseIdx = -1;
            int delimIdx = -1;

            for (int i = 0; i < span.Length; i++) {
                switch (span[i]) {
                    case ';': lineCommentIdx = i; goto LoopEnd;
                    case '#': lineCommentIdx = i; goto LoopEnd;
                    case '[': sectionHeadOpenIdx = i; break;
                    case ']': sectionHeadCloseIdx = i; goto LoopEnd;
                    case '=': {
                            delimIdx = (i * Convert.ToInt32(delimIdx < 0)) + (delimIdx * Convert.ToInt32(delimIdx >= 0));
                            break;
                        }
                    case ':': {
                            delimIdx = (i * Convert.ToInt32(delimIdx < 0)) + (delimIdx * Convert.ToInt32(delimIdx >= 0));
                            break;
                        }
                }
            }
        LoopEnd:
            if (lineCommentIdx == 0) { continue; }
            if (lineCommentIdx > 0) { span = span.Slice(0, lineCommentIdx); }


            if (sectionHeadOpenIdx >= 0) {
                if (sectionHeadCloseIdx >= span.Length) { throw new FormatException($"Malformed section header on line:{lineIndex}: missing closing tag"); }

                currentSection = new string(span.Slice(sectionHeadOpenIdx, sectionHeadCloseIdx));
                continue;
            }

            if (delimIdx == 0) { throw new FormatException($"Malformed key on line:{lineIndex}: Keynames cannot contain delimiters"); }
            if (delimIdx < 0) { throw new FormatException($"Malformed key/value pair on line:{lineIndex}: Missing delimiter (Values cannot contain newlines)"); }
            ReadOnlySpan<char> keyspan = span.Slice(0, delimIdx);
            ReadOnlySpan<char> valspan = span.Slice(delimIdx + 1);
            result.SetValue(currentSection, new string(keyspan).TrimEnd(), new string(valspan));
        }

        return result;
    }
    public static IniFile Load(Stream stream) { return Load(stream, DefaultEncoding); }
    public static IniFile Load(FileInfo file, Encoding encoding) {
        using Stream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        return Load(stream, encoding);
    }
    public static IniFile Load(FileInfo file) { return Load(file, DefaultEncoding); }
    public static IniFile Load(string filePath, Encoding encoding) { return Load(new FileInfo(filePath), encoding); }
    public static IniFile Load(string filePath) { return Load(new FileInfo(filePath), DefaultEncoding); }

    public string GetValue(string sectionName, string key) {
        if (!this.Sections.TryGetValue(sectionName, out StringDictionary section)) { throw new ArgumentOutOfRangeException($"Could not find section [{sectionName}]"); }
        if (section[key] is null) { throw new ArgumentOutOfRangeException($"Section [{sectionName}] does not contain key \"{key}\""); }
        return section[key];
    }
    public string GetValue(string key) { return this.GetValue(DefaultSectionName, key); }

    public void SetValue(string sectionName, string key, string value) {
        if (!this.Sections.ContainsKey(sectionName)) { this.Sections[sectionName] = new(); }
        this.Sections[sectionName][key] = value;
    }
    public void SetValue(string key, string value) { this.SetValue(DefaultSectionName, key, value); }
}
