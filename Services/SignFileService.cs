using Grpc.Core;

namespace GrpcServiceSigner.Services
{
    public class SignFileService : SignFile.SignFileBase
    {
        private readonly Dictionary<string, ClientPayload> clients = new Dictionary<string, ClientPayload>();
        private readonly string input = Path.Join(Directory.GetCurrentDirectory(), "input");

        public SignFileService(Dictionary<string, ClientPayload> _clients) 
        {
            clients = _clients;
        }

        public override Task<LoginRes> Login(LoginReq request, ServerCallContext context)
        {
            clients.Add(request.UserId, new ClientPayload { ClientId = request.ClientId, ClientSecret = request.ClientSecret, ProfileId = request.ProfileId });
            return Task.FromResult(new LoginRes
            {
                Msg = "Success"
            });
        }

        public override async Task<SignRes> Sign(SignReq req, ServerCallContext context)
        {
            ClientPayload user = clients[req.UserId];
            RemoteSigner remoteSigner = new RemoteSigner(user.ClientSecret, user.ClientId, user.ProfileId, req.UserId);
            // Save file to input
            string pdfFile = Path.Join(input, $"presign_{req.FileName}.pdf");
            string signatureImg = Path.Join(input, $"{req.FileName}.png");
            await File.WriteAllBytesAsync(pdfFile, req.Chunk.ToByteArray());
            await File.WriteAllBytesAsync(signatureImg, req.SignatureImg.ToByteArray());
            string signedFile = await remoteSigner.SignAsync(pdfFile, req.Description, req.FileName, signatureImg, req.NumPage, req.X, req.Y, req.W, req.H);
            switch (signedFile)
            {
                case "timeout":
                    return new SignRes
                    {
                        Success = false,
                        Msg = "timeout"
                    };
                case null:
                    return new SignRes
                    {
                        Success = false,
                        Msg = "Something wrong"
                    };
                default:
                    byte[] fileContent = await File.ReadAllBytesAsync(signedFile);
                    File.Delete(pdfFile);
                    File.Delete(signatureImg);
                    File.Delete(signedFile);
                    return new SignRes
                    {
                        Success = true,
                        Chunk = Google.Protobuf.ByteString.CopyFrom(fileContent)
                    };
            }
            
        }
    }
}
