using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CimBios.Core.CimModel.Context;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.RdfSchema;
using CimBios.Tools.ModelDebug.Models;
using CommunityToolkit.Mvvm.Input;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class DataSelectorViewModel : ViewModelBase
{
    public Avalonia.Visual OwnerView { get; }
    public string ResultMessage 
    { 
        get => _resultMessage; 
        set
        {
            _resultMessage = value;
            OnPropertyChanged(nameof(ResultMessage));   
        } 
    }
    public ObservableCollection<ModelDataContextModel> DataContexties { get; }
    public ObservableCollection<SchemaSelectorModel> Schemas { get; }
    public ModelDataContextModel? SelectedDataContext { get; set; }
    public SchemaSelectorModel? SelectedSchema { get; set; }
    public AsyncRelayCommand ShowDataContextSourceSelectorCommand { get; }
    public AsyncRelayCommand ShowSchemaSourceSelectorCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand GetCommand { get; }

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

    public DataSelectorViewModel(Window parentWindow)
    {
        OwnerView = parentWindow;

        DataContexties = new ObservableCollection<ModelDataContextModel>()
        {
            new ModelDataContextModel("CIMXML File", 
                new RdfXmlFileModelDataContextFactory(),
                new FileDialogSourceSelector() { OwnerWindow = OwnerView }),
        };
        SelectedDataContext = DataContexties.FirstOrDefault();

        Schemas = new ObservableCollection<SchemaSelectorModel>()
        {
            new SchemaSelectorModel("RDFS",
                new CimRdfSchemaFactory(),
                new FileDialogSourceSelector() 
                    { OwnerWindow = OwnerView, MultiSelect = true }),
        };
        SelectedSchema = Schemas.FirstOrDefault();

        ShowDataContextSourceSelectorCommand = 
            new AsyncRelayCommand(ShowProviderSourceSelector);

        ShowSchemaSourceSelectorCommand = 
            new AsyncRelayCommand(ShowSchemaSourceSelector);

        CancelCommand = new RelayCommand(Cancel, () => !_isWork);
        GetCommand = new RelayCommand(Get, () => !_isWork);
    }

    private void Get()
    {
        _isWork = true;

        try
        {
            var cimSchema = LoadSchemas();
            if (cimSchema != null)
            {
                ResultMessage += "Schemas successfully loaded.\n";

                CreateModelContext(cimSchema);
            }
        }
        catch (Exception ex)
        {
            ResultMessage += $"Exception while loading: {ex.Message}\n";
        }

        _isWork = false;
    }

    private void Cancel()
    {
        if (OwnerView is Window ownerWindow)
        {
            ownerWindow.Close();
        } 
    }

    private async Task ShowProviderSourceSelector()
    {
        if (SelectedDataContext != null)
        {
            var sources = await GetSourceList(
                SelectedDataContext.SourceSelector);
            
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

    private ICimSchema? LoadSchemas()
    {
        if (SelectedSchema == null
            || SchemasUri == null
            || SchemasUri.Count() == 0)
        {
            ResultMessage += "Schemas have not loaded. Schema provider missed?\n";
            return null;
        }

        var cimSchema = SelectedSchema.SchemaFactory.CreateSchema();
        cimSchema.Load(new StreamReader(SchemasUri.First().LocalPath));

        if (SchemasUri.Count() > 1)
        {
            foreach (var schemaUri in SchemasUri.Skip(1))
            {
                var addSchema = SelectedSchema.SchemaFactory.CreateSchema();
                addSchema.Load(new StreamReader(schemaUri.LocalPath));
                cimSchema.Join(addSchema);
            }
        }

        return cimSchema;
    }

    private bool CreateModelContext(ICimSchema cimSchema)
    {
        if (Services.ServiceLocator.GetInstance()
            .TryGetService<ModelContext>(out var modelContext) == false
            || modelContext == null)
        {
            ResultMessage += "Model context service has not registered!\n";
            return false;
        }

        if (SelectedDataContext == null
            || SourceUri == null)
        {
            ResultMessage += "Model context provider/source has not selected!\n";
            return false;
        }

        var dataContext = SelectedDataContext.ModelDataContextFactory
            .Create(SourceUri, cimSchema);

        modelContext.Load(dataContext);

        return true;
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    private Uri? _sourceUri;
    private IEnumerable<Uri>? _schemasUri;
    private string _resultMessage = string.Empty;

    private bool _isWork = false;
}
