using ArcGIS.ServiceModel;
using ArcGIS.ServiceModel.Serializers;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ArcGIS.Test
{
    public class TokenProviderTests
    {
        [Fact]
        public async Task FederatedTokenRequestHasCorrectUrl()
        {
            var fakeProvider = A.Fake<FederatedTokenProvider>(() => new FederatedTokenProvider("","", "", new ServiceStackSerializer());
            
            fakeProvider.
        }
    }
}
