using H5.Lib.Utils;

using System.Text;

namespace H5.Lib;
/// <summary>Represents a .ini document</summary>
public sealed class IniDocument {
	private const string DefaultSectionName = "";
	private static readonly Encoding DefaultEncoding = Encoding.UTF8;

	/// <summary><see cref="StringComparer"/> used to sort sections and keys</summary>
	public StringComparer KeyComparer = StringComparer.InvariantCultureIgnoreCase;
	private readonly SortedDictionary<string, SortedDictionary<string, string>> Sections;

	/// <summary>Creates a new <see cref="IniDocument"/> with no data</summary>
	public IniDocument() {
		this.Sections = new(this.KeyComparer);
	}

	/// <summary>Writes this <see cref="IniDocument"/> as a string to a <see cref="System.IO.Stream"/></summary>
	/// <param name="stream"><see cref="Stream"/> to write to</param>
	/// <param name="encoding"><see cref="Encoding"/> to encode string as</param>
	public void Save(Stream stream, Encoding encoding) {
		using StreamWriter sw = new(stream, encoding);
		int sectionIndex = 0;
		foreach (string sectionName in this.Sections.Keys) {
			if (sectionIndex > 0) { sw.Write('\n'); }
			if (sectionName.Length > 0) { sw.Write($"[{sectionName}]\n"); }

			foreach (string k in this.Sections[sectionName].Keys) {
				string v = this.Sections[sectionName][k];
				sw.Write($"{k}={v.EscapeWhitespace()}\n");
			}
			sectionIndex += 1;
		}
	}
	/// <inheritdoc cref="Save(Stream, Encoding)"/>
	public void Save(Stream stream) { this.Save(stream, DefaultEncoding); }
	/// <summary>Writes this <see cref="IniDocument"/> as a string to a file. Overwrites the file!</summary>
	/// <param name="file"><see cref="FileInfo"/> of file to overwrite</param>
	/// <param name="encoding"><inheritdoc cref="Save(Stream, Encoding)"/></param>
	public void Save(FileInfo file, Encoding encoding) {
		using FileStream stream = file.Open(FileMode.Create, FileAccess.Write, FileShare.Read);
		this.Save(stream, encoding);
	}
	/// <inheritdoc cref="Save(FileInfo, Encoding)"/>
	public void Save(FileInfo file) { this.Save(file, DefaultEncoding); }
	/// <summary><inheritdoc cref="Save(FileInfo, Encoding)"/></summary>
	/// <param name="filePath">Path of file to overwrite</param>
	/// <param name="encoding"><inheritdoc cref="Save(Stream, Encoding)"/></param>
	public void Save(string filePath, Encoding encoding) { this.Save(new FileInfo(filePath), encoding); }
	/// <inheritdoc cref="Save(string, Encoding)"/>
	public void Save(string filePath) { this.Save(filePath, DefaultEncoding); }

