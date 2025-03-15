using Lapo.Services;
using Microsoft.AspNetCore.Components;

namespace Lapo.Mud.Components.Pages;

public partial class Configuration : ComponentBase
{
    [Inject] ConfigurationService ConfigurationService { get; set; } = null!;

    List<string> _items = [];
    string _newItem = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        _items = await ConfigurationService.ReadAsync<List<string>>("Tables") ?? [];
    }

    async Task AddItem()
    {
        if (string.IsNullOrWhiteSpace(_newItem)) return;
        
        _items.Add(_newItem);

        await ConfigurationService.UpsertAsync("Tables", _items);
        
        _newItem = string.Empty;
    }

    async Task RemoveItem(string item)
    {
        _items.Remove(item);
        await ConfigurationService.UpsertAsync("Tables", _items);
    }
}