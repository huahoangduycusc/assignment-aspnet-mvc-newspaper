using PagedList;
using Project.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using Rotativa;
using System.Web.Helpers;
using System.Collections;

namespace Project.Controllers
{
    public class ArticleController : Controller
    {
        NewspaperEntities db = new NewspaperEntities();
        // GET: Article
        public ActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "admin,btv,ctv,nb")]
        // create article
        public ActionResult Create()
        {
            var result = db.Categories.ToList();
            TempData["category"] = result;
            TempData.Keep("category");
            return View();
        }
        [Authorize]
        [HttpPost,ValidateInput(false)]
        public ActionResult Create(article a, HttpPostedFileBase thumbnail)
        {
            TempData.Keep("category");
            string filename = "";
            string extension = "";
            int uid = (int)Session["uid"];
            string name = "";
            try
            {
                if (ModelState.IsValid)
                {
                    if(thumbnail != null && thumbnail.ContentLength > 0)
                    {
                        filename = Path.GetFileName(thumbnail.FileName);
                        extension = Path.GetExtension(thumbnail.FileName).ToLower();
                        name = DateTime.Now.ToFileTime().ToString();
                        name += extension;
                        var allowExtension = new[] {".png",".jpg",".jpeg"};
                        if (allowExtension.Contains(extension))
                        {
                            string foldername = Path.Combine(Server.MapPath("~/images/thumbnail"), name);
                            thumbnail.SaveAs(foldername);
                            article obj = new article();
                            obj.article_name = a.article_name;
                            obj.description = a.description;
                            obj.article_name_en = a.article_name_en;
                            obj.description_en = a.description_en;
                            obj.category_id = a.category_id;
                            obj.ghim = a.ghim;
                            obj.views = 0;
                            obj.user_id = (int)Session["uid"];
                            obj.tags = a.tags;
                            if (User.IsInRole("admin") || User.IsInRole("btv"))
                            {
                                obj.status = 1;
                            }
                            else
                            {
                                obj.status = 0;
                            }
                            obj.thumbnail = name;
                            obj.created_at = DateTime.Now;
                            db.articles.Add(obj);
                            db.SaveChanges();
                            int a_id = (int)obj.article_id;
                            ViewBag.Success = "Tạo chủ đề mới thành công, bạn có thể xem bài viết tại !<a href=\"" + Url.Action("View", "Article", new { id = a_id }) + "\">Đây !</a>";
                            var thongbao = db.follows.Where(m => m.to_user == uid).ToList();
                            string mesTb = getNickname(uid)+" đã đăng chủ đề <a href=\"" + Url.Action("View", "Article", new { id = a_id }) + "\"><b>"+a.article_name+ "</b></a> lên trang chủ";
                            foreach (var item in thongbao) // gui thong bao den nhưng ng theo doi
                            {
                                thongbao objTb = new thongbao();
                                objTb.user_id = item.from_user;
                                objTb.msg = mesTb;
                                objTb.seen = 0;
                                objTb.created_at = DateTime.Now;
                                db.thongbaos.Add(objTb);
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            ViewBag.Msg = "Dinh dang tep tin khong hop le, duoi anh phai la png, jpg, jpeg!";
                        }
                    }
                }
                else
                {
                    ViewBag.Msg = "There is something wrong";
                }
            }
            catch(Exception ex)
            {
                ViewBag.Msg = ex.Message;
            }
            return View();
        }
        [Authorize(Roles = "admin,btv")]
        // edit article
        public ActionResult Edit(int id)
        {
            var result = db.articles.Where(m => m.article_id == id).FirstOrDefault();
            TempData["category"] = db.Categories.ToList();
            TempData.Keep("category");
            return View(result);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult Edit(int id, article a, HttpPostedFileBase thumbnail)
        {
            TempData.Keep("category");
            var result = db.articles.Where(m => m.article_id == id).FirstOrDefault();
            result.article_name = a.article_name;
            result.description = a.description;
            result.article_name_en = a.article_name_en;
            result.description_en = a.description_en;
            result.category_id = a.category_id;
            result.ghim = a.ghim;
            result.tags = a.tags;
            if(thumbnail != null && thumbnail.ContentLength > 0)
            {
                var allowExtension = new[] {".png",".jpg",".jpeg"};
                string extension = Path.GetExtension(thumbnail.FileName).ToLower();
                string name = DateTime.Now.ToFileTime().ToString();
                name += extension;
                if(!allowExtension.Contains(extension))
                {
                    ViewBag.Msg = "Dinh dang tep khong hop le, duoi anh phai la png, jpg, jpeg!";
                }
                else
                {
                    string foldername = Path.Combine(Server.MapPath("~/images/thumbnail"), name);
                    thumbnail.SaveAs(foldername);
                    result.thumbnail = name;
                }
            }
            db.SaveChanges();
            ViewBag.Success = "Cập nhật dữ liệu mới thành công !";
            return View();
        }
        // view article
        public ActionResult View(int id, string title)
        {
            var kt = db.articles.Where(m => m.article_id == id).FirstOrDefault();
            if (kt != null)
            {
                kt.views = kt.views + 1;
                DateTime today = DateTime.Now.Date;
                var thongke = db.statistics.Where(m => m.article_id == id && m.created_at.Value == today).FirstOrDefault();
                if(thongke != null)
                {
                    thongke.view = thongke.view + 1;
                }
                else
                {
                    statistic obj = new statistic();
                    obj.article_id = id;
                    obj.view = 1;
                    obj.created_at = today;
                    db.statistics.Add(obj);
                }
                db.SaveChanges();
            }
            ViewData["demCmt"] = db.Comments.Where(m => m.article_id == id).Count();
            var result = (from a in db.articles
                          join c in db.Categories on a.category_id equals c.category_id
                          join u in db.Users on a.user_id equals u.user_id where a.article_id == id
                          select new Project.Models.baiviet
                          {
                              aid = a.article_id,
                              name = a.article_name,
                              name_en = a.article_name_en,
                              content = a.description,
                              content_en = a.description_en,
                              img = a.thumbnail,
                              cate = c.category_name,
                              cate_en = c.category_name_en,
                              user = u.nickname,
                              day = a.created_at.ToString(),
                              uid = (int)a.user_id,
                              tags = a.tags,
                              cid = c.category_id,
                              view = (int)a.views
                          }).First();
            // display like
            ViewData["heart"] = db.likes.Where(m => m.article_id == id && m.camxuc == "heart").Count();
            ViewData["smile"] = db.likes.Where(m => m.article_id == id && m.camxuc == "smile").Count();
            ViewData["angry"] = db.likes.Where(m => m.article_id == id && m.camxuc == "angry").Count();
            ViewData["cry"] = db.likes.Where(m => m.article_id == id && m.camxuc == "cry").Count();
            ViewData["scared"] = db.likes.Where(m => m.article_id == id && m.camxuc == "scared").Count();
            return View(result);
        }
        // comment
        [Authorize]
        [HttpPost]
        public ActionResult View(int id,Comment c)
        {
            int user_id = (int)Session["uid"]; // session
            DateTime today = DateTime.Now.Date;
            var khoa = db.blocks.Where(m => m.block_user == user_id && m.block_time > today).OrderByDescending(m => m.block_id).FirstOrDefault();
            if (khoa != null)
            {
                return Redirect("/");
            }
            var selfBv = db.articles.Where(m => m.article_id == id).FirstOrDefault();
            try
            {
                if (ModelState.IsValid)
                {
                    var userSelf = db.Users.Where(m => m.user_id == user_id).FirstOrDefault();
                    userSelf.post = userSelf.post + 1;
                    Comment obj = new Comment();
                    obj.message = c.message;
                    obj.created_at = DateTime.Now;
                    obj.article_id = id;
                    obj.user_id = user_id;
                    obj.report = 0;
                    db.Comments.Add(obj);
                    db.SaveChanges();
                    return RedirectToAction("View","Article",new {id=id, title = UrlEncoder.ToFriendlyUrl(selfBv.article_name) });
                }
                else
                {
                    ViewBag.Msg = "There is something wrong";
                    return RedirectToAction("View", "Article", new { id = id, title = UrlEncoder.ToFriendlyUrl(selfBv.article_name) });
                }
            }
            catch(Exception ex)
            {
                ViewBag.Msg = ex.Message;
            }
            return RedirectToAction("View", "Article", new { id = id, title = UrlEncoder.ToFriendlyUrl(selfBv.article_name)});
        }
        // view comment
        public ActionResult getComment(int id, int skip = 0)
        {
            System.Threading.Thread.Sleep(1000);
            var result = (from c in db.Comments
                          join u in db.Users on c.user_id equals u.user_id
                          where c.article_id == id
                          orderby c.cmt_id descending
                          select new Project.Models.binhluan
                          {
                              id = c.cmt_id,
                              uid = u.user_id,
                              name = u.nickname,
                              img = u.avatar,
                              day = c.created_at.ToString(),
                              msg = c.message,
                              role = u.role
                          }).Skip(skip).Take(5).ToList();
            return PartialView("_Comment", result);
        }
        // del comment
        [Authorize]
        public ActionResult DelComment(int id)
        {
            var result = db.Comments.Where(m => m.cmt_id == id).FirstOrDefault();
            int art = (int)result.article_id;
            if(result == null)
            {
                return RedirectToAction("Index", "Home", "");
            }
            else
            {
                if(User.IsInRole("admin") || Session["uid"] != null && Session["uid"].ToString() == result.user_id.ToString())
                {
                    db.Comments.Remove(result);
                    db.SaveChanges();
                    return RedirectToAction("View", "Article", new { id = art });
                }
                else
                {
                   return RedirectToAction("Login", "Home", "");
                }
            }
        }
        [Authorize(Roles = "admin,btv,nb,ctv")]
        // bai viet cho duyet
        public ActionResult Waiting(int? page)
        {
            var result = (from a in db.articles
                          join u in db.Users
                          on a.user_id equals u.user_id
                          where a.status == 0
                          select new Project.Models.baiviet
                          {
                              aid = a.article_id,
                              name = a.article_name,
                              day = a.created_at.ToString(),
                              uid = u.user_id,
                              status = (int)a.status,
                              img = a.thumbnail,
                              user = u.username
                          }).OrderByDescending(m => m.aid).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
        }
        [Authorize(Roles = "admin,btv")]
        // duyet bai viet
        public ActionResult Accept(int id)
        {
            var result = db.articles.Where(m => m.article_id == id).FirstOrDefault();
            if(result == null)
            {
                return RedirectToAction("Index", "Home", "");
            }
            else
            {
                result.status = 1;
                db.SaveChanges();
                return RedirectToAction("Waiting", "Article","");
            }
        }
        //chinh sua khi chua duyet
        [Authorize(Roles = "admin,btv,ctv,nb")]
        public ActionResult EditWaiting(int id = -1)
        {
            var result = db.articles.Where(m => m.article_id == id && m.status == 0).FirstOrDefault();
            if(result == null)
            {
                return RedirectToAction("Index", "Home", null);
            }
            TempData["category"] = db.Categories.ToList();
            TempData.Keep("category");
            return View(result);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult EditWaiting(int id, article a, HttpPostedFileBase thumbnail)
        {
            TempData.Keep("category");
            var result = db.articles.Where(m => m.article_id == id && m.status == 0).FirstOrDefault();
            if (result == null)
            {
                return RedirectToAction("Index", "Home", null);
            }
            result.article_name = a.article_name;
            result.description = a.description;
            result.article_name_en = a.article_name_en;
            result.description_en = a.description_en;
            result.category_id = a.category_id;
            result.ghim = a.ghim;
            result.tags = a.tags;
            if (thumbnail != null && thumbnail.ContentLength > 0)
            {
                var allowExtension = new[] { ".png", ".jpg", ".jpeg" };
                string extension = Path.GetExtension(thumbnail.FileName).ToLower();
                string name = DateTime.Now.ToFileTime().ToString();
                name += extension;
                if (!allowExtension.Contains(extension))
                {
                    ViewBag.Msg = "Dinh dang tep khong hop le, duoi anh phai la png, jpg, jpeg!";
                }
                else
                {
                    string foldername = Path.Combine(Server.MapPath("~/images/thumbnail"), name);
                    thumbnail.SaveAs(foldername);
                    result.thumbnail = name;
                }
            }
            db.SaveChanges();
            ViewBag.Success = "Cập nhật dữ liệu mới thành công !";
            return View();
        }
        [Authorize(Roles = "admin,btv")]
        // del article
        public ActionResult DelArticle(int id)
        {
            var result = db.articles.Where(m => m.article_id == id).FirstOrDefault();
            if(result == null)
            {
                return RedirectToAction("Index", "Home", "");
            }
            else
            {
                db.articles.Remove(result);
                db.SaveChanges();
                return RedirectToAction("List","Article","");
            }
        }
        [Authorize(Roles = "admin,btv")]
        // del waiting
        public ActionResult DelArticleWaiting(int id)
        {
            var result = db.articles.Where(m => m.article_id == id).FirstOrDefault();
            if (result == null)
            {
                return RedirectToAction("Index", "Home", "");
            }
            else
            {
                db.articles.Remove(result);
                db.SaveChanges();
                return RedirectToAction("Waiting", "Article", "");
            }
        }
        [Authorize(Roles = "admin,btv")]
        // list article
        public ActionResult List(int ? page)
        {
            var result = (from a in db.articles
                          join u in db.Users
                          on a.user_id equals u.user_id
                          where a.status == 1
                          select new Project.Models.baiviet
                          {
                              aid = a.article_id,
                              name = a.article_name,
                              day = a.created_at.ToString(),
                              view = (int)a.views,
                              img = a.thumbnail,
                              user = u.username,
                              uid = u.user_id
                          }).OrderByDescending(m => m.aid).ToList().ToPagedList(page ?? 1,10);
            return View(result);
        }
        // print
        public ActionResult printAllReport()
        {
            var result = (from a in db.articles
                          join u in db.Users
                          on a.user_id equals u.user_id
                          where a.status == 1
                          select new Project.Models.baiviet
                          {
                              aid = a.article_id,
                              name = a.article_name,
                              day = a.created_at.ToString(),
                              view = (int)a.views,
                              img = a.thumbnail,
                              user = u.username,
                              uid = u.user_id
                          }).OrderByDescending(m => m.aid).ToList();
            var report = new ViewAsPdf(result);
            return report;
        }
        // the most view
        public ActionResult topView()
        {
            DateTime today = DateTime.Now.Date;
            var result = (from a in db.articles join c in db.Categories
                          on a.category_id equals c.category_id
                          join s in db.statistics
                          on a.article_id equals s.article_id
                          where s.created_at == today
                          && a.status == 1 orderby s.view descending
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
                          }).Take(9).ToList();
            return PartialView("_MostView", result);
        }
        // spotlight tag article
        public ActionResult Spotlight()
        {
            var result = (from a in db.articles join c in db.Categories
                          on a.category_id equals c.category_id
                          where a.status == 1 && a.tags.Contains(@"spotlight") orderby a.views descending
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
                          }).Take(6).ToList();
            return PartialView("_Spotlight",result);
        }
        // smilar article
        public ActionResult Similar(int aid,int cid)
        {
            var result = (from a in db.articles join c in db.Categories
                          on a.category_id equals c.category_id
                          where a.status == 1 && a.article_id != aid && a.category_id == cid 
                          orderby a.article_id descending
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
                          }).Take(3).ToList();
            return PartialView("_SimilarArticle",result);
        }
        // Related Article
        public ActionResult Related()
        {
            var result = (from a in db.articles join c in db.Categories
                          on a.category_id equals c.category_id
                          where a.status == 1 
                          orderby Guid.NewGuid()
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
                          }).Take(5).ToList();
            return PartialView("_RelatedArticle",result);
        }
        // like article
        public ActionResult Like(int aid,int uid,string emojii)
        {
            System.Threading.Thread.Sleep(1000);
            var result = db.articles.Where(m => m.article_id == aid).FirstOrDefault();
            if(result == null)
            {
                return RedirectToAction("Index","Home");
            }
            else
            {
                var checkLike = db.likes.Where(m => m.article_id == aid && m.user_id == uid).FirstOrDefault();
                // ktra xem người này đã like bài này chưa
                // nếu like rôi thì xem cảm xúc có thay đổi gì không? Nếu có thì xóa 
                if (checkLike != null)
                {
                    if(checkLike.camxuc == emojii)
                    {
                        db.likes.Remove(checkLike);
                        db.SaveChanges();
                    }
                    else
                    {
                        db.likes.Remove(checkLike);
                        like obj = new like();
                        obj.article_id = aid;
                        obj.user_id = uid;
                        obj.camxuc = emojii;
                        db.likes.Add(obj);
                        db.SaveChanges();
                    }
                }
                // nếu thả cảm xúc lần đầu thì thêm vào csdl
                else
                {
                    like obj = new like();
                    obj.article_id = aid;
                    obj.user_id = uid;
                    obj.camxuc = emojii;
                    db.likes.Add(obj);
                    db.SaveChanges();

                }
            }
            // dem cam xuc
            int heart = db.likes.Where(m => m.article_id == aid && m.camxuc == "heart").Count();
            int smile = db.likes.Where(m => m.article_id == aid && m.camxuc == "smile").Count();
            int angry = db.likes.Where(m => m.article_id == aid && m.camxuc == "angry").Count();
            int cry = db.likes.Where(m => m.article_id == aid && m.camxuc == "cry").Count();
            int scared = db.likes.Where(m => m.article_id == aid && m.camxuc == "scared").Count();
            // tra ve client la so luot cam xuc moi
            return Json(new {heart=heart,smile=smile,angry=angry,cry=cry,scared=scared },JsonRequestBehavior.AllowGet);
        }
        // tag article
        public ActionResult Tag(string tags)
        {
            TempData["countTags"] = db.articles.Where(m => m.tags.Contains(tags) && m.status == 1).Count();
            return View();
        }
        // display tags
        public ActionResult DisplayTags(string tags, int skip = 0)
        {
            System.Threading.Thread.Sleep(1000);
            var result = (from a in db.articles
                          where a.status == 1 && a.tags.Contains(tags)
                          orderby a.views descending
                          select new Project.Models.baiviet
                          {
                              aid = a.article_id,
                              name = a.article_name,
                              day = a.created_at.ToString(),
                              img = a.thumbnail
                          }).Skip(skip).Take(6).ToList();
            return PartialView("_DisplayTags", result);
        }
        // Your article
        [Authorize]
        public ActionResult YourArticle(int ? page)
        {
            int id = (int)Session["uid"];
            var result = db.articles.Where(m => m.user_id == id).OrderByDescending(m => m.article_id).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
        }
        // thong ke luot xem bai viet
        public ActionResult Thongke(int id)
        {
            ArrayList xValue = new ArrayList();
            ArrayList yValue = new ArrayList();
            var result = (from a in db.statistics
                          where a.article_id == id
                          && a.created_at.Value.Month == DateTime.Now.Month
                          select a);
            result.ToList().ForEach(rs => xValue.Add(rs.created_at));
            result.ToList().ForEach(rs => yValue.Add(rs.view));
            new Chart(width: 600, height: 400, theme: ChartTheme.Green)
                .AddTitle("Thống kê lượt xem")
                .AddSeries("Default", chartType: "Column", xValue: xValue, yValues: yValue)
                .Write("bmp");
            return null;
        }
        // get nick name of user
        public string getNickname(int id)
        {
            string nName = "";
            var result = db.Users.Where(m => m.user_id == id).FirstOrDefault();
            if (result == null)
            {
                nName = "Unkown";
            }
            else
            {
                nName = result.nickname;
            }
            return nName;
        }
    }
}