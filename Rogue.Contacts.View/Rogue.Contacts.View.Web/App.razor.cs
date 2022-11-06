using Avalonia.Web.Blazor;

namespace Rogue.Contacts.View.Web
{
    public partial class App
    {
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            WebAppBuilder.Configure<Rogue.Contacts.View.App>()
                .SetupWithSingleViewLifetime();
        }
    }
}