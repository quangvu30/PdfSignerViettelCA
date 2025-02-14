using iTextSharp.text;

namespace PdfSigner
{
    public class DisplayConfig
    {
        public static int SIGN_TYPE_CREATE_NEW_EMPTY_SIGNATURE_FIELD = 1;
        public static int SIGN_TYPE_EXISTED_EMPTY_SIGNATURE_FIELD = 2;
        public static int DISPLAY_RECTANGLE_TEXT = 1;
        public static int DISPLAY_IMAGE_STAMP = 2;
        public static int DISPLAY_TABLE = 3;
        public static int DISPLAY_IMAGE_WITH_TEXT = 4;
        public static int DISPLAY_IMAGE_WITH_TEXT_VALID = 5;
        public static string SIGN_TEXT_FORMAT_1 = "Người Ký: {0}";
        public static string SIGN_TEXT_FORMAT_2 = "Người Ký: {0} \r\nNgày Ký: {1}";
        public static string SIGN_TEXT_FORMAT_3 = "Người Ký: {0} \r\nNgày Ký: {1} \r\nLý Do: {2}";
        public static string SIGN_TEXT_FORMAT_4 = "Người Ký: {0} \r\nNgày Ký: {1} \r\nLý Do: {2} \r\nĐịa Điểm: {3}";
        public static string SIGN_TEXT_FORMAT_5 = "Digital signed by: {0}";
        public static string SIGN_TEXT_FORMAT_6 = "Digital signed by: {0} \r\nDate: {1}";
        public static string SIGN_TEXT_FORMAT_7 = "Digital signed by: {0} \r\nDate: {1} \r\nReason: {2}";
        public static string SIGN_TEXT_FORMAT_8 = "Digital signed by: {0} \r\nDate: {1} \r\nReason: {2} \r\nLocation: {3}";
        public static string DATE_FORMAT_1 = "{0:dd/MM/yyyy HH:mm:ss}";
        public static string DATE_FORMAT_2 = "{0:yyyy/MM/dd HH:mm:ss}";
        public static string DATE_FORMAT_3 = "{0:HH:mm:ss dd/MM/yyyy}";
        public static string DATE_FORMAT_4 = "{0:HH:mm:ss yyyy/MM/dd}";
        public static string FONT_PATH_TIMESNEWROMAN_WINDOWS = "C:/windows/fonts/times.ttf";
        public static string FONT_PATH_TAHOMA_WINDOWS = "C:/windows/fonts/tahoma.ttf";
        public static string FONT_PATH_ARIAL_WINDOWS = "C:/windows/fonts/arial.ttf";
        private bool isDisplaySignature = true;
        private int typeDisplay = DisplayConfig.DISPLAY_RECTANGLE_TEXT;
        private int signType = DisplayConfig.SIGN_TYPE_CREATE_NEW_EMPTY_SIGNATURE_FIELD;
        private int numberPageSign = 1;
        private float coorXRectangle = 10f;
        private float coorYRectangle = 10f;
        private float widthRectangle = 200f;
        private float heightRectangle = 50f;
        private string formatRectangleText = DisplayConfig.SIGN_TEXT_FORMAT_2;
        private string contact = "";
        private string reason = "";
        private string location = "";
        private string displayText = "";
        private DateTime signDate;
        private string dateFormatstring = DisplayConfig.DATE_FORMAT_1;
        private string pathImage;
        private Rectangle pageSize = iTextSharp.text.PageSize.A4;
        private int totalPageSign = 1;
        private int maxPageSign = 10;
        private string[] titles;
        private float[] widthsPercen;
        private float heightTitle = 30f;
        private BaseColor backgroundColorTitle = new BaseColor(240, 240, 240);
        private string fontPath = DisplayConfig.FONT_PATH_TIMESNEWROMAN_WINDOWS;
        private int sizeFont = 11;
        private int[] alignmentArray;
        private string[] textArray;
        private float marginTop = 80f;
        private float marginBottom = 80f;
        private float marginRight = 60f;
        private bool isDisplayTitlePageSign = true;
        private string titlePageSign = "TRANG KÝ";
        private float heightRowTitlePageSign = 40f;
        private int fontSizeTitlePageSign = 15;

