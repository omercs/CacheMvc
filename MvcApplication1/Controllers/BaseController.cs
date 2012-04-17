using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;

namespace MvcApplication1.Controllers
{
    public class BaseController : Controller
    {
        protected static string ConnectionStr = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;

        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    }
}
