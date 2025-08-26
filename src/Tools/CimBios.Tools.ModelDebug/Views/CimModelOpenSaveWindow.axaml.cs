using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Tools.ModelDebug.ViewModels;

namespace CimBios.Tools.ModelDebug.Views;

public partial class CimModelOpenSaveWindow : Window, IDialog
{
    public enum DialogMode
    {
        Load,
        Save
    }

    private readonly CimModelFileSelectorViewModel _model;

    public CimModelOpenSaveWindow()
    {
        _model = new CimModelFileSelectorViewModel(this);

        DataContext = _model;

        InitializeComponent();
    }

    public IDialogResult Result => new CimModelOpenSaveResult(_model);

    public Task Show(Window owner, params object[]? args)
    {
        if (args != null && args.Length > 0 && args[0] is DialogMode mode)
        {
            _model.SaveMode = mode == DialogMode.Save;
        }

        return this.ShowDialog(owner);
    }
}

public class CimModelOpenSaveResult(CimModelFileSelectorViewModel model)
    : IDialogResult
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

    public bool Succeed => model.DialogState ?? false;
}