        public static DisplayConfig generateDisplayConfigRectangleTextDefault(
          string contact,
          string reason,
          string location)
        {
            return new DisplayConfig()
            {
                IsDisplaySignature = true,
                TypeDisplay = DisplayConfig.DISPLAY_RECTANGLE_TEXT,
                Contact = contact,
                Reason = reason,
                Location = location,
                SignDate = DateTime.Now
            };
        }

        public static DisplayConfig generateDisplayConfigRectangleTextDefault_ExistSignatureFieldDefault(
          string contact,
          string reason,
          string location)
        {
            DisplayConfig rectangleTextDefault = DisplayConfig.generateDisplayConfigRectangleTextDefault(contact, reason, location);
            rectangleTextDefault.SignType = DisplayConfig.SIGN_TYPE_EXISTED_EMPTY_SIGNATURE_FIELD;
            return rectangleTextDefault;
        }

        public static DisplayConfig generateDisplayConfigRectangleText(
          int numberPageSign,
          float coorX,
          float coorY,
          float width,
          float height,
          string displayText,
          string formatRectangleText,
          string contact,
          string reason,
          string location,
          string dateFormatstring)
        {
            return new DisplayConfig()
            {
                IsDisplaySignature = true,
                TypeDisplay = DisplayConfig.DISPLAY_RECTANGLE_TEXT,
                NumberPageSign = numberPageSign,
                CoorXRectangle = coorX,
                CoorYRectangle = coorY,
                WidthRectangle = width,
                HeightRectangle = height,
                DisplayText = displayText,
                FormatRectangleText = formatRectangleText,
                Contact = contact,
                Reason = reason,
                Location = location,
                DateFormatstring = dateFormatstring,
                SignDate = DateTime.Now
            };
        }

        public static DisplayConfig generateDisplayConfigRectangleText_ExistedSignatureField(
          int numberPageSign,
          float coorX,
          float coorY,
          float width,
          float height,
          string displayText,
          string formatRectangleText,
          string contact,
          string reason,
          string location,
          string dateFormatString)
        {
            DisplayConfig configRectangleText = DisplayConfig.generateDisplayConfigRectangleText(numberPageSign, coorX, coorY, width, height, displayText, formatRectangleText, contact, reason, location, dateFormatString);
            configRectangleText.SignType = DisplayConfig.SIGN_TYPE_EXISTED_EMPTY_SIGNATURE_FIELD;
            return configRectangleText;
        }

        public static DisplayConfig generateDisplayConfigImageDefault(
          int numberPageSign,
          float coorX,
          float coorY,
          float width,
          float height,
          string pathImage)
        {
            return new DisplayConfig()
            {
                IsDisplaySignature = true,
                TypeDisplay = DisplayConfig.DISPLAY_IMAGE_STAMP,
                NumberPageSign = numberPageSign,
                CoorXRectangle = coorX,
                CoorYRectangle = coorY,
                WidthRectangle = width,
                HeightRectangle = height,
                PathImage = pathImage,
                SignDate = DateTime.Now
            };
        }

        public static DisplayConfig generateDisplayConfigImageDefault_ExistedSignatureField(
          int numberPageSign,
          float coorX,
          float coorY,
          float width,
          float height,
          string pathImage)
        {
            DisplayConfig configImageDefault = DisplayConfig.generateDisplayConfigImageDefault(numberPageSign, coorX, coorY, width, height, pathImage);
            configImageDefault.SignType = DisplayConfig.SIGN_TYPE_EXISTED_EMPTY_SIGNATURE_FIELD;
            return configImageDefault;
        }

        public static DisplayConfig generateDisplayConfigImage(
          int numberPageSign,
          float coorX,
          float coorY,
          float width,
          float height,
          string contact,
          string reason,
          string location,
          string pathImage)
        {
            return new DisplayConfig()
            {
                IsDisplaySignature = true,
                TypeDisplay = DisplayConfig.DISPLAY_IMAGE_STAMP,
                NumberPageSign = numberPageSign,
                CoorXRectangle = coorX,
                CoorYRectangle = coorY,
                WidthRectangle = width,
                HeightRectangle = height,
                Contact = contact,
                Reason = reason,
                Location = location,
                PathImage = pathImage,
                SignDate = DateTime.Now
            };
        }

