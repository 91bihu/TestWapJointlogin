using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace TestWeiXinApi.Controllers
{
    public class HttpClientHelper
    {
        private static string UrlUpDataStr = "http://wx.91bihu.com/api/Unite/Login";
        public static Tuple<bool, string, int> PostUrlGetToken(Dictionary<string, string> dt)
        {
            var response = PostXRequest(UrlUpDataStr, dt);
            return response;
        }
        /// <summary>
        /// 参数拼装并加密
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        private static string ToQueryString(Dictionary<string, string> dict)
        {
            var tmp = dict.Where(x => !string.IsNullOrWhiteSpace(x.Value)).OrderBy(y => y.Key);
            StringBuilder data = new StringBuilder();
            string result = string.Join("&", tmp.Select(p => p.Key + '=' + p.Value.Trim()).ToArray());

            return '?' + result + "&SecCode=" + ToMD5(result).ToLower();
        }
        /// <summary>
        /// md5加密下
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string ToMD5(string s)
        {
            var md5Hasher = new MD5CryptoServiceProvider();
            byte[] hash = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(s));

            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
        /// <summary>
        /// 使用HttpClient进行post请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Tuple<bool, string, int> PostXRequest<T>(string url, T entity)
        {
            int httpCode = 200;
            string body = JsonConvert.SerializeObject(entity);
            StringBuilder sb = new StringBuilder();
            try
            {

                using (XHttpClient client = new XHttpClient(new XHttpClientHandler()))
                {
                    HttpContent content = new StringContent(body);
                    MediaTypeHeaderValue typeHeader = new MediaTypeHeaderValue("application/json");
                    typeHeader.CharSet = "UTF-8";
                    content.Headers.ContentType = typeHeader;

                    var res = client.PostAsync(url, content, new CancellationTokenSource().Token).Result;
                    string queryString = client.BaseAddress.AbsoluteUri + url;
                    if (url.IndexOf(client.BaseAddress.AbsoluteUri) > 0)
                    {
                        queryString = url;
                    }

                    if (res.IsSuccessStatusCode)
                    {
                        string result = res.Content.ReadAsStringAsync().Result;
                        if (res.StatusCode == HttpStatusCode.OK)
                        {
                            return Tuple.Create(true, result, (int)res.StatusCode);
                        }

                    }
                    return Tuple.Create(false, "", (int)res.StatusCode);
                }
            }
            catch (Exception ex)
            {
                sb.AppendFormat("requestUrl:{0}{1}", url, Environment.NewLine);
                sb.AppendFormat("requestBody:{0}{1}", body, Environment.NewLine);
                sb.AppendFormat("responseContent:{0}{1}", ex.Message, Environment.NewLine);
                if (ex is TaskCanceledException)
                {
                    httpCode = 408;
                }
                else
                {
                    httpCode = 500;
                }
            }
            return Tuple.Create(false, sb.ToString(), httpCode);
        }

        private static Tuple<bool, string, int> GetXResponse(string url)
        {
            int httpCode = 200;
            Action<string> errorLog = (content) =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("requestUrl:{0}{1}", url, Environment.NewLine);
                sb.AppendFormat("responseContent:{0}{1}", content, Environment.NewLine);
            };

            try
            {
                using (XHttpClient client = new XHttpClient(new XHttpClientHandler()))
                {
                    var res = client.GetAsync(url, HttpCompletionOption.ResponseContentRead, new CancellationTokenSource().Token).Result;

                    string queryString = client.BaseAddress.AbsoluteUri + url;
                    if (url.IndexOf(client.BaseAddress.AbsoluteUri) > 0)
                    {
                        queryString = url;
                    }

                    if (res.IsSuccessStatusCode)
                    {
                        string result = res.Content.ReadAsStringAsync().Result;
                        if (res.StatusCode == HttpStatusCode.OK)
                        {
                            return Tuple.Create(true, result, (int)res.StatusCode);
                        }
                        else
                        {
                            if (url.IndexOf("CarInsurance/PostPrecisePrice") > 1)
                            {
                                JObject job = JObject.Parse(result);

                                var errCode = job["BusinessStatus"].ToString();
                                int code = Convert.ToInt32(errCode);
                                if (code == -10001 || code == -10000 || code == -10006)
                                {
                                    return Tuple.Create(true, result, (int)res.StatusCode);
                                }
                            }

                        }

                    }
                    else
                    {
                        errorLog(res.ToString());

                    }
                    return Tuple.Create(false, "", (int)res.StatusCode);
                }
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException)
                {
                    httpCode = 408;
                }
                else
                {

                    httpCode = 500;
                }
                errorLog(ex.Message);
            }
            return Tuple.Create(false, "", httpCode);
        }
    }

    internal class XHttpClientHandler : HttpClientHandler
    {

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            if (request.Method == HttpMethod.Post && !request.RequestUri.AbsoluteUri.Contains("UploadMultipleImg"))
            {
                var body = request.Content.ReadAsStringAsync().Result;
            }

            MediaTypeHeaderValue typeHeader = new MediaTypeHeaderValue("application/json");
            typeHeader.CharSet = "UTF-8";

            AllowAutoRedirect = false;
            AutomaticDecompression = DecompressionMethods.GZip;

            return base.SendAsync(request, cancellationToken);
        }
    }
    public sealed class XHttpClient : HttpClient
    {
        private static string baseUrl = "http://wx.91bihu.com/";
        public XHttpClient() : base() { }

        public XHttpClient(HttpClientHandler handler)
            : base(handler)
        {
            Timeout = TimeSpan.FromMilliseconds(1000 * 60 * 3);
            BaseAddress = new Uri(baseUrl);
        }

        public XHttpClient(HttpMessageHandler handler, bool disposeHandler) : base(handler, disposeHandler) { }

    }
}
