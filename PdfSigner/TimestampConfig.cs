namespace PdfSigner
{
    public class TimestampConfig
    {
        private bool useTimestamp = false;
        private string tsa_url = "http://tsa.viettel-ca.vn/";
        private string tsa_acc;
        private string tsa_pass;

        public bool UseTimestamp
        {
            get => this.useTimestamp;
            set => this.useTimestamp = value;
        }

        public string TsaUrl
        {
            get => this.tsa_url;
            set => this.tsa_url = value;
        }

        public string TsaAcc
        {
            get => this.tsa_acc;
            set => this.tsa_acc = value;
        }

        public string TsaPass
        {
            get => this.tsa_pass;
            set => this.tsa_pass = value;
        }
    }
}
