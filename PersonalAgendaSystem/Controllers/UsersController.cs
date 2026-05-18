using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PersonalAgendaSystem.Models;

namespace PersonalAgendaSystem.Controllers
{
    public class UsersController : Controller
    {
        AgendaModelContainer db = new AgendaModelContainer();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            base.OnActionExecuting(filterContext);
        }

        private bool IsAdmin() //Session içindeki Role bilgisine bakýyoruz. Role Admin ise kullanýcý yönetimi iþlemlerine izin veriyoruz.
        {
            return Session["Role"] != null && Session["Role"].ToString() == "Admin";
        }

        private ActionResult RedirectIfNotAdmin()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Agenda");
            }

            return null;  //Kullanýcý giriþ yapmamýþsa Login sayfasýna gider. Giriþ yapmýþ ama admin deðilse Agenda sayfasýna yönlendirilir.
        }

        private void FillRoleList(string selectedRole = null)
        {
            ViewBag.Roles = new SelectList(new[]
            {
                new SelectListItem { Text = "Admin", Value = "Admin" },
                new SelectListItem { Text = "Kullanici", Value = "Kullanici" }
            }, "Value", "Text", selectedRole);
        }

        private void ValidateUser(Users user)
        {
            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                ModelState.AddModelError("FullName", "Ad soyad bos birakilamaz.");
            }

            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                ModelState.AddModelError("UserName", "Kullanici adi bos birakilamaz.");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                ModelState.AddModelError("Email", "E-posta bos birakilamaz.");
            }

            if (string.IsNullOrWhiteSpace(user.Password))
            {
                ModelState.AddModelError("Password", "Sifre bos birakilamaz.");
            }

            if (string.IsNullOrWhiteSpace(user.Role))
            {
                ModelState.AddModelError("Role", "Rol seciniz.");
            }
        }

        public ActionResult Index(string search, string role) //Kullanýcý listesini getirir. Admin bu ekranda kullanýcýlarý arayabilir ve role göre filtreleyebilir.
        {
            ActionResult redirect = RedirectIfNotAdmin();
            if (redirect != null)
            {
                return redirect;
            }

            var users = db.Users.AsQueryable(); //Kullanýcýlarý veritabanýndan çeker ve sorgulanabilir hale getirir.

            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(x =>
                    x.FullName.Contains(search) ||
                    x.UserName.Contains(search) ||
                    x.Email.Contains(search)); //Kullanýcýnýn adý, kullanýcý adý veya e-postasý içinde arama yapýlýr.
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                users = users.Where(x => x.Role == role); //Kullanýcýlarýn rolüne göre filtreleme yapýlýr. Admin veya Kullanici olarak filtreleme yapýlabilir.
            }

            ViewBag.Search = search;
            ViewBag.Role = role;

            return View(users.OrderBy(x => x.FullName).ToList()); //Kullanýcýlar tam adý sýrasýna göre sýralanýr ve liste olarak görüntülenir.
        }

        [HttpGet] //Yeni kullanýcý oluþturma sayfasýný getirir. Admin bu sayfada yeni bir kullanýcý ekleyebilir.
        public ActionResult Create()
        {
            ActionResult redirect = RedirectIfNotAdmin();
            if (redirect != null)
            {
                return redirect;
            }

            FillRoleList();
            return View();
        }

        [HttpPost] //Yeni kullanýcý oluþturma iþlemini gerçekleþtirir. Admin tarafýndan girilen kullanýcý bilgileri doðrulanýr ve geçerliyse veritabanýna kaydedilir.
        [ValidateAntiForgeryToken]
        public ActionResult Create(Users user)
        {
            ActionResult redirect = RedirectIfNotAdmin();
            if (redirect != null)
            {
                return redirect;
            }

            ValidateUser(user);

            if (ModelState.IsValid)
            {
                var existingUser = db.Users.FirstOrDefault(x => x.Email == user.Email || x.UserName == user.UserName);

                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Bu e-posta veya kullanici adi zaten kullaniliyor.");
                    FillRoleList(user.Role);
                    return View(user);
                }

                user.IsActive = true;

                db.Users.Add(user);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            FillRoleList(user.Role);
            return View(user);

        }

        public ActionResult Details(int? id) //Kullanýcý detay sayfasýný getirir. Admin bu sayfada kullanýcýnýn detay bilgilerini görebilir.
        {
            ActionResult redirect = RedirectIfNotAdmin();
            if (redirect != null)
            {
                return redirect;
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Users user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [HttpGet] //Kullanýcý düzenleme sayfasýný getirir. Admin bu sayfada kullanýcýnýn bilgilerini düzenleyebilir.
        public ActionResult Edit(int? id)
        {
            ActionResult redirect = RedirectIfNotAdmin();
            if (redirect != null)
            {
                return redirect;
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Users user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            FillRoleList(user.Role);
            return View(user);
        }

        [HttpPost] //Kullanýcý düzenleme iþlemini gerçekleþtirir. Admin tarafýndan girilen kullanýcý bilgileri doðrulanýr ve geçerliyse veritabanýnda güncellenir.
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Users user)
        {
            ActionResult redirect = RedirectIfNotAdmin();
            if (redirect != null)
            {
                return redirect;
            }

            ValidateUser(user);

            if (ModelState.IsValid)
            {
                Users updatedUser = db.Users.Find(user.UserID);

                if (updatedUser == null)
                {
                    return HttpNotFound();
                }

                var existingUser = db.Users.FirstOrDefault(x => x.UserID != user.UserID && (x.Email == user.Email || x.UserName == user.UserName));

                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Bu e-posta veya kullanici adi zaten kullaniliyor.");
                    FillRoleList(user.Role);
                    return View(user);
                }

                updatedUser.FullName = user.FullName;
                updatedUser.UserName = user.UserName;
                updatedUser.Password = user.Password;
                updatedUser.Email = user.Email;
                updatedUser.Role = user.Role;
                updatedUser.IsActive = user.IsActive;

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            FillRoleList(user.Role);
            return View(user);
        }

        [HttpGet] //Silme onay ekranýný açar.
        public ActionResult Delete(int? id)
        {
            ActionResult redirect = RedirectIfNotAdmin();
            if (redirect != null)
            {
                return redirect;
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Users user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")] //Kullanýcý silme iþlemini gerçekleþtirir. Admin tarafýndan onaylanan kullanýcý silinmek yerine pasif hale getirilir.(soft delete) Böylece kullanýcý veritabanýnda kalýr ancak aktif olarak kullanýlamaz.
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int UserID)
        {
            ActionResult redirect = RedirectIfNotAdmin();
            if (redirect != null)
            {
                return redirect;
            }

            Users user = db.Users.Find(UserID);

            if (user == null)
            {
                return HttpNotFound();
            }

            user.IsActive = false;
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
//UsersController admin kullanýcý yönetimi için yazýldý.
//Önce kullanýcýnýn admin olup olmadýðý Session’daki Role bilgisiyle kontrol ediliyor.
//Admin deðilse bu sayfalara eriþemiyor.
//Index metodunda kullanýcýlar listeleniyor, arama ve role göre filtreleme yapýlýyor.
//Create ile yeni kullanýcý ekleniyor, Edit ile kullanýcý bilgileri güncelleniyor, Details ile detaylarý gösteriliyor.
//Delete iþleminde fiziksel silme yapýlmýyor, IsActive false yapýlarak soft delete uygulanýyor.