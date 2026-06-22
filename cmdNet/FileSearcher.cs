using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using UglyToad.PdfPig;

namespace cmdNet
{
    public class FileSearcher
    {
        public event Action<string> FileMatched;
        public event Action<string> FileError;
        public event Action<double> ProgressChanged;
        public event Action SearchCompleted;


        private string ReadFileContent(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLower();

            if (ext == ".docx")
            {
                using var doc = WordprocessingDocument.Open(filePath, false);
                return doc.MainDocumentPart.Document.Body.InnerText;
            }
            else if (ext == ".pdf")
            {
                using var pdf = PdfDocument.Open(filePath);
                var sb = new StringBuilder();
                foreach (var page in pdf.GetPages())
                    sb.AppendLine(page.Text);
                return sb.ToString();
            }
            else
            {
                return File.ReadAllText(filePath);
            }
        }
        public void Search(string folderPath, string keyword, bool includeSubfolders,
                     bool searchDocx, bool searchPdf, long maxSizeBytes)
        {
            var alwaysExtensions = new[]
            {
        ".txt", ".html", ".htm", ".css", ".js", ".ts", ".json", ".xml",
        ".md", ".csv", ".yml", ".yaml", ".ini", ".log", ".config", ".bat", ".ps1", ".sh"
    };

            var conditionalExtensions = new List<string>();
            if (searchDocx) conditionalExtensions.Add(".docx");
            if (searchPdf) conditionalExtensions.Add(".pdf");

            var allExtensions = alwaysExtensions.Concat(conditionalExtensions).ToList();

            List<string> files = new List<string>();
            var pending = new Stack<string>();
            pending.Push(folderPath);

            while (pending.Count > 0)
            {
                var current = pending.Pop();

                try
                {
                    files.AddRange(Directory.GetFiles(current));

                    if (includeSubfolders)
                    {
                        foreach (var dir in Directory.GetDirectories(current))
                        {
                            try
                            {
                                Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly); // تست دسترسی
                                pending.Push(dir);
                            }
                            catch
                            {
                                //if (!dir.EndsWith("System Volume Information", StringComparison.OrdinalIgnoreCase))
                                    //FileError?.Invoke($"⛔ دسترسی به پوشه ممنوع است: {dir}");


                                //FileError?.Invoke($"⛔ دسترسی به پوشه ممنوع است: {dir}");
                            }
                        }
                    }
                }
                catch
                {
                    FileError?.Invoke($"⛔ دسترسی به پوشه ممنوع است: {current}");
                }
            }

            files = files
                .Where(f => allExtensions.Contains(Path.GetExtension(f).ToLower()))
                .Where(f => maxSizeBytes == 0 || new FileInfo(f).Length <= maxSizeBytes)
                .ToList();
            if (files.Count == 0)
            {
                //ProgressChanged?.Invoke(100);
                SearchCompleted?.Invoke();
                return;
            }


            int totalFiles = files.Count;
            int processedCount = 0;

            Parallel.ForEach(files, file =>
            {
                try
                {
                    string ext = Path.GetExtension(file).ToLower();
                    string content = "";

                    if (alwaysExtensions.Contains(ext))
                    {
                        content = File.ReadAllText(file);
                    }
                    else if (ext == ".docx")
                    {
                        using var doc = WordprocessingDocument.Open(file, false);
                        content = doc.MainDocumentPart.Document.Body.InnerText;
                    }
                    else if (ext == ".pdf")
                    {
                        using var pdf = PdfDocument.Open(file);
                        var sb = new StringBuilder();
                        foreach (var page in pdf.GetPages())
                            sb.AppendLine(page.Text);
                        content = sb.ToString();
                    }

                    if (content.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        FileMatched?.Invoke(file);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    FileError?.Invoke($"⛔ دسترسی غیرمجاز به فایل: {file}");
                }
                catch (Exception ex)
                {
                    FileError?.Invoke($"❌ خطا در {file}: {ex.Message}");
                }
                int current = Interlocked.Increment(ref processedCount);
                double percent = (double)current / totalFiles * 100;
                ProgressChanged?.Invoke(percent);

                if (current == totalFiles)
                {
                    SearchCompleted?.Invoke();
                }

                
            });
        }

        //    public async Task Search(string folderPath, string keyword, bool includeSubfolders,
        //                       bool searchDocx, bool searchPdf, long maxSizeBytes)
        //    {
        //        var alwaysExtensions = new[]
        //        {
        //        ".txt", ".html", ".htm", ".css", ".js", ".ts", ".json", ".xml",
        //        ".md", ".csv", ".yml", ".yaml", ".ini", ".log", ".config", ".bat", ".ps1", ".sh"
        //    };

        //        var conditionalExtensions = new List<string>();
        //        if (searchDocx) conditionalExtensions.Add(".docx");
        //        if (searchPdf) conditionalExtensions.Add(".pdf");

        //        var allExtensions = alwaysExtensions.Concat(conditionalExtensions).ToList();

        //        var files = await GetAllFilesSafeAsync(folderPath, includeSubfolders, blocked =>
        //        {
        //            FileError?.Invoke($"⛔ دسترسی به پوشه ممنوع است: {blocked}");
        //        });

        //        files = files
        //.Where(f => allExtensions.Contains(Path.GetExtension(f).ToLower()))
        //.Where(f => maxSizeBytes == 0 || new FileInfo(f).Length <= maxSizeBytes)
        //.ToList();

        //        //var files = Directory.GetFiles(folderPath, "*.*",
        //        //    includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
        //        //    .Where(f => allExtensions.Contains(Path.GetExtension(f).ToLower()))
        //        //    .Where(f => maxSizeBytes == 0 || new FileInfo(f).Length <= maxSizeBytes)
        //        //    .ToList();

