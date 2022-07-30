using Blazorise.Localization;
using Microsoft.AspNetCore.Components;

namespace DiscordBot.Dashboard.Shared;

public partial class MainLayout {
    protected override async Task OnInitializedAsync() {
        await SelectCulture("en-GB");
        await base.OnInitializedAsync();
    }

    private Task SelectCulture(string name) {
        LocalizationService.ChangeLanguage(name);
        return Task.CompletedTask;
    }

    [Inject]
    protected ITextLocalizerService LocalizationService { get; set; }
}