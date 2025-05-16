using H5.API;
using H5.Lib;

using static H5.API.ApiSettings;
namespace H5.Testing.UnitTests;
internal class ApiSettingsTest : IUnitTest {
	public string GetName() { return typeof(H5.API.ApiSettings).FullName; }

	public void Run() {
		ApiSettings.LoadDefault();
		IniFile defaultFile = ApiSettings.ToIniFile();
		HTTPSettings defaultHTTP = HTTPSettings.Parse(defaultFile.GetSection(HTTPSettings.SectionName));
		LoggingSettings defaultLogging = LoggingSettings.Parse(defaultFile.GetSection(LoggingSettings.SectionName));
		ApiSettings.Save();

		ApiSettings.Load();
		IniFile loadedFile = ApiSettings.ToIniFile();
		HTTPSettings loadedHTTP = HTTPSettings.Parse(loadedFile.GetSection(HTTPSettings.SectionName));
		LoggingSettings loadedLogging = LoggingSettings.Parse(loadedFile.GetSection(LoggingSettings.SectionName));


		ReadOnlySpan<KeyValuePair<string, string>> def_KvpsHttp = defaultHTTP.ToArray();
		ReadOnlySpan<KeyValuePair<string, string>> lod_KvpsHttp = loadedHTTP.ToArray();
		TestHelpers.ExpectEqualSpans<KeyValuePair<string, string>>(ref def_KvpsHttp, ref lod_KvpsHttp);

		ReadOnlySpan<KeyValuePair<string, string>> def_KvpsLogging = defaultLogging.ToArray();
		ReadOnlySpan<KeyValuePair<string, string>> lod_KvpsLogging = loadedLogging.ToArray();
		TestHelpers.ExpectEqualSpans<KeyValuePair<string, string>>(ref def_KvpsLogging, ref lod_KvpsLogging);
	}
}
