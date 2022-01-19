using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ObjectServer.API.Models
{
    public class DataLakeUploadResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Path { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public DataLakeUploadResponse Good(string path)
        {
            return new DataLakeUploadResponse
            {
                Message = "File uploaded",
                Path = path,
                Success = true,
                StatusCode = HttpStatusCode.OK
            };
        }
        public DataLakeUploadResponse Bad(string message, HttpStatusCode statusCode)
        {
            return new DataLakeUploadResponse
            {
                Message = message,
                Path = null,
                Success = false,
                StatusCode = statusCode
            };
        }
    }
}
