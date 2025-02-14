using Org.BouncyCastle.Security;


namespace PdfSigner
{
    public class CertUtils
    {
        public static byte[] GetBytes(string str)
        {
            try
            {
                byte[] dst = new byte[str.Length * 2];
                Buffer.BlockCopy((Array)str.ToCharArray(), 0, (Array)dst, 0, dst.Length);
                return dst;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error get Byte from string: " + ex.Message);
                return (byte[])null;
            }
        }

        public static Org.BouncyCastle.X509.X509Certificate GetX509Cert(string CertStr)
        {
            try
            {
                byte[] bytes = CertUtils.GetBytes(CertStr);
                System.Security.Cryptography.X509Certificates.X509Certificate2 x509Cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(bytes);
                return DotNetUtilities.FromX509Certificate(x509Cert);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error get Chain cert: " + ex.Message);
                return (Org.BouncyCastle.X509.X509Certificate)null;
            }
        }

        public static string ConvertTVKhongDau(string str)
        {
            string[] strArray1 = new string[17]
            {
        "à",
        "á",
        "ạ",
        "ả",
        "ã",
        "â",
        "ầ",
        "ấ",
        "ậ",
        "ẩ",
        "ẫ",
        "ă",
        "ằ",
        "ắ",
        "ặ",
        "ẳ",
        "ẵ"
            };
            string[] strArray2 = new string[17]
            {
        "À",
        "Á",
        "Ạ",
        "Ả",
        "Ã",
        "Â",
        "Ầ",
        "Ấ",
        "Ậ",
        "Ẩ",
        "Ẫ",
        "Ă",
        "Ằ",
        "Ắ",
        "Ặ",
        "Ẳ",
        "Ẵ"
            };
            string[] strArray3 = new string[11]
            {
        "è",
        "é",
        "ẹ",
        "ẻ",
        "ẽ",
        "ê",
        "ề",
        "ế",
        "ệ",
        "ể",
        "ễ"
            };
            string[] strArray4 = new string[11]
            {
        "È",
        "É",
        "Ẹ",
        "Ẻ",
        "Ẽ",
        "Ê",
        "Ề",
        "Ế",
        "Ệ",
        "Ể",
        "Ễ"
            };
            string[] strArray5 = new string[5]
            {
        "ì",
        "í",
        "ị",
        "ỉ",
        "ĩ"
            };
            string[] strArray6 = new string[5]
            {
        "Ì",
        "Í",
        "Ị",
        "Ỉ",
        "Ĩ"
            };
            string[] strArray7 = new string[17]
            {
        "ò",
        "ó",
        "ọ",
        "ỏ",
        "õ",
        "ô",
        "ồ",
        "ố",
        "ộ",
        "ổ",
        "ỗ",
        "ơ",
        "ờ",
        "ớ",
        "ợ",
        "ở",
        "ỡ"
            };
            string[] strArray8 = new string[17]
            {
        "Ò",
        "Ó",
        "Ọ",
        "Ỏ",
        "Õ",
        "Ô",
        "Ồ",
        "Ố",
        "Ộ",
        "Ổ",
        "Ỗ",
        "Ơ",
        "Ờ",
        "Ớ",
        "Ợ",
        "Ở",
        "Ỡ"
            };
            string[] strArray9 = new string[11]
            {
        "ù",
        "ú",
        "ụ",
        "ủ",
        "ũ",
        "ư",
        "ừ",
        "ứ",
        "ự",
        "ử",
        "ữ"
            };
            string[] strArray10 = new string[11]
            {
        "Ù",
        "Ú",
        "Ụ",
        "Ủ",
        "Ũ",
        "Ư",
        "Ừ",
        "Ứ",
        "Ự",
        "Ử",
        "Ữ"
            };
            string[] strArray11 = new string[5]
            {
        "ỳ",
        "ý",
        "ỵ",
        "ỷ",
        "ỹ"
            };
            string[] strArray12 = new string[5]
            {
        "Ỳ",
        "Ý",
        "Ỵ",
        "Ỷ",
        "Ỹ"
            };
            str = str.Replace("đ", "d");
            str = str.Replace("Đ", "D");
            foreach (string oldValue in strArray1)
                str = str.Replace(oldValue, "a");
            foreach (string oldValue in strArray2)
                str = str.Replace(oldValue, "A");
            foreach (string oldValue in strArray3)
                str = str.Replace(oldValue, "e");
            foreach (string oldValue in strArray4)
                str = str.Replace(oldValue, "E");
            foreach (string oldValue in strArray5)
                str = str.Replace(oldValue, "i");
            foreach (string oldValue in strArray6)
                str = str.Replace(oldValue, "I");
            foreach (string oldValue in strArray7)
                str = str.Replace(oldValue, "o");
            foreach (string oldValue in strArray8)
                str = str.Replace(oldValue, "O");
            foreach (string oldValue in strArray9)
                str = str.Replace(oldValue, "u");
            foreach (string oldValue in strArray10)
                str = str.Replace(oldValue, "U");
            foreach (string oldValue in strArray11)
                str = str.Replace(oldValue, "y");
            foreach (string oldValue in strArray12)
                str = str.Replace(oldValue, "Y");
            return str;
        }

        public static string GetCNFromDN(string dn)
        {
            try
            {
                string[] strArray = dn.Split(',');
                string str = "";
                for (int index = 0; index < strArray.Length; ++index)
                {
                    if (strArray[index].IndexOf("CN=") != -1)
                        str = strArray[index].Split('=')[1];
                }
                return str != "" ? CertUtils.ConvertTVKhongDau(str) : str;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetCNFromDN: " + ex.Message);
                return "";
            }
        }

        public static string GetLocationFromDN(string dn)
        {
            try
            {
                string[] strArray = dn.Split(',');
                string str = "";
                for (int index = 0; index < strArray.Length; ++index)
                {
                    if (strArray[index].IndexOf("L=") != -1)
                        str = strArray[index].Split('=')[1];
                }
                return str != "" ? CertUtils.ConvertTVKhongDau(str) : str;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetLocationFromDN: " + ex.Message);
                return "";
            }
        }

        public static string GetCN(Org.BouncyCastle.X509.X509Certificate cert)
        {
            try
            {
                return CertUtils.GetCNFromDN(CertUtils.GetSubject(cert));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getCN: " + ex.Message);
                return (string)null;
            }
        }

        public static string GetSubject(Org.BouncyCastle.X509.X509Certificate certificate)
        {
            try
            {
                return certificate.SubjectDN.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getSubject: " + ex.Message);
                return (string)null;
            }
        }

        public static string GetLocation(Org.BouncyCastle.X509.X509Certificate certificate)
        {
            try
            {
                return CertUtils.GetLocationFromDN(CertUtils.GetSubject(certificate));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getLocation: " + ex.Message);
                return (string)null;
            }
        }

        public static string GenerateTempFile()
        {
            try
            {
                return Path.GetTempFileName();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error create temp file: " + ex.Message);
                return "";
            }
        }

        public static string getSignName() => string.Concat((object)CertUtils.GetCurrentMilli());

        public static double GetCurrentMilli()
        {
            try
            {
                return (double)(long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error get current milli: " + ex.Message);
                return 0.0;
            }
        }
    }
}
