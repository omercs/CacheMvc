using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication1.Models;

namespace MvcApplication1.Controllers
{ 
    public class MarketEventController : BaseController
    {
        private MarketContext db = new MarketContext(ConnectionStr);

        //
        // GET: /MarketEvent/

        public ViewResult Index()
        {
            var marketevents = db.MarketEvents.Include(m => m.Company);
            return View(marketevents.ToList());
        }

        //
        // GET: /MarketEvent/Details/5

        public ViewResult Details(int id)
        {
            MarketEvent marketevent = db.MarketEvents.Find(id);
            return View(marketevent);
        }

        //
        // GET: /MarketEvent/Create

        public ActionResult Create()
        {
            ViewBag.CompanyId = new SelectList(db.Companys, "CompanyId", "Name");
            return View();
        } 

        //
        // POST: /MarketEvent/Create

        [HttpPost]
        public ActionResult Create(MarketEvent marketevent)
        {
            if (ModelState.IsValid)
            {
                marketevent.RowGuid = Guid.NewGuid();

                db.MarketEvents.Add(marketevent);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.CompanyId = new SelectList(db.Companys, "CompanyId", "Name", marketevent.CompanyId);
            return View(marketevent);
        }
        
        //
        // GET: /MarketEvent/Edit/5
 
        public ActionResult Edit(int id)
        {
            MarketEvent marketevent = db.MarketEvents.Find(id);
            ViewBag.CompanyId = new SelectList(db.Companys, "CompanyId", "Name", marketevent.CompanyId);
            return View(marketevent);
        }

        //
        // POST: /MarketEvent/Edit/5

        [HttpPost]
        public ActionResult Edit(MarketEvent marketevent)
        {
            if (ModelState.IsValid)
            {
                var edititem =
                    db.MarketEvents.Where(a => a.MarketEventId == marketevent.MarketEventId).SingleOrDefault();

                if (edititem != null)
                {
                    TryUpdateModel(edititem, null, null, new[] { "RowGuid" });
                    
                    db.SaveChanges();
                    return RedirectToAction("Index");

                }
            }
            ViewBag.CompanyId = new SelectList(db.Companys, "CompanyId", "Name", marketevent.CompanyId);
            return View(marketevent);
        }

        //
        // GET: /MarketEvent/Delete/5
 
        public ActionResult Delete(int id)
        {
            MarketEvent marketevent = db.MarketEvents.Find(id);
            return View(marketevent);
        }

        //
        // POST: /MarketEvent/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            MarketEvent marketevent = db.MarketEvents.Find(id);
            db.MarketEvents.Remove(marketevent);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}