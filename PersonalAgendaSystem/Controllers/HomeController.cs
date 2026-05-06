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

        // Login sayfasını açan GET metodu
        // Kullanıcı ilk giriş yaptığında bu çalışır
        public ActionResult Login()
        {
            return View();
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

            var user = db.Users
                         .FirstOrDefault(x =>
                             x.Email == username &&  // e posta ile giriş yapıyoruz.
                             x.Password == password &&
                             x.IsActive == true);

            // Eğer kullanıcı bulunduysa
            if (user != null)
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

            // Kullanıcıyı tekrar login sayfasına gönderir
            return RedirectToAction("Login");
        }
    }
}