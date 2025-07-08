using System;
using Avalonia.Controls;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Tools.ModelDebug.ViewModels;

namespace CimBios.Tools.ModelDebug.Views;

public partial class CimModelOpenSaveWindow : Window
{
    public enum DialogMode
    {
        Load,
        Save
    }

    private readonly CimModelFileSelectorViewModel _model;

    public CimModelOpenSaveWindow(DialogMode dialogMode)
    {
        _model = new CimModelFileSelectorViewModel(this)
        {
            SaveMode = dialogMode == DialogMode.Save
        };

        DataContext = _model;

        InitializeComponent();
    }

    public CimModelOpenSaveResult Result => new(_model);
    public bool? DialogState => _model.DialogState;
}

public class CimModelOpenSaveResult(CimModelFileSelectorViewModel model)
{
    public string ModelPath { get; } = model.CimModelFilePath;

    public string SchemaPath { get; } = model.CimSerializerFilePath;

    public IRdfSerializerFactory RdfSerializerFactory { get; } =
        model.SelectedModelSerializer?
            .RdfSerializerFactory
        ?? throw new InvalidOperationException("No selected Rdf serializer!");

    public ICimSchemaFactory SchemaFactory { get; } =
        model.SelectedSchema?.SchemaFactory
        ?? throw new InvalidOperationException("No selected schema type!");

    public IOIDDescriptorFactory DescriptorFactory { get; } =
        model.SelectedOIDDescriptor?.OIDDescriptorFactory
        ?? throw new InvalidOperationException("No selected OID descriptor type!");

    public RdfSerializerSettings SerializerSettings { get; }
        = model.SelectedRdfsSerializerSetting;
}