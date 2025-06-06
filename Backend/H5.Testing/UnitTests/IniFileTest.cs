﻿using H5.Lib.Utils;

using System.Text;
namespace H5.Testing.UnitTests;
internal class IniFileTest : IUnitTest {

	public string GetName() { return typeof(H5.Lib.IniDocument).FullName; }

	public void Run() {
		string srcFilePath = Path.Join(PathUtils.ExeDirectory.FullName, "TestData", "test.ini");
		FileInfo srcFileInfo = new(srcFilePath);
		string dstFilePath = Path.Join(srcFileInfo.DirectoryName, "_" + srcFileInfo.Name);
		FileInfo dstFileInfo = new(dstFilePath);
		Lib.IniDocument iniFile = Lib.IniDocument.Load(srcFileInfo);

		TestHelpers.ExpectEqual(@" orchard rental service (with app)", iniFile.GetValue("project", "name"));
		TestHelpers.ExpectEqual(" \"Bay Area\"", iniFile.GetValue("project", "target region"));
		TestHelpers.ExpectEqual(" (vacant)", iniFile.GetValue("project", "legal team"));

		TestHelpers.ExpectEqual(" foreseeable", iniFile.GetValue("fruit \"Apple\"", "trademark issues"));
		TestHelpers.ExpectEqual(" known", iniFile.GetValue("fruit \"Apple\"", "taste"));

		TestHelpers.ExpectEqual(" novel", iniFile.GetValue("fruit.Date", "taste"));
		TestHelpers.ExpectEqual("\"truly unlikely\"", iniFile.GetValue("fruit.Date", "Trademark Issues"));

		TestHelpers.ExpectEqual("possible", iniFile.GetValue("fruit \"Raspberry\"", "Trademark Issues"));
		TestHelpers.ExpectEqual("\"logistics (fragile fruit)\"", iniFile.GetValue("fruit \"Raspberry\"", "anticipated problems"));

		TestHelpers.ExpectEqual(" 2021-11-23, 08:54 +0900", iniFile.GetValue("fruit.raspberry.proponents.fred", "date"));
		TestHelpers.ExpectEqual(" \"I like red fruit.\"", iniFile.GetValue("fruit.raspberry.proponents.fred", "comment"));

		iniFile.Save(dstFileInfo, Encoding.Unicode);
	}

}
