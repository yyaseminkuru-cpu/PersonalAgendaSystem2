using System;
using System.Collections.Generic;
using System.Linq;
using System.Net; // HttpStatusCode için
using System.Web;
using System.Web.Mvc;
using PersonalAgendaSystem.Models;


namespace PersonalAgendaSystem.Controllers
{
    public class AgendaController : Controller
    {
        // Entity Framework bağlantısı
        AgendaModelContainer db = new AgendaModelContainer();

        // Kullanıcı login olduktan sonra ilk buraya gelecek
        public ActionResult Index(string priority, string status, DateTime? filterDate)


        {
            // Eğer kullanıcı giriş yapmadıysa
            // tekrar login sayfasına yönlendir
            if (Session["UserID"] == null) //kullanıcı giriş yapmamışsa erişemesin diye
            {
                return RedirectToAction("Login", "Home");
            }

            // Session içindeki kullanıcı ID'sini alıyoruz
            int userId = Convert.ToInt32(Session["UserID"]);

            // Giriş yapan kullanıcının rolünü alıyoruz
            string role = Session["Role"].ToString();

            // Listeyi tutacak değişken
            List<AgendaItems> agendaList;

            // Eğer kullanıcı Admin ise
            if (role == "Admin")
            {
                // Tüm ajanda kayıtlarını getir
                agendaList = db.AgendaItems
                               .Where(x => x.IsActive == true) // soft delete yapılmış kayıtları göstermemesi için
                               .ToList();
            }
            else
            {
                // Normal kullanıcıysa
                // sadece kendi kayıtlarını getir
                agendaList = db.AgendaItems
                               .Where(x =>
                                   x.Users.UserID == userId &&
                                   x.IsActive == true)
                               .ToList();
            }

            if (!string.IsNullOrEmpty(priority)) // Eğer priority parametresi boş değilse
            {
                agendaList = agendaList // Önceki sorgudan dönen liste üzerinde filtreleme yapıyoruz
                             .Where(x => x.Priority == priority) // Önceliği eşit olan kayıtları alıyoruz
                             .ToList(); // Sonuçları listeye atıyoruz
            }
            // Benzer şekilde status parametresi için de filtreleme yapıyoruz
            if (!string.IsNullOrEmpty(status))
            {
                agendaList = agendaList
                             .Where(x => x.Status == status)
                             .ToList();
            }
            // Eğer filterDate parametresi null değilse, yani bir tarih seçilmişse
            if (filterDate.HasValue)
            {
                agendaList = agendaList
                             .Where(x => x.StartDate.Date <= filterDate.Value.Date &&
            x.EndDate.Date >= filterDate.Value.Date)

                             .ToList();
            }


            ViewBag.Priority = priority;
            ViewBag.Status = status;
            ViewBag.FilterDate = filterDate;

            agendaList = agendaList
             .OrderBy(x => x.StartDate)
             .ToList();

            ViewBag.TaskTitles = agendaList
                                 .Select(x => x.Title) //her ajanda kaydında sadece title alanını seçer
                                 .ToList(); 

            // Listeyi View'a gönderiyoruz
            return View(agendaList);

        }

        // CREATE - GET
        [HttpGet]
        public ActionResult Create()
        {

            ViewBag.Categories = new SelectList(db.Categories.Where(x => x.IsActive == true).ToList(), "CategoryID", "CategoryName");
            return View();
        }

        // CREATE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AgendaItems item, int categoryId)
        {
            if (Session["UserID"] == null)  // kullanıcı giriş yapmamışsa erişemesin diye
            {
                return RedirectToAction("Login", "Home");
            }

            if (ModelState.IsValid)
            {
                int userId = Convert.ToInt32(Session["UserID"]);

                Users user = db.Users.Find(userId);
                Categories category = db.Categories.Find(categoryId);

                item.Users = user;
                item.Categories = category;

                item.IsApproved = false;
                item.IsActive = true;
                item.CreatedDate = DateTime.Now;

                db.AgendaItems.Add(item);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.Categories = new SelectList(db.Categories.Where(x => x.IsActive == true).ToList(), "CategoryID", "CategoryName");

            return View(item);

        }
        // DETAILS
        public ActionResult Details(int? id) // id null olabilir diye nullable yaptık
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AgendaItems item = db.AgendaItems.Find(id); // id'ye göre kaydı buluyoruz

            if (item == null)
            {
                return HttpNotFound();
            }

            return View(item); // bulduğumuz kaydı View'a gönderiyoruz
        }
        // EDIT - GET
        [HttpGet]
        public ActionResult Edit(int? id) // id null olabilir diye nullable yaptık
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AgendaItems item = db.AgendaItems.Find(id);

            if (item == null)
            {
                return HttpNotFound();
            }

