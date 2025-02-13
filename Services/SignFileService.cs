using Grpc.Core;

namespace GrpcServiceSigner.Services
{
    public class SignFileService : SignFile.SignFileBase
    {
        public override Task<SignRes> Sign(SignReq req, ServerCallContext context)
        {
            Console.WriteLine(req);
            return Task.FromResult(new SignRes
            {
                Success = true,
                Txn = "asdkasdk",
                Msg = "sdsda"
            });
        }
    }
}
