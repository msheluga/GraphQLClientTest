using GraphQL;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GraphQLClientTest
{
    internal class GraphQL_Client : IGraphQL_Client
    {
        private readonly HttpClient _httpClient;
        private IOptionsMonitor<GraphqlAPIOptions> _graphqlMonitor;
       

        public GraphQL_Client(HttpClient httpClient
            , IOptionsMonitor<GraphqlAPIOptions> graphqlMonitor)
        {
            _httpClient = httpClient;
            _graphqlMonitor = graphqlMonitor;
            _httpClient.BaseAddress = new Uri(_graphqlMonitor.CurrentValue.Uri);
          
        }

        internal static void Run()
        {
            var query = new StringContent("{\"query\":\"query Version { version {nodes {systemInformationId databaseVersion versionDate modifiedDate}}}\",\"variables\":{}}", null, "application/json");
            var response = SendGraphQLQuery(query).Result;
            Task.FromResult(response);
        }

        //send out the http query
        public static async Task<dynamic> SendGraphQLQuery(StringContent query)
        {
            string responseString = string.Empty;

            try
            {
                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.BaseAddress = new Uri("https://localhost/graphQL/");
                BuildHttpMessage(query, client);


                var jsonResponse = await client.PostAsync("https://localhost/graphQL/", query);
                jsonResponse.EnsureSuccessStatusCode();
                responseString = jsonResponse.Content.ReadAsStringAsync().Result;
                Console.WriteLine(responseString);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                
            }           
            return Task.FromResult(JsonConvert.DeserializeObject(responseString));
        }

        private static void BuildHttpMessage(StringContent query, HttpClient httpClient)
        {
            string method = HttpMethod.Post.ToString();
            string encodedPathandQuery = httpClient.BaseAddress?.PathAndQuery ?? "";
            string contentMD5 = query.ToString().ComputeMd5Hash();
            string authorizationHeader = GenerateAuthHeader(contentMD5, method, encodedPathandQuery);
                                 
            
            query.Headers.ContentMD5 = Convert.FromBase64String(contentMD5);
            httpClient.DefaultRequestHeaders.Add("Date", DateTime.Now.ToString("R"));
            httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeader);

        }

        //generate the Hmac auth header 
        public static string GenerateAuthHeader(string contentMD5, string method, string encodedPathandQuery)
        {
            string authHeader = string.Empty;
            string nonce = GenerateNonce();
            string timestamp = GenerateTimestamp();
            string privateApiKey = GetPrivateKey();
            string message = String.Empty;
            if (string.IsNullOrEmpty(privateApiKey))
            {
                message = "Error retrieving private api key for portal application.";
                Console.WriteLine(message);
                return authHeader;
            }

            string applicationId = GetAppId();
            if (string.IsNullOrEmpty(applicationId))
            {
                message = "Error retrieving application id for portal application.";
                Console.WriteLine(message);

                return authHeader;
            }

            if (string.IsNullOrEmpty(encodedPathandQuery) || string.IsNullOrEmpty(contentMD5) || string.IsNullOrEmpty(method))
                return authHeader;

            string[] requestSignatureArray = new string[]
            {
                applicationId,
                timestamp,
                nonce,
                method,
                encodedPathandQuery,
                contentMD5
            };

            string delimitedSignature = string.Join('|', requestSignatureArray);

            using HMAC hmac = new HMACSHA256(Convert.FromBase64String(privateApiKey));
            string encryptedComputedSignature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(delimitedSignature)));

            authHeader = "E920" + " " + applicationId + "|" + timestamp + "|" + nonce + "|" + encryptedComputedSignature;

            return authHeader;
        }

        private static string GetAppId()
        {
            return "3ee650d9-9c93-4d26-af10-cd67a1abd10f";
        }

        private static string GetPrivateKey()
        {
            return "iOgA8HnU+zcz9/eIq8ZOznUSeyfgTahXkaUlTe1PDBQ=";
        }

        private static string GenerateTimestamp()
        {
            return DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
        }
        private static string GenerateNonce()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
