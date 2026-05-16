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

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            base.OnActionExecuting(filterContext);
        }

        private bool IsLoggedIn()
        {
            return Session["UserID"] != null;
        }

        private bool IsAdmin() // Kullanıcının admin olup olmadığını kontrol eden yardımcı metot
        {
            return Session["Role"] != null && Session["Role"].ToString() == "Admin";
        }

        private int CurrentUserId()
        {
            return Convert.ToInt32(Session["UserID"]);
        }

        private bool CanAccessAgendaItem(AgendaItems item)
        //Bu metod kullanıcının bir göreve erişip erişemeyeceğini kontrol ediyor.
        //Admin tüm aktif görevlere erişebilir.
        //Normal kullanıcı ise sadece kendi UserID’sine bağlı görevlere erişebilir.

        {
            if (item == null || item.IsActive == false)
            {
                return false;
            }

            if (IsAdmin())
            {
                return true;
            }

            return item.Users != null && item.Users.UserID == CurrentUserId();
        }

        private void UpdateExpiredAgendaItems()
        // Bu metod, süresi geçmiş görevlerin durumunu "Süresi Geçti" olarak günceller.
        {
            DateTime now = DateTime.Now;

            var expiredItems = db.AgendaItems
                                 .Where(x =>
                                     x.IsActive == true &&
                                     x.EndDate < now &&
                                     x.Status != "Tamamlandı" &&
                                     x.Status != "Onaylandı" &&
                                     x.Status != "İptal Edildi" &&
                                     x.Status != "Süresi Geçti")
                                 .ToList();

            foreach (var item in expiredItems)
            {
                item.Status = "Süresi Geçti";
            }

            if (expiredItems.Count > 0)
            {
                db.SaveChanges();
            }
        }

        private ActionResult RedirectIfNotLoggedIn() //Kullanıcı giriş yapmamışsa Login sayfasına yönlendiriyoruz. Böylece görev ekranlarına direkt URL ile girilemez.
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Home");
            }

            return null;
        }

        private void ValidateAgendaItem(AgendaItems item, int? categoryId)
        // Bu metod, görev oluşturulurken veya düzenlenirken girilen bilgilerin doğruluğunu kontrol eder.
        {
            if (string.IsNullOrWhiteSpace(item.Title))
            {
                ModelState.AddModelError("Title", "Başlık boş bırakılamaz.");
            }

            if (string.IsNullOrWhiteSpace(item.Description))
            {
                ModelState.AddModelError("Description", "Açıklama boş bırakılamaz.");
            }

            if (!categoryId.HasValue)
            {
                ModelState.AddModelError("categoryId", "Kategori seçiniz.");
            }

            if (string.IsNullOrWhiteSpace(item.Priority))
            {
                ModelState.AddModelError("Priority", "Öncelik seçiniz.");
            }

            if (string.IsNullOrWhiteSpace(item.Status))
            {
                ModelState.AddModelError("Status", "Durum seçiniz.");
            }

            if (item.StartDate == DateTime.MinValue)
            {
                ModelState.AddModelError("StartDate", "Başlangıç tarihi seçiniz.");
            }

            if (item.EndDate == DateTime.MinValue)
            {
                ModelState.AddModelError("EndDate", "Bitiş tarihi seçiniz.");
            }

            if (item.EndDate != DateTime.MinValue && item.EndDate.Date < DateTime.Today)
            {
                ModelState.AddModelError("EndDate", "Geçmiş tarihe görev eklenemez.");
            }

            if (item.StartDate != DateTime.MinValue &&
                item.EndDate != DateTime.MinValue &&
                item.EndDate < item.StartDate)
            {
                ModelState.AddModelError("EndDate", "Bitiş tarihi başlangıç tarihinden önce olamaz.");
            }
        }

        // Görev listesi ekranını açar. Arama, öncelik, durum ve tarihe göre filtreleme yapabilir.
        public ActionResult Index(string search, string priority, string status, DateTime? filterDate)
        {
            ActionResult redirect = RedirectIfNotLoggedIn();
            if (redirect != null)
            {
                return redirect;
            }

            UpdateExpiredAgendaItems();

            int userId = CurrentUserId();
            string role = Session["Role"].ToString();
            List<AgendaItems> agendaList;

            if (role == "Admin")
            {
                agendaList = db.AgendaItems
                               .Where(x => x.IsActive == true)
                               .ToList();
            }
            else
            {
                agendaList = db.AgendaItems
                               .Where(x =>
                                   x.Users.UserID == userId &&
                                   x.IsActive == true)
                               .ToList();
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                agendaList = agendaList
                             .Where(x =>
                                 x.Title.Contains(search) ||
                                 x.Description.Contains(search))
                             .ToList();
            }

            if (!string.IsNullOrEmpty(priority))
            {
                agendaList = agendaList
                             .Where(x => x.Priority == priority)
                             .ToList();
            }

            if (!string.IsNullOrEmpty(status))
            {
                agendaList = agendaList
                             .Where(x => x.Status == status) // Durumu eşit olan kayıtları alıyoruz
                             .ToList();
            }

            if (filterDate.HasValue)
            {
                agendaList = agendaList
                             .Where(x => x.StartDate.Date <= filterDate.Value.Date &&
            x.EndDate.Date >= filterDate.Value.Date)

                             .ToList();
            }

            ViewBag.Search = search;
            ViewBag.Priority = priority;
            ViewBag.Status = status;
            ViewBag.FilterDate = filterDate;

            agendaList = agendaList
             .OrderBy(x => x.StartDate)  // Ajanda kayıtlarını başlangıç tarihine göre sıralıyoruz
             .ToList();

            ViewBag.TaskTitles = agendaList
                                 .Select(x => x.Title)
                                 .ToList();

            return View(agendaList);

        }

        // CREATE - GET
        [HttpGet]  // Yeni bir görev oluşturma sayfasını açar
        public ActionResult Create()
        {
            ActionResult redirect = RedirectIfNotLoggedIn();
            if (redirect != null)
            {
                return redirect;
            }

            ViewBag.Categories = new SelectList(db.Categories.Where(x => x.IsActive == true).ToList(), "CategoryID", "CategoryName");
            return View();
        }

        // CREATE - POST
        [HttpPost]  // Yeni bir görev oluşturmak için formdan gelen verileri alır, doğrular ve kaydeder
        [ValidateAntiForgeryToken]
        public ActionResult Create(AgendaItems item, int? categoryId)
        {
            ActionResult redirect = RedirectIfNotLoggedIn();
            if (redirect != null)
            {
                return redirect;
            }

           

            if (ModelState.IsValid)
            {
                int userId = CurrentUserId();

                Users user = db.Users.Find(userId);
                Categories category = db.Categories.Find(categoryId.Value);

                if (user == null || category == null)
                {
                    return HttpNotFound();
                }

                item.Users = user;
                item.Categories = category;

                item.IsApproved = false;
                item.IsActive = true;
                item.CreatedDate = DateTime.Now;

                db.AgendaItems.Add(item);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.Categories = new SelectList(
                db.Categories.Where(x => x.IsActive == true).ToList(),
                "CategoryID",
                "CategoryName",
                categoryId
            );

            return View(item);
        }


        // DETAILS
        public ActionResult Details(int? id) // id null olabilir diye nullable yaptık
        {
            ActionResult redirect = RedirectIfNotLoggedIn();
            if (redirect != null)
            {
                return redirect;
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AgendaItems item = db.AgendaItems.Find(id); // id'ye göre kaydı buluyoruz

            if (item == null)
            {
                return HttpNotFound();
            }

            if (!CanAccessAgendaItem(item))
            {
                return RedirectToAction("Index");
            }

            return View(item); // bulduğumuz kaydı View'a gönderiyoruz
        }
        // EDIT - GET
        [HttpGet] // Var olan bir görevi düzenleme sayfasını açar
        public ActionResult Edit(int? id) // id null olabilir diye nullable yaptık
        {
            ActionResult redirect = RedirectIfNotLoggedIn();
            if (redirect != null)
            {
                return redirect;
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

            if (!CanAccessAgendaItem(item))
            {
                return RedirectToAction("Index");
            }

            if (item.IsApproved == true)
            {
                return RedirectToAction("Details", new { id = item.AgendaItemId });
            }

            ViewBag.Categories = new SelectList(db.Categories.Where(x => x.IsActive == true).ToList(), "CategoryID", "CategoryName", item.Categories.CategoryID); // Kategori dropdown'ı için

            return View(item);
        }

        // EDIT - POST
        [HttpPost]  // Var olan bir görevi düzenlemek için formdan gelen verileri alır, doğrular ve kaydeder
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AgendaItems item, int? categoryId)
        {
            ActionResult redirect = RedirectIfNotLoggedIn();
            if (redirect != null)
            {
                return redirect;
            }
            ValidateAgendaItem(item, categoryId);

            if (ModelState.IsValid)
            {
                AgendaItems updatedItem = db.AgendaItems.Find(item.AgendaItemId);

                if (updatedItem == null)
                {
                    return HttpNotFound();
                }

                if (!CanAccessAgendaItem(updatedItem))
                {
                    return RedirectToAction("Index");
                }

                if (updatedItem.IsApproved == true)
                {
                    return RedirectToAction("Details", new { id = updatedItem.AgendaItemId });
                }

                Categories category = categoryId.HasValue
                    ? db.Categories.Find(categoryId.Value)
                    : updatedItem.Categories;

                updatedItem.Title = item.Title;
                updatedItem.Description = item.Description;
                if (category != null)
                {
                    updatedItem.Categories = category;
                }
                updatedItem.StartDate = item.StartDate;
                updatedItem.EndDate = item.EndDate;
                updatedItem.Priority = item.Priority;
                updatedItem.Status = item.Status;

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.Categories = new SelectList(db.Categories.Where(x => x.IsActive == true).ToList(), "CategoryID", "CategoryName", categoryId);

            return View(item);
        }
        // DELETE - GET
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            ActionResult redirect = RedirectIfNotLoggedIn();
            if (redirect != null)
            {
                return redirect;
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

            if (!CanAccessAgendaItem(item))
            {
                return RedirectToAction("Index");
            }

            return View(item);
        }
        // DELETE - POST
        [HttpPost, ActionName("Delete")]  // Silme işlemi için formdan gelen verileri alır ve kaydı pasif yaparak siler (soft delete)
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int AgendaItemId)
        {
            ActionResult redirect = RedirectIfNotLoggedIn();
            if (redirect != null)
            {
                return redirect;
            }

            AgendaItems item = db.AgendaItems.Find(AgendaItemId);

            if (item == null)
            {
                return HttpNotFound();
            }

            if (!CanAccessAgendaItem(item))
            {
                return RedirectToAction("Index");
            }

            item.IsActive = false;

            db.SaveChanges(); // değişiklikleri veri tabanına kayıt et

            return RedirectToAction("Index");
        }


        // COMPLETE - GET


        [HttpGet]  // Bir görevi tamamlanmış olarak işaretleme sayfasını açar
        public ActionResult Complete(int? id)
        {
            ActionResult redirect = RedirectIfNotLoggedIn();
            if (redirect != null)
            {
                return redirect;
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

            if (!CanAccessAgendaItem(item))
            {
                return RedirectToAction("Index");
            }

            return View(item);
        }

        // COMPLETE - POST
        [HttpPost, ActionName("Complete")]  // Bir görevi tamamlanmış olarak işaretlemek için formdan gelen verileri alır, doğrular ve kaydeder
        [ValidateAntiForgeryToken]
        public ActionResult CompleteConfirmed(int AgendaItemId)
        {
            ActionResult redirect = RedirectIfNotLoggedIn();
            if (redirect != null)
            {
                return redirect;
            }

            AgendaItems item = db.AgendaItems.Find(AgendaItemId);

            if (item == null)
            {
                return HttpNotFound();
            }

            if (!CanAccessAgendaItem(item))
            {
                return RedirectToAction("Index");
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
            ActionResult redirect = RedirectIfNotLoggedIn();
            if (redirect != null)
            {
                return redirect;
            }

            if (!IsAdmin()) // Admin olmayan kullanıcıların bu sayfaya erişmesini engellemek için
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

            if (item.IsActive == false)
            {
                return RedirectToAction("Index");
            }

            if (item.Status != "Tamamlandı") // Sadece tamamlanmış kayıtların onaylanabilmesi için
            {
                return RedirectToAction("Index");
            }

            return View(item); // Onaylama sayfasına kaydı gönderiyoruz
        }
        // APPROVE - POST
        [HttpPost, ActionName("Approve")]  //Sadece admin kullanabilir. Tamamlandı durumundaki görev admin tarafından onaylanır. Status Onaylandı, IsApproved true yapılır.
        [ValidateAntiForgeryToken]
        public ActionResult ApproveConfirmed(int AgendaItemId)
        {
            ActionResult redirect = RedirectIfNotLoggedIn();
            if (redirect != null)
            {
                return redirect;
            }

            if (!IsAdmin()) // Admin olmayan kullanıcıların bu işlemi yapmasını engellemek için
            {
                return RedirectToAction("Index");
            }

            AgendaItems item = db.AgendaItems.Find(AgendaItemId);

            if (item == null)
            {
                return HttpNotFound();
            }

            if (item.IsActive == false)
            {
                return RedirectToAction("Index");
            }

            if (item.Status == "Tamamlandı") // Sadece tamamlanmış kayıtların onaylanabilmesi için
            {
                item.Status = "Onaylandı"; // admin onay işlemi
                item.IsApproved = true; // Onaylandı olarak işaretliyoruz

                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
        // CALENDAR
        public ActionResult Calendar(int? year, int? month)
        {
            ActionResult redirect = RedirectIfNotLoggedIn();
            if (redirect != null)
            {
                return redirect;
            }

            UpdateExpiredAgendaItems();

            DateTime today = DateTime.Today;

            int selectedYear = year ?? today.Year; // Eğer year parametresi null ise, bugünün yılını kullan
            int selectedMonth = month ?? today.Month; // Eğer month parametresi null ise, bugünün ayını kullan
            int daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);

            DateTime firstDay = new DateTime(selectedYear, selectedMonth, 1);
            int startDay = (int)firstDay.DayOfWeek;
            int startOffset = startDay == 0 ? 6 : startDay - 1;


            DateTime previousMonth = firstDay.AddMonths(-1); // Bir önceki ayın ilk günü
            DateTime nextMonth = firstDay.AddMonths(1);// Bir sonraki ayın ilk günü

            ViewBag.Year = selectedYear;
            ViewBag.Month = selectedMonth;
            ViewBag.MonthName = firstDay.ToString("MMMM"); // Ay adını göstermek için
            ViewBag.PreviousYear = previousMonth.Year;
            ViewBag.PreviousMonth = previousMonth.Month;
            ViewBag.NextYear = nextMonth.Year;
            ViewBag.NextMonth = nextMonth.Month;

            ViewBag.DaysInMonth = daysInMonth;
            ViewBag.StartOffset = startOffset;

            int userId = CurrentUserId();
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

}//AgendaController görev modülünün ana controller’ıdır.
 //Önce kullanıcının giriş yapıp yapmadığı Session ile kontrol edilir.
 //Admin tüm görevleri görebilir, normal kullanıcı sadece kendi görevlerini görür.
 //Listeleme ekranında Where ile filtreleme, OrderBy ile tarih sıralama yapılır.
 //Create ve Edit işlemlerinde boş alan, tarih ve kategori kontrolleri yapılır.
 //Delete işleminde kayıt silinmez, IsActive false yapılır.
 //Complete işleminde görev Tamamlandı olur, Approve işleminde admin görevi Onaylandı yapar.
 //Calendar metodunda görevler ay bazlı takvimde gösterilir ve önceliğe göre renklendirilir.
