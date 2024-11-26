using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityModel;
using Flurl;
using golf1052.atproto.net.Models.Bsky.Actor;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace golf1052.atproto.net
{
    public class AtProtoOAuthClient
    {
        private readonly Uri baseUri;
        private readonly HttpClient httpClient;
        private readonly JsonSerializerSettings serializer;
        private readonly string accessToken;
        private readonly JsonWebTokenHandler jsonWebTokenHandler;
        private readonly DateTimeOffset? accessTokenExpiration;
        private readonly SigningCredentials signingCredentials;
        private readonly Dictionary<string, object> proofHeader;

        private string? Nonce { get; set; }

        public string Did { get; private set; } = default!;

        private bool IsAccessTokenExpired
        {
            get
            {
                if (accessTokenExpiration == null)
                {
                    return false;
                }

                return accessTokenExpiration < DateTimeOffset.UtcNow;
            }
        }

        public AtProtoOAuthClient(string did,
            string keyId,
            string privateKey,
            string baseUri,
            string accessToken) : this(new HttpClient(), did, keyId, privateKey, new Uri(baseUri), accessToken)
        {
        }

        public AtProtoOAuthClient(HttpClient httpClient,
            string did,
            string keyId,
            string privateKey,
            Uri baseUri,
            string accessToken)
        {
            this.baseUri = baseUri;
            this.httpClient = httpClient;
            Did = did;
            serializer = new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore
            };
            this.accessToken = accessToken;
            jsonWebTokenHandler = new JsonWebTokenHandler()
            {
                SetDefaultTimesOnTokenCreation = false
            };
            accessTokenExpiration = GetAccessTokenExpirationTime(this.accessToken);
            signingCredentials = CreateSigningCredentials(keyId, privateKey);
            proofHeader = CreateProofHeader(signingCredentials);
        }

        public async Task<BskyProfileViewBasic> GetProfile()
        {
            Func<HttpRequestMessage> getRequest = () =>
            {
                Url url = new Url(baseUri).AppendPathSegments("xrpc", "app.bsky.actor.getProfile")
                    .SetQueryParam("actor", Did);
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                return requestMessage;
            };

            HttpResponseMessage responseMessage = await SendAuthorizedRequest(getRequest);
            return await Deserialize<BskyProfileViewBasic>(responseMessage);
        }

        private DateTimeOffset? GetAccessTokenExpirationTime(string accessToken)
        {
            JsonWebToken jsonWebToken = new JsonWebToken(accessToken);
            if (jsonWebToken.TryGetClaim("exp", out Claim expirationClaim) &&
                long.TryParse(expirationClaim.Value, out long expiration))
            {
                return DateTimeOffset.FromUnixTimeSeconds(expiration);
            }

            return null;
        }

        private Dictionary<string, object> CreateProofHeader(SigningCredentials signingCredentials)
        {
            var jsonWebKey = (JsonWebKey)signingCredentials.Key;
            Dictionary<string, object> headerKey = new Dictionary<string, object>()
            {
                { "kty", jsonWebKey.Kty },
                { "x", jsonWebKey.X },
                { "y", jsonWebKey.Y },
                { "crv", jsonWebKey.Crv }
            };

            Dictionary<string, object> header = new Dictionary<string, object>()
            {
                { "typ", JwtClaimTypes.JwtTypes.DPoPProofToken },
                { JwtClaimTypes.JsonWebKey, headerKey }
            };

            return header;
        }

        private SigningCredentials CreateSigningCredentials(string keyId,
            string privateKey)
        {
            // TODO: Should pass in the algorithm information from the client instead of hardcoding it here
            ECDsa curve = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            curve.ImportFromPem(privateKey);
            var ecKey = new ECDsaSecurityKey(curve);
            var jwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(ecKey);
            jwk.Alg = OidcConstants.Algorithms.Asymmetric.ES256;
            jwk.KeyId = keyId;
            return new SigningCredentials(jwk, jwk.Alg);
        }

        private DPoPProofPayload CreatePayload(string method, string url, string? nonce)
        {
            // Create the payload for the DPoP proof
            var payload = new DPoPProofPayload()
            {
                JwtId = CryptoRandom.CreateUniqueId(),
                DPoPHttpMethod = method,
                DPoPHttpUrl = url,
                IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            // Need to add the access token hash to the payload
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(accessToken));
            var ath = Base64Url.Encode(hash);
            payload.DPoPAccessTokenHash = ath;

            if (nonce != null)
            {
                payload.Nonce = nonce;
            }

            return payload;
        }

        private async Task<HttpResponseMessage> SendAuthorizedRequest(Func<HttpRequestMessage> getHttpRequestMessage)
        {
            if (IsAccessTokenExpired)
            {
                throw new AtProtoException("Access token is expired. Unable to send authorized request.");
            }

            Func<string?, HttpRequestMessage> getRequest = (string? nonce) =>
            {
                HttpRequestMessage request = getHttpRequestMessage();
                request.Headers.Authorization = new AuthenticationHeaderValue("DPoP", accessToken);

                var payload = CreatePayload(request.Method.Method, request.RequestUri!.ToString(), nonce);
                var proofToken = jsonWebTokenHandler.CreateToken(JsonConvert.SerializeObject(payload), signingCredentials, proofHeader);
                request.Headers.Add("DPoP", proofToken);
                return request;
            };

            return await SendRequest(getRequest);
        }

        private async Task<HttpResponseMessage> SendRequest(Func<string?, HttpRequestMessage> getHttpRequestMessage)
        {
            int tries = 0;
            const int maxRetries = 3;
            do
            {
                tries += 1;
                HttpRequestMessage request = getHttpRequestMessage(Nonce);
                HttpResponseMessage response = await httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    string errorString = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        string? nonce = response.Headers.GetValues("dpop-nonce").FirstOrDefault();
                        if (nonce == null)
                        {
                            throw new AtProtoException($"Unauthorized request. Error string: {errorString}");
                        }
                        Nonce = nonce;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
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
            return JsonConvert.DeserializeObject<T>(responseString, serializer)!;
        }
    }

    public class DPoPProofPayload
    {
        [JsonProperty(JwtClaimTypes.JwtId)]
        public string JwtId { get; set; } = default!;

        [JsonProperty(JwtClaimTypes.DPoPHttpMethod)]
        public string DPoPHttpMethod { get; set; } = default!;

        [JsonProperty(JwtClaimTypes.DPoPHttpUrl)]
        public string DPoPHttpUrl { get; set; } = default!;

        [JsonProperty(JwtClaimTypes.IssuedAt)]
        public long IssuedAt { get; set; }

        [JsonProperty(JwtClaimTypes.DPoPAccessTokenHash)]
        public string? DPoPAccessTokenHash { get; set; }

        [JsonProperty(JwtClaimTypes.Nonce)]
        public string? Nonce { get; set; }
    }
}
