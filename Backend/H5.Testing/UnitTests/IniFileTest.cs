using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using H5.Lib;
using H5.Lib.PathUtils;
namespace H5.Testing.UnitTests;
internal class IniFile : IUnitTest {

    public string GetName() { return typeof(H5.Lib.IniFile).FullName; }

    public void Run() {
        string TestFilePath = Path.Join(Utils.ExeDirectory.FullName, "TestData", "test.ini");

        FileInfo TestFile = new(TestFilePath);
        Lib.IniFile iniFile = Lib.IniFile.Load(TestFile);
    }

}
