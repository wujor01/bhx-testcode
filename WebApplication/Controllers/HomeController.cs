using RestSharp;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //using (var client = new WebClient())
            //{
            //    client.Headers.Add("AuthenKey", "BCNB@!2018@a03");
            //    client.Headers.Add("UserLogin", "45123");
            //    
            //    var by = client.DownloadData(string.Format("http://betaapiinternal.thegioididong.com/api/storebusinessinfodetail/downloadfile?filedisplay={0}&fileattach={1}", Uri.EscapeDataString//("string.docx"), Uri.EscapeDataString("2022/04/dd748f0d-aba0-4055-a662-f6cb4bf2c17d.docx")));
            //
            //    return File(by, "dd748f0d-aba0-4055-a662-f6cb4bf2c17d.png");
            //}

            string html = System.IO.File.ReadAllText(@"D:\work\repos\templatea8.html");
            var file = ConvertHTMLToPDF(html, PdfPageSize.A7, PdfPageOrientation.Landscape, 10,10,0,10);
            return File(file, "test.pdf");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        /// <summary>
        /// 142199-HuyHoang Hàm convert html sang pdf
        /// </summary>
        /// <param name="html"></param>
        /// <param name="pageSizeType">Loại giấy in: A4,A5,...</param>
        /// <param name="pdfPageOrientation">In dọc hay ngang: Portrait, Landscape</param>
        /// <param name="marginTop"></param>
        /// <param name="marginRight"></param>
        /// <param name="marginBot"></param>
        /// <param name="marginLeft"></param>
        /// <param name="pageWidth">793px tương đương với A4</param>
        /// <param name="pageHeigth"></param>
        /// <returns></returns>
        public Byte[] ConvertHTMLToPDF(string html, PdfPageSize pageSizeType = PdfPageSize.A4, PdfPageOrientation pdfPageOrientation = PdfPageOrientation.Portrait, int marginTop = 20, int marginRight = 20, int marginBot = 20, int marginLeft = 20, int pageWidth = 793, int pageHeigth = 0, bool isOnePage = false)
        {
            try
            {
                var converter = new HtmlToPdf();
                converter.Options.PdfPageSize = pageSizeType;
                converter.Options.PdfPageOrientation = pdfPageOrientation;

                converter.Options.WebPageWidth = pageWidth;
                converter.Options.WebPageHeight = pageHeigth;
                converter.Options.WebPageFixedSize = false;

                converter.Options.AutoFitWidth = HtmlToPdfPageFitMode.ShrinkOnly;
                converter.Options.AutoFitHeight = HtmlToPdfPageFitMode.NoAdjustment;

                converter.Options.MarginTop = marginTop;
                converter.Options.MarginRight = marginRight;
                converter.Options.MarginBottom = marginBot;
                converter.Options.MarginLeft = marginLeft;

                var pdf = converter.ConvertHtmlString(html);

                //Nếu chỉ in 1 trang thì loại bỏ các trang còn lại
                if (pdf.Pages.Count > 1 && isOnePage)
                {
                    for (int i = 1; i < pdf.Pages.Count; i++)
                    {
                        pdf.RemovePageAt(i);
                    }
                }

                return pdf.Save();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Byte[] ConvertHTMLToImage(string html)
        {
            try
            {
                var converter = new HtmlToImage();
                var img = converter.ConvertHtmlString(html);
                using (var ms = new MemoryStream())
                {
                    img.Save(ms, img.RawFormat);
                    return ms.ToArray();
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}