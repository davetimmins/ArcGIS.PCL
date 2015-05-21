namespace ArcGIS.Test
{
    using ArcGIS.ServiceModel.Serializers;
    using System;

    public class TestsFixture : IDisposable
    {
        static TestsFixture()
        {
            ServiceStackSerializer.Init();
        }

        public void Dispose()
        {
            // Do "global" teardown here; Only called once.
        }
    }
}
