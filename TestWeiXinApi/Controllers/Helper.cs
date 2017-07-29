using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace TestWeiXinApi.Controllers
{
    public static class Helper
    {
        public static Dictionary<string, string> InstanceToDic(this object o)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            Type t = o.GetType();
            if (!t.IsClass) return dic;
            PropertyInfo[] pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in pi)
            {
                if (p != null && p.Name.ToLower() != "seccode")
                {
                    string value = Convert.ToString(p.GetValue(o, null));
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        dic.Add(p.Name, value);
                    }
                }
            }
            return dic;
        }
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long GetUnixTimestamp(this DateTime dt)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long intResult = (long)(dt - startTime).TotalSeconds;
            return intResult;
        }
        public static string GetSecCodeOrderBy(this Dictionary<string, string> dic)
        {
            string url = string.Join("&", dic.Select(p => p.Key + '=' + p.Value).ToArray().OrderBy(x => x));
            return url.ToMD52().ToLower();
        }
        /// <summary>
        /// 输出MD5加密串
        /// </summary>
        /// <param name="s">当前字符串对象</param>
        /// <returns></returns>       
        public static string ToMD52(this string s)
        {
            var md5Hasher = new MD5CryptoServiceProvider();
            byte[] hash = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(s));

            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
    public class UnionRequest
    {
        public int AgentId { get; set; }
        public string UserName { get; set; }
        public long Timestamp { get; set; }
        public long ExpireTime { get; set; }
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string UniqueCode { get; set; }
        public string SecCode { get; set; }
    }
}