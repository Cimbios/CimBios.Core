using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class CimObjectCreatorViewModel : ViewModelBase
{
    private Visual OwnerView { get; }

    public bool? DialogState { get; private set; }

    public ICimMetaClass? MetaClass { get; set; }

    public IOIDDescriptor? Descriptor { get; private set; }

    public string TextOID { get; set; } = string.Empty;

    public string TextMetaClass { get; set; } = string.Empty;

    public List<string> MetaClassesList { get; }

    public CimObjectCreatorViewModel(Window parentWindow)
    {
        OwnerView = parentWindow;

        MetaClassesList = [];
        if (GlobalServices.LoaderService.DataContext == null)
        {
            return;
        }

        var autoOID = GlobalServices.LoaderService.DataContext
            .OIDDescriptorFactory.TryCreate();
        if (autoOID is not null)
        {
            TextOID = autoOID.ToString();
        }

        var schema = GlobalServices.LoaderService.DataContext.Schema;

        foreach (var metaClass in schema.Classes.Where(schema.CanCreateClass))
        {
            MetaClassesList.Add(metaClass.BaseUri.AbsoluteUri);
        }
    }

    public void Ok()
    {
        if (ValidateOID(TextOID) == false)
        {
            return;
        }

        if (ValidateMetaClass(TextMetaClass) == false)
        {
            return;
        }

        DialogState = true;

        if (OwnerView is Window ownerWindow) ownerWindow.Close();
    }

    public void Cancel()
    {
        DialogState = false;

        if (OwnerView is Window ownerWindow) ownerWindow.Close();
    }

    private bool ValidateOID(string textOID)
    {
        if (GlobalServices.LoaderService.DataContext == null)
        {
            return false;
        }

        var newOID = GlobalServices.LoaderService.DataContext
            .OIDDescriptorFactory.TryCreate(textOID);

        if (newOID == null) return false;

        Descriptor = newOID;

        return true;
    }

    private bool ValidateMetaClass(string metaClassUri)
    {
        if (GlobalServices.LoaderService.DataContext == null)
        {
            return false;
        }

        var schema = GlobalServices.LoaderService.DataContext.Schema;

        if (Uri.TryCreate(metaClassUri, UriKind.Absolute, out var classUri) == false)
        {
            return false;
        }

        var metaClass = schema.TryGetResource<ICimMetaClass>(classUri);
        if (metaClass == null) return false;

        MetaClass = metaClass;

        return true;
    }
}