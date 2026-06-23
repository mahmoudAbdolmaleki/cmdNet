using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace cmdNet
{
    public class LanguageManager : INotifyPropertyChanged
    {
        private static LanguageManager _instance;
        public static LanguageManager Instance => _instance ??= new LanguageManager();

        private CultureInfo _currentCulture;
        private readonly ResourceManager _resourceManager;

        public LanguageManager()
        {
            // نام کامل: فضای نام پروژه + نام فایل Resource (بدون پسوند)
            _resourceManager = new ResourceManager("cmdNet.AppResources", Assembly.GetExecutingAssembly());
            _currentCulture = new CultureInfo("fa-IR"); // زبان پیش‌فرض فارسی
            Thread.CurrentThread.CurrentUICulture = _currentCulture;
        }

        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                if (_currentCulture != value)
                {
                    _currentCulture = value;
                    Thread.CurrentThread.CurrentUICulture = value;
                    OnPropertyChanged(nameof(CurrentCulture));

                    // تغییر جهت صفحه (راست‌چین/چپ‌چین)
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Application.Current.Resources["FlowDirection"] =
                            value.Name.StartsWith("fa") ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                    });
                    OnPropertyChanged(nameof(CmdTabHeader));
                    OnPropertyChanged(nameof(BtnUserActiveDirectory));
                    OnPropertyChanged(nameof(BtnPasswordRar));
                    // به‌روزرسانی تمام پراپرتی‌های متنی
                    OnPropertyChanged(nameof(AppTitle));
                    OnPropertyChanged(nameof(BtnRun));
                    OnPropertyChanged(nameof(BtnPersian));
                    OnPropertyChanged(nameof(BtnEnglish));
                    OnPropertyChanged(nameof(BtnClear));
                    OnPropertyChanged(nameof(BtnShowWifi));
                    OnPropertyChanged(nameof(LblInsertCommand));
                    OnPropertyChanged(nameof(DescriptionPrefix));
                    OnPropertyChanged(nameof(SearchTabHeader));
                    OnPropertyChanged(nameof(PasswordTabHeader));
                    OnPropertyChanged(nameof(OtherTabHeader));
                    OnPropertyChanged(nameof(BtnSearch));
                    OnPropertyChanged(nameof(BtnClearSearch));
                    OnPropertyChanged(nameof(PasswordLengthLabel));
                    OnPropertyChanged(nameof(PasswordCharTypesLabel));
                    OnPropertyChanged(nameof(PasswordLowerCaseLabel));
                    OnPropertyChanged(nameof(PasswordUpperCaseLabel));
                    OnPropertyChanged(nameof(PasswordDigitsLabel));
                    OnPropertyChanged(nameof(PasswordSpecialLabel));
                    OnPropertyChanged(nameof(PasswordGeneratedLabel));
                    OnPropertyChanged(nameof(BtnGenerate));
                    OnPropertyChanged(nameof(BtnCopy));
                    OnPropertyChanged(nameof(SearchInsertWordLabel));
                    OnPropertyChanged(nameof(SearchSubDirLabel));
                    OnPropertyChanged(nameof(SearchDocxLabel));
                    OnPropertyChanged(nameof(SearchPdfLabel));
                    OnPropertyChanged(nameof(SearchTxtLabel));
                }
            }
        }

        // متد دریافت متن از Resource
        public string GetString(string key)
        {
            return _resourceManager.GetString(key, _currentCulture) ?? key;
        }

        // پراپرتی‌های متنی برای استفاده در XAML (Binding)
        public string CmdTabHeader => GetString("Btn_CmdTabHeader");
        public string BtnUserActiveDirectory => GetString("Btn_UserActiveDirectory");
        public string BtnPasswordRar => GetString("Btn_PasswordRar");
        public string AppTitle => GetString("AppTitle");
        public string BtnRun => GetString("Btn_Run");
        public string BtnPersian => GetString("Btn_Persian");
        public string BtnEnglish => GetString("Btn_English");
        public string BtnClear => GetString("Btn_Clear");
        public string BtnShowWifi => GetString("Btn_ShowWifi");
        public string LblInsertCommand => GetString("Lbl_InsertCommand");
        public string DescriptionPrefix => GetString("Description_Prefix");
        public string SearchTabHeader => GetString("Search_TabHeader");
        public string PasswordTabHeader => GetString("Password_TabHeader");
        public string OtherTabHeader => GetString("Other_TabHeader");
        public string BtnSearch => GetString("Btn_Search");
        public string BtnClearSearch => GetString("Btn_ClearSearch");
        public string PasswordLengthLabel => GetString("Password_LengthLabel");
        public string PasswordCharTypesLabel => GetString("Password_CharTypesLabel");
        public string PasswordLowerCaseLabel => GetString("Password_LowerCaseLabel");
        public string PasswordUpperCaseLabel => GetString("Password_UpperCaseLabel");
        public string PasswordDigitsLabel => GetString("Password_DigitsLabel");
        public string PasswordSpecialLabel => GetString("Password_SpecialLabel");
        public string PasswordGeneratedLabel => GetString("Password_GeneratedLabel");
        public string BtnGenerate => GetString("Btn_Generate");
        public string BtnCopy => GetString("Btn_Copy");
        public string SearchInsertWordLabel => GetString("Search_InsertWordLabel");
        public string SearchSubDirLabel => GetString("Search_SubDirLabel");
        public string SearchDocxLabel => GetString("Search_DocxLabel");
        public string SearchPdfLabel => GetString("Search_PdfLabel");
        public string SearchTxtLabel => GetString("Search_TxtLabel");

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}