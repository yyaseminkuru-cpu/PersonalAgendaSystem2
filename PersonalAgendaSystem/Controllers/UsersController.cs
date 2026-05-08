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

        private bool IsAdmin()
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

            return null;
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

        public ActionResult Index(string search, string role)
        {
            ActionResult redirect = RedirectIfNotAdmin();
            if (redirect != null)
            {
                return redirect;
            }

            var users = db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(x =>
                    x.FullName.Contains(search) ||
                    x.UserName.Contains(search) ||
                    x.Email.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                users = users.Where(x => x.Role == role);
            }

            ViewBag.Search = search;
            ViewBag.Role = role;

            return View(users.OrderBy(x => x.FullName).ToList());
        }

        [HttpGet]
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

        [HttpPost]
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
                user.IsActive = true;

                db.Users.Add(user);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            FillRoleList(user.Role);
            return View(user);
        }

        public ActionResult Details(int? id)
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

        [HttpGet]
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

        [HttpPost]
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

        [HttpGet]
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

        [HttpPost, ActionName("Delete")]
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
