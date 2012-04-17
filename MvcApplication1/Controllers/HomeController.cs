using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication1.Models;
using NLog;
using MvcApplication1.Utils;
namespace MvcApplication1.Controllers
{
    public class HomeController : BaseController
    {

        

        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            int activecompanyid = 1;

            //this will load from cache and use it until we need to update the cache
            ViewBag.Mailcodes = GetCacheMarketCodes(activecompanyid);
            return View();
        }



        public ActionResult About()
        {
            return View();
        }

        
       

        public const string RefreshKeyword = "refreshed";

        /// <summary>
        /// we will cache market codes
        /// cache will be updated if something changes from website or db.
        /// Db triggers handle changes and record in a simple table. We check that 
        /// every time,so we can skip expensive queries and only check audit tables to refresh cache.
        /// </summary>
        /// <returns></returns>
        private string[] GetCacheMarketCodes(int companyid)
        {
            //keep update flag for mailcodes. we will update this when some change happens at db
            string updateflag = (string)HttpContext.Cache["MailCodesUpdate" + companyid];
            bool needsupdate = CheckTriggerRecordsForUpdate("MarketEvents", updateflag);
            
            Logger.Trace("Cache check:" +
                               HttpContext.Cache["MailCodesUpdate" + companyid]);
            if (needsupdate ||
                HttpContext.Cache["MailCodes" + companyid] == null)
            {

                using (var db = new MarketContext(ConnectionStr))
                {


                    //make call to db to pget items
                    var listitems =
                        (from x in db.MarketEvents orderby x.Campaign select x.Campaign).Distinct().OrderBy(a => a).
                            ToArray();

                    //Add item to cache with expiration time, sliding, priority and callback if needed
                    HttpContext.Cache["MailCodes" + companyid] = listitems;

                    //log this for tracing
                    Logger.Trace("Cache will update for MailCodes:" +
                                 HttpContext.Cache["MailCodesUpdate" + companyid]);

                    //set as refreshed to track when we refreshed this
                    HttpContext.Cache["MailCodesUpdate" + companyid] = RefreshKeyword + DateTime.UtcNow.ToString();

                }
            }

            return HttpContext.Cache["MailCodes" + companyid] as string[];
        }



        private bool CheckTriggerRecordsForUpdate(string tablename, string updateflag)
        {
            if (updateflag.HasSomething() && updateflag.Contains(RefreshKeyword))
            {
                using (var db = new MarketContext(ConnectionStr))
                {
                    var entry = db.ProcGetCacheUpdateTime(tablename).FirstOrDefault();
                    //now check if we have something in cache records
                    if (entry != null)
                    {
                        DateTime lastupdate = DateTime.Parse(updateflag.Replace(RefreshKeyword, ""));
                        //compare
                        if (lastupdate >= entry.UpdateTime)
                        {
                            //dont need update. our cache is better than what triggered in db.
                            return false;
                        }
                    }
                    else
                    {
                        return false; //no need to refresh that. we dont have any entry in cache table
                    }
                }
            }

            return true;
        }
    }
}
