using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace CimBios.Tools.ModelDebug.Models;

public interface ISourceSelector
{
    public IEnumerable<Uri> GetSource();
    public Task<IEnumerable<Uri>> GetSourceAsync();
}

public class FileDialogSourceSelector : ISourceSelector
{
    public Window? OwnerWindow { get; set; }
    public bool MultiSelect { get; set; } = false;

    public IEnumerable<Uri> GetSource()
    {
        return GetSourceAsync().Result;
    }

    public async Task<IEnumerable<Uri>> GetSourceAsync()
    {
        var result = new List<Uri>();

        if (OwnerWindow == null)
        {
            return result;
        }

        var topLevel = TopLevel.GetTopLevel(OwnerWindow);
        if (topLevel == null)
        {
            return result;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Open model File",
                AllowMultiple = MultiSelect
            });

        result.AddRange(files.Select(f => f.Path));
        return result;
    }
}