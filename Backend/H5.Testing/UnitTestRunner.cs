using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5.Testing;
internal class UnitTestRunner {
    public readonly List<IUnitTest> UnitTests;

    public UnitTestRunner() {
        this.UnitTests = new();
    }

    public void AddTest(IUnitTest test) {
        this.UnitTests.Add(test);
    }

    private static void WriteColor(string message, ConsoleColor foreground, ConsoleColor background) {

        Console.BackgroundColor = background;
        Console.Write(message);
        Console.ResetColor();
    }
    private void PrintPass(IUnitTest test) {
        if (Console.CursorLeft != 0) Console.WriteLine("");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("PASS");
        Console.ResetColor();
        Console.WriteLine($"  {test.GetName()}");
    }
    private void PrintFail(IUnitTest test, Exception e) {
        if (Console.CursorLeft != 0) Console.WriteLine("");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("FAIL");
        Console.ResetColor();
        Console.WriteLine($"  {test.GetName()}:");
        Console.WriteLine(e);
    }
    public void Run() {
        int passCount = 0;
        int failCount = 0;
        foreach (IUnitTest test in this.UnitTests) {
            try {
                test.Run();
                passCount++;
                PrintPass(test);
            }
            catch (Exception e) {
                failCount++;
                PrintFail(test, e);
            }
        }

        Console.WriteLine("========== Test Summary ==========");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Passed {passCount} / {this.UnitTests.Count}");
        if (failCount > 0) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Failed {failCount} / {this.UnitTests.Count}");
        }

        Console.ResetColor();
    }
}
