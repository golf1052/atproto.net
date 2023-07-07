using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using golf1052.atproto.net.Models.AtProto.Repo;
using golf1052.atproto.net.Models.AtProto.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace golf1052.atproto.net
{
    public class AtProtoClient
    {
        private Uri baseUri;
        private HttpClient httpClient;
        private string? accessJwt;

        public AtProtoClient() : this(new Uri(Constants.BlueskyBaseUrl), new HttpClient())
        {
        }

        public AtProtoClient(Uri baseUri, HttpClient httpClient)
        {
            if (baseUri.AbsolutePath != "/xrpc")
            {
                throw new Exception("Library currently only supports HTTP API (XRPC)");
            }

            this.baseUri = baseUri;
            this.httpClient = httpClient;
        }

        public async Task<CreateSessionResponse> CreateSession(CreateSessionRequest request)
        {
            Func<HttpRequestMessage> getRequest = () =>
            {
                Url url = new Url(baseUri).AppendPathSegment("com.atproto.server.createSession");
                StringContent content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };
                return requestMessage;
            };

            HttpResponseMessage responseMessage = await SendRequest(getRequest);
            CreateSessionResponse response = await Deserialize<CreateSessionResponse>(responseMessage);
            accessJwt = response.AccessJwt;
            return response;
        }

        public async Task<CreateRecordResponse> CreateRecord<T>(CreateRecordRequest<T> request)
        {
            Func<HttpRequestMessage> getRequest = () =>
            {
                Url url = new Url(baseUri).AppendPathSegment("com.atproto.repo.createRecord");
                StringContent content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };
                return requestMessage;
            };

            HttpResponseMessage responseMessage = await SendAuthorizedRequest(getRequest);
            return await Deserialize<CreateRecordResponse>(responseMessage);
        }

        public async Task DeleteRecord(DeleteRecordRequest request)
        {
            Func<HttpRequestMessage> getRequest = () =>
            {
                Url url = new Url(baseUri).AppendPathSegment("com.atproto.repo.deleteRecord");
                StringContent content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };
                return requestMessage;
            };

            HttpResponseMessage responseMessage = await SendAuthorizedRequest(getRequest);
        }   

        public async Task<UploadBlobResponse> UploadBlob(UploadBlobRequest request)
        {
            MemoryStream? newStream = null;
            Func<HttpRequestMessage> getRequest = () =>
            {
                newStream = new MemoryStream();
                request.Content.CopyTo(newStream);
                request.Content.Position = 0;
                newStream.Position = 0;
                Url url = new Url(baseUri).AppendPathSegment("com.atproto.repo.uploadBlob");
                StreamContent content = new StreamContent(newStream);
                content.Headers.ContentType = new MediaTypeHeaderValue(request.MimeType);
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };
                return requestMessage;
            };

            HttpResponseMessage responseMessage = await SendAuthorizedRequest(getRequest);
            if (newStream != null)
            {
                newStream.Dispose();
            }
            return await Deserialize<UploadBlobResponse>(responseMessage);
        }

        private async Task<HttpResponseMessage> SendAuthorizedRequest(Func<HttpRequestMessage> getHttpRequestMessage)
        {
            if (string.IsNullOrEmpty(accessJwt))
            {
                throw new AtProtoException("Access JWT is not set. Unable to send authorized request.");
            }

            Func<HttpRequestMessage> getRequest = () =>
            {
                HttpRequestMessage request = getHttpRequestMessage();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessJwt);
                return request;
            };

            return await SendRequest(getRequest);
        }

        private async Task<HttpResponseMessage> SendRequest(Func<HttpRequestMessage> getHttpRequestMessage)
        {
            int tries = 0;
            const int maxRetries = 3;
            do
            {
                tries += 1;
                HttpRequestMessage request = getHttpRequestMessage();
                HttpResponseMessage response = await httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    string errorString = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        throw new Exception("Inspect rate limit headers");
                    }
                    else
                    {
                        JObject obj = JObject.Parse(errorString);
                        throw new AtProtoException(errorString);
                    }
                }
                else
                {
                    return response;
                }
            }
            while (tries <= maxRetries);
            throw new AtProtoException($"Hit limit of {maxRetries}");
        }

        private async Task<T> Deserialize<T>(HttpResponseMessage responseMessage)
        {
            string responseString = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseString)!;
        }
    }
}
