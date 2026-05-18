using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PersonalAgendaSystem.Models;

namespace PersonalAgendaSystem.Controllers
{
    public class HomeController : Controller
    {
        // Entity Framework bağlantısı
        // Veritabanındaki tablolarla iletişim kurmamızı sağlar
        AgendaModelContainer db = new AgendaModelContainer();

        public ActionResult Index()
        {
            return View();
        }

        // Kullanıcı giriş sayfasını açmak istediğinde bu metod çalışır. Eğer kullanıcı zaten giriş yapmışsa tekrar login ekranı göstermiyoruz, görev listesine yönlendiriyoruz. Giriş yapmamışsa login sayfası açılır.
        public ActionResult Login()
        {
            if (Session["UserID"] != null)
            {
                return RedirectToAction("Index", "Agenda");
            }

            return View();
        }

        public ActionResult Register()
        {
            if (Session["UserID"] != null)
            {
                return RedirectToAction("Index", "Agenda");
            }

            return View();
        }

        [HttpPost]
        public ActionResult Register(string fullName, string userName, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                ViewBag.Error = "Ad soyad boş bırakılamaz.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(userName))
            {
                ViewBag.Error = "Kullanıcı adı boş bırakılamaz.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "E-posta boş bırakılamaz.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Şifre boş bırakılamaz.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Şifreler aynı olmalıdır.";
                return View();
            }

            var existingUser = db.Users 
                                 .FirstOrDefault(x => x.Email == email || x.UserName == userName);

            if (existingUser != null)
            {
                ViewBag.Error = "Bu e-posta veya kullanıcı adı zaten kullanılıyor.";
                return View();
            }

            Users newUser = new Users(); // Yeni kullanıcı nesnesi oluşturuyoruz
            newUser.FullName = fullName;
            newUser.UserName = userName;
            newUser.Email = email;
            newUser.Password = password;
            newUser.Role = "Kullanici"; // Yeni kullanıcıya varsayılan olarak "Kullanici" rolü veriyoruz. Admin rolü manuel olarak verilebilir.
            newUser.IsActive = true;

            db.Users.Add(newUser);
            db.SaveChanges();

            ViewBag.Success = "Kaydınız oluşturuldu. Şimdi giriş yapabilirsiniz.";
            return View("Login");
        }

        // Kullanıcı form gönderdiğinde çalışan POST metodu
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            // Users tablosunda:
            // kullanıcı adı doğru mu?
            // şifre doğru mu?
            // kullanıcı aktif mi?
            // kontrol ediyoruz

            var user = db.Users    //Users tablosunda girilen e-posta ve şifreye sahip, aktif olan kullanıcı aranıyor. FirstOrDefault eşleşen ilk kullanıcıyı getirir. Kullanıcı yoksa null döner.
                         .FirstOrDefault(x =>
                             x.Email == username && //|| x.UserName == username && // e posta ile giriş yapıyoruz. Kullanıcı adıyla giriş yapmak isterseniz bu satırı açabilirsiniz.
                             x.Password == password &&
                             x.IsActive == true); // Kullanıcı aktif mi? değilse giriş yapamaz.

         
            if (user != null) // kullanıcı adı, şifre ve aktiflik kontrolünden geçen bir kullanıcı bulunduysa
            {
                // Session içine kullanıcı bilgilerini kaydediyoruz
                // Böylece sistem giriş yapan kişiyi hatırlıyor

                Session["UserID"] = user.UserID;

                // Kullanıcının adı
                Session["FullName"] = user.FullName;

                // Rol bilgisi
                // Admin mi Kullanici mi?
                Session["Role"] = user.Role;

                // Login başarılıysa AgendaController içindeki
                // Index sayfasına yönlendiriyoruz
                return RedirectToAction("Index", "Agenda");
            }

            // Kullanıcı bulunamazsa hata mesajı gösteriyoruz
            ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";

            // Tekrar login ekranını açıyoruz
            return View();
        }

        // Çıkış işlemi
        public ActionResult Logout()
        {
            // Session içindeki tüm bilgileri temizler
            Session.Clear();
            Session.Abandon();

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            // Kullanıcıyı tekrar login sayfasına gönderir
            return RedirectToAction("Login");
            
            
            //HomeController giriş, kayıt ve çıkış işlemlerini yönetiyor.
            //Login işleminde Users tablosunda e-posta, şifre ve IsActive kontrolü yapılıyor.
            //Kullanıcı bulunursa UserID, FullName ve Role bilgileri Session’a kaydediliyor.
            //Bu Session bilgileri diğer controller’larda yetki ve kullanıcı kontrolü için kullanılıyor.
            //Register işleminde yeni kullanıcı Users tablosuna Kullanici rolüyle ve aktif olarak ekleniyor.
            //Logout işleminde Session temizleniyor.
        }
    }
}
