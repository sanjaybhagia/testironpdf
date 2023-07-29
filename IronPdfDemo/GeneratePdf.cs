using System;
using System.IO;
using System.Threading.Tasks;
using IronPdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IronPdfDemo;

public static class GeneratePdf
{
    /// <summary>
    /// Function returns a pdf from a html string provided in request
    /// </summary>
    /// <param name="req">HTML text</param>
    /// <param name="log"></param>
    /// <returns></returns>
    [FunctionName("GeneratePdf")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
    {
        if (req.Body == null)
        {
            return null;
        }
        var stream = new StreamReader(req.Body);
        var html = stream.ReadToEndAsync().GetAwaiter().GetResult();
        
        log.LogTrace($"Received html {html?.Length} bytes");
        var renderer = new IronPdf.ChromePdfRenderer();

        renderer.RenderingOptions.MarginTop = 10;
        renderer.RenderingOptions.MarginLeft = 10;
        renderer.RenderingOptions.MarginBottom = 10;
        renderer.RenderingOptions.MarginRight = 10;

        renderer.RenderingOptions.PaperOrientation = IronPdf.Rendering.PdfPaperOrientation.Landscape;
        renderer.RenderingOptions.Title = "TEST TITLE";
        renderer.RenderingOptions.Zoom = 100;
        renderer.RenderingOptions.CssMediaType = IronPdf.Rendering.PdfCssMediaType.Print;
        renderer.RenderingOptions.PrintHtmlBackgrounds = true;
        renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
        renderer.RenderingOptions.EnableJavaScript = true;
        renderer.RenderingOptions.WaitFor.JavaScript();

        renderer.RenderingOptions.TextFooter.DrawDividerLine = true;
        renderer.RenderingOptions.TextFooter.Font = IronPdf.Font.FontTypes.Helvetica;
        renderer.RenderingOptions.TextFooter.LeftText = $"{DateTime.UtcNow}";
        renderer.RenderingOptions.TextFooter.RightText = "Page {page} of {total-pages}";
        renderer.RenderingOptions.TextFooter.FontSize = 8;
        renderer.RenderingOptions.UseMarginsOnHeaderAndFooter = UseMargins.All;

        var pdf = renderer.RenderHtmlAsPdf(html);
        var binaryData = pdf.BinaryData;
        pdf.Dispose();

        // return a pdf file
        return new FileContentResult(binaryData, "application/pdf") { FileDownloadName = "test.pdf" };
    }
}