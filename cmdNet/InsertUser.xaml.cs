
using cmdNet.Model;
using cmdNet.Services;
using cmdNet.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace cmdNet;

public partial class InsertUser : Window
{
    private readonly MainViewModel _vm;//=   new();
    bool _creauteUser, _analize;
    //string ldp = @"";
    public InsertUser()
    {
        InitializeComponent();
        _vm = new MainViewModel();
        DataContext = _vm;


        _vm.DomainController = "LDAP://OU=*****,OU=*****,DC=*****,DC=****";
        _vm.AdminUsername = @"*****\*******";
        //_vm.OuPath = @"OU=R&D";
        txtPassword.Password = @"**********";
        
        DataContext = _vm;
        CreateUser.IsEnabled = _analize;
        ExcelReport.IsEnabled= _creauteUser;


    }

    private void Browse_Click(
        object sender,
        RoutedEventArgs e)
    {
        OpenFileDialog dlg =
            new();

        dlg.Filter =
            "Excel Files|*.xlsx";

        if (dlg.ShowDialog() == true)
        {
            _vm.ExcelFilePath =
                dlg.FileName;
        }
    }

    private async void Analyze_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _vm.Results.Clear();

            string adminPassword =
                txtPassword.Password;

            ExcelService excel =
                new();

            var users =
                excel.Read(_vm.ExcelFilePath);

            var adService =
                new ActiveDirectoryService(
                    _vm.AdminUsername,
                    adminPassword);

            int total = users.Count;
            int current = 0;

            foreach (var user in users)
            {
                current++;

                string userName =
                    GenerateUniqueUserName(
                        user.FirstName,
                        user.LastName,
                        adService);

                var similar =
                    adService.FindSimilarUser(
                        user.FirstName,
                        user.LastName);

                _vm.Results.Add(
                    new UserResultModel
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        NationalCode = user.NationalCode,
                        Mobile = user.Mobile,
                        UserName = userName,

                        SimilarUserName =
                            similar?.UserName ?? "",

                        SimilarDisplayName =
                            similar?.DisplayName ?? "",

                        Action =
                            similar == null
                                ? "Create"
                                : "Skip",

                        Status = "Analyzed"
                    });

                _vm.ProgressPercent =
                    current * 100.0 / total;

