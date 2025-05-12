using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace H5.Lib;
public sealed class IniFile {
    string Path;
    string EXE = Assembly.GetExecutingAssembly().GetName().Name;


    public void Save(FileInfo file) {
        throw NotImplementedException();
    }

    public void Save(string filePath) {
        this.Save(new FileInfo(filePath));
    }
}
