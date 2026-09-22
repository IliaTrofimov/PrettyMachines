namespace PrettyMachines.BlazorUI;

public static class UrlPrefix
{
	public const string PREFIX
	#if DEBUG
		= ""
	#else
		= ""
	#endif
	;
}
