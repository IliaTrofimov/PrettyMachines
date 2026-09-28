using System.Globalization;
using Microsoft.JSInterop;

namespace PrettyMachines.BlazorUI.Services;


public static class CultureHelper
{
	public static readonly CultureInfo DefaultCulture = CultureInfo.GetCultureInfo("en-US");

	public static readonly CultureInfo[] SupportedCultures = new CultureInfo[]
	{
		new CultureInfo("en-US"),
		new CultureInfo("ru-RU") 
	};

	public static async Task UpdateCultureWithJS(IJSRuntime js)
	{
		CultureInfo culture;
		var language = await js.InvokeAsync<string?>("getUrlQueryParam", "lang");

		if (!string.IsNullOrEmpty(language))
		{
			bool isDefaultCulture = false;
			switch (language.ToLowerInvariant())
			{
				case "ru":
				case "rus":
					culture = CultureInfo.GetCultureInfo("ru-RU");
					break;
				case "en":
				case "eng":
					culture = CultureInfo.GetCultureInfo("en-US");
					break;
				// todo: add more
				default:
					culture = DefaultCulture;
					isDefaultCulture = true;
					Console.WriteLine($"URL query contains invalid culture: using default {DefaultCulture}");
					break;
			}

			if (!isDefaultCulture)
			{
				await js.InvokeVoidAsync("saveCultureToLocalStorage", culture.Name);
			}
		}
		else
		{
			var previousCultureName = await js.InvokeAsync<string?>("loadCultureFromLocalStorage");
			if (previousCultureName == null)
			{
				culture = DefaultCulture;
				Console.WriteLine($"Can't find culture info: using default {DefaultCulture}");
			}
			else
			{
				try
				{
					culture = CultureInfo.GetCultureInfo(previousCultureName);			
				}
				catch (CultureNotFoundException)
				{
					culture = DefaultCulture;
					Console.WriteLine($"LocalStorage contains invalid culture: using default {DefaultCulture}");	
				}
			}
		}
		
		CultureInfo.DefaultThreadCurrentCulture = culture;
		CultureInfo.DefaultThreadCurrentUICulture = culture;
		Console.WriteLine($"Application culture is set to {culture}");	
	}
}