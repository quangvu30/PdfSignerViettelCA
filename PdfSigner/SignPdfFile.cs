using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Security.Cryptography;
using Org.BouncyCastle.X509;

namespace PdfSigner
{
    public class SignPdfFile
    {
        private string tmpFile;
        private DateTime signDate;
        private byte[] hash;
        private X509Certificate[] chain;
        private string fieldName;
        public static string HASH_ALGORITHM_SHA_1 = "SHA1";
        public static string HASH_ALGORITHM_SHA_256 = "SHA256";

        public string createHashExistedSignatureField(
          string filePath,
          X509Certificate[] chain,
          DisplayConfig displayConfig,
          string fieldName,
          string hashAlg)
        {
            try
            {
                SignPdfAsynchronous signPdfAsynchronous = new SignPdfAsynchronous();
                string tempFile = CertUtils.GenerateTempFile();
                this.fieldName = fieldName;
                DateTime signDate = displayConfig.SignDate;
                if (false)
                {
                    DateTime now = DateTime.Now;
                    displayConfig.SignDate = now;
                    this.signDate = now;
                }
                else
                    this.signDate = displayConfig.SignDate;
                List<byte[]> hash = signPdfAsynchronous.createHash(filePath, tempFile, fieldName, chain, displayConfig, hashAlg);
                if (hash == null)
                    return (string)null;
                this.tmpFile = tempFile;
                this.hash = hash[1];
                this.chain = chain;
                return Convert.ToBase64String(this.encodeData(hash[0], hashAlg));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return (string)null;
            }
        }

        public string createHash(
          string filePath,
          X509Certificate[] chain,
          DisplayConfig displayConfig,
          string hashAlg)
        {
            try
            {
                SignPdfAsynchronous signPdfAsynchronous = new SignPdfAsynchronous();
                string tempFile = CertUtils.GenerateTempFile();
                this.fieldName = CertUtils.getSignName();
                DateTime signDate = displayConfig.SignDate;
                if (false)
                {
                    DateTime now = DateTime.Now;
                    displayConfig.SignDate = now;
                    this.signDate = now;
                }
                else
                    this.signDate = displayConfig.SignDate;
                List<byte[]> hash = signPdfAsynchronous.createHash(filePath, tempFile, this.fieldName, chain, displayConfig, hashAlg);
                if (hash == null)
                    return (string)null;
                this.tmpFile = tempFile;
                this.hash = hash[1];
                this.chain = chain;
                return Convert.ToBase64String(this.encodeData(hash[0], hashAlg));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return (string)null;
            }
        }

        public bool insertSignature(
          string extSig,
          string destFile,
          TimestampConfig timestampConfig,
          string hashAlg)
        {
            try
            {
                SignPdfAsynchronous signPdfAsynchronous = new SignPdfAsynchronous();
                if (File.Exists(this.tmpFile))
                {
                    if (signPdfAsynchronous.insertSignature(this.tmpFile, destFile, this.fieldName, this.hash, Convert.FromBase64String(extSig), this.chain, this.signDate, timestampConfig, hashAlg))
                    {
                        try
                        {
                            File.Delete(this.tmpFile);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: delete file Temp is fail ");
                        }
                        return true;
                    }
                    try
                    {
                        File.Delete(this.tmpFile);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: delete file Temp is fail ");
                    }
                    return false;
                }
                Console.WriteLine("Error: file Temp is not exist ");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }
        }

        private byte[] encodeData(byte[] orginalData, string algorithm)
        {
            byte[] numArray = (byte[])null;
            if (SignPdfFile.HASH_ALGORITHM_SHA_1.Equals(algorithm))
                numArray = new SHA1Managed().ComputeHash(orginalData);
            else if (SignPdfFile.HASH_ALGORITHM_SHA_256.Equals(algorithm))
                numArray = new SHA256Managed().ComputeHash(orginalData);
            return numArray;
        }

        public void addPageEmpty(string src, string dest, DisplayConfig config)
        {
            PdfReader reader = (PdfReader)null;
            FileStream os = (FileStream)null;
            try
            {
                reader = new PdfReader(src);
                os = new FileStream(dest, FileMode.Append);
                PdfStamper pdfStamper = new PdfStamper(reader, (Stream)os);
                for (int pageNumber = 0; pageNumber < config.NumberPageSign; ++pageNumber)
                    pdfStamper.InsertPage(pageNumber, config.PageSize);
                PdfContentByte underContent = pdfStamper.GetUnderContent(1);
                float height = config.PageSize.Height;
                float width = config.PageSize.Width;
                float marginRight = config.MarginRight;
                float lly = height - config.MarginTop - config.HeightTitle;
                float heightTitle = config.HeightTitle;
                float num = width - config.MarginRight * 2f;
                BaseFont font = BaseFont.CreateFont(config.FontPath, "Identity-H", true);
                if (config.IsDisplayTitlePageSign)
                {
                    ColumnText columnText = new ColumnText(underContent);
                    columnText.SetSimpleColumn(marginRight, lly + config.HeightTitle, marginRight + num, lly + config.HeightTitle + config.HeightRowTitlePageSign);
                    columnText.AddElement((IElement)new Paragraph(config.TitlePageSign, new iTextSharp.text.Font(font, (float)config.FontSizeTitlePageSign, 1))
                    {
                        Alignment = 1
                    });
                    columnText.Go();
                }
                PdfPTable pdfPtable = new PdfPTable(config.WidthsPercen.Length);
                pdfPtable.SetWidths(config.WidthsPercen);
                pdfPtable.WidthPercentage = 100f;
                foreach (string title in config.Titles)
                {
                    Paragraph paragraph = new Paragraph(title, new iTextSharp.text.Font(font, (float)config.SizeFont, 1));
                    paragraph.Alignment = 1;
                    PdfPCell cell = new PdfPCell();
                    cell.AddElement((IElement)paragraph);
                    cell.FixedHeight = config.HeightTitle;
                    cell.BackgroundColor = config.BackgroundColorTitle;
                    pdfPtable.AddCell(cell);
                }
                ColumnText columnText1 = new ColumnText(underContent);
                columnText1.SetSimpleColumn(marginRight, lly, marginRight + num, lly + config.HeightTitle);
                columnText1.AddElement((IElement)pdfPtable);
                columnText1.Go();
                pdfStamper.Close();
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                if (os != null)
                {
                    try
                    {
                        os.Close();
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                }
                reader?.Close();
            }
        }
    }
}
