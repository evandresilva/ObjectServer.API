using ObjectServer.API.ServiceInterfaces;
using System;
using Xunit;

namespace ObjectServer.API.Tests
{
    public class TestDataLake
    {
        private readonly IDataLakeService _dataLake;
        public TestDataLake(IDataLakeService dataLake)
        {
            _dataLake = dataLake;
        }

        [Fact]
        public void ShouldUpload()
        {

        }
        [Fact]
        public void ShouldDownload()
        {

        }
    }
}
