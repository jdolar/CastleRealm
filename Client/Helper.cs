using System.Reflection;
namespace ApiClient;

public static class Helper
{
    public static string GetUrlExtension(string baseUri, object queryParams)
    {
        if (queryParams == null)
            return baseUri;

        PropertyInfo[] props = queryParams.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        string query = string.Join("&", props
            .Where(p => p.GetValue(queryParams) != null)
            .Select(p =>
                $"{Uri.EscapeDataString(p.Name)}={Uri.EscapeDataString(p.GetValue(queryParams)!.ToString()!)}"
            )
        );

        return string.IsNullOrWhiteSpace(query)
            ? baseUri
            : $"{baseUri}?{query}";
    }
}