            if (item.IsApproved == true)
            {
                return RedirectToAction("Details", new { id = item.AgendaItemId });
            }

            ViewBag.Categories = new SelectList(db.Categories.Where(x => x.IsActive == true).ToList(), "CategoryID", "CategoryName"); // Kategori dropdown'ı için

            return View(item);
        }

        // EDIT - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AgendaItems item, int categoryId)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            if (ModelState.IsValid)
            {
                AgendaItems updatedItem = db.AgendaItems.Find(item.AgendaItemId);

                if (updatedItem == null)
                {
                    return HttpNotFound();
                }

                if (updatedItem.IsApproved == true)
                {
                    return RedirectToAction("Details", new { id = updatedItem.AgendaItemId });
                }

                Categories category = db.Categories.Find(categoryId);

                updatedItem.Title = item.Title;
                updatedItem.Description = item.Description;
                updatedItem.Categories = category;
                updatedItem.StartDate = item.StartDate;
                updatedItem.EndDate = item.EndDate;
                updatedItem.Priority = item.Priority;
                updatedItem.Status = item.Status;

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.Categories = new SelectList(db.Categories.Where(x => x.IsActive == true).ToList(), "CategoryID", "CategoryName");

            return View(item);
        }
        // DELETE - GET
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AgendaItems item = db.AgendaItems.Find(id);

            if (item == null)
            {
                return HttpNotFound();
            }

            return View(item);
        }
        // DELETE - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int AgendaItemId)
        {
            AgendaItems item = db.AgendaItems.Find(AgendaItemId);

            if (item == null)
            {
                return HttpNotFound();
            }

            item.IsActive = false;

            db.SaveChanges(); // değişiklikleri veri tabanına kayıt et

            return RedirectToAction("Index");
        }


        // COMPLETE - GET


        [HttpGet]
        public ActionResult Complete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AgendaItems item = db.AgendaItems.Find(id);

            if (item == null)
            {
                return HttpNotFound();
            }

            return View(item);
        }

        // COMPLETE - POST
        [HttpPost, ActionName("Complete")]
        [ValidateAntiForgeryToken]
        public ActionResult CompleteConfirmed(int AgendaItemId)
        {
            AgendaItems item = db.AgendaItems.Find(AgendaItemId);

            if (item == null)
            {
                return HttpNotFound();
            }

            item.Status = "Tamamlandı";
            item.IsApproved = false;

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // APPROVE - GET
        [HttpGet]
        public ActionResult Approve(int? id)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin") // Admin olmayan kullanıcıların bu sayfaya erişmesini engellemek için
            {
                return RedirectToAction("Index");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AgendaItems item = db.AgendaItems.Find(id);

            if (item == null)
            {
                return HttpNotFound();
            }

            if (item.Status != "Tamamlandı") // Sadece tamamlanmış kayıtların onaylanabilmesi için
            {
                return RedirectToAction("Index");
            }

            return View(item); // Onaylama sayfasına kaydı gönderiyoruz
        }
        // APPROVE - POST
        [HttpPost, ActionName("Approve")]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveConfirmed(int AgendaItemId)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin") // Admin olmayan kullanıcıların bu işlemi yapmasını engellemek için
            {
                return RedirectToAction("Index");
            }

            AgendaItems item = db.AgendaItems.Find(AgendaItemId);

            if (item == null)
            {
                return HttpNotFound();
            }

            if (item.Status == "Tamamlandı") // Sadece tamamlanmış kayıtların onaylanabilmesi için
            {
                item.Status = "Onaylandı";// admin onahy işlemi
                item.IsApproved = true; // Onaylandı olarak işaretliyoruz

                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
        // CALENDAR
        public ActionResult Calendar()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            DateTime today = DateTime.Today;

            int year = today.Year;
            int month = today.Month;
            int daysInMonth = DateTime.DaysInMonth(year, month);

            DateTime firstDay = new DateTime(year, month, 1);
            int startDay = (int)firstDay.DayOfWeek;
            int startOffset = startDay == 0 ? 6 : startDay - 1;

            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.MonthName = today.ToString("MMMM"); // Ay adını göstermek için

            ViewBag.DaysInMonth = daysInMonth;
            ViewBag.StartOffset = startOffset;

            int userId = Convert.ToInt32(Session["UserID"]);
            string role = Session["Role"].ToString();

            List<AgendaItems> calendarItems;

            if (role == "Admin")
            {
                calendarItems = db.AgendaItems
                                  .Where(x => x.IsActive == true)
                                  .ToList();
            }
            else
            {
                calendarItems = db.AgendaItems
                                  .Where(x => x.Users.UserID == userId && x.IsActive == true)
                                  .ToList();
            }

            return View(calendarItems);


        
        }



    }

}
