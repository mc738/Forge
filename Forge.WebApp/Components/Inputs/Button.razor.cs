using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Forge.WebApp.Models;

namespace Forge.WebApp.Components.Inputs;

public class ButtonBase: ComponentBase, IDisposable
{
    [Parameter]
    public string Id { get; set; }

    [Parameter]
    public string For { get; set; }

    [Parameter]
    public string Label { get; set; }

    [Parameter]
    public string Icon { get; set; }

    [Parameter]
    public bool IconButton { get; set; } = false;

    [Parameter] 
    public ButtonType Type { get; set; } = ButtonType.Secondary;
        
    [Parameter]
    public EventCallback Clicked { get; set; }

    protected string Classes => Type switch
    {
        ButtonType.Primary => "btn-primary",
        ButtonType.Secondary => "btn-secondary",
        ButtonType.Success => "btn-success",
        ButtonType.Danger => "btn-danger",
        ButtonType.Warning => "btn-warning",
        ButtonType.Information => "btn-information",
        _ => throw new ArgumentOutOfRangeException()
    };
        
    void ButtonPressed(MouseEventArgs args)
    {
        StateHasChanged();

        Clicked.InvokeAsync(this);
    }
        
    public void Dispose()
    {
    }
}