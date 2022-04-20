using System;
using System.IO;
using System.Threading.Tasks;
using PuppeteerSharp;
using System.Net;
using System.Reflection;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace RainstormTech.Puppeteer_Pdf
{
    public class GeneratePdf
    {
        /// <summary>
        /// Display the assembly version of the functions
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("version")]
        public IActionResult Version([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req, ILogger log)
        {
            return new OkObjectResult(Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        /// <summary>
        /// Create a PDF from a given URL
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("GeneratePdf")]
        public async Task<IActionResult> GenerateThePdf([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            // we need a url at the very least
            string url = req.Query["url"];

            if (string.IsNullOrEmpty(url))
            {
                return new BadRequestObjectResult("url not given");
            }

            // make sure we have a full url
            url = url.ToWebsite();
            int width = req.Query["w"].ToString().ToInt(1024);
            int margin = req.Query["m"].ToString().ToInt(0);
            string name = req.Query["n"].ToString().Clean();
            bool printBackground = req.Query["pb"].ToString().ToBool(true);
            
            if (string.IsNullOrEmpty(name))
                name = $"{DateTime.UtcNow:yyyy-MM-dd-hh-mm-ss}.pdf";
            if (!name.EndsWith(".pdf"))
                name = $"{name}.pdf";

            try
            {
                // create a browserfetcher object which will handle the downloading of the chrome browser image
                using var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions()
                {
                    // for azure functions, we need to use the temp path so we don't get a permission issue
                    Path = Path.GetTempPath()
                }) ;
                // download the browser image
                await browserFetcher.DownloadAsync();

                // launch the browser in headless mode from the temp dir we downloaded the image to
                await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { 
                    Headless = true,
                    ExecutablePath = browserFetcher.RevisionInfo(BrowserFetcher.DefaultChromiumRevision).ExecutablePath
                });

                // create a new page
                await using var page = await browser.NewPageAsync();
                await page.GoToAsync(url, WaitUntilNavigation.Networkidle0); // In case of fonts being loaded from a CDN, use WaitUntilNavigation.Networkidle0 as a second param.

                // change the viewport to the width of your choosing
                await page.SetViewportAsync(new ViewPortOptions
                {
                    DeviceScaleFactor = 1,
                    Width = width,
                    Height = 1080
                });

                // dimensions = await page.EvaluateExpressionAsync<string>(jsWidth);
                await page.EvaluateExpressionHandleAsync("document.fonts.ready"); // Wait for fonts to be loaded. Omitting this might result in no text rendered in pdf.

                // use the screen mode for viewing the web page
                await page.EmulateMediaTypeAsync(PuppeteerSharp.Media.MediaType.Screen);

                // define some options
                var options = new PdfOptions()
                {
                    Width = width,
                    Height = 1080,
                    Format = PuppeteerSharp.Media.PaperFormat.Letter,
                    DisplayHeaderFooter = false,
                    PrintBackground = printBackground
                };

                // throws an error if margin is less than 10
                if (margin >= 10)
                {
                    options.MarginOptions = new PuppeteerSharp.Media.MarginOptions()
                    {
                        Top = $"{margin}",
                        Bottom = $"{margin}",
                        Left = $"{margin}",
                        Right = $"{margin}"
                    };
                }

                // get the bytes of the pdf
                var pdfData = await page.PdfDataAsync(options);

                // write out the pdf
                return new FileContentResult(pdfData, "application/pdf")
                {
                    FileDownloadName = name
                };
            }
            catch(Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }            
        }
    }
}