	/// <summary>Parses text content from a <see cref="Stream"/> as an <see cref="IniDocument"/> </summary>
	/// <param name="stream">The <see cref="Stream"/> to load from</param>
	/// <param name="encoding"><see cref="Encoding"/> of the input text</param>
	/// <returns>A new <see cref="IniDocument"/>The parsed <see cref="IniDocument"/></returns>
	/// <exception cref="FormatException"></exception>
	public static IniDocument Load(Stream stream, Encoding encoding) {
		IniDocument result = new();

		using StreamReader sr = new(stream, encoding);
		string currentSection = DefaultSectionName;

		int lineIndex = 0;
		foreach (string l in sr.ReadLines()) {
			lineIndex++;
			string line = l.Trim();
			if (string.IsNullOrWhiteSpace(line)) {
				continue;
			}

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

				currentSection = new string(span.Slice(sectionHeadOpenIdx, sectionHeadCloseIdx).Slice(1));
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
	/// <inheritdoc cref="Load(Stream, Encoding)"/>
	public static IniDocument Load(Stream stream) { return Load(stream, DefaultEncoding); }
	/// <summary>Parses text content from a file as an <see cref="IniDocument"/> </summary>
	/// <param name="file"><see cref="FileInfo"/> of the file to read</param>
	/// <param name="encoding"><see cref="Encoding"/><inheritdoc cref="Load(Stream, Encoding)"/></param>
	/// <returns>A new <see cref="IniDocument"/><inheritdoc cref="Load(Stream, Encoding)"/></returns>
	/// <exception cref="FormatException"><inheritdoc cref="Load(Stream, Encoding)"/></exception>
	public static IniDocument Load(FileInfo file, Encoding encoding) {
		using Stream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
		return Load(stream, encoding);
	}
	/// <inheritdoc cref="Load(FileInfo, Encoding)"/>
	public static IniDocument Load(FileInfo file) { return Load(file, DefaultEncoding); }
	/// <summary><inheritdoc cref="Load(FileInfo, Encoding)"/></summary>
	/// <param name="filePath">Path of the file to read</param>
	/// <param name="encoding"><see cref="Encoding"/><inheritdoc cref="Load(Stream, Encoding)"/></param>
	/// <returns>A new <see cref="IniDocument"/><inheritdoc cref="Load(Stream, Encoding)"/></returns>
	/// <exception cref="FormatException"><inheritdoc cref="Load(Stream, Encoding)"/></exception>
	public static IniDocument Load(string filePath, Encoding encoding) { return Load(new FileInfo(filePath), encoding); }
	/// <inheritdoc cref="Load(FileInfo, Encoding)"/>
	public static IniDocument Load(string filePath) { return Load(new FileInfo(filePath), DefaultEncoding); }

	/// <summary>
	/// Get's a value from this <see cref="IniDocument"/>
	/// </summary>
	/// <param name="sectionName">Section the value is stored in</param>
	/// <param name="key"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if key not found in section</exception>
	public string GetValue(string sectionName, string key) {
		IDictionary<string, string> section = this.GetSection(sectionName);
		if (section[key] is null) { throw new ArgumentOutOfRangeException($"Section [{sectionName}] does not contain key \"{key}\""); }
		return section[key];
	}
	/// <inheritdoc cref="GetValue(string, string)"/>
	public string GetValue(string key) { return this.GetValue(DefaultSectionName, key); }

	/// <summary>
	/// Adds or overwrites a value in this <see cref="IniDocument"/>.
	/// If the section does not exists, it is created
	/// </summary>
	/// <param name="sectionName">Section to add or overwrite value in</param>
	/// <param name="key">Key of the value to add or overwrite</param>
	/// <param name="value">New value</param>
	public void SetValue(string sectionName, string key, string value) {
		if (this.Sections.ContainsKey(sectionName)) {
			this.Sections[sectionName][key] = value;
		}
		else {
			IEnumerable<KeyValuePair<string, string>> kvps = new KeyValuePair<string, string>[] {
				new KeyValuePair<string, string>(key, value)
			};
			this.SetSection(sectionName, kvps);
		}
	}
	/// <inheritdoc cref="SetValue(string, string, string)"/>
	public void SetValue(string key, string value) { this.SetValue(DefaultSectionName, key, value); }

	/// <summary>Adds or overrides the section with the specified name</summary>
	public void SetSection(string sectionName, IEnumerable<KeyValuePair<string, string>> kvps) {
		SortedDictionary<string, string> section = new(this.KeyComparer);
		foreach (KeyValuePair<string, string> kvp in kvps) {
			section[kvp.Key] = kvp.Value;
		}
		this.Sections[sectionName] = section;
	}
	/// <summary>
	/// Get's a section from this <see cref="IniDocument"/>
	/// </summary>
	/// <param name="sectionName">Name of the section to get</param>
	/// <returns>Section as a <see cref="IDictionary{String, String}"/></returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public IDictionary<string, string> GetSection(string sectionName) {
		if (!this.Sections.TryGetValue(sectionName, out var section)) {
			throw new ArgumentOutOfRangeException($"Could not find section [{sectionName}]");
		}
		return section;
	}

	/// <summary>
	/// Returns this <see cref="IniDocument"/> as a string
	/// </summary>
	/// <returns>this <see cref="IniDocument"/> as a string</returns>
	public override string ToString() {
		MemoryStream stream = new();
		this.Save(stream, Encoding.Unicode);
		return Encoding.Unicode.GetString(stream.GetBuffer());
	}
}
