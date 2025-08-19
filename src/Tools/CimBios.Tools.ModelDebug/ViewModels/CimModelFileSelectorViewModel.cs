using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema.AutoSchema;
using CimBios.Core.CimModel.Schema.RdfSchema;
using CimBios.Tools.ModelDebug.Models;
using CommunityToolkit.Mvvm.Input;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class CimModelFileSelectorViewModel : ViewModelBase
{
    private readonly List<CimSerializerSelectorModel>
        _cimModelSerializers =
        [
            new("cimxml", new RdfXmlSerializerFactory())
        ];

    private readonly List<CimSchemaSelectorModel>
        _cimSchemaSerializers =
        [
            new("rdfs [xml]", new CimRdfSchemaXmlFactory()),
            new("Auto [xml]", new CimAutoSchemaXmlFactory())
        ];

    private readonly List<OIDDescriptorSelectorModel>
        _oidDescriptors =
        [
            new("uuid", new UuidDescriptorFactory()),
            new("text", new TextDescriptorFactory())
        ];

    private string _cimModelFilePath = string.Empty;
    private string _cimSerializerFilePath = string.Empty;

    public CimModelFileSelectorViewModel(Window parentWindow)
    {
        OwnerView = parentWindow;

        SelectCimModelFilePathCommand = new AsyncRelayCommand(async () =>
        {
            IStorageFile? result = null;
            if (SaveMode)
                result = await GetSaveStorageFile();
            else
                result = await GetOpenStorageFile();

            if (result != null) CimModelFilePath = result.Path.LocalPath;
        });

        SelectCimSchemaFilePathCommand = new AsyncRelayCommand(async () =>
        {
            var result = await GetOpenStorageFile();
            if (result != null) CimSerializerFilePath = result.Path.LocalPath;
        });

        CancelCommand = new RelayCommand(Cancel);
        OkCommand = new RelayCommand(Ok);

        SelectedModelSerializer = CimModelSerializers.FirstOrDefault();
        SelectedSchema = CimSchemaSerializers.FirstOrDefault();
        SelectedOIDDescriptor = OIDDescriptors.FirstOrDefault();
    }

    private Visual OwnerView { get; }
    public bool SaveMode { get; set; } = false;

    public bool? DialogState { get; private set; }

    public ICollection<CimSerializerSelectorModel> CimModelSerializers
        => _cimModelSerializers;

    public ICollection<CimSchemaSelectorModel> CimSchemaSerializers
        => _cimSchemaSerializers;

    public ICollection<OIDDescriptorSelectorModel> OIDDescriptors
        => _oidDescriptors;

    public string CimModelFilePath
    {
        get => _cimModelFilePath;
        set
        {
            _cimModelFilePath = value;
            OnPropertyChanged();
        }
    }

    public string CimSerializerFilePath
    {
        get => _cimSerializerFilePath;
        set
        {
            _cimSerializerFilePath = value;
            OnPropertyChanged();
        }
    }

    public CimSerializerSelectorModel? SelectedModelSerializer { get; set; }
    public CimSchemaSelectorModel? SelectedSchema { get; set; }
    public OIDDescriptorSelectorModel? SelectedOIDDescriptor { get; set; }

    public RdfSerializerSettings SelectedRdfsSerializerSetting { get; } = new();

    public AsyncRelayCommand SelectCimModelFilePathCommand { get; }
    public AsyncRelayCommand SelectCimSchemaFilePathCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand OkCommand { get; }

    private void Ok()
    {
        DialogState = true;

        if (OwnerView is Window ownerWindow) ownerWindow.Close();
    }

    private void Cancel()
    {
        DialogState = false;

        if (OwnerView is Window ownerWindow) ownerWindow.Close();
    }

    private async Task<IStorageFile?> GetOpenStorageFile()
    {
        if (OwnerView == null)
            throw new NotSupportedException(
                "Unable to set owner view to file dialog!");

        var topLevel = TopLevel.GetTopLevel(OwnerView);
        if (topLevel == null)
            throw new NotSupportedException(
                "Unable to set owner view to file dialog!");

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Open model File",
                AllowMultiple = false
            });

        return files.FirstOrDefault();
    }

    private async Task<IStorageFile?> GetSaveStorageFile()
    {
        if (OwnerView == null)
            throw new NotSupportedException(
                "Unable to set owner view to file dialog!");

        var topLevel = TopLevel.GetTopLevel(OwnerView);
        if (topLevel == null)
            throw new NotSupportedException(
                "Unable to set owner view to file dialog!");

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "Save model File"
            });

        return file;
    }
}