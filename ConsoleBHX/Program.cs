using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using Spire.Xls;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using System.Data;
using System.Xml;
using System.Globalization;
using System.ComponentModel;
using System.Reflection;
using RestSharp;

namespace ConsoleBHX
{

    class Program
    {
        private const string browserCookie = "_ga=GA1.2.24078282.1648085907; _trmccid=4f05812ba762d88a; XSRF-TOKEN=1bc1a57c-5121-4348-9247-8bede50f740f; _trmcdisabled2=-1; _trmcuser={\"id\":\"\"}; _gcl_au=1.1.28053603.1649513675; _ga2=GA1.2.24078282.1648085907; _mkto_trk=id:872-KNP-101&token:_mch-line.biz-1649513675827-91890; trd_cid=16495136780739654; trd_vid_l=2306%3A16495136780739654; trd_vuid_l=-6142126659905871673; trd_first_visit=1649513679; trd_pw=1; ses=896M92Uykgw9cGp0pM3Xl3UuLFzpUtvw3Zix5R4hHgCAha3Ji3/Y6RdTzTUxuT7aYYtAJ4iys4f5X3rqr1zCjBpga960FoHUo1YeJjrars0wJ7L1pG+DHma+kcYieW0WX5u+Mo1BcskKP+5Jw3SNBkp06f6BCejHXWsrwxmv66XxSQUyhKgFF00z4UJ+3wH+RUKInkzoLeux2DKdfC3HS+23OaAGAqOIzeyWcQ/jWVvTEKenvuVzaQPbvBVbj2nM7CUe3vE2qnshbvJyUXikwBEvUbE51/Dx33e/4Q5d5I0l6hG3XCDktZSJj+kG/1sK6zr06qrViXs2ckxN5tGYeonWh1FhbYW5z52HqPGNLPDgjoyqHu7DzcfRU/IRLWfVDmZ69vM2Z5ZFou0pyRKbwKwBv9Iq9i+1e8Ve8A8Et85F8Pd4lNLU7fa1A7JRL2yRfl62N8AcBSohnhshoI7pWdAKT4/EEAZn0M8raLLoK3A=; _gid=GA1.2.51325996.1651755350; __try__=1651755350481; _trmcsession={\"id\":\"6dd570cb5ddf6c19\",\"path\":\"/account/@444htdlx\",\"query\":\"?status=success\",\"params\":{},\"time\":1651755350487}; _gat=1; _trmcpage=/account/searchId/broadcast/create?from_copy=1; XSRF-TOKEN=42cc4126-c1ed-45a4-8c0e-468ebc8581a7";

        private const string browserXSRF = "1bc1a57c-5121-4348-9247-8bede50f740f";

        public static void Main(string[] args)
        {
            CallAPICreateBroadcastWujoBOT();
        }

        private static DataTable ParseParkData(string strData)
        {
            string strMetaTag = "<META>";
            string strTypeTag = "<TYPES>";
            string strDataTag = "<DATA>";
            string strTotalTag = "<TOTAL>";

            int indexMeta = strData.IndexOf(strMetaTag);
            int indexType = strData.IndexOf(strTypeTag);
            int indexData = strData.IndexOf(strDataTag);
            int indexTotal = strData.IndexOf(strTotalTag);

            //-8: </TOTAL> length
            int totalRow = Int32.Parse(strData.Substring(indexTotal + strTotalTag.Length, indexMeta - (indexTotal + strTotalTag.Length) - 8));

            string[] arrMeta = strData.Substring(indexMeta + strMetaTag.Length, indexType - (indexMeta + strMetaTag.Length)).Split(new char[] { '|' });
            string[] arrDatas = strData.Substring(indexData + strDataTag.Length).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            DataTable dtbData = new DataTable();
            foreach (string strMeta in arrMeta)
            {
                dtbData.Columns.Add(strMeta.Trim());
            }
            if (!dtbData.Columns.Contains("totalrow"))
            {
                dtbData.Columns.Add("totalrow", typeof(int));
            }

            if (arrDatas.Length > 1)
            {
                foreach (var strDataRow in arrDatas)
                {
                    string dataRow = strDataRow.Trim();
                    string[] arrData = dataRow.Split(new char[] { '|' });
                    dtbData.Rows.Add(arrData);
                }
                dtbData.Rows[0]["totalrow"] = totalRow;
            }

            return dtbData;
        }

