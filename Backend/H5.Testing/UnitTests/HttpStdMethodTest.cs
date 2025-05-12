using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using H5.Http;

namespace H5.Testing.UnitTests;
internal sealed class HttpStdMethod : IUnitTest {
    public string GetName() { return typeof(Http.HttpStdMethod).FullName; }

    private void TestComponents() {
        Http.HttpStdMethod[] values = Enum.GetValues<Http.HttpStdMethod>();
        Span<Http.HttpStdMethod> values_span = values;
        values_span.Sort(HttpStdMethodExt.Compare);

        Http.HttpStdMethod[] anyComponents = HttpStdMethodExt.ANY.Components();
        Span<Http.HttpStdMethod> anyComponents_span = anyComponents;
        anyComponents_span.Sort(HttpStdMethodExt.Compare);

        TestHelpers.ExpectEqual(values.Length, anyComponents.Length);
        for (int i = 0; i < values.Length; i++) {
            TestHelpers.ExpectEqual(values[i], anyComponents[i]);
        }
    }

    public void Run() {
        TestComponents();

    }
}
