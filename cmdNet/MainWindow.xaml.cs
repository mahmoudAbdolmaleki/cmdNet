using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using System.Windows.Forms;






namespace cmdNet
{
    public partial class MainWindow : Window
    {
        private const string LowerChars = "abcdefghijklmnopqrstuvwxyz";
        private const string UpperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string DigitChars = "0123456789";
        private const string SpecialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        private static readonly Random RandomGenerator = new Random();


        public MainWindow()
        {
            InitializeComponent();
            comQuery.SelectedIndex = 0;
            LengthLabel.Content = ((int)LengthSlider.Value).ToString();


        }
        List<string> allCommands;
        Dictionary<string, string> commandDescriptions = new Dictionary<string, string>
{
    // اطلاعات کلی سیستم و شبکه
    { "ipconfig", "نمایش تنظیمات IP، آدرس MAC، Gateway و DNS سیستم." },
    { "ipconfig /all", "نمایش کامل تنظیمات شبکه شامل DHCP، DNS، آدرس‌های MAC و بیشتر." },
    { "ipconfig /release", "آزاد کردن آدرس IP فعلی از DHCP." },
    { "ipconfig /renew", "درخواست مجدد آدرس IP از DHCP." },
    { "ipconfig /flushdns", "پاک کردن کش DNS برای رفع خطاهای باز نشدن وب‌سایت‌ها." },
    { "ipconfig /displaydns", "نمایش محتویات کش DNS (آی‌پی‌های ذخیره‌شده)." },

    // ابزارهای شبکه
    { "ping google.com", "تست اتصال به سرور گوگل و بررسی پاسخ‌دهی شبکه." },
    { "tracert 8.8.8.8", "نمایش مسیر بسته‌ها تا رسیدن به سرور 8.8.8.8 (DNS گوگل)." },
    { "nslookup bing.com", "بررسی آدرس IP دامنه bing.com از طریق DNS." },
    { "arp -a", "نمایش جدول ARP شامل آدرس‌های MAC و IP دستگاه‌های متصل." },
    { "route print", "نمایش جدول مسیریابی سیستم و مسیرهای شبکه." },

    // دستورات netsh (با ادغام توضیحات)
    { "netsh winsock reset", "ریست Winsock برای رفع اختلالات اتصال اینترنت و خطاهای نرم‌افزاری (نیاز به ریستارت)." },
    { "netsh int ip reset", "ریست کامل TCP/IP و استک (پشته) شبکه به حالت اولیه (نیاز به ریستارت)." },
    { "netsh int ipv4 reset", "بازنشانی اختصاصی پروتکل IPv4 (نیاز به ریستارت)." },
    { "netsh int ipv6 reset", "بازنشانی اختصاصی پروتکل IPv6 (نیاز به ریستارت)." },
    { "netsh advfirewall reset", "بازنشانی فایروال ویندوز به تنظیمات کارخانه." },
    { "netsh interface ip show config", "نمایش تنظیمات آی‌پی تمام کارت‌های شبکه." },
    { "netsh wlan show profile", "نمایش پروفایل‌های وای‌فای ذخیره‌شده در سیستم." },

    // netstat و اطلاعات اتصالات
    { "netstat", "نمایش اتصالات شبکه فعلی، ممکن است کند باشد." },
    { "netstat -an", "نمایش تمام اتصالات و پورت‌ها به‌صورت عددی بدون تبدیل DNS." },
    { "netstat -b", "نمایش برنامه‌هایی که از پورت‌ها استفاده می‌کنند (نیاز به دسترسی ادمین)." },
    { "netstat -o", "نمایش شناسه پردازش (PID) برای هر اتصال." },
    { "netstat -n", "نمایش آدرس‌ها بدون تبدیل به نام دامنه." },
    { "netstat -r", "نمایش جدول مسیریابی سیستم (Route Table)." },
    { "netstat -s", "نمایش آمار پروتکل‌های شبکه مانند TCP، UDP، ICMP." },
    { "netstat -e", "نمایش آمار Ethernet شامل تعداد بسته‌ها، خطاها و غیره." },
    { "netstat -an | find \"ESTABLISHED\"", "فیلتر اتصالات فعال که در وضعیت ESTABLISHED هستند." },
    { "netstat -an | find \"LISTEN\"", "نمایش پورت‌هایی که در حالت LISTEN هستند." },
    { "netstat -an | find \":80\"", "نمایش اتصالات روی پورت 80 (HTTP)." },
    { "netstat -an | find \":443\"", "نمایش اتصالات روی پورت 443 (HTTPS)." },

    // اطلاعات سیستمی
    { "getmac", "نمایش آدرس MAC کارت‌های شبکه سیستم." },
    { "hostname", "نمایش نام کامپیوتر فعلی." },
    { "systeminfo", "نمایش اطلاعات کامل سیستم شامل نسخه ویندوز، شبکه و سخت‌افزار." }
};

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
            allCommands = commandDescriptions.Keys.ToList();
            comQuery.ItemsSource = allCommands;
            comQuery.SelectedIndex = 0;
        }




