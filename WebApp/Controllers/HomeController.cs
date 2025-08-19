using Microsoft.AspNetCore.Mvc;
using OptionsLocalization;
using System.Diagnostics;

namespace WebApp.Controllers
{
    public class HomeController
    {
        /// <summary>
        /// 本地化工具示例
        /// </summary>
        /// <param name="appOptions">当前线程语言区域对应的 AppOptions</param>
        /// <param name="homeOptions">当前线程语言区域对应的 HomeOptions</param>
        /// <param name="localizer"></param>
        /// <returns></returns>
        [HttpGet("/")]
        public HomeOptions Index(
            [FromServices] AppOptions appOptions,
            [FromServices] HomeOptions homeOptions,
            [FromServices] IOptionsLocalizer<HomeOptions> localizer)
        {
            Debug.Assert(homeOptions == localizer.CurrentValue);
            return homeOptions;
        }
    }
}