        public static DisplayConfig generateDisplayConfigImage_ExistedSignatureField(
          int numberPageSign,
          float coorX,
          float coorY,
          float width,
          float height,
          string contact,
          string reason,
          string location,
          string pathImage)
        {
            DisplayConfig displayConfigImage = DisplayConfig.generateDisplayConfigImage(numberPageSign, coorX, coorY, width, height, contact, reason, location, pathImage);
            displayConfigImage.SignType = DisplayConfig.SIGN_TYPE_EXISTED_EMPTY_SIGNATURE_FIELD;
            return displayConfigImage;
        }

        public static DisplayConfig generateDisplayConfigTableDefault(
          int totalPageSign,
          string[] textArray)
        {
            DisplayConfig configTableDefault = new DisplayConfig();
            configTableDefault.IsDisplaySignature = true;
            configTableDefault.TypeDisplay = DisplayConfig.DISPLAY_TABLE;
            configTableDefault.TotalPageSign = totalPageSign;
            configTableDefault.titles = new string[5];
            configTableDefault.titles[0] = "STT";
            configTableDefault.titles[1] = "Người Ký";
            configTableDefault.titles[2] = "Đơn vị";
            configTableDefault.titles[3] = "Thời gian ký";
            configTableDefault.titles[4] = "Ý kiến";
            configTableDefault.widthsPercen = new float[5];
            configTableDefault.widthsPercen[0] = 0.06f;
            configTableDefault.widthsPercen[1] = 0.18f;
            configTableDefault.widthsPercen[2] = 0.2f;
            configTableDefault.widthsPercen[3] = 0.14f;
            configTableDefault.widthsPercen[4] = 0.42f;
            configTableDefault.alignmentArray = new int[5];
            configTableDefault.alignmentArray[0] = 1;
            configTableDefault.alignmentArray[1] = 0;
            configTableDefault.alignmentArray[2] = 3;
            configTableDefault.alignmentArray[3] = 0;
            configTableDefault.alignmentArray[4] = 3;
            configTableDefault.TextArray = textArray;
            configTableDefault.SignDate = DateTime.Now;
            return configTableDefault;
        }

        public static DisplayConfig generateDisplayConfigTable(
          int totalPageSign,
          string[] titles,
          float[] widthsPercen,
          int[] alignmentArray,
          string[] textArray)
        {
            return new DisplayConfig()
            {
                IsDisplaySignature = true,
                TypeDisplay = DisplayConfig.DISPLAY_TABLE,
                TotalPageSign = totalPageSign,
                Titles = titles,
                WidthsPercen = widthsPercen,
                AlignmentArray = alignmentArray,
                TextArray = textArray,
                SignDate = DateTime.Now
            };
        }

        public static DisplayConfig generateDisplayConfigImageText(
          int numberPageSign,
          float coorX,
          float coorY,
          float width,
          float height,
          string displayText,
          string formatRectangleText,
          string contact,
          string reason,
          string location,
          string dateFormatString,
          string pathImage)
        {
            return new DisplayConfig()
            {
                IsDisplaySignature = true,
                TypeDisplay = DisplayConfig.DISPLAY_IMAGE_WITH_TEXT,
                NumberPageSign = numberPageSign,
                CoorXRectangle = coorX,
                CoorYRectangle = coorY,
                WidthRectangle = width,
                HeightRectangle = height,
                DisplayText = displayText,
                FormatRectangleText = formatRectangleText,
                Contact = contact,
                Reason = reason,
                Location = location,
                DateFormatstring = dateFormatString,
                SignDate = DateTime.Now,
                PathImage = pathImage
            };
        }

        public static DisplayConfig generateDisplayConfigImageText_ExistedSignatureField(
          string displayText,
          string formatRectangleText,
          string contact,
          string reason,
          string location,
          string dateFormatString,
          string pathImage)
        {
            return new DisplayConfig()
            {
                IsDisplaySignature = true,
                TypeDisplay = DisplayConfig.DISPLAY_IMAGE_WITH_TEXT,
                DisplayText = displayText,
                FormatRectangleText = formatRectangleText,
                Contact = contact,
                Reason = reason,
                Location = location,
                DateFormatstring = dateFormatString,
                SignDate = DateTime.Now,
                SignType = DisplayConfig.SIGN_TYPE_EXISTED_EMPTY_SIGNATURE_FIELD
            };
        }

