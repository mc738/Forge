using Microsoft.AspNetCore.Components;

namespace Forge.WebApp.Services;

public class ModalService
{
    public event Action<string, RenderFragment> OnShow;

    public event Action OnClose;

    public void Show(string title, RenderFragment content)
    {
        // if (contentType.BaseType != typeof(ComponentBase))
        //    throw new ArgumentException($"{contentType.FullName} must be a Blazor Component");
        // var content = new RenderFragment(x => { x.OpenComponent(1, contentType); x.CloseComponent(); });
        OnShow?.Invoke(title, content);
    }

    public void Close()
    {
        OnClose?.Invoke();
    }
}