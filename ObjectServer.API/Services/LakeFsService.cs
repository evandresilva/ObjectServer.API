using Microsoft.Extensions.Configuration;
using ObjectServer.API.Models;
using ObjectServer.API.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using tusdotnet.Interfaces;
using tusdotnet.Models;

namespace ObjectServer.API.Services
{
    public class LakeFsService : IDataLakeService, ITusFileIdProvider
    {
        public string Token { get; set; }
        public string ClientId { get; set; }
        public string SecretKey { get; set; }
        public string Server { get; set; }
        public string Repository { get; set; }
        public string Branch { get; set; }
        private readonly IConfiguration _configuration;

        public LakeFsService(IConfiguration configuration)
        {
            _configuration = configuration;
            Token = _configuration.GetValue<string>("lakeFsToken");
            ClientId = _configuration.GetValue<string>("lakeFsClientId");
            SecretKey = _configuration.GetValue<string>("lakeFsSecreteKey");

            Server = _configuration.GetValue<string>("lakeFsServer");
            Branch = _configuration.GetValue<string>("lakeFsBranch");
            Repository = _configuration.GetValue<string>("lakeFsRepository");
        }

        public bool Download()
        {
            throw new NotImplementedException();
        }

        public async Task<DataLakeUploadResponse> UploadAsync(Stream content, string fileName)
        {
            var response = new DataLakeUploadResponse();

            try
            {
                if (content == null)
                    return response.Bad("File is required", HttpStatusCode.BadRequest);

                using (var client = new HttpClient())
                {
                    using (var formData = new MultipartFormDataContent())
                    {
                        byte[] bytesData;
                        using (var reader = new BinaryReader(content))
                        {
                            bytesData = reader.ReadBytes((int)content.Length);
                        }

                        formData.Add(new StreamContent(new MemoryStream(bytesData)), "content", fileName);

                        var authenticationString = $"{ClientId}:{SecretKey}";
                        var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));

                        client.DefaultRequestHeaders.Authorization
                                      = new AuthenticationHeaderValue("BASIC", base64EncodedAuthenticationString);
                        var lakefsResponse = await client
                            .PostAsync($"{Server}/repositories/{Repository}/branches/{Branch}/objects?path={fileName}", formData);

                        //if (response.StatusCode != HttpStatusCode.Created)
                        //    return StatusCode(response.StatusCode);
                        if (!lakefsResponse.IsSuccessStatusCode)
                        {
                            var message = await lakefsResponse.RequestMessage.Content.ReadAsStringAsync();
                            return response.Bad(message, lakefsResponse.StatusCode);
                        }
                        return response.Good(fileName);
                    }
                }
            }
            catch (Exception e)
            {
                return response.Bad(e.Message, HttpStatusCode.InternalServerError);
            }
        }

        public Task<string> CreateId(string metadata)
        {

            var metadataBytes = System.Convert.FromBase64String(metadata.Split(",")[0]);
            var fileExtession = Encoding.UTF8.GetString(metadataBytes).Split(".")[1];
            var id = $"{ Guid.NewGuid().ToString().Replace("-", "")}.{fileExtession}";
            return Task.FromResult(id);
        }

        public Task<bool> ValidateId(string fileId)
        {
            return Task.FromResult(true);
        }
    }
}
