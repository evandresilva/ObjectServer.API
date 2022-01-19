using ObjectServer.API.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ObjectServer.API.ServiceInterfaces
{
    public interface IDataLakeService
    {
        public Task<DataLakeUploadResponse> UploadAsync(Stream content, string fileName);
        public bool Download();
    }
}
