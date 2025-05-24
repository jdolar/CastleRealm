using System.Reflection;
namespace Domain.Info;
public sealed class Ident
{
    private readonly Shared.Responses.Info.Ident info;
    public Ident(Assembly assembly, DateTime started)
    {
        info = new Shared.Responses.Info.Ident()
        {
            Started = FormatDate(started),
            Name = assembly?.GetName().Name!.ToString() ?? "N/A",
            Title = GetAssemblyAttribute<AssemblyTitleAttribute>(assembly!)?.Title ?? "N/A",
            Version = assembly?.GetName()?.Version?.ToString() ?? "N/A",
            FileVersion = GetAssemblyAttribute<AssemblyFileVersionAttribute>(assembly!)?.Version ?? "N/A",
            InformationalVersion = GetAssemblyAttribute<AssemblyInformationalVersionAttribute>(assembly!)?.InformationalVersion ?? "N/A",

            Description = GetAssemblyAttribute<AssemblyDescriptionAttribute>(assembly!)?.Description ?? "N/A",
            Product = GetAssemblyAttribute<AssemblyProductAttribute>(assembly!)?.Product ?? "N/A",
            Company = GetAssemblyAttribute<AssemblyCompanyAttribute>(assembly!)?.Company ?? "N/A",
        };
    }
    public Shared.Responses.Info.Ident Get() => info;
    static T? GetAssemblyAttribute<T>(Assembly assembly) where T : Attribute
    {
        object[] attributes = assembly.GetCustomAttributes(typeof(T), false);
        return attributes.Length > 0 ? (T)attributes[0] : null;
    }
    static string FormatDate(DateTime date)
    {
        return date.ToString();
    }
}
