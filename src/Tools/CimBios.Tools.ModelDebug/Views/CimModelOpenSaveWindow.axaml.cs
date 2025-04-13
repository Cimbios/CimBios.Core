using System;
using Avalonia.Controls;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Tools.ModelDebug.ViewModels;

namespace CimBios.Tools.ModelDebug.Views;

public partial class CimModelOpenSaveWindow : Window
{
    public bool? DialogState => _Model.DialogState;
    public string ModelPath => _Model.CimModelFilePath;
    public string SchemaPath => _Model.CimSerializerFilePath;
    public IRdfSerializerFactory RdfSerializerFactory 
        => _Model.SelectedModelSerializer?.RdfSerializerFactory 
            ?? throw new InvalidOperationException("No selected Rdf serializer!");
    public ICimSchemaFactory SchemaFactory
        => _Model.SelectedSchema?.SchemaFactory 
            ?? throw new InvalidOperationException("No selected schema type!");
    public IOIDDescriptorFactory DescriptorFactory
        => _Model.SelectedOIDDescriptor?.OIDDescriptorFactory 
            ?? throw new InvalidOperationException("No selected OID descriptor type!");  
    public RdfSerializerSettings SerializerSettings
        => _Model.SelectedRdfsSerializerSetting;

    protected CimModelFileSelectorViewModel _Model;

    public CimModelOpenSaveWindow(DialogMode dialogMode)
    {
        _Model = new CimModelFileSelectorViewModel(this)
        {
            SaveMode = dialogMode == DialogMode.Save
        };

        DataContext = _Model;

        InitializeComponent();
    }

    public enum DialogMode
    {
        Load, Save
    }
}