                await Task.Delay(1);
            }
            //        MessageBox.Show(
            //$"Results Count = {_vm.Results.Count}");

            MessageBox.Show(
                "Analyze completed.",
                "Information",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            _analize = true;
            CreateUser.IsEnabled = _analize;
          

        }


        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void Execute_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string result = "";
            string adminPassword =
                txtPassword.Password;

            var adService =
                new ActiveDirectoryService(
                    _vm.AdminUsername,
                    adminPassword);

            int total = _vm.Results.Count;
            int current = 0;

            foreach (var item in _vm.Results)
            {
                current++;

                try
                {
                    if (item.Action == "Skip")
                    {
                        item.UserName =
                            item.SimilarUserName;

                        item.Status =
                            "Skipped";

                        continue;
                    }
                    if (item.Action == "ResetPassword")
                    {
                        if (string.IsNullOrWhiteSpace(item.SimilarUserName))
                        {
                            item.Status = "UserNotFound";
                            continue;
                        }

                        string newPass =
                            PasswordGenerator.Generate();

                        adService.ResetPassword(
                            _vm.DomainController,
                            item.SimilarUserName,
                            newPass);

                        item.UserName =
                            item.SimilarUserName;

                        item.Password =
                            newPass;

                        item.Status =
                            "PasswordReset";

                        continue;
                    }

                    if (item.Action == "Create")
                    {
                        //var excelUser =
                        //    _excelUsers.FirstOrDefault(x =>
                        //        x.FirstName == item.FirstName &&
                        //        x.LastName == item.LastName);
                        var excelUser = new UserExcelModel
                        {
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            NationalCode = item.NationalCode,

                            Mobile = item.Mobile
                        };

                        if (excelUser == null)
                        {
                            item.Status =
                                "ExcelUserNotFound";

                            continue;
                        }

                        string password =
                            PasswordGenerator.Generate();
                        _vm.OuPath = _vm.OuPath + "," + _vm.DomainController;

                        adService.CreateUser(
                            _vm.DomainController,
                            excelUser,
                            item.UserName,
                            password);

                        item.Password =
                            password;

                        item.Status =
                            "Created";


                    }
                    TxtService txtService = new TxtService();

                    SaveFileDialog saveDialog = new();

                    saveDialog.Filter =
                        "txt Files (*.txt)|*.txt";

                    saveDialog.FileName =
                        $"ADUsersReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                    if (saveDialog.ShowDialog() != true)
                        return;
                    result = txtService.WriteFileTxt(saveDialog.FileName, _vm.Results.ToList());
                    _creauteUser = true;
                    ExcelReport.IsEnabled = _creauteUser;
                }
                catch (Exception ex)
                {
                    item.Status =
                        "Error";

                    // اگر خواستی خطا را هم ذخیره کن
                    // item.ErrorMessage = ex.Message;
                }

                _vm.ProgressPercent =
                    current * 100.0 / total;

                await Task.Delay(1);
                _creauteUser = true;
                ExcelReport.IsEnabled= _creauteUser;
            }

            _vm.BuildSummary();

            MessageBox.Show(
                "Operation completed successfully.",
                "Done",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }


    private void ExportReport_Click(
    object sender,
    RoutedEventArgs e)
    {
        try
        {
            SaveFileDialog saveDialog = new();

            saveDialog.Filter =
                "Excel Files (*.xlsx)|*.xlsx";

            saveDialog.FileName =
                $"ADUsersReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            if (saveDialog.ShowDialog() != true)
                return;

            ExcelService excelService = new();

            excelService.Export(
                saveDialog.FileName,
                _vm.Results.ToList());

            MessageBox.Show(
                "Report exported successfully.",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }



   

   
    private string GenerateUniqueUserName(
    string firstName,
    string lastName,
    ActiveDirectoryService adService)
    {
        firstName =
            firstName.Trim().ToLower();

        lastName =
            lastName.Trim()
                    .Replace(" ", "")
                    .ToLower();

        for (int i = 1; i <= firstName.Length; i++)
        {
            string userName =
                $"{firstName.Substring(0, i)}.{lastName}";
          
            if (!adService.UserExists(
                    _vm.DomainController,
                    userName))
            {
                return userName;
            }
        }

        return
            $"{firstName}.{lastName}";
    }

    private void ComboBox_Loaded(object sender, RoutedEventArgs e)
    {
        var cb = (ComboBox)sender;
        cb.IsDropDownOpen = true;
    }





}


#region code
/*




private string GenerateUniqueUserName(
string firstName,
string lastName,
ActiveDirectoryService adService)
{
    firstName = firstName.Trim().ToLower();
    lastName = lastName.Trim().ToLower();

    for (int i = 1; i <= firstName.Length; i++)
    {
        string userName =
            $"{firstName.Substring(0, i)}.{lastName}";

        if (!adService.UserExists(
                _vm.DomainController,
                userName))
        {
            return userName;
        }
    }

    return null;
}


private async void CreateUsers_Click(
    object sender,
    RoutedEventArgs e)
{
    try
    {

        string password =
            txtPassword.Password;

        ExcelService excel =
            new();

        var users =
            excel.Read(
                _vm.ExcelFilePath);

        var ad =
            new ActiveDirectoryService(
                _vm.AdminUsername,
                password);

        _vm.Results.Clear();

        int total =
            users.Count;

        int current = 0;

        foreach (var user in users)
        {
            current++;

            try
            {
                string userName = GenerateUniqueUserName(
    user.FirstName,
    user.LastName,
    ad);

                if (userName == null)
                {
                    _vm.Results.Add(
                        new UserResultModel
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Status = "UserAlreadyExists"
                        });

                    continue;
                }

                if (ad.UserExists(
                    _vm.DomainController,
                    userName))
                {
                    _vm.Results.Add(
                        new UserResultModel
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            UserName = userName,
                            Status = "UserAlreadyExists"
                        });

                    continue;
                }

                string generatedPassword =
                    PasswordGenerator.Generate();
                _vm.OuPath = _vm.OuPath + "," + _vm.DomainController;// @"OU=R&D,DC=mcinext,DC=org";// _vm.DomainController;
                ad.CreateUser(
                     _vm.DomainController,

                    user,
                    userName,
                    generatedPassword);

                _vm.Results.Add(
                    new UserResultModel
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserName = userName,
                        Password = generatedPassword,
                        Status = "Created"
                    });




            }
            catch (Exception ex)
            {
                _vm.Results.Add(
                    new UserResultModel
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Status = "Error"
                    });
            }

            _vm.ProgressPercent =
                current * 100.0 / total;

            await Task.Delay(1);
        }

        _vm.BuildSummary();


    }
    catch (Exception ex)
    {
        MessageBox.Show(
            ex.Message);
    }
}





*/
#endregion