        public string PathImage
        {
            get => this.pathImage;
            set => this.pathImage = value;
        }

        public bool IsDisplaySignature
        {
            get => this.isDisplaySignature;
            set => this.isDisplaySignature = value;
        }

        public int TypeDisplay
        {
            get => this.typeDisplay;
            set => this.typeDisplay = value;
        }

        public int SignType
        {
            get => this.signType;
            set => this.signType = value;
        }

        public int NumberPageSign
        {
            get => this.numberPageSign;
            set => this.numberPageSign = value;
        }

        public float CoorXRectangle
        {
            get => this.coorXRectangle;
            set => this.coorXRectangle = value;
        }

        public float CoorYRectangle
        {
            get => this.coorYRectangle;
            set => this.coorYRectangle = value;
        }

        public float WidthRectangle
        {
            get => this.widthRectangle;
            set => this.widthRectangle = value;
        }

        public float HeightRectangle
        {
            get => this.heightRectangle;
            set => this.heightRectangle = value;
        }

        public string FormatRectangleText
        {
            get => this.formatRectangleText;
            set => this.formatRectangleText = value;
        }

        public string Contact
        {
            get => this.contact;
            set => this.contact = value;
        }

        public string Reason
        {
            get => this.reason;
            set => this.reason = value;
        }

        public string Location
        {
            get => this.location;
            set => this.location = value;
        }

        public string DisplayText
        {
            get => this.displayText;
            set => this.displayText = value;
        }

        public DateTime SignDate
        {
            get => this.signDate;
            set => this.signDate = value;
        }

        public string DateFormatstring
        {
            get => this.dateFormatstring;
            set => this.dateFormatstring = value;
        }

        public Rectangle PageSize
        {
            get => this.pageSize;
            set => this.pageSize = value;
        }

        public int TotalPageSign
        {
            get => this.totalPageSign;
            set => this.totalPageSign = value;
        }

        public int MaxPageSign
        {
            get => this.maxPageSign;
            set => this.maxPageSign = value;
        }

        public string[] Titles
        {
            get => this.titles;
            set => this.titles = value;
        }

        public float[] WidthsPercen
        {
            get => this.widthsPercen;
            set => this.widthsPercen = value;
        }

        public float HeightTitle
        {
            get => this.heightTitle;
            set => this.heightTitle = value;
        }

        public BaseColor BackgroundColorTitle
        {
            get => this.backgroundColorTitle;
            set => this.backgroundColorTitle = value;
        }

        public string FontPath
        {
            get => this.fontPath;
            set => this.fontPath = value;
        }

        public int SizeFont
        {
            get => this.sizeFont;
            set => this.sizeFont = value;
        }

        public int[] AlignmentArray
        {
            get => this.alignmentArray;
            set => this.alignmentArray = value;
        }

        public string[] TextArray
        {
            get => this.textArray;
            set => this.textArray = value;
        }

        public float MarginTop
        {
            get => this.marginTop;
            set => this.marginTop = value;
        }

        public float MarginBottom
        {
            get => this.marginBottom;
            set => this.marginBottom = value;
        }

        public float MarginRight
        {
            get => this.marginRight;
            set => this.marginRight = value;
        }

        public bool IsDisplayTitlePageSign
        {
            get => this.isDisplayTitlePageSign;
            set => this.isDisplayTitlePageSign = value;
        }

        public string TitlePageSign
        {
            get => this.titlePageSign;
            set => this.titlePageSign = value;
        }

        public float HeightRowTitlePageSign
        {
            get => this.heightRowTitlePageSign;
            set => this.heightRowTitlePageSign = value;
        }

        public int FontSizeTitlePageSign
        {
            get => this.fontSizeTitlePageSign;
            set => this.fontSizeTitlePageSign = value;
        }
    }
}