        #region passoword creator

        private void LengthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LengthLabel != null)
                LengthLabel.Content = ((int)LengthSlider.Value).ToString();
        }

        // رویداد کلیک دکمه تولید رمز
        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            // بررسی انتخاب حداقل یک نوع کاراکتر
            if (!ChkLowerCase.IsChecked.Value && !ChkUpperCase.IsChecked.Value &&
                !ChkDigits.IsChecked.Value && !ChkSpecial.IsChecked.Value)
            {
                MessageBox.Show("حداقل یک نوع کاراکتر را انتخاب کنید.", "خطا",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int length = (int)LengthSlider.Value;
            string password = GeneratePassword(length);
            TxtPassword.Text = password;
        }

        // تولید رمز عبور تصادفی
        private string GeneratePassword(int length)
        {
            // ساخت رشته کاراکترهای مجاز بر اساس انتخاب کاربر
            string allowedChars = string.Empty;
            if (ChkLowerCase.IsChecked.Value)
                allowedChars += LowerChars;
            if (ChkUpperCase.IsChecked.Value)
                allowedChars += UpperChars;
            if (ChkDigits.IsChecked.Value)
                allowedChars += DigitChars;
            if (ChkSpecial.IsChecked.Value)
                allowedChars += SpecialChars;

            // تولید رمز تصادفی
            char[] password = new char[length];
            for (int i = 0; i < length; i++)
            {
                password[i] = allowedChars[RandomGenerator.Next(allowedChars.Length)];
            }

            // تضمین وجود حداقل یک کاراکتر از هر نوع انتخاب شده (اختیاری)
            EnsureMinimumRequirements(password);

            // درهم‌ریزی نهایی برای امنیت بیشتر
            Shuffle(password);

            return new string(password);
        }

        // تضمین حضور حداقل یک کاراکتر از هر مجموعه فعال
        private void EnsureMinimumRequirements(char[] password)
        {
            int idx = 0;
            if (ChkLowerCase.IsChecked.Value && password.Length > 0)
                password[idx++ % password.Length] = LowerChars[RandomGenerator.Next(LowerChars.Length)];
            if (ChkUpperCase.IsChecked.Value && password.Length > 0)
                password[idx++ % password.Length] = UpperChars[RandomGenerator.Next(UpperChars.Length)];
            if (ChkDigits.IsChecked.Value && password.Length > 0)
                password[idx++ % password.Length] = DigitChars[RandomGenerator.Next(DigitChars.Length)];
            if (ChkSpecial.IsChecked.Value && password.Length > 0)
                password[idx++ % password.Length] = SpecialChars[RandomGenerator.Next(SpecialChars.Length)];
        }

        // درهم‌ریزی آرایه (الگوریتم Fisher-Yates)
        private void Shuffle(char[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = RandomGenerator.Next(n--);
                (array[n], array[k]) = (array[k], array[n]);
            }
        }

        // کپی رمز به کلیپ‌بورد
        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TxtPassword.Text))
            {
                Clipboard.SetText(TxtPassword.Text);
                //MessageBox.Show("رمز عبور در حافظه کپی شد.", "موفقیت",
                //    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("ابتدا یک رمز عبور تولید کنید.", "هشدار",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        #endregion password creator


        private async void run_ClickAsync(object sender, RoutedEventArgs e)
        {
         
            string query = null;
            query = insertQuery.Text;
            if (string.IsNullOrWhiteSpace(query))
            {
                if (comQuery.SelectedItem is ComboBoxItem item)
                    query = item.Content.ToString();
                else if (comQuery.SelectedItem is string str)
                    query = str;

                // اگر هنوز null بود، مقدار پیش‌فرض بده
                if (string.IsNullOrWhiteSpace(query))
                    query = "ipconfig";
            }
           PrintResult(query,outputPanel);
            Clipboard.SetText(insertQuery.Text);
            insertQuery.Text = "";
        }
         private void Clear_Click(object sender, RoutedEventArgs e)
        {
            outputPanel.Children.Clear();
            insertQuery.Text = "";
        }

        private void insertQuery_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                run.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                e.Handled = true; // جلوگیری از ایجاد خط جدید یا رفتار پیش‌فرض
            }
        }
        private void comQuery_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                run.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                e.Handled = true; // جلوگیری از رفتار پیش‌فرض
            }
        }
        private void comQuery_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedCommand = comQuery.SelectedItem as string;

            if (!string.IsNullOrWhiteSpace(selectedCommand) && commandDescriptions.ContainsKey(selectedCommand))
                DescriptionBlock.Text = commandDescriptions[selectedCommand];
            else
                DescriptionBlock.Text = "توضیحی برای این دستور موجود نیست.";


        }


      
        private void ShowWifi_Click(object sender, RoutedEventArgs e)
        {
            string wifiScript = @"
$profiles = netsh wlan show profiles | Select-String 'All User Profile' | ForEach-Object {
    ($_ -split ':')[1].Trim()
}
foreach ($name in $profiles) {
    Write-Host ''
    Write-Host $name -ForegroundColor Cyan
    $details = netsh wlan show profile name=$name key=clear
    $password = ($details | Select-String 'Key Content').Line
    if ($password) {
        Write-Host $password -ForegroundColor Green
    } else {
        Write-Host 'رمز ذخیره نشده یا قابل نمایش نیست.' -ForegroundColor Yellow
    }
}
";
            PrintResult(wifiScript,outputPanel,"نمایش رمز وای فای موجود" ,true);
        }

        private void search_Click(object sender, RoutedEventArgs e)
        {
            string word = txtInsertKey.Text;
            string path = "";
            if(word=="")
            {
               System.Windows.Forms.MessageBox.Show($"یک کلمه برای جستجو وارد کنید");
            }
            else {
                using var dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    path = dialog.SelectedPath;

                }
                if (path != "" )
                {
                    bool searchPdf = CheckPdf.IsChecked == true;

                    bool searchWord = CheckDocx.IsChecked == true;

                    bool searchSubDir = CheckSubDir.IsChecked == true;

                   
                    PrintResultLive(path,word,searchSubDir,searchPdf,searchWord,500000,"");
                    //PrintResult(path, word, outputPanelSearch, true);
                }
            }
                
        }
    

        #region method

        private string ExecuteCmd(string query, bool usePowerShell = false)
        {

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    //FileName = "cmd.exe",
                    //Arguments = "/c " + query, // می‌تونی دستور دلخواه رو اینجا بذاری
                    //RedirectStandardOutput = true,
                    //UseShellExecute = false,
                    //CreateNoWindow = true
                    FileName = usePowerShell ? "powershell.exe" : "cmd.exe",
                    Arguments = usePowerShell ? $"-Command \"{query}\"" : "/c " + query,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true


                };
                Process process = new Process { StartInfo = psi };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();


                process.WaitForExit();
                if (string.IsNullOrWhiteSpace(output) && !string.IsNullOrWhiteSpace(error))
                    return "خطا: " + error;



                return output;
            }

            catch (Exception ex)
            {
                throw new InvalidOperationException("خطا در اجرای دستور CMD", ex);
            }


        }
        private string SearchInFile(string path, string word, bool subFolder)
        {

            try
            {
                //query = "Get-ChildItem '"+query+"' -Recurse -File -ErrorAction SilentlyContinue | ForEach-Object { Select-String -Path $_.FullName -Pattern 'details' -ErrorAction SilentlyContinue }";
                string recurse = "";
                if (subFolder) { recurse = "-Recurse"; }
                string query = @"Get-ChildItem '" + path + @"' " + recurse + @" -File -ErrorAction SilentlyContinue | 
Where-Object { $_.Extension -in '.php', '.html', '.htm', '.css',
                  '.js', '.ts', '.txt', '.json', '.xml',
                  '.md', '.csv', '.yml', '.yaml', '.ini',
                  '.log', '.config', '.bat', '.ps1', '.sh'
} | ForEach-Object {Select-String -Path $_.FullName  -Pattern '" + word + "' -CaseSensitive:$false -ErrorAction SilentlyContinue}";


                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -Command \"{query}\"",

                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Process process = new Process { StartInfo = psi };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();


                process.WaitForExit();
                if (string.IsNullOrWhiteSpace(output) && !string.IsNullOrWhiteSpace(error))
                    return "خطا: " + error;



                return output;
            }

            catch (Exception ex)
            {
                throw new InvalidOperationException("خطا در اجرای دستور CMD", ex);
            }


        }


        private Expander AddPendingOutput(string title, Color headerColor, StackPanel stackPanelPrint)
        {
            var headerText = new TextBox
            {
                Text = title,
                Foreground = new SolidColorBrush(headerColor),
                FontWeight = FontWeights.Bold,
                FontSize = 14
            };

            var deleteButton = new Button
            {

                Content = "❌",
                Style = (Style)FindResource("DeleteButtonStyle"),
                Margin = new Thickness(10, 0, 0, 0)

            };

            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            headerPanel.Children.Add(headerText);
            headerPanel.Children.Add(deleteButton);

            var contentText = new TextBox
            {
                Text = "⏳ در حال اجرا",
                Foreground = Brushes.Gray,
                FontSize = 13,
                Margin = new Thickness(5),
                TextWrapping = TextWrapping.Wrap,
                IsReadOnly = true,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Focusable = true, // ✅ این خط مهمه
                Cursor = Cursors.IBeam // برای نمایش نشانگر انتخاب متن



                //Text = "⏳ در حال اجرا",
                //Foreground = Brushes.Gray,
                //FontSize = 13,
                //Margin = new Thickness(5)

            };

            var expander = new Expander
            {
                
                Header = headerPanel,
                Content = contentText,
                IsExpanded = true,
                Margin = new Thickness(5),
                Background = Brushes.WhiteSmoke,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1)
               
            };

            deleteButton.Click += (s, e) => stackPanelPrint.Children.Remove(expander);

            stackPanelPrint.Children.Insert(0, expander);
            return expander;
        }


        private void PrintResultLive(string folderPath, string keyword, bool includeSubfolders,
                             bool searchDocx, bool searchPdf, long maxSizeBytes, string title = "")
        {
            Expander expander1 = new Expander();
            TextBox textBox = new TextBox();
            int index = 1;
            ProgressBar.Value = 0;
            if (string.IsNullOrWhiteSpace(title))
                title = keyword;

            expander1 =  AddPendingOutput(title,Colors.Red, outputPanelSearch);
            
            var searcher = new FileSearcher();

            searcher.FileMatched += file =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    textBox.Text += $"{index}={file}\n";
                    //textBox.Text += $"{index}=✅{file}\n";
                    expander1.Content = textBox;
                    textBox.IsReadOnly = true;
                    textBox.TextWrapping = TextWrapping.Wrap;
                    textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    textBox.AcceptsReturn = true;


                    index++;
                  
                });
            };

            searcher.FileError += error =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    textBox.Text += error;
                    expander1.Content = textBox;
                    
                });
            };

            searcher.ProgressChanged += percent =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    ProgressBar.Value = percent;
                    //ProgressText.Text = $"{percent:F0}%";
                });
            };
            searcher.SearchCompleted += () =>
            {
                Dispatcher.Invoke(() =>
                {
                    ProgressText.Text = " ✅ جستجو کامل شد";
                    if (textBox.Text == "")
                        expander1.Content = "";
                   
                });
            };

            Task.Run(async () =>
            {
                searcher.Search(folderPath, keyword, includeSubfolders, searchDocx, searchPdf, maxSizeBytes);
            });
        }


        private async void PrintResult(string query, StackPanel stackPanelPrint, string title = "", bool powerShell = false)
        {
            if (title == "")
                title = query;
            var expander = AddPendingOutput($"✅ نتیجه: {title}", Colors.DarkBlue, stackPanelPrint);

            try
            {
                string output = await Task.Run(() => ExecuteCmd(query, powerShell));


                expander.Content = new TextBlock
                {
                    Text = output,
                    Foreground = Brushes.Black,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 14,
                    Margin = new Thickness(5)
                };
            }
            catch (Exception ex)
            {
                expander.Content = new TextBlock
                {
                    Text = $"❌ خطا در اجرا:\n{ex.Message}",
                    Foreground = Brushes.Red,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 14,
                    Margin = new Thickness(5)
                };
            }


        }
        private async void PrintResult(string path, string word, StackPanel stackPanelPrint, bool subFolder, string title = "")
        {
            if (title == "")
                title = word;
            var expander = AddPendingOutput($"✅ نتیجه: {title}", Colors.DarkBlue, stackPanelPrint);

            try
            {
                string output = await Task.Run(() => SearchInFile(path, word, subFolder));


                expander.Content = new TextBlock
                {
                    Text = output,
                    Foreground = Brushes.Black,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 14,
                    Margin = new Thickness(5)
                };
            }
            catch (Exception ex)
            {
                expander.Content = new TextBlock
                {
                    Text = $"❌ خطا در اجرا:\n{ex.Message}",
                    Foreground = Brushes.Red,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 14,
                    Margin = new Thickness(5)
                };
            }


        }



        #endregion

        private void CreateUserActiveDriectory_Click(object sender, RoutedEventArgs e)
        {
            InsertUser objInsertUser= new InsertUser();
            objInsertUser.Show();
        }

        private void PasswordRar_Click(object sender, RoutedEventArgs e)
        {
            PassRar objPassRar= new PassRar();
            objPassRar.Show();
        }
    }
}
