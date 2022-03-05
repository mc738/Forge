using Forge.WebApp.Services;
using Microsoft.AspNetCore.Components;

namespace Forge.WebApp.Components.Structure;

public class ModalBase: ComponentBase, IDisposable
{
    [Inject] ModalService ModalService { get; set; }

    protected bool IsVisible { get; set; }
    protected string Title { get; set; }
    protected RenderFragment Content { get; set; }

    // public ModalBase()
    // {
    //     ModalService.OnShow += ShowModal;
    //     ModalService.OnClose += CloseModal;
    // }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ModalService.OnShow += ShowModal;
        ModalService.OnClose += CloseModal;
    }

    public async void ShowModal(string title, RenderFragment content)
    {
        Title = title;
        Content = content;
        IsVisible = true;

        await InvokeAsync(() => StateHasChanged());
    }

    public async void CloseModal()
    {
        IsVisible = false;
        Title = "";
        Content = null;

        // https://stackoverflow.com/questions/56477829/how-to-fix-the-current-thread-is-not-associated-with-the-renderers-synchroniza
        await InvokeAsync(() => StateHasChanged());
    }

    public void Dispose()
    {
        ModalService.OnShow -= ShowModal;
        ModalService.OnClose -= CloseModal;
    }
}