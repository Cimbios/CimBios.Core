using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.AutoSchema;
using CimBios.Core.CimModel.Schema.RdfSchema;
using CimBios.Tools.ModelDebug.Models;
using CimBios.Tools.ModelDebug.Services;
using CommunityToolkit.Mvvm.Input;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Platform.Storage;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class CimModelFileSelectorViewModel : ViewModelBase
{
    public Avalonia.Visual OwnerView { get; }
    public bool SaveMode { get; set; } = false;

    public bool? DialogState { get; private set; } = null;

    public ICollection<CimSerializerSelectorModel> CimModelSerializers 
        => _CimModelSerializers;

    public ICollection<CimSchemaSelectorModel> CimSchemaSerializers 
        => _CimSchemaSerializers;

    public ICollection<OIDDescriptorSelectorModel> OIDDescriptors 
        => _OIDDescriptors;
        
    public string CimModelFilePath
    {
        get => _CimModelFilePath;
        set
        {
            _CimModelFilePath = value;
            OnPropertyChanged();
        }
    }

    public string CimSerializerFilePath
    {
        get => _CimSerializerFilePath;
        set
        {
            _CimSerializerFilePath = value;
            OnPropertyChanged();
        }
    }

    public CimSerializerSelectorModel? SelectedModelSerializer { get; set; }
    public CimSchemaSelectorModel? SelectedSchema { get; set; }
    public OIDDescriptorSelectorModel? SelectedOIDDescriptor { get; set; }
    public RdfSerializerSettings SelectedRdfsSerializerSetting { get; }
        = new RdfSerializerSettings();

    public AsyncRelayCommand SelectCimModelFilePathCommand { get; }
    public AsyncRelayCommand SelectCimSchemaFilePathCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand OkCommand { get; }

    public CimModelFileSelectorViewModel(Window parentWindow)
    {
        OwnerView = parentWindow;

        SelectCimModelFilePathCommand = new AsyncRelayCommand(async () =>
        {
            IStorageFile? result = null;
            if (SaveMode)
            {
                result = await GetSaveStorageFile();
            }
            else
            {
                result = await GetOpenStorageFile();
            }

            if (result != null)
            {
                CimModelFilePath = result.Path.LocalPath;
            }
        });

        SelectCimSchemaFilePathCommand = new AsyncRelayCommand(async () =>
        {
            var result = await GetOpenStorageFile();
            if (result != null)
            {
                CimSerializerFilePath = result.Path.LocalPath;
            }
        });

        CancelCommand = new RelayCommand(Cancel);
        OkCommand = new RelayCommand(Ok);

        SelectedModelSerializer = CimModelSerializers.FirstOrDefault();
        SelectedSchema = CimSchemaSerializers.FirstOrDefault();
        SelectedOIDDescriptor = OIDDescriptors.FirstOrDefault();
    }

    private void Ok()
    {
        DialogState = true;

        if (OwnerView is Window ownerWindow)
        {
            ownerWindow.Close();
        } 
    }

    private void Cancel()
    {
        DialogState = false;

        if (OwnerView is Window ownerWindow)
        {
            ownerWindow.Close();
        } 
    }

    private async Task<IStorageFile?> GetOpenStorageFile()
    {
        if (OwnerView == null)
        {
            throw new NotSupportedException(
                "Unable to set owner view to file dialog!");
        }

        var topLevel = TopLevel.GetTopLevel(OwnerView);
        if (topLevel == null)
        {
            throw new NotSupportedException(
                "Unable to set owner view to file dialog!");
        }

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
        {
            throw new NotSupportedException(
                "Unable to set owner view to file dialog!");
        }

        var topLevel = TopLevel.GetTopLevel(OwnerView);
        if (topLevel == null)
        {
            throw new NotSupportedException(
                "Unable to set owner view to file dialog!");
        }

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "Save model File",
            });

        return file;
    }

    private string _CimModelFilePath = string.Empty;
    private string _CimSerializerFilePath = string.Empty;

    private readonly List<CimSerializerSelectorModel>
        _CimModelSerializers = 
        [
            new ("cimxml", new RdfXmlSerializerFactory())
        ];

    private readonly List<CimSchemaSelectorModel>
        _CimSchemaSerializers = 
        [
            new ("rdfs [xml]", new CimRdfSchemaXmlFactory()),
            new ("Auto [xml]", new CimAutoSchemaXmlFactory())
        ]; 

    private readonly List<OIDDescriptorSelectorModel>
        _OIDDescriptors = 
        [
            new("uuid", new UuidDescriptorFactory()),
            new("text", new TextDescriptorFactory())
        ];
}
