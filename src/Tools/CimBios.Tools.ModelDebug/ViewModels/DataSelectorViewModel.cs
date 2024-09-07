using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CimBios.Core.CimModel.Schema.RdfSchema;
using CimBios.Core.DataProvider;
using CimBios.Tools.ModelDebug.Models;
using CommunityToolkit.Mvvm.Input;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class DataSelectorViewModel : ViewModelBase, 
    INotifyPropertyChanged
{
    public ObservableCollection<DataProviderModel> DataProviders { get; }
    public ObservableCollection<SchemaSelectorModel> Schemas { get; }
    public DataProviderModel? SelectedDataProvider { get; set; }
    public SchemaSelectorModel? SelectedSchema { get; set; }
    public AsyncRelayCommand ShowProviderSourceSelectorCommand { get; }
    public AsyncRelayCommand ShowSchemaSourceSelectorCommand { get; }

    public string SourceStringUri 
    { 
        get => SourceUri != null ? SourceUri.AbsoluteUri : string.Empty;
    }

    public string SchemasStringUri 
    { 
        get
        {
            if (SchemasUri != null)
            {
                return string.Join(";", SchemasUri
                    .Select(s => $"\"{s.AbsoluteUri}\""));
            }
            else
            {
                return string.Empty;
            }
        }
    }

    private Uri? SourceUri 
    {
        get => _sourceUri;
        set
        {
            _sourceUri = value;
            OnPropertyChanged(nameof(SourceStringUri));       
        }
    }

    private IEnumerable<Uri>? SchemasUri
    {
        get => _schemasUri;
        set
        {
            _schemasUri = value;
            OnPropertyChanged(nameof(SchemasStringUri));       
        }
    }

    public Avalonia.Visual OwnerView { get; }

    public DataSelectorViewModel(Window parentWindow)
    {
        OwnerView = parentWindow;

        DataProviders = new ObservableCollection<DataProviderModel>()
        {
            new DataProviderModel("CIMXML File", 
                new RdfXmlFileDataProviderFactory(),
                new FileDialogSourceSelector() { OwnerWindow = OwnerView }),
        };
        SelectedDataProvider = DataProviders.FirstOrDefault();

        Schemas = new ObservableCollection<SchemaSelectorModel>()
        {
            new SchemaSelectorModel("RDFS",
                new CimRdfSchemaFactory(),
                new FileDialogSourceSelector() 
                    { OwnerWindow = OwnerView, MultiSelect = true }),
        };
        SelectedSchema = Schemas.FirstOrDefault();

        ShowProviderSourceSelectorCommand = 
            new AsyncRelayCommand(ShowProviderSourceSelector);

        ShowSchemaSourceSelectorCommand = 
            new AsyncRelayCommand(ShowSchemaSourceSelector);
    }

    protected new virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async Task ShowProviderSourceSelector()
    {
        if (SelectedDataProvider != null)
        {
            var sources = await GetSourceList(
                SelectedDataProvider.SourceSelector);
            
            if (sources != null)
            {
                SourceUri = sources.FirstOrDefault();
            }
        }
    }

    private async Task ShowSchemaSourceSelector()
    {
        if (SelectedSchema != null)
        {
            var sources = await GetSourceList(
                SelectedSchema.SourceSelector);
            
            if (sources != null)
            {
                SchemasUri = sources;
            }
        }
    }

    private async Task<IEnumerable<Uri>> GetSourceList(
        ISourceSelector sourceSelector)
    {
        var result = await sourceSelector.GetSourceAsync();
        if (result != null)
        {
            return result;
        }

        return new List<Uri>();
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    private Uri? _sourceUri;
    private IEnumerable<Uri>? _schemasUri;
}
