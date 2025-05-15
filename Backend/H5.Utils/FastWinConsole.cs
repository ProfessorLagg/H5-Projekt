using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H5.Lib;
public sealed unsafe class FastWindowsConsole : IDisposable {
    #region Pinvoke
    const uint STD_INPUT_HANDLE = 4294967286;
    const uint STD_OUTPUT_HANDLE = 4294967285;
    const uint STD_ERROR_HANDLE = 4294967284;
    const Int64 INVALID_HANDLE_VALUE = -1;

    /// https://learn.microsoft.com/en-us/windows/console/getstdhandle
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(uint nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    //[SuppressUnmanagedCodeSecurity]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool CloseHandle(IntPtr hObject);

    // https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-writefile
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool WriteFile(
         IntPtr hFile
        , IntPtr lpBuffer
        , uint nNumberOfBytesToWrite
        , ref uint lpNumberOfBytesWritten
    //,[Optional] ref System.Threading.NativeOverlapped lpOverlapped
    );
    #endregion


    private readonly IntPtr Handle;
    public FastWindowsConsole() {
        this.Handle = GetStdHandle(STD_OUTPUT_HANDLE);
        if (this.Handle == INVALID_HANDLE_VALUE) {
            throw new Exception("Could not get handle to stdout");
        }
    }

    public bool TryWriteBytes(byte[] bytes, out uint result) {
        uint numberOfBytesWritten = 0;
        bool sucess = false;
        fixed (byte* buf_ptr = &bytes[0]) {
            uint buf_len = Convert.ToUInt32(bytes.Length);
            sucess = WriteFile(this.Handle, (IntPtr)buf_ptr, buf_len, ref numberOfBytesWritten);
        }
        result = numberOfBytesWritten;
        return sucess;
    }
    public bool TryWriteString(string str, out uint result) {
        byte[] strBytes = Console.OutputEncoding.GetBytes(str);
        return this.TryWriteBytes(strBytes, out result);
    }

    public void Dispose() {
        CloseHandle(this.Handle);
    }
}
