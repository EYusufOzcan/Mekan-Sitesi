using Mekan_Sitesi.Helper;
using Mekan_Sitesi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Mekan_Sitesi.Controllers
{
    public class HomeController : Controller
    {
        private MekanEntities10 db = new MekanEntities10();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UserRegister()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UserRegister(Kullanicilar kln)
        {
            try
            {
                var kayit = (from data in db.Kullanicilar where data.Email == kln.Email select data).FirstOrDefault();
                var kayit1 = (from data in db.Kullanicilar where data.KullaniciAd == kln.KullaniciAd select data).FirstOrDefault();

                if (kayit != null)
                {
                    ModelState.AddModelError(string.Empty, ErrorMessage.VeriGirisi);
                }

                if (kayit1 != null)
                {
                    ModelState.AddModelError(string.Empty, ErrorMessage.VeriGirisi1); 
                }

                if (kln.KullaniciAd == null)
                {
                    ModelState.AddModelError(string.Empty, ErrorMessage.KullaniciAd);
                }

                if (kln.Ad == null)
                {
                    ModelState.AddModelError(string.Empty, ErrorMessage.Ad);
                }

                if (kln.Soyad == null)
                {
                    ModelState.AddModelError(string.Empty, ErrorMessage.Soyad);
                }

                if (kln.Email == null)
                {
                    ModelState.AddModelError(string.Empty, ErrorMessage.Email);
                }

                if (kln.Sifre == null)
                {
                    ModelState.AddModelError(string.Empty, ErrorMessage.Sifre);
                }

                if (ModelState.IsValid)
                {
                    kln.EmailOnay = false;
                    kln.KatilimTarihi = DateTime.Now;
                    db.Kullanicilar.Add(kln);
                    db.SaveChanges();

                    SmtpClient client = new SmtpClient();
                    client.EnableSsl = true;
                    client.Port = 587;
                    client.Host = "smtp.gmail.com";
                    client.Credentials = new NetworkCredential("stok.satis.otomasyan@gmail.com", "deli90*-");

                    MailMessage Msg = new MailMessage();
                    Msg.From = new MailAddress("stok.satis.otomasyan@gmail.com");
                    Msg.To.Add(kln.Email);
                    Msg.Subject = "Üyelik Onay";
                    Msg.Body = string.Format("Sayın {0} {1}, <BR/>Üyeliğinizi tamamlamak için lütfen aşağıdaki linke tıklayın. <BR/> <a href=\"{2}\" title=\"Email Onay\">{2}</a>", kln.Ad, kln.Soyad, Url.Action("ConfirmEmail", "Home", new { Token = kln.ID, Email = kln.Email }, Request.Url.Scheme)); ;
                    Msg.IsBodyHtml = true;
                    client.Send(Msg);

                    return RedirectToAction("List");
                }
                return View();

            }

            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult ConfirmEmail(Kullanicilar kln)
        {
            try
            {
                var item = (from data in db.Kullanicilar where data.Email == kln.Email select data).FirstOrDefault();
                Session["ad"] = item.KullaniciAd;
                item.EmailOnay = true;
                //db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Insert");
            }

            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Kullanicilar kln)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (MekanEntities10 db = new MekanEntities10())
                    {
                        var kayit = db.Kullanicilar.Where(a => a.KullaniciAd == kln.KullaniciAd && a.Sifre == kln.Sifre && a.EmailOnay == true).FirstOrDefault();

                        if (kayit != null)
                        {
                            Session["ad"] = kln.KullaniciAd;
                            if (kln.KullaniciAd == "admin")
                            {
                                return RedirectToAction("OnayBekleyen");
                            }
                            else
                            {
                                return RedirectToAction("Insert");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ErrorMessage.GirisBasarisiz);
                        }
                    }
                }
                return View(kln);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult Insert()
        {
            if (Session["ad"] == null)
            {
                Session["ad"] = "";
            }
            string key = Session["ad"].ToString();
            if (key == "admin" || key == "")
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Insert(OnayBekleyen onybk, HttpPostedFileBase file, string Lat, string Lng)
        {
            try
            {
                if (Lat == 0.ToString() || Lng == 0.ToString())
                {
                    ModelState.AddModelError(string.Empty, ErrorMessage.Location);
                }

                if (ModelState.IsValid)
                {
                    if (file != null)
                    {
                        string [] allowedExtensions = new[] { ".png", ".jpg", ".JPG", ".PNG", ".gif", ".GIF" };
                        string extension = Path.GetExtension(file.FileName);
                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError(string.Empty, ErrorMessage.ResimBasarisiz);
                        }
                        else
                        {
                            string pic = Path.GetFileName(file.FileName);
                            string path = Path.Combine(Server.MapPath("~/Dosyalar"), pic);
                            file.SaveAs(path);
                            onybk.Resim = pic;
                            onybk.latitude = Lat;
                            onybk.longitude = Lng;
                            onybk.Gonderen = Session["ad"].ToString();
                            db.OnayBekleyen.Add(onybk);
                            db.SaveChanges();
                            return RedirectToAction("OnayBekleyen2");
                        }
                    }
                }

                return View();
            }

            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult List()
        {
            try
            {
                var kayit = db.Mekanlar.ToList();
                return View(kayit);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpGet]
        public ActionResult List(string mekanad = "", string adres = "", string mekansahibi = "", int puan = 0)
        {
            try
            {
                if (mekansahibi == "" && adres == "" && puan == 0)
                { 
                   string key = mekanad.ToLower();
                   var kayit = db.Mekanlar.Where(a => a.MekanAd.ToLower().Contains(key) || key == "");
                   return View(kayit);
                }
                if (mekanad == "" && mekansahibi == "" && puan == 0)
                {
                    string key = adres.ToLower();
                    var kayit = db.Mekanlar.Where(a => a.Adres.ToLower().Contains(key) || key == "");
                    return View(kayit);
                }
                if (mekanad == "" && adres == "" && puan == 0)
                {
                    string key = mekansahibi.ToLower();
                    var kayit = db.Mekanlar.Where(a => a.MekanSahibi.ToLower().Contains(key) || key == "");
                    return View(kayit);
                }
                if (mekanad == "" && mekansahibi == "" && adres == "")
                {
                    var kayit = (from data in db.Mekanlar where data.Puan >= puan select data);
                    return View(kayit);
                }
                if (puan == 0)
                {
                    string key = mekanad.ToLower();
                    string key2 = mekansahibi.ToLower();
                    string key3 = adres.ToLower();
                    var kayit = db.Mekanlar.Where(a => a.MekanAd.ToLower().Contains(key) && a.MekanSahibi.ToLower().Contains(key2) && a.Adres.ToLower().Contains(key3));
                    return View(kayit);
                }
                if (puan != 0)
                {
                    string key = mekanad.ToLower();
                    string key2 = mekansahibi.ToLower();
                    string key3 = adres.ToLower();
                    var kayit = db.Mekanlar.Where(a => a.MekanAd.ToLower().Contains(key) && a.MekanSahibi.ToLower().Contains(key2) && a.Adres.ToLower().Contains(key3) && a.Puan >= puan);
                    return View(kayit);
                }
                return View();
            }

            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult OnayBekleyen()
        {
            try
            {
                if (Session["ad"] == null)
                {
                    Session["ad"] = "";
                }
                string key = Session["ad"].ToString();
                if (key != "admin")
                {
                    return RedirectToAction("Login"); 
                }
                var kayit = db.OnayBekleyen.ToList();
                return View(kayit);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult Onaylanan()
        {
            try
            {
                if (Session["ad"] == null)
                {
                    Session["ad"] = "";
                }
                string key = Session["ad"].ToString();
                if (key != "admin")
                {
                    return RedirectToAction("Login");
                }
                var kayit = db.Mekanlar.ToList();
                return View(kayit);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult Reddedilen()
        {
            try
            {
                if (Session["ad"] == null)
                {
                    Session["ad"] = "";
                }
                string key = Session["ad"].ToString();
                if (key != "admin")
                {
                    return RedirectToAction("Login");
                }
                var kayit = db.Reddedilen.ToList();
                return View(kayit);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult Exit()
        {
            Session["ad"] = null;
            return RedirectToAction("List");
        }

        public ActionResult Onayla(int id)
        {
            try
            {
                if (Session["ad"] == null)
                {
                    Session["ad"] = "";
                }
                string key = Session["ad"].ToString();
                if (key != "admin")
                {
                    return RedirectToAction("Login");
                }
                var kayit = db.OnayBekleyen.Find(id);
                return View(kayit);
            }

            catch (Exception)
            {
                return View("Error");
            } 
        }

        [HttpPost]
        public ActionResult Onayla(Mekanlar mkn, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var kayit = db.OnayBekleyen.Find(id);
                    mkn.latitude = kayit.latitude;
                    mkn.longitude = kayit.longitude;
                    db.Entry(mkn).State = EntityState.Added;
                    db.OnayBekleyen.Remove(kayit);
                    db.SaveChanges();

                    var kayit2 = (from data in db.Kullanicilar where data.KullaniciAd == mkn.Gonderen select data).FirstOrDefault();

                    SmtpClient client = new SmtpClient();
                    client.EnableSsl = true;
                    client.Port = 587;
                    client.Host = "smtp.gmail.com";
                    client.Credentials = new NetworkCredential("stok.satis.otomasyan@gmail.com", "deli90*-");

                    MailMessage Msg = new MailMessage();
                    Msg.From = new MailAddress("stok.satis.otomasyan@gmail.com");
                    Msg.To.Add(kayit2.Email);
                    Msg.Subject = "Öneri Kabul";
                    Msg.Body = string.Format("Sayın {0} {1},<BR/>{2} isimli mekan admin tarafından onaylanmıştır.", kayit2.Ad, kayit2.Soyad, mkn.MekanAd);
                    Msg.IsBodyHtml = true;
                    client.Send(Msg);

                    return RedirectToAction("OnayBekleyen", new { id = mkn.ID });
                }
                return View();
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult Reddet(int id)
        {
            try
            {
                if (Session["ad"] == null)
                {
                    Session["ad"] = "";
                }
                string key = Session["ad"].ToString();
                if (key != "admin")
                {
                    return RedirectToAction("Login");
                }
                var kayit = db.OnayBekleyen.Find(id);
                return View(kayit);
            }
            catch (Exception)
            {
                return View("Error");
            } 
        }

        [HttpPost]
        public ActionResult Reddet(Reddedilen rddln, int id, string rednedeni)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var kayit = db.OnayBekleyen.Find(id);
                    rddln.Rednedeni = rednedeni;
                    rddln.latitude = kayit.latitude;
                    rddln.longitude = kayit.longitude;
                    db.Entry(rddln).State = EntityState.Added;
                    db.OnayBekleyen.Remove(kayit);
                    db.SaveChanges();

                    var kayit2 = (from data in db.Kullanicilar where data.KullaniciAd == rddln.Gonderen select data).FirstOrDefault();

                    SmtpClient client = new SmtpClient();
                    client.EnableSsl = true;
                    client.Port = 587;
                    client.Host = "smtp.gmail.com";
                    client.Credentials = new NetworkCredential("stok.satis.otomasyan@gmail.com", "deli90*-");

                    MailMessage Msg = new MailMessage();
                    Msg.From = new MailAddress("stok.satis.otomasyan@gmail.com");
                    Msg.To.Add(kayit2.Email);
                    Msg.Subject = "Öneri Red";
                    Msg.Body = string.Format("Sayın {0} {1},<BR/>{2} isimli mekan admin tarafından kabul edilmedi.<BR/> Red nedeni: {3} </BR/><BR/> Dilerseniz sisteme girip bilgileri güncelledikten sonra geri gönderebilirsiniz.", kayit2.Ad, kayit2.Soyad, rddln.MekanAd, rddln.Rednedeni);
                    Msg.IsBodyHtml = true;
                    client.Send(Msg);   
          
                    return RedirectToAction("OnayBekleyen", new { id = rddln.ID });
                }
                return View();
            }

            catch
            {
                return View("Error");
            }
        }

        public ActionResult Reddedilmis()
        {
            try
            {
                if (Session["ad"] == null)
                {
                    Session["ad"] = "";
                }
                string key = Session["ad"].ToString();
                if (key == "admin" || key == "")
                {
                    return RedirectToAction("Login");
                }
                var kayit = db.Reddedilen.Where(a => a.Gonderen == key).ToList();
                return View(kayit);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult Onaylanmıs()
        {
            try
            {
                if (Session["ad"] == null)
                {
                    Session["ad"] = "";
                }
                string key = Session["ad"].ToString();
                if (key == "admin" || key == "")
                {
                    return RedirectToAction("Login");
                }
                var kayit = db.Mekanlar.Where(a => a.Gonderen == key).ToList();
                return View(kayit);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult OnayBekleyen2()
        {
            try
            {
                if (Session["ad"] == null)
                {
                    Session["ad"] = "";
                }
                string key = Session["ad"].ToString();
                if (key == "admin" || key == "")
                {
                    return RedirectToAction("Login");
                }
                var kayit = db.OnayBekleyen.Where(a => a.Gonderen == key).ToList();
                return View(kayit);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult List2()
        {
            try
            {
                if (Session["ad"] == null)
                {
                    Session["ad"] = "";
                }
                string key = Session["ad"].ToString();
                if (key == "admin" || key == "")
                {
                    return RedirectToAction("Login");
                }
                var kayit = db.Mekanlar.Where(a => a.Gonderen != key).ToList();
                return View(kayit);
            }
            catch (Exception)
            {

                return View("Error");
            }
        }

        public ActionResult Detail(int id)
        {
            try
            {
                var kayit = db.Mekanlar.Find(id);
                Session["Mekanad"] = kayit.MekanAd;
                return View(kayit);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult Comment()
        {
            try
            {
                if (Session["ad"] == null)
                {
                    Session["ad"] = "";
                }
                string key = Session["ad"].ToString();
                if (key == "admin" || key == "")
                {
                    return RedirectToAction("Login");
                }
                return PartialView();
            }
            catch (Exception)
            {
                return View("Error");
            }  
        }

        [HttpPost]
        public ActionResult Comment(Degerlendirme dgld)
        {
            try
            {
                string key = Session["Mekanad"].ToString();
                string key2 = Session["ad"].ToString();
                var kayit = db.Degerlendirme.Where(a => a.Mekanad == key && a.Gonderen == key2).FirstOrDefault();
                if (kayit != null)
                {
                    ModelState.AddModelError(string.Empty, ErrorMessage.AyniYorum);
                }
                
                if (ModelState.IsValid)
                {
                    dgld.Mekanad = key;
                    dgld.Gonderen = key2;
                    db.Degerlendirme.Add(dgld);
                    db.SaveChanges();
                    return RedirectToAction("List2");
                }
                return PartialView();
            }
            catch (Exception)
            {
                return View("Error");
            }
        
        }

        public ActionResult Show(int id)
        {
            try
            {
                var kayit = db.Mekanlar.Find(id);
                return View(kayit);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult Edit(int id)
        {
            try
            {
                if (Session["ad"] == null)
                {
                    Session["ad"] = "";
                }
                string key = Session["ad"].ToString();
                if (key == "admin")
                {
                    return RedirectToAction("Login");
                }
                var kayit = db.Reddedilen.Find(id);
                return View(kayit);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult Edit(OnayBekleyen mkn, HttpPostedFileBase file, int id, string Lat, string Lng)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (file != null)
                    {
                        var allowedExtensions = new[] { ".png", ".jpg", ".JPG", ".PNG", ".gif", ".GIF" };
                        var extension = System.IO.Path.GetExtension(file.FileName);
                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError(string.Empty, ErrorMessage.ResimBasarisiz);
                        }
                        else
                        {
                            string pic = System.IO.Path.GetFileName(file.FileName);
                            string path = System.IO.Path.Combine(Server.MapPath("~/Dosyalar"), pic);
                            file.SaveAs(path);
                            using (MemoryStream ms = new MemoryStream())
                            {
                                file.InputStream.CopyTo(ms);
                                byte[] array = ms.GetBuffer();
                            }
                            Reddedilen kayit = (from data in db.Reddedilen where data.ID == id select data).FirstOrDefault();
                            mkn.Resim = pic;
                            mkn.latitude = Lat;
                            mkn.longitude = Lng;
                            mkn.Gonderen = Session["ad"].ToString();
                            db.OnayBekleyen.Add(mkn);
                            db.Reddedilen.Remove(kayit);
                            db.SaveChanges();
                            return RedirectToAction("OnayBekleyen2");
                        }
                    }
                    else
                    {
                        Reddedilen kayit = (from data in db.Reddedilen where data.ID == id select data).FirstOrDefault();
                        mkn.latitude = Lat;
                        mkn.longitude = Lng;
                        mkn.Gonderen = Session["ad"].ToString();
                        db.OnayBekleyen.Add(mkn);
                        db.Reddedilen.Remove(kayit);
                        db.SaveChanges();
                        return RedirectToAction("OnayBekleyen2");
                    }
                }
                return View();
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult ShowComment(int id)
        {
            try
            {    
                var kayit = (from data in db.Mekanlar where data.ID == id select data).FirstOrDefault();
                var kayit2 = db.Degerlendirme.Where(a => a.Mekanad == kayit.MekanAd).ToList();
                return View(kayit2);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
    }
}

                
