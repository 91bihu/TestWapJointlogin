using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace TestWeiXinApi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetUrl(GetJsonData thisModel)
        {
            if (!ModelState.IsValid)
            {
                return Json("数据不完整", JsonRequestBehavior.AllowGet);
            }
            UnionRequest urModel = new UnionRequest();

            urModel.AgentId = thisModel.AgentId;
            urModel.ExpireTime = thisModel.ExpireTime.GetUnixTimestamp();
            urModel.Timestamp = DateTime.Now.GetUnixTimestamp();
            urModel.UserName = thisModel.UserName;
            urModel.UniqueCode = thisModel.UniqueCode;
            urModel.AppId = thisModel.AppId;
            urModel.AppSecret = thisModel.AppSecret;
            Dictionary<string, string> dic = urModel.InstanceToDic();
            dic.Add("SecretKey", thisModel.SecretKey);
            urModel.SecCode = dic.GetSecCodeOrderBy();

            //这里可以修改在页面上展示一下传值：return Json(SecCode, JsonRequestBehavior.AllowGet);

            Dictionary<string, string> dt = new Dictionary<string, string>();

            dt.Add("AgentId", urModel.AgentId.ToString());
            dt.Add("UserName", urModel.UserName);
            dt.Add("Timestamp", urModel.Timestamp.ToString());
            dt.Add("UniqueCode", urModel.UniqueCode);
            dt.Add("AppId", urModel.AppId);
            dt.Add("AppSecret", urModel.AppSecret);
            dt.Add("ExpireTime", urModel.ExpireTime.ToString());
            dt.Add("SecCode", urModel.SecCode);
            var response = HttpClientHelper.PostUrlGetToken(dt);
            if (response.Item1)
            {
                return Json(response.Item2, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(response, JsonRequestBehavior.AllowGet);
            }

        }
    }

    public class GetJsonData
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public int AgentId { get; set; }
        [Required]
        public DateTime ExpireTime { get; set; }

        public string UniqueCode { get; set; }
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        [Required]
        public string SecretKey { get; set; }
    }
}