        public object ConvertNumberToString(string field, object number)
        {
            if (field.ToUpper().Contains("ID") || field.ToUpper().Contains("TOTALROW"))
            {
                return number;
            }
            if (number.GetType() == typeof(DateTime))
            {
                DateTime resultDate = (DateTime)number;
                if (resultDate.TimeOfDay == new TimeSpan(12, 0, 0))
                {
                    return (object)(resultDate.ToString("dd/MM/yyyy"));
                }
                else
                {
                    return (object)(resultDate.ToString("dd/MM/yyyy HH:mm"));
                }

            }
            if (number.GetType() == typeof(double))
            {
                return (object)string.Format("{0:0,0.###}", number);
            }
            else if (number.GetType() == typeof(float))
            {
                return (object)string.Format("{0:0,0.###}", number);
            }
            else if (number.GetType() == typeof(int))
            {
                return (object)string.Format("{0:n0}", number);
            }
            else if (number.GetType() == typeof(long))
            {
                return (object)string.Format("{0:n0}", number);
            }
            else if (number.GetType() == typeof(decimal))
            {
                return (object)string.Format("{0:0,0.###}", number);
            }
            return number;
        }

        public static void SetVatByOutputtypeSpecialVat(int vatType, ref BlanketSalesOrder4KDetail item)
        {
            //1: VAT OUT => Lấy VAT theo khai báo sản phẩm MASTERDATA.PM_PRODUCT.VAT
            if (vatType == 1)
            {
                item.NOVAT = false;
                item.VAT = item.VAT; //VAT lấy lúc bắn sản phẩm (SearchSalePriceProduct)
            }
            //2: VAT IN => Lấy VAT theo khai báo sản phẩm MASTERDATA.PM_PRODUCT.VATIN
            else if (vatType == 2)
            {
                item.NOVAT = false;
                item.VAT = item.VATIN; //VATIN lấy lúc bắn sản phẩm (SearchSalePriceProduct)
            }
            //3: VAT = 0
            else if (vatType == 3)
            {
                item.NOVAT = false;
                item.VAT = 0;
            }
            //4: NOVAT không VAT
            else if (vatType == 4)
            {
                item.NOVAT = true;
                item.VAT = 0;
            }
        }

        public static string GetEnumDescription<TEnum>(TEnum value)
        {
            try
            {
                FieldInfo fi = value.GetType().GetField(value.ToString());
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.
                       GetCustomAttributes(typeof(DescriptionAttribute), false);
                if ((attributes != null) && (attributes.Length > 0))
                    return attributes[0].Description;
                else
                    return value.ToString();
            }
            catch (Exception)
            {
                return value.ToString();
            }
        }

