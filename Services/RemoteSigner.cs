using Org.BouncyCastle.X509;
using PdfSigner;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace GrpcServiceSigner.Services
{
    public class CertificateInfo
    {
        [JsonPropertyName("credential_id")]
        public string CredentialId { get; set; }

        [JsonPropertyName("key")]
        public KeyInfo Key { get; set; }

        [JsonPropertyName("cert")]
        public CertInfo Cert { get; set; }
    }

    public class CertInfo
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("issuerDN")]
        public string IssuerDN { get; set; }

        [JsonPropertyName("subjectDN")]
        public string SubjectDN { get; set; }

        [JsonPropertyName("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonPropertyName("validFrom")]
        public string ValidFrom { get; set; }

        [JsonPropertyName("validTo")]
        public string ValidTo { get; set; }

        [JsonPropertyName("certificates")]
        public List<string> Certificates { get; set; }
    }

    public class KeyInfo
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("algo")]
        public List<string> Algo { get; set; }

        [JsonPropertyName("len")]
        public int Len { get; set; }

        [JsonPropertyName("curve")]
        public string Curve { get; set; }
    }

    public class RemoteSigner
    {
        private readonly HttpClient client = new HttpClient();
        private SignPdfFile signer = new SignPdfFile();
        private string host = "https://remotesigning.viettel.vn";
        private string clientSecret = "";
        private string clientId = "";
        private string profileId = "";
        private string userId = "";
        private string hashAlgo = "";
        string output = Path.Join(Directory.GetCurrentDirectory(), "output");
        string input = Path.Join(Directory.GetCurrentDirectory(), "input");

        private string token = string.Empty;
        private string transactionId = string.Empty;
        private CertificateInfo certInfo;
        private string signature = string.Empty;

        public RemoteSigner(string _clientSecret, string _clientId, string _profileId, string _userId)
        {
            if (!Directory.Exists(output)) 
            {
                Directory.CreateDirectory(output); 
            }
            if (!Directory.Exists(input))
            {
                Directory.CreateDirectory(input);
            }
            clientId = _clientId;
            profileId = _profileId;
            userId = _userId;
            clientSecret = _clientSecret;
        }

        public async Task Login()
        {
            var url = $"{host}/vtss/service/ras/v1/login";
            var data = new
            {
                client_id = clientId,
                client_secret = clientSecret,
                profile_id = profileId,
                user_id = userId
            };
            var response = await client.PostAsync(url, CreateJsonContent(data));
            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(responseBody);
            token = json.GetProperty("access_token").GetString();
            Console.WriteLine($"[{userId}] Login successful.");
        }

        public async Task GetCertificateInfo()
        {
            var url = $"{host}/vtss/service/certificates/info";
            var data = new
            {
                client_id = clientId,
                client_secret = clientSecret,
                profile_id = profileId,
                user_id = userId,
                certificates = "chain",
                certInfo = true,
                authInfo = true
            };
            var response = await client.PostAsync(url, CreateJsonContent(data, token));
            var responseBody = await response.Content.ReadAsStringAsync();
            var certList = JsonSerializer.Deserialize<CertificateInfo[]>(responseBody);
            certInfo = certList[0];
            Console.WriteLine($"[{userId}] Certificate info retrieved.");
        }

        public async Task SignDocument(string filePath, string description, string documentId, string signatureImgPath, int numPageSign, int x, int y, int w, int h)
        {
            string documentName = Path.GetFileName(filePath);
            // Load cert
            List<X509Certificate> chain = new List<X509Certificate>();
            foreach(string cert in certInfo.Cert.Certificates)
            {
                chain.Add(CertUtils.GetX509Cert(cert));
                if (chain.Count == 2) break;
            }
            Console.WriteLine($"[{userId}] Load cert done.");

            // Create display config
            DisplayConfig displayConfig = DisplayConfig.generateDisplayConfigImageDefault(numPageSign, x, y, w, h, signatureImgPath);
            string hash = signer.createHash(filePath, chain.ToArray(), displayConfig, SignPdfFile.HASH_ALGORITHM_SHA_256);
            var url = $"{host}/vtss/service/signHash";
            var data = new
            {
                credentialID = certInfo.CredentialId,
                client_id = clientId,
                client_secret = clientSecret,
                numSignatures = 1,
                description,
                documents = new[]
                {
                    new { document_id = documentId, document_name = Convert.ToBase64String(Encoding.UTF8.GetBytes(documentName)) }
                },
                hash = new[] { hash },
                hashAlgo,
                signAlgo = certInfo.Key.Algo[0],
                async = 2
            };

            var response = await client.PostAsync(url, CreateJsonContent(data, token));
            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(responseBody);
            transactionId = json.GetProperty("transactionId").GetString();
            Console.WriteLine($"[{userId}] Document is signing - {transactionId}");
        }

        public async Task<bool> GetSignStatus()
        {
            var url = $"{host}/vtss/service/requests/status";
            var data = new { transactionId };
            var response = await client.PostAsync(url, CreateJsonContent(data, token));
            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(responseBody);
            string status = json.GetProperty("status").GetString();

            switch (status)
            {
                case "1":
                    string rawSignature = json.GetProperty("signatures")[0].GetString();
                    signature = Regex.Replace(rawSignature, @"\r\n", "");
                    Console.WriteLine("Status Code: 1, Message: Success");
                    return true;
                case "4000":
                    Console.WriteLine("Status Code: 4000, Message: Waiting for user confirmation");
                    break;
                case "6000":
                    Console.WriteLine("Status Code: 6000, Message: Request has been confirmed by the user, system is signing");
                    break;
                case "4001":
                    Console.WriteLine("Status Code: 4001, Message: Signing request timed out");
                    break;
                case "4002":
                    Console.WriteLine("Status Code: 4002, Message: User declined to sign");
                    break;
                case "4004":
                    Console.WriteLine("Status Code: 4004, Message: Signing failed (an error occurred)");
                    break;
                case "4005":
                    Console.WriteLine("Status Code: 4005, Message: Insufficient balance/signing quota");
                    break;
                case "13004":
                    Console.WriteLine("Status Code: 13004, Message: Digital certificate has expired or been revoked");
                    break;
                case "50000":
                    Console.WriteLine("Status Code: 50000, Message: An error occurred while retrieving information");
                    break;
                default:
                    Console.WriteLine("Signing in progress...");
                    return false;
            }
            return false;
        }

        public async Task<string> SignAsync(string filePath, string description, string documentId, string signatureImgPath, int numPageSign, int x, int y, int w, int h)
        {
            string o = Path.Join(output, Path.GetFileName(filePath));
            await Login();
            await GetCertificateInfo();
            await SignDocument(filePath, description, documentId, signatureImgPath, numPageSign, x, y, w, h);
            bool status = await GetSignStatus();
            int timeout = 0;
            while (!status)
            {
                await Task.Delay(2000);
                status = await GetSignStatus();
                if (timeout > 100)
                {
                    return "timeout";
                }
            }
            Console.WriteLine($"[{userId}] Signing process finished.");
            TimestampConfig timestampConfig = new TimestampConfig { UseTimestamp = false };
            var isSuccess = signer.insertSignature(signature, o, timestampConfig, SignPdfFile.HASH_ALGORITHM_SHA_256);
            if (isSuccess)
            {
                Console.WriteLine($"[{userId}] Embed signature successful.");
            }
            else
            {
                Console.WriteLine($"[{userId}] Embed signature fail.");
                return null;
            }

            return o;
        }

        private StringContent CreateJsonContent(object data, string authToken = null)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            if (authToken != null)
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            }
            return content;
        }
    }
}
