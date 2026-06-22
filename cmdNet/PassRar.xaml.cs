using Microsoft.Win32;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace cmdNet
{
    public partial class PassRar : Window
    {
        private List<string> baseWords = new List<string>();
        private List<string> passwordsToTest = new List<string>();
        private CancellationTokenSource cts;
        private bool isTesting = false;

        public PassRar()
        {
            InitializeComponent();
        }

        private void BtnSelectRar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "RAR files (*.rar)|*.rar|All files (*.*)|*.*";
            if (ofd.ShowDialog() == true)
            {
                txtRarFile.Text = ofd.FileName;
            }
        }

        private void BtnSelectWords_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (ofd.ShowDialog() == true)
            {
                txtWordsFile.Text = ofd.FileName;
                LoadWordsFromFile(ofd.FileName);
            }
        }

        private void LoadWordsFromFile(string filePath)
        {
            try
            {
                baseWords = File.ReadAllLines(filePath)
                               .Where(line => !string.IsNullOrWhiteSpace(line))
                               .Select(line => line.Trim())
                               .Distinct()
                               .ToList();

                lstWords.Items.Clear();
                foreach (var word in baseWords)
                {
                    lstWords.Items.Add(word);
                }

                MessageBox.Show($"{baseWords.Count} کلمه بارگذاری شد.", "اطلاع", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdatePasswordsPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در خواندن فایل: {ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePasswordsPreview()
        {
            if (baseWords.Count == 0) return;

            GeneratePasswordsList();
            lstResults.Items.Clear();
            lstResults.Items.Add($"تعداد کل رمزهای تولید شده: {passwordsToTest.Count:N0}");
            lstResults.Items.Add("------- پیش‌نمایش 10 رمز اول -------");
            foreach (var pwd in passwordsToTest.Take(10))
            {
                lstResults.Items.Add(pwd);
            }
            if (passwordsToTest.Count > 10)
                lstResults.Items.Add($"... و {passwordsToTest.Count - 10} رمز دیگر");
        }

        private void GeneratePasswordsList()
        {
            passwordsToTest.Clear();

            // کلمات تکی
            if (chkSingleWords.IsChecked == true)
            {
                passwordsToTest.AddRange(baseWords);
            }

            // ترکیبات دوتایی
            if (chkDoubleCombinations.IsChecked == true)
            {
                for (int i = 0; i < baseWords.Count; i++)
                {
                    for (int j = 0; j < baseWords.Count; j++)
                    {
                        if (i != j || chkIncludeSelfCombination.IsChecked == true)
                        {
                            passwordsToTest.Add(baseWords[i] + baseWords[j]);
                        }
                    }
                }
            }

            // ترکیبات دوتایی معکوس
            if (chkDoubleCombinationsReverse.IsChecked == true)
            {
                for (int i = 0; i < baseWords.Count; i++)
                {
                    for (int j = 0; j < baseWords.Count; j++)
                    {
                        if (i != j || chkIncludeSelfCombination.IsChecked == true)
                        {
                            passwordsToTest.Add(baseWords[j] + baseWords[i]);
                        }
                    }
                }
            }

            // حذف تکراری‌ها
            passwordsToTest = passwordsToTest.Distinct().ToList();
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtRarFile.Text))
            {
                MessageBox.Show("لطفا فایل RAR را انتخاب کنید.", "خطا", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (baseWords.Count == 0)
            {
                MessageBox.Show("لطفا فایل کلمات را انتخاب کنید.", "خطا", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!File.Exists(txtRarFile.Text))
            {
                MessageBox.Show("فایل RAR وجود ندارد.", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            GeneratePasswordsList();

            if (passwordsToTest.Count == 0)
            {
                MessageBox.Show("هیچ رمزی برای تست وجود ندارد.", "خطا", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
            isTesting = true;
            cts = new CancellationTokenSource();

            lstResults.Items.Clear();
            lstResults.Items.Add($"شروع تست روی {passwordsToTest.Count:N0} رمز...");
            lstResults.Items.Add("========================================");

            progressBar.Maximum = passwordsToTest.Count;
            progressBar.Value = 0;

            try
            {
                var result = await TestPasswordsAsync(txtRarFile.Text, passwordsToTest, cts.Token);

                if (result.Found)
                {
                    lstResults.Items.Add("");
                    lstResults.Items.Add("✓✓✓ رمز پیدا شد! ✓✓✓");
                    lstResults.Items.Add($"رمز صحیح: {result.Password}");
                    lstResults.Items.Add($"تعداد تست‌های انجام شده: {result.AttemptsCount:N0}");
                    lblStatus.Text = $"پیدا شد: {result.Password}";
                    lblStatus.Foreground = System.Windows.Media.Brushes.Green;
                    MessageBox.Show($"رمز با موفقیت پیدا شد!\n\nرمز: {result.Password}", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    lstResults.Items.Add("");
                    lstResults.Items.Add("✗ رمز پیدا نشد.");
                    lstResults.Items.Add($"تعداد کل تست‌ها: {result.AttemptsCount:N0}");
                    lblStatus.Text = "رمز پیدا نشد.";
                    lblStatus.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (OperationCanceledException)
            {
                lstResults.Items.Add("");
                lstResults.Items.Add("تست توسط کاربر متوقف شد.");
                lblStatus.Text = "متوقف شده";
            }
            finally
            {
                btnStart.IsEnabled = true;
                btnStop.IsEnabled = false;
                isTesting = false;
                cts?.Dispose();
            }
        }

        private async Task<(bool Found, string Password, int AttemptsCount)> TestPasswordsAsync(
            string rarPath,
            List<string> passwords,
            CancellationToken token)
        {
            int attempts = 0;

            foreach (string pwd in passwords)
            {
                token.ThrowIfCancellationRequested();

                attempts++;

                await Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        progressBar.Value = attempts;
                        if (attempts % 100 == 0 || attempts <= 10)
                        {
                            lstResults.Items.Add($"تست {attempts:N0}: {pwd}");
                            lstResults.ScrollIntoView(lstResults.Items[^1]);
                        }
                    });
                });

                if (TestRarPassword(rarPath, pwd))
                {
                    return (true, pwd, attempts);
                }
            }

            return (false, null, attempts);
        }

        private bool TestRarPassword(string rarPath, string password)
        {
            try
            {
                using (var archive = RarArchive.OpenArchive(rarPath, new SharpCompress.Readers.ReaderOptions
                {
                    Password = password
                }))
                {
                    var firstEntry = archive.Entries.FirstOrDefault(e => !e.IsDirectory);

                    if (firstEntry != null)
                    {
                        using (var stream = firstEntry.OpenEntryStream())
                        {
                            byte[] buffer = new byte[1];
                            stream.Read(buffer, 0, buffer.Length);
                        }
                    }
                    return true;
                }
            }
            catch (SharpCompress.Common.InvalidFormatException)
            {
                return false;
            }
            catch (SharpCompress.Common.CryptographicException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (isTesting && cts != null)
            {
                cts.Cancel();
                btnStop.IsEnabled = false;
                lstResults.Items.Add("در حال توقف...");
            }
        }

        private void BtnSaveResult_Click(object sender, RoutedEventArgs e)
        {
            if (lstResults.Items.Count == 0)
            {
                MessageBox.Show("نتیجه‌ای برای ذخیره وجود ندارد.", "اطلاع", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text files (*.txt)|*.txt";
            sfd.FileName = $"RarTest_Result_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

            if (sfd.ShowDialog() == true)
            {
                using (StreamWriter sw = new StreamWriter(sfd.FileName))
                {
                    foreach (var item in lstResults.Items)
                    {
                        sw.WriteLine(item.ToString());
                    }
                }
                MessageBox.Show("نتیجه با موفقیت ذخیره شد.", "اطلاع", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (baseWords.Count > 0)
            {
                UpdatePasswordsPreview();
            }
        }
    }
}