        /// <summary>
        /// Gọi API tạo broadcast cho bot cà phê
        /// </summary>
        public static void CallAPICreateBroadcastWujoBOT()
        {
            //giây (2022/07/07)
            var secNow = 1657168800;

            for (int i = 1; i < 100; i++)
            {
                var sec = secNow + (60 * 60 * 24 * i);
                var date = new DateTime(1970, 1, 1).AddSeconds(sec).AddHours(7);
                if (date.DayOfWeek != DayOfWeek.Sunday && date.DayOfWeek != DayOfWeek.Saturday)
                {
                    Console.WriteLine($"{sec}   {date.ToString("F")}");

                    var client = new RestClient("https://manager.line.biz/api/bots/@444htdlx/broadcasts");
                    client.Timeout = -1;
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("authority", "manager.line.biz");
                    request.AddHeader("accept", "application/json, text/plain, */*");
                    request.AddHeader("content-type", "application/json;charset=UTF-8");
                    request.AddHeader("cookie", browserCookie);
                    request.AddHeader("x-xsrf-token", browserXSRF);
                    object body = new
                    {
                        status = "SCHEDULED",
                        withTimeline = false,
                        immediate = false,
                        scheduledDate = sec,
                        balloons = new List<object> {
                        new {
                            contentType = "TEXT",
                            text = "Cà phê thôi mọi người ơi (coffee)(coffee)(coffee)\n(～￣▽￣)～Order nào～(￣▽￣～)",
                            updateToken = "EhAPYOZpeQ6XlGZdOajY2wPtGqABYn2Ss8CJZaV8-jj1V9oPz7uX-   nBBQsbQpcEDhqKoOAlLczIxTxOz1CKxa6IMOYOCpdxDAbaVtUtznev8JurvSP7Azk9sUb5VZKu69nrDAPnvthpfxC6ckwaK_Ys8QKIjo91P31Sasey2ualzWG7BhuTQc4rXJw_4XuZMh1AkzRsHI8au66R8cmA6HeUfQCYtlRTXXKRVqR0o9GMIdJcE-w"
                        },
                        new {
                            contentType = "STICKER",
                            stickerPackageId = 8522,
                            packageVersion = 4,
                            stickerId = 16581276,
                            updateToken = "EhCkkBhM9xR_gj4H8Rm0icRJGnscotiCrbOKsbKlxfvZ-fuOK-wj693zvUePnuaYXcf4iXYVRqKONN2e6D6E-UXUfY-o4bqwkvcP3YzRknRc8SY3rOh6AQRs3AqbaZqz2ngrUOlFF_w25PQNo-fui_oz7RpAyOQCL_NmOtJRWfGH5q2CoZSvvJRny1LIkhc"
                        }
                    }
                    };
                    request.AddParameter("application/json;charset=UTF-8", JsonConvert.SerializeObject(body), ParameterType.RequestBody);
                    var response = client.Execute(request);
                    Console.WriteLine(response.Content);
                }
            }

            Console.ReadLine();
        }

    }

    public class BlanketSalesOrder4KDetail
    {
        public string BLANKETSALESORDERID { get; set; }
        public long BLANKETSALESORDERDETAILID { get; set; }
        public int OUTPUTTYPEID { get; set; }
        public string PRODUCTID { get; set; }
        public string PRODUCTNAME { get; set; }
        public int INVENTORYSTATUS { get; set; }
        public double QUANTITY { get; set; }
        public double ORGRINALQUANTITY { get; set; }

        public int QUANTITYUNITID { get; set; }
        public string QUANTITYUNIT { get; set; }
        public double? STANDAPRICE { get; set; }
        public int VAT { get; set; }
        public bool NOVAT { get; set; }
        public double? ADJUSMENTVALUE { get; set; }
        public string ADJUSMENTUSER { get; set; }
        public DateTime? ADJUSMENTDATE { get; set; }
        public string ADJUSMENTCONTENT { get; set; }
        public double? SALEPRICE { get; set; }
        /// <summary>
        /// Quy cách chia
        /// </summary>
        public int? PACKINGQUANTITY { get; set; }
        /// <summary>
        /// Thành tiền sản phẩm
        /// </summary>
        public double? TOTALAMOUNT { get; set; }
        public bool ISALLOWDECIMAL { get; set; }
        /// <summary>
        ///  Giá thay đổi
        /// </summary>
        public double? SALEPRICECHANGE { get; set; }
        public bool ISCHECKCHANGEPRICE { get; set; }
        public double? PRESALEPRICE { get; set; }
        public int VATIN { get; set; }
        public int SUBGROUPID { get; set; }
        /// <summary>
        /// Cờ đã lấy VAT
        /// </summary>
        public bool FLAGGOTVAT { get; set; }
        /// <summary>
        /// Giá đối tác
        /// </summary>
        public double? PARTNERSALEPRICE { get; set; }
    }

    /// <summary>
    /// Hình thức xuất cho loại vat không kê khai nộp thuế
    /// </summary>
    public enum OUTPUTTYPE_VATTYPE5
    {
        [Description("Xuất bán nội bộ")]
        XUATBANNOIBO = 17,
        [Description("Xuất bán hàng hóa cho DN _ 4K")]
        XUATBANHANGHOADOANHNGHIEP4K = 2759
    }

    
}
