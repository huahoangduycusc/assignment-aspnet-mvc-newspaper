using Project.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Project.Controllers
{
    public class HomeController : Controller
    {
        NewspaperEntities db = new NewspaperEntities();
        public ActionResult Index()
        {
            // article ghim
            TempData["ghim"] = (from a in db.articles join c in db.Categories
                          on a.category_id equals c.category_id
                          orderby a.article_id descending
                          where a.ghim == 1
                          select new Project.Models.baiviet
                          {
                              aid = a.article_id,
                              cate = c.category_name,
                              cate_en = c.category_name_en,
                              name_en = a.article_name_en,
                              day = a.created_at.ToString(),
                              name = a.article_name,
                              img = a.thumbnail,
                              cid = c.category_id
                          }).FirstOrDefault();
            // article trend
            TempData["trend"] = (from a in db.articles join c in db.Categories
                          on a.category_id equals c.category_id
                          orderby a.article_id descending
                          where a.ghim == 2
                          select new Project.Models.baiviet
                          {
                              aid = a.article_id,
                              cate = c.category_name,
                              cate_en = c.category_name_en,
                              day = a.created_at.ToString(),
                              name = a.article_name,
                              name_en = a.article_name_en,
                              img = a.thumbnail,
                              cid = c.category_id
                          }).Take(4).ToList();
            // category tv show
            TempData["tvShow"] = (from a in db.articles join c in db.Categories
                          on a.category_id equals c.category_id
                          orderby a.article_id descending
                          where a.status == 1 && a.category_id == 2
                          select new Project.Models.baiviet
                          {
                              aid = a.article_id,
                              cate = c.category_name,
                              cate_en = c.category_name_en,
                              day = a.created_at.ToString(),
                              name = a.article_name,
                              name_en = a.article_name_en,
                              img = a.thumbnail,
                              cid = c.category_id
                          }).Take(3).ToList();
            // list new lasted article
            var result = (from a in db.articles join c in db.Categories
                          on a.category_id equals c.category_id
                          orderby a.article_id descending
                          where a.status == 1
                          select new Project.Models.baiviet
                          {
                              aid = a.article_id,
                              cate = c.category_name,
                              cate_en = c.category_name_en,
                              day = a.created_at.ToString(),
                              name = a.article_name,
                              name_en = a.article_name_en,
                              img = a.thumbnail,
                              cid = c.category_id
                          }).Take(10).ToList();
            return View(result);
        }
        // menu
        public ActionResult SideMenu()
        {
            try
            {
                var result = (from m in db.Categories
                              select new Project.Models.Menu_List
                              {
                                  id = m.category_id,
                                  name = m.category_name,
                                  name_en = m.category_name_en
                              }).ToList();
                return PartialView("menu", result);
            }
            catch(Exception ex)
            {
                var error = ex.Message.ToString();
                return Content("Error");
            }
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(User u)
        {
            string pass = "";
            pass += u.password;
            if(pass != "")
            {
                pass = encrypt(pass);
            }
            var result = db.Users.FirstOrDefault(m => m.username == u.username && m.password == pass);
            if(result == null)
            {
                ViewBag.Msg = "Login failed, username or password is not correctly!";
            }
            else
            {
                FormsAuthentication.SetAuthCookie(u.username, false);
                User obj = db.Users.FirstOrDefault(m => m.username == u.username);
                Session["username"] = u.username;
                Session["uid"] = obj.user_id;
                Session["avatar"] = obj.avatar;
                return RedirectToAction("Index");
            }
            return View();
        }
        // log out , đăng xuất
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Remove("username");
            Session.Remove("uid");
            Session.Remove("avatar");
            return Redirect("/");
        }
        // Register
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(User u)
        {
            var result = db.Users.SingleOrDefault(m => m.username == u.username);
            try
            {
                if (ModelState.IsValid)
                {
                    if(result != null)
                    {
                        ViewBag.Msg = "The username has already exists!, Please use different username";
                    }
                    else
                    {
                        User obj = new User();
                        obj.username = u.username;
                        obj.password = encrypt(u.password);
                        obj.fullname = u.fullname;
                        obj.address = "empty";
                        obj.gender = u.gender;
                        obj.email = u.email;
                        obj.role = "mem";
                        obj.timeJoin = DateTime.Now;
                        obj.avatar = "default.jpg";
                        obj.post = 0;
                        obj.nickname = u.username;
                        db.Users.Add(obj);
                        db.SaveChanges();
                        FormsAuthentication.SetAuthCookie(u.username, false);
                        Session["username"] = u.username;
                        Session["uid"] = obj.user_id;
                        Session["avatar"] = "default.jpg";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    ViewBag.Msg = "There something went wrong, try again !";
                }
            }
            catch(Exception ex)
            {
                ViewBag.Msg = ex.Message;
            }
            return View();
        }
        // Search
        public ActionResult Search(string s)
        {
            TempData["countS"] = db.articles.Where(m => m.status == 1 && m.article_name.Contains(s) || m.article_name_en.Contains(s)).Count();
            return View();
        }
        // search engine
        public ActionResult SearchDisplay(string s, int skip = 0)
        {
            System.Threading.Thread.Sleep(1500);
            var result = (from a in db.articles join c in db.Categories
                          on a.category_id equals c.category_id
                          where a.status == 1 && a.article_name.Contains(s) || a.article_name_en.Contains(s)
                          orderby a.views descending
                          select new Project.Models.baiviet
                          {
                              aid = a.article_id,
                              name = a.article_name,
                              name_en = a.article_name_en,
                              cid = c.category_id,
                              cate = c.category_name,
                              cate_en = c.category_name_en,
                              day = a.created_at.ToString(),
                              img = a.thumbnail
                          }).Skip(skip).Take(9).ToList();
            return PartialView("_SharePost", result);
        }
        // mã hóa password
        public string encrypt(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        // giải mã password
        public string Decrypt(string cipherText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
        // category
        public ActionResult Category(int id)
        {
            TempData["CountCate"] = db.articles.Where(m => m.category_id == id && m.status == 1).Count();
            var result = db.Categories.Where(m => m.category_id == id).FirstOrDefault();
            return View(result);
        }
        public ActionResult CategoryDisplay(int id,int skip=0)
        {
            System.Threading.Thread.Sleep(1500);
            var result = (from a in db.articles
                          where a.status == 1 && a.category_id == id
                          orderby a.article_id descending
                          select new Project.Models.baiviet
                          {
                              aid = a.article_id,
                              name = a.article_name,
                              name_en = a.article_name_en,
                              day = a.created_at.ToString(),
                              img = a.thumbnail
                          }).Skip(skip).Take(6).ToList();
            return PartialView("_Category", result);
        }
        // top
        public ActionResult Top()
        {
            DateTime today = DateTime.Now.Date;
            DateTime inWeek = DateTime.Now.Date.AddDays(-7); // ngay hien tai tru di 7
            // xem nhieu trong ngay
            var topNgay = (from a in db.statistics
                           join b in db.articles
                           on a.article_id equals b.article_id
                           join c in db.Categories 
                           on b.category_id equals c.category_id
                           where b.status == 1 && a.created_at == today
                           orderby a.view descending
                           select new Project.Models.baiviet
                           {
                               aid = (int)a.article_id,
                               cate = c.category_name,
                               cate_en = c.category_name_en,
                               cid = c.category_id,
                               name = b.article_name,
                               name_en = b.article_name_en,
                               day = b.created_at.ToString(),
                               img = b.thumbnail,
                               view = (int)a.view
                           }).Take(9).ToList();
            // xem nhieu trong tuan
            var topTuan = (from a in db.statistics
                           join b in db.articles
                           on a.article_id equals b.article_id
                           join c in db.Categories
                           on b.category_id equals c.category_id
                           where b.status == 1 && a.created_at > inWeek
                           group a by new {a.article_id,c.category_id,c.category_name,c.category_name_en,b.article_name,b.article_name_en,b.created_at,b.thumbnail}
                            into grp
                            select new Project.Models.baiviet
                            {
                                aid = (int)grp.Key.article_id,
                                cate = grp.Key.category_name,
                                cate_en = grp.Key.category_name_en,
                                cid = grp.Key.category_id,
                                name = grp.Key.article_name,
                                name_en = grp.Key.article_name_en,
                                day = grp.Key.created_at.ToString(),
                                img = grp.Key.thumbnail,
                                view = (int)grp.Sum(m => m.view)
                            }).OrderByDescending(m => m.view).Take(6).ToList();
            // xem nhieu trong thang
            var topThang = (from a in db.statistics
                            join b in db.articles
                            on a.article_id equals b.article_id
                            join c in db.Categories
                            on b.category_id equals c.category_id
                            where b.status == 1 && a.created_at.Value.Year == DateTime.Now.Year
                            && a.created_at.Value.Month == DateTime.Now.Month
                            group a by new {a.article_id,c.category_id,c.category_name,c.category_name_en,b.article_name,b.article_name_en,b.created_at,b.thumbnail}
                            into grp
                            select new Project.Models.baiviet
                            {
                                aid = (int)grp.Key.article_id,
                                cate = grp.Key.category_name,
                                cate_en = grp.Key.category_name_en,
                                cid = grp.Key.category_id,
                                name = grp.Key.article_name,
                                name_en = grp.Key.article_name_en,
                                day = grp.Key.created_at.ToString(),
                                img = grp.Key.thumbnail,
                                view = (int)grp.Sum(m => m.view)
                            }).OrderByDescending(m => m.view).Take(6).ToList();
            var result = new bangxephang
            {
                TopDay = topNgay,
                TopWeek = topTuan,
                TopMonth = topThang
            };
            return View(result);
        }
        // ve chung toi
        public ActionResult Vechungtoi()
        {
            return View();
        }
        // chinh sach
        public ActionResult ChinhSach()
        {
            return View();
        }
        // Giay phep
        public ActionResult Giayphep()
        {
            return View();
        }
        // Tro giup
        public ActionResult Trogiup()
        {
            return View();
        }
    }
}