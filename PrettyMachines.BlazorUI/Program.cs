using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using PrettyMachines.BlazorUI;
using PrettyMachines.BlazorUI.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<AlgorithmDraftStore>();
builder.Services.AddSingleton<AlgorithmLocalizer>();
builder.Services.AddSingleton<DraftPersistenceService>();
builder.Services.AddLocalization();

var host = builder.Build();

var js = host.Services.GetRequiredService<IJSRuntime>();
await CultureHelper.UpdateCultureWithJS(js);

var persistence = host.Services.GetRequiredService<DraftPersistenceService>();
await persistence.InitializeAsync();

await host.RunAsync();
