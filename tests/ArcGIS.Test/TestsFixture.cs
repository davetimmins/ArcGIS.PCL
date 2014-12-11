namespace ArcGIS.Test
{
    using ArcGIS.ServiceModel.Serializers;
    using System;

    public class TestsFixture : IDisposable
    {
        public TestsFixture()
        {
            ServiceStackSerializer.Init();
        }

        public void Dispose()
        {
            // Do "global" teardown here; Only called once.
        }
    }
}
