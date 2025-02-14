using com.itextpdf.text.pdf.security;
using iTextSharp.text.io;
using iTextSharp.text.pdf.security;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace PdfSigner
{
    public class SignPdfAsynchronous
    {
        private static string HASH_ALGORITHM_SHA_1 = "SHA1";
        private static string HASH_ALGORITHM_SHA_256 = "SHA256";
        private static string CRYPT_ALG = "RSA";

        public bool insertSignature(
          string src,
          string dest,
          string fieldName,
          byte[] hash,
          byte[] extSignature,
          X509Certificate[] chain,
          DateTime signDate,
          TimestampConfig timestampConfig,
          string hashAlgorithm)
        {
            PdfReader pdfReader = (PdfReader)null;
            FileStream fileStream = (FileStream)null;
            try
            {
                pdfReader = new PdfReader(src);
                fileStream = new FileStream(dest, FileMode.Append);
                AcroFields acroFields = pdfReader.AcroFields;
                PdfDictionary signatureDictionary = acroFields.GetSignatureDictionary(fieldName);
                if (signatureDictionary == null)
                {
                    Console.WriteLine("No field");
                    return false;
                }
                if (!acroFields.SignatureCoversWholeDocument(fieldName))
                {
                    Console.WriteLine("Not the last signature");
                    return false;
                }
                PdfArray asArray = signatureDictionary.GetAsArray(PdfName.BYTERANGE);
                long[] numArray = asArray.AsLongArray();
                if (asArray.Size != 4 || numArray[0] != 0L)
                {
                    Console.WriteLine("Single exclusion space supported");
                    return false;
                }
                IRandomAccessSource sourceView = pdfReader.SafeFile.CreateSourceView();
                PdfPKCS7 pdfPkcS7 = new PdfPKCS7((ICipherParameters)null, (ICollection<X509Certificate>)chain, hashAlgorithm, false);
                pdfPkcS7.SetExternalDigest(extSignature, (byte[])null, SignPdfAsynchronous.CRYPT_ALG);
                DateTime signingTime = signDate;
                TSAClientBouncyCastle clientBouncyCastle = (TSAClientBouncyCastle)null;
                if (timestampConfig.UseTimestamp)
                    clientBouncyCastle = new TSAClientBouncyCastle(timestampConfig.TsaUrl, timestampConfig.TsaAcc, timestampConfig.TsaPass);
                byte[] encodedPkcS7 = pdfPkcS7.GetEncodedPKCS7(hash, signingTime, (ITSAClient)clientBouncyCastle, (byte[])null, (ICollection<byte[]>)null, CryptoStandard.CMS);
                int num1 = (int)(numArray[2] - numArray[1]) - 2;
                if ((num1 & 1) != 0)
                {
                    Console.WriteLine("Gap is not a multiple of 2");
                    return false;
                }
                int num2 = num1 / 2;
                if (num2 < encodedPkcS7.Length)
                {
                    Console.WriteLine("Not enough space");
                    return false;
                }
                StreamUtil.CopyBytes(sourceView, 0L, numArray[1] + 1L, (Stream)fileStream);
                ByteBuffer byteBuffer = new ByteBuffer(num2 * 2);
                foreach (byte b in encodedPkcS7)
                    byteBuffer.AppendHex(b);
                int num3 = (num2 - encodedPkcS7.Length) * 2;
                for (int index = 0; index < num3; ++index)
                    byteBuffer.Append((byte)48);
                byteBuffer.WriteTo((Stream)fileStream);
                StreamUtil.CopyBytes(sourceView, numArray[2] - 1L, numArray[3] + 1L, (Stream)fileStream);
                fileStream.Close();
                sourceView.Close();
                byteBuffer.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error insertSignature: " + ex.Message);
                return false;
            }
            finally
            {
                if (fileStream != null)
                {
                    try
                    {
                        fileStream.Close();
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("Error insertSignature: " + ex.Message);
                    }
                }
                pdfReader?.Close();
            }
        }

        public List<byte[]> createHash(
          string src,
          string tempFile,
          string fileName,
          X509Certificate[] chain,
          DisplayConfig displayConfig,
          string hashAlgorithm)
        {
            if (displayConfig.SignType == DisplayConfig.SIGN_TYPE_CREATE_NEW_EMPTY_SIGNATURE_FIELD)
            {
                if (displayConfig.TypeDisplay == DisplayConfig.DISPLAY_TABLE)
                {
                    if (!this.emptySignatureTable(src, tempFile, fileName, displayConfig, chain[0]))
                        return (List<byte[]>)null;
                }
                else if (!this.emptySignature(src, tempFile, fileName, displayConfig, chain[0]))
                    return (List<byte[]>)null;
            }
            else if (!this.emptySignature(src, tempFile, fileName, displayConfig, chain[0]))
                return (List<byte[]>)null;
            return this.preSign(tempFile, fileName, chain, displayConfig.SignDate, hashAlgorithm);
        }

        private List<byte[]> preSign(
          string src,
          string fieldName,
          X509Certificate[] chain,
          DateTime signDate,
          string hashAlgorithm)
        {
            PdfReader pdfReader = (PdfReader)null;
            try
            {
                List<byte[]> numArrayList = new List<byte[]>();
                pdfReader = new PdfReader(src);
                PdfDictionary signatureDictionary = pdfReader.AcroFields.GetSignatureDictionary(fieldName);
                if (signatureDictionary == null)
                {
                    Console.WriteLine("No field");
                    return (List<byte[]>)null;
                }
                PdfArray asArray = signatureDictionary.GetAsArray(PdfName.BYTERANGE);
                long[] ranges = asArray.AsLongArray();
                if (asArray.Size != 4 || ranges[0] != 0L)
                {
                    Console.WriteLine("Single exclusion space supported");
                    return (List<byte[]>)null;
                }
                Stream data = (Stream)new RASInputStream(new RandomAccessSourceFactory().CreateRanged(pdfReader.SafeFile.CreateSourceView(), (IList<long>)ranges));
                PdfPKCS7 pdfPkcS7 = new PdfPKCS7((ICipherParameters)null, (ICollection<X509Certificate>)chain, hashAlgorithm, false);
                byte[] secondDigest = DigestAlgorithms.Digest(data, DigestUtilities.GetDigest(hashAlgorithm));
                DateTime signingTime = signDate;
                byte[] authenticatedAttributeBytes = pdfPkcS7.getAuthenticatedAttributeBytes(secondDigest, signingTime, (byte[])null, (ICollection<byte[]>)null, CryptoStandard.CMS);
                numArrayList.Add(authenticatedAttributeBytes);
                numArrayList.Add(secondDigest);
                return numArrayList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error create hash: " + ex.Message);
                return (List<byte[]>)null;
            }
            finally
            {
                try
                {
                    pdfReader?.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error create hash: " + ex.Message);
                }
            }
        }

        private bool emptySignature(
          string src,
          string dest,
          string fieldName,
          DisplayConfig displayConfig,
          X509Certificate cert)
        {
            PdfReader reader = (PdfReader)null;
            FileStream os = (FileStream)null;
            try
            {
                reader = new PdfReader(src);
                int numberOfPages = reader.NumberOfPages;
                int page = displayConfig.NumberPageSign;
                if (page < 1 || page > numberOfPages)
                    page = 1;
                os = new FileStream(dest, FileMode.Create);
                PdfSignatureAppearance signatureAppearance = PdfStamper.CreateSignature(reader, (Stream)os, char.MinValue, (string)null, true).SignatureAppearance;
                DateTime signDate = displayConfig.SignDate;
                if ("".Equals(displayConfig.Contact))
                    displayConfig.Contact = CertUtils.GetCN(cert);
                signatureAppearance.Contact = displayConfig.Contact;
                signatureAppearance.SignDate = signDate;
                signatureAppearance.Reason = displayConfig.Reason;
                signatureAppearance.Location = displayConfig.Location;
                string str1 = string.Format(displayConfig.DateFormatstring, (object)signDate);
                if (displayConfig.IsDisplaySignature)
                {
                    float coorXrectangle = displayConfig.CoorXRectangle;
                    float coorYrectangle = displayConfig.CoorYRectangle;
                    float widthRectangle = displayConfig.WidthRectangle;
                    float heightRectangle = displayConfig.HeightRectangle;
                    Rectangle pageRect = new Rectangle(coorXrectangle, coorYrectangle, coorXrectangle + widthRectangle, coorYrectangle + heightRectangle);
                    if (displayConfig.SignType == DisplayConfig.SIGN_TYPE_CREATE_NEW_EMPTY_SIGNATURE_FIELD)
                        signatureAppearance.SetVisibleSignature(pageRect, page, fieldName);
                    else
                        signatureAppearance.SetVisibleSignature(fieldName);
                    if (displayConfig.TypeDisplay == DisplayConfig.DISPLAY_IMAGE_STAMP)
                    {
                        Image instance = Image.GetInstance(displayConfig.PathImage);
                        float height1 = instance.Height;
                        float width1 = instance.Width;
                        Rectangle rect = signatureAppearance.Rect;
                        float width2 = rect.Width;
                        float height2 = rect.Height;
                        float num1 = width2 / width1;
                        float num2 = height2 / height1;
                        float num3 = num1;
                        if ((double)num2 < (double)num1)
                            num3 = num2;
                        if ((double)num3 > 1.0)
                            num3 = 1f;
                        signatureAppearance.Layer2Text = "";
                        signatureAppearance.Image = instance;
                        signatureAppearance.ImageScale = num3;
                    }
                    else if (displayConfig.TypeDisplay == DisplayConfig.DISPLAY_RECTANGLE_TEXT)
                    {
                        PdfTemplate layer = signatureAppearance.GetLayer(2);
                        float left = layer.BoundingBox.Left;
                        float bottom = layer.BoundingBox.Bottom;
                        float width = layer.BoundingBox.Width;
                        float height = layer.BoundingBox.Height;
                        ColumnText columnText = new ColumnText((PdfContentByte)layer);
                        columnText.SetSimpleColumn(left, bottom, width, height);
                        BaseFont font = BaseFont.CreateFont(displayConfig.FontPath, "Identity-H", true);
                        string str2;
                        if (displayConfig.DisplayText != null && displayConfig.DisplayText.Length != 0)
                            str2 = displayConfig.DisplayText;
                        else
                            str2 = string.Format(displayConfig.FormatRectangleText, (object)displayConfig.Contact, (object)str1, (object)displayConfig.Reason, (object)displayConfig.Location);
                        columnText.AddElement((IElement)new Paragraph(str2, new iTextSharp.text.Font(font, (float)displayConfig.SizeFont))
                        {
                            Alignment = 0
                        });
                        columnText.Go();
                    }
                    else if (displayConfig.TypeDisplay == DisplayConfig.DISPLAY_IMAGE_WITH_TEXT)
                    {
                        Image instance = Image.GetInstance(displayConfig.PathImage);
                        signatureAppearance.SignatureGraphic = instance;
                        signatureAppearance.ImageScale = -1f;
                        signatureAppearance.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION;
                        BaseFont font1 = BaseFont.CreateFont(displayConfig.FontPath, "Identity-H", true);
                        string str3;
                        if (displayConfig.DisplayText != null && displayConfig.DisplayText.Length != 0)
                            str3 = displayConfig.DisplayText;
                        else
                            str3 = string.Format(displayConfig.FormatRectangleText, (object)displayConfig.Contact, (object)str1, (object)displayConfig.Reason, (object)displayConfig.Location);
                        signatureAppearance.Layer2Text = str3;
                        iTextSharp.text.Font font2 = new iTextSharp.text.Font(font1, 6f);
                        signatureAppearance.Layer2Font = font2;
                    }
                }
                else if (displayConfig.SignType == DisplayConfig.SIGN_TYPE_CREATE_NEW_EMPTY_SIGNATURE_FIELD)
                    signatureAppearance.SetVisibleSignature(new Rectangle(0.0f, 0.0f, 0.0f, 0.0f), 1, fieldName);
                else
                    signatureAppearance.SetVisibleSignature(fieldName);
                IExternalSignatureContainer externalSignatureContainer = (IExternalSignatureContainer)new ExternalBlankSignatureContainer(PdfName.ADOBE_PPKLITE, PdfName.ADBE_PKCS7_DETACHED);
                MakeSignature.SignExternalContainer(signatureAppearance, externalSignatureContainer, 8192);
                reader.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error emptySignature: " + ex.Message);
                return false;
            }
            finally
            {
                reader?.Close();
                if (os != null)
                {
                    try
                    {
                        os.Close();
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("Error emptySignature: " + ex.Message);
                    }
                }
            }
        }

        private bool emptySignatureTable(
          string src,
          string dest,
          string fieldName,
          DisplayConfig displayConfig,
          X509Certificate cert)
        {
            PdfReader reader = (PdfReader)null;
            FileStream os = (FileStream)null;
            try
            {
                reader = new PdfReader(src);
                AcroFields acroFields = reader.AcroFields;
                List<string> signatureNames = acroFields.GetSignatureNames();
                int index1 = 1;
                float[] numArray = new float[displayConfig.MaxPageSign];
                foreach (string name in signatureNames)
                {
                    IList<AcroFields.FieldPosition> fieldPositions = acroFields.GetFieldPositions(name);
                    int page = fieldPositions[0].page;
                    if (page > index1)
                        index1 = page;
                    float height = fieldPositions[0].position.Height;
                    numArray[page] += height;
                }
                Rectangle pageSize = reader.GetPageSize(index1);
                float height1 = pageSize.Height;
                float width1 = pageSize.Width;
                float marginRight = displayConfig.MarginRight;
                float num = width1 - displayConfig.MarginRight * 2f;
                os = new FileStream(dest, FileMode.Create);
                PdfSignatureAppearance signatureAppearance = PdfStamper.CreateSignature(reader, (Stream)os, char.MinValue, (string)null, true).SignatureAppearance;
                if ("".Equals(displayConfig.Contact))
                    displayConfig.Contact = CertUtils.GetCN(cert);
                signatureAppearance.Contact = displayConfig.Contact;
                signatureAppearance.Reason = displayConfig.Reason;
                signatureAppearance.Location = displayConfig.Location;
                DateTime signDate = displayConfig.SignDate;
                signatureAppearance.SignDate = signDate;
                PdfPTable pdfPtable = new PdfPTable(displayConfig.WidthsPercen.Length);
                pdfPtable.SetWidths(displayConfig.WidthsPercen);
                pdfPtable.WidthPercentage = 100f;
                pdfPtable.TotalWidth = num;
                BaseFont font = BaseFont.CreateFont(displayConfig.FontPath, "Identity-H", true);
                for (int index2 = 0; index2 < displayConfig.TextArray.Length; ++index2)
                {
                    Paragraph paragraph = new Paragraph(displayConfig.TextArray[index2], new iTextSharp.text.Font(font, (float)displayConfig.SizeFont));
                    paragraph.Alignment = displayConfig.AlignmentArray[index2];
                    PdfPCell cell = new PdfPCell();
                    cell.AddElement((IElement)paragraph);
                    pdfPtable.AddCell(cell);
                }
                float totalHeight = pdfPtable.TotalHeight;
                float lly = height1 - numArray[index1] - displayConfig.MarginTop - totalHeight - displayConfig.HeightTitle;
                if ((double)lly < (double)displayConfig.MarginBottom)
                {
                    if (index1 < displayConfig.TotalPageSign)
                    {
                        ++index1;
                        lly = height1 - displayConfig.MarginTop - totalHeight - displayConfig.HeightTitle;
                    }
                    else
                    {
                        Rectangle pageRect = new Rectangle(0.0f, 0.0f, 0.0f, 0.0f);
                        signatureAppearance.SetVisibleSignature(pageRect, index1, fieldName);
                        IExternalSignatureContainer externalSignatureContainer = (IExternalSignatureContainer)new ExternalBlankSignatureContainer(PdfName.ADOBE_PPKLITE, PdfName.ADBE_PKCS7_DETACHED);
                        MakeSignature.SignExternalContainer(signatureAppearance, externalSignatureContainer, 8192);
                        reader.Close();
                        return true;
                    }
                }
                Rectangle pageRect1 = new Rectangle(marginRight, lly, marginRight + num, lly + totalHeight);
                signatureAppearance.SetVisibleSignature(pageRect1, index1, fieldName);
                PdfTemplate layer = signatureAppearance.GetLayer(0);
                float left = layer.BoundingBox.Left;
                float bottom = layer.BoundingBox.Bottom;
                float width2 = layer.BoundingBox.Width;
                float height2 = layer.BoundingBox.Height;
                ColumnText columnText = new ColumnText((PdfContentByte)signatureAppearance.GetLayer(2));
                columnText.SetSimpleColumn(left, bottom, left + width2, bottom + height2);
                columnText.AddElement((IElement)pdfPtable);
                columnText.Go();
                IExternalSignatureContainer externalSignatureContainer1 = (IExternalSignatureContainer)new ExternalBlankSignatureContainer(PdfName.ADOBE_PPKLITE, PdfName.ADBE_PKCS7_DETACHED);
                MakeSignature.SignExternalContainer(signatureAppearance, externalSignatureContainer1, 8192);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error emptySignatureTable: " + ex.Message);
                return false;
            }
            finally
            {
                reader?.Close();
                if (os != null)
                {
                    try
                    {
                        os.Close();
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("Error emptySignatureTable: " + ex.Message);
                    }
                }
            }
        }
    }
}