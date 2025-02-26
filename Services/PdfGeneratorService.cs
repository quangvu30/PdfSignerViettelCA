using Grpc.Core;
using Spire.Doc;
using Newtonsoft.Json.Linq;

namespace GrpcServiceSigner.Services
{
    public class PdfGeneratorService : PdfGenerator.PdfGeneratorBase
    {
        private readonly string input = Path.Join(Directory.GetCurrentDirectory(), "input");
        private readonly string output = Path.Join(Directory.GetCurrentDirectory(), "output");

        public override async Task<UploadRes> UploadFile(UploadReq request, ServerCallContext context)
        {
            string fileExt = Path.GetExtension(request.FileName);
            if (fileExt != "docx" || fileExt != "doc")
            {
                return new UploadRes
                {
                    Msg = "Error: Only upload doc/docx file."
                };
            }
            string filePath = Path.Join(input, request.FileName);
            File.WriteAllBytes(filePath, request.Buffers.ToByteArray());
            return new UploadRes
            {
                Msg = "Ok"
            };
        }

        public override async Task<GenPdfRes> GeneratePdf(GenPdfReq request, ServerCallContext context)
        {
            Document document = new Document();
            string filePath = Path.Join(input, request.FileName);
            document.LoadFromFile(filePath);

            JObject data = JObject.Parse(request.Data);
            string[] keys = data.Properties().Select(p => p.Name).ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                document.Replace(keys[i], data[keys[i]].ToString(), false, true);
            }

            string pdfFile = Path.Join(output, $"{Path.GetFileName(request.FileName)}.pdf");
            document.SaveToFile(pdfFile, FileFormat.PDF);
            document.Close();
            document.Dispose();
            
            byte[] fileContent = File.ReadAllBytes(pdfFile);
            File.Delete(pdfFile);
            return new GenPdfRes
            {
                Buffers = Google.Protobuf.ByteString.CopyFrom(fileContent),
            };
        }
    }
}
