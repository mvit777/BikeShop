using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeShopWS.Controllers
{
    public class ExampleController : Controller
    {
        // GET: ExampleController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ExampleController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ExampleController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ExampleController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ExampleController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ExampleController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ExampleController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ExampleController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
