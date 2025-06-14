using Microsoft.Extensions.Logging;
using Shared.Api;
using Shared.Tools.Swagger.Models;
using System.Text;
namespace Shared.Tools.FileClient;
public sealed class MdFile(ILogger logger) : BaseFile(logger)
{
    private readonly ILogger _logger = logger;
    public string? GenerateSimplifiedMarkdown(List<EndpointMatch>? matches, List<string> facadeNames)
    {
        if (matches == null || matches.Count == 0)
        {
            _logger.LogError("❌ No endpoints matched.");
            return string.Empty;
        }

        StringBuilder sb = new();

        // Facade labels (one per block of 5 columns)
        sb.AppendLine($"| {facadeNames[0]} | Name | Method | Parameters | Misc | " +
                      $"{facadeNames[1]} | Name | Method | Parameters | Misc | " +
                      $"{facadeNames[2]} | Name | Method | Parameters | Misc |");

        // Markdown header alignment
        sb.AppendLine("|------------------|------|--------|------------|------|" +
                      "------------------|------|--------|------------|------|" +
                      "------------------|------|--------|------------|------|");

        foreach (var match in matches)
        {
            string FormatEndpoint(Endpoint? ep)
            {
                if (ep == null)
                    return "- | - | - | - | -";

                string parameters = FormatParameters(ep.Parameters).Replace("\r", "").Replace("\n", "<br>");
                string miscParts = "";

                if (!string.IsNullOrWhiteSpace(ep.Operation))
                    miscParts += $"[Operation={Escape(ep.Operation)}]";
                if (!string.IsNullOrWhiteSpace(ep.Tags))
                    miscParts += $" [Tags={Escape(ep.Tags)}]";
                if (!string.IsNullOrWhiteSpace(ep.Title))
                    miscParts += $" [Title={Escape(ep.Title)}]";

                return $"{Escape(ep.Path)} | {Escape(ep.Name)} | {Escape(ep.Method)} | {parameters} | {Escape(miscParts.Trim())}";
            }

            string row = $"| {FormatEndpoint(match.A)} | {FormatEndpoint(match.B)} | {FormatEndpoint(match.C)} |";
            sb.AppendLine(row);
        }

        return sb.ToString();
    }
    private string Escape(string? input)
    {
        return string.IsNullOrWhiteSpace(input)
            ? "-"
            : input.Replace("|", "\\|").Replace("\n", "").Replace("\r", "").Trim();
    }
    private string FormatParameters(List<Parameter>? parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return "-";

        return string.Join("<br>", parameters.Select(p =>
            $"**{p.Name}** ({p.Type}) [{p.In}]{(p.Required ? " (required)" : "")}"
        ));
    }
}
