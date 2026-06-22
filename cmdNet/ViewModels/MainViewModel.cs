
using cmdNet.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace cmdNet.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private string _domainController;
    private string _adminUsername;
    private string _ouPath;
    private string _excelFilePath;
    private double _progressPercent;
    private string _summary;

    public string DomainController
    {
        get => _domainController;
        set
        {
            _domainController = value;
            OnPropertyChanged();
        }
    }

    public string AdminUsername
    {
        get => _adminUsername;
        set
        {
            _adminUsername = value;
            OnPropertyChanged();
        }
    }

    public string OuPath
    {
        get => _ouPath;
        set
        {
            _ouPath = value;
            OnPropertyChanged();
        }
    }

    public string ExcelFilePath
    {
        get => _excelFilePath;
        set
        {
            _excelFilePath = value;
            OnPropertyChanged();
        }
    }

    public double ProgressPercent
    {
        get => _progressPercent;
        set
        {
            _progressPercent = value;
            OnPropertyChanged();
        }
    }

    public string Summary
    {
        get => _summary;
        set
        {
            _summary = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<UserResultModel> Results { get; }
     = new ObservableCollection<UserResultModel>();

    public List<string> Actions { get; } = new()
{
    "Create",
    "ResetPassword",
    "Skip"
};



    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }

    public void BuildSummary()
    {
        int total = Results.Count;

        int created =
            Results.Count(x => x.Status == "Created");

        int exists =
            Results.Count(x => x.Status == "UserAlreadyExists");

        int error =
            Results.Count(x => x.Status == "Error");

        Summary =
            $"Total: {total}   " +
            $"Created: {created}   " +
            $"Exists: {exists}   " +
            $"Error: {error}";
    }


   
}