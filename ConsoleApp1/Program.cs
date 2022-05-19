using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {

        static void Main(string[] args)
        {
            string email = "thhcs10@a.com";

            if (IsValidEmail(email))
            {
                Console.WriteLine("OK");
            }
        }

        public static bool IsValidEmail(string email)
        {
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(email);
            if (match.Success)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Hàm viết text vào hình
        /// </summary>
        /// <param name="text"></param>
        /// <param name="currentImageFilePath"></param>
        /// <returns></returns>
        public static string WriteTextToImage(string text, string base64Image, string fontFamily = "Arial", int fontSize = 30)
        {
            string physicalPath = @"C:\webs\pos.bachhoaxanh.com\UI\Images";

            byte[] file = Convert.FromBase64String(base64Image);

            Stream stream = new MemoryStream(file);

            Bitmap bitmap = (Bitmap)Image.FromStream(stream);//load the image stream

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                using (Font arialFont = new Font(fontFamily, fontSize))
                {
                    graphics.DrawString(text, arialFont, Brushes.Red, new PointF(10f, 10f));
                }
            }

            string newFileName = $"{Guid.NewGuid().ToString()}.jpg";

            string subPath = Path.Combine(physicalPath, "ImageDrawText");
            if (!Directory.Exists(subPath))
            {
                Directory.CreateDirectory(subPath);
            }

            string newFilePath = Path.Combine(subPath, newFileName);

            bitmap.Save(newFilePath);//save the image file

            stream.Close();
            //Trả về đường dẫn đến file đến file
            return Path.Combine("ImageDrawText", newFileName);
        }

    }



    /// <summary>
    /// Khai báo điều chỉnh rộng theo nhóm hàng
    /// </summary>
    public class ShowSubGroupWidthConfigBO
    {
        /// <summary>
        /// Nhóm hàng
        /// </summary>
        public int SUBGROUPID { get; set; }
        public int SUBGROUPPLUS => SUBGROUPID + 100;
        /// <summary>
        /// Tỷ lệ điều chỉnh rộng
        /// </summary>
        public double RATIOCONFIG { get; set; }
        /// <summary>
        /// Mã sản phẩm đích danh
        /// </summary>
        public string PRODUCTID { get; set; }
    }
}