        //        int totalFiles = files.Count;
        //        int processedCount = 0;

        //        Parallel.ForEach(files, file =>
        //        {
        //            string ext = Path.GetExtension(file).ToLower();
        //            string content = "";

        //            try
        //            {
        //                if (alwaysExtensions.Contains(ext))
        //                {
        //                    content = File.ReadAllText(file);
        //                }
        //                else if (ext == ".docx")
        //                {
        //                    using (var doc = WordprocessingDocument.Open(file, false))
        //                    {
        //                        content = doc.MainDocumentPart.Document.Body.InnerText;
        //                    }
        //                }
        //                else if (ext == ".pdf")
        //                {
        //                    using (var pdf = PdfDocument.Open(file))
        //                    {
        //                        var sb = new StringBuilder();
        //                        foreach (var page in pdf.GetPages())
        //                        {
        //                            sb.AppendLine(page.Text);
        //                        }
        //                        content = sb.ToString();
        //                    }
        //                }

        //                if (content.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        //                {
        //                    FileMatched?.Invoke(file);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                FileError?.Invoke($"❌ خطا در {file}: {ex.Message}");
        //            }

        //            int current = Interlocked.Increment(ref processedCount);
        //            double percent = (double)current / totalFiles * 100;
        //            ProgressChanged?.Invoke(percent);
        //        });
        //    }







        //public async Task TraverseFilesAsync(string rootPath, bool includeSubfolders,
        //                             Func<string, Task> onFileFound,
        //                             Func<string, Task> onFolderBlocked)
        //{
        //    var pending = new Stack<string>();
        //    pending.Push(rootPath);

        //    await Task.Run(async () =>
        //    {
        //        while (pending.Count > 0)
        //        {
        //            var current = pending.Pop();

        //            try
        //            {
        //                foreach (var file in Directory.GetFiles(current))
        //                {

        //                   await onFileFound(file); // ← نمایش زنده
        //                }

        //                if (includeSubfolders)
        //                {
        //                    foreach (var dir in Directory.GetDirectories(current))
        //                    {
        //                        if (CanAccessDirectory(dir))
        //                            pending.Push(dir);
        //                        else
        //                            await onFolderBlocked(dir); // ← گزارش پوشه ممنوع
        //                    }
        //                }
        //            }
        //            catch
        //            {
        //                await onFolderBlocked(current);
        //            }
        //        }
        //    });
        //}
        public async Task<List<string>> GetAllFilesSafeAsync(string rootPath, bool includeSubfolders, Action<string> reportBlockedFolder)
        {
            var result = new List<string>();
            var pending = new Stack<string>();
            pending.Push(rootPath);

            await Task.Run(() =>
            {
                while (pending.Count > 0)
                {
                    var current = pending.Pop();

                    try
                    {
                        var files = Directory.GetFiles(current);
                        result.AddRange(files);

                        if (includeSubfolders)
                        {
                            //foreach (var dir in Directory.GetDirectories(current))
                            //{
                            //    if (CanAccessDirectory(dir))
                            //    {
                            //        pending.Push(dir);
                            //    }
                            //    else
                            //    {
                            //        reportBlockedFolder?.Invoke(dir);
                            //    }
                            //}
                            foreach (var dir in Directory.GetDirectories(current))
                            {
                                try
                                {
                                    var dirInfo = new DirectoryInfo(dir);
                                    if ((dirInfo.Attributes & FileAttributes.System) == 0)
                                    {
                                        pending.Push(dir);
                                    }
                                }
                                catch
                                {
                                    reportBlockedFolder?.Invoke(dir); // گزارش پوشه غیرقابل دسترسی
                                }
                            }
                        }
                    }
                    catch
                    {
                        reportBlockedFolder?.Invoke(current); // گزارش پوشه غیرقابل دسترسی
                    }
                }
            });

            return result;
        }
        private bool CanAccessDirectory(string path)
        {
            try
            {
                Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //    public List<string> Search(string folderPath, string keyword, List<string> extensions,bool subDir)
        //    {
        //        SearchOption option = subDir == true
        //? SearchOption.AllDirectories
        //: SearchOption.TopDirectoryOnly;
        //        var results = new List<string>();
        //        var files = Directory.GetFiles(folderPath, "*.*", option)
        //                             .Where(r => extensions.Contains(Path.GetExtension(r).ToLower()))
        //                             .ToList();

        //        //var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
        //        //                     .Where(f => extensions.Contains(Path.GetExtension(f).ToLower()))
        //        //                     .ToList();

        //        var lockObj = new object();

        //        Parallel.ForEach(files, file =>
        //        {
        //            string ext = Path.GetExtension(file).ToLower();
        //            string content = "";

        //            try
        //            {
        //                if (ext == ".txt")
        //                {
        //                    content = File.ReadAllText(file);
        //                }
        //                else if (ext == ".docx")
        //                {
        //                    using (var doc = WordprocessingDocument.Open(file, false))
        //                    {
        //                        content = doc.MainDocumentPart.Document.Body.InnerText;
        //                    }
        //                }
        //                else if (ext == ".pdf")
        //                {
        //                    using (var pdf = PdfDocument.Open(file))
        //                    {
        //                        var sb = new StringBuilder();
        //                        foreach (var page in pdf.GetPages())
        //                        {
        //                            sb.AppendLine(page.Text);
        //                        }
        //                        content = sb.ToString();
        //                    }
        //                }

        //                if (content.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        //                {
        //                    lock (lockObj)
        //                    {
        //                        results.Add(file);
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                lock (lockObj)
        //                {
        //                    results.Add($"⚠️ خطا در خواندن {file}: {ex.Message}");
        //                }
        //            }
        //        });

        //        return results;
        //    }
    }
}
