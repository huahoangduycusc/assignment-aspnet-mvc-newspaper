using PagedList;
using Project.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project.Controllers
{
    public class BlogController : Controller
    {
        // GET: Blog
        NewspaperEntities db = new NewspaperEntities();
        public ActionResult Index()
        {
            TempData["countBlog"] = db.blogs.Count();
            return View();
        }
        // show blog
        public ActionResult ShowBlog(int skip = 0, int take = 4)
        {
            System.Threading.Thread.Sleep(1500);
            var result = (from b in db.blogs
                          join u in db.Users
                          on b.user_id equals u.user_id
                          orderby b.blog_id descending
                          select new Project.Models.viewBlog
                          {
                              bid = b.blog_id,
                              title = b.blog_title,
                              title_en = b.blog_title_en,
                              created_at = b.created_at.ToString(),
                              thumbnail = b.thumbnail,
                              nickname = u.nickname,
   
                          }).Skip(skip).Take(take).ToList();
            return PartialView("_BlogItem",result);
        }
        // view blog
        public ActionResult View(int id, string title)
        {
            var result = (from b in db.blogs
                          join u in db.Users
                          on b.user_id equals u.user_id
                          where b.blog_id == id
                          select new Project.Models.viewBlog
                          {
                              bid = b.blog_id,
                              title = b.blog_title,
                              title_en = b.blog_title_en,
                              msg = b.blog_msg,
                              msg_en = b.blog_msg_en,
                              created_at = b.created_at.ToString(),
                              uid = u.user_id,
                              thumbnail = b.thumbnail,
                              nickname = u.nickname,
                              avatar = u.avatar,
                              post = (int)u.post,
                              role = u.role
                          }).FirstOrDefault();
            return View(result);
        }
        // list
        [Authorize(Roles ="admin,btv,ctv,nb")]
        public ActionResult List(int? page)
        {
            var result = (from b in db.blogs
                          join u in db.Users
                          on b.user_id equals u.user_id
                          orderby b.blog_id descending
                          select new Project.Models.viewBlog
                          {
                              bid = b.blog_id,
                              title = b.blog_title,
                              created_at = b.created_at.ToString(),
                              thumbnail = b.thumbnail,
                              nickname = u.nickname,
                              uid = u.user_id
                          }).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
        }
        [Authorize(Roles = "admin,btv,ctv,nb")]
        // create
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult Create(blog b, HttpPostedFileBase thumbnail)
        {
            string filename = "";
            string extension = "";
            int uid = (int)Session["uid"];
            string name = "";
            try
            {
                if (ModelState.IsValid)
                {
                    if (thumbnail != null && thumbnail.ContentLength > 0)
                    {
                        filename = Path.GetFileName(thumbnail.FileName);
                        extension = Path.GetExtension(thumbnail.FileName).ToLower();
                        name = DateTime.Now.ToFileTime().ToString();
                        name += extension;
                        var allowExtension = new[] { ".png", ".jpg", ".jpeg" };
                        if (allowExtension.Contains(extension))
                        {
                            string foldername = Path.Combine(Server.MapPath("~/images/thumbnail"), name);
                            thumbnail.SaveAs(foldername);
                            blog obj = new blog();
                            obj.blog_title = b.blog_title;
                            obj.blog_msg = b.blog_msg;
                            obj.blog_title_en = b.blog_title_en;
                            obj.blog_msg_en = b.blog_msg_en;
                            obj.user_id = (int)Session["uid"];
                            obj.thumbnail = name;
                            obj.created_at = DateTime.Now;
                            db.blogs.Add(obj);
                            db.SaveChanges();
                            int a_id = (int)obj.blog_id;
                            ViewBag.Success = "Tạo chủ đề mới thành công, bạn có thể xem bài viết tại !<a href=\"" + Url.Action("View", "Blog", new { id = a_id }) + "\">Đây !</a>";
                            var thongbao = db.follows.Where(m => m.to_user == uid).ToList();
                            string mesTb = getNickname(uid) + " đã đăng blog <a href=\"" + Url.Action("View", "Blog", new { id = a_id }) + "\"><b>" + b.blog_title + "</b></a> lên trang chủ";
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
            catch (Exception ex)
            {
                ViewBag.Msg = ex.Message;
            }
            return View();
        }
        // del
        [Authorize(Roles ="admin,btv")]
        public ActionResult Delete(int id)
        {
            var result = db.blogs.Where(m => m.blog_id == id).FirstOrDefault();
            db.blogs.Remove(result);
            db.SaveChanges();
            return RedirectToAction("List", "Blog", null);
        }
        // edit 
        [Authorize(Roles = "admin,btv,ctv,nb")]
        public ActionResult Edit(int id)
        {
            var result = db.blogs.Where(m => m.blog_id == id).FirstOrDefault();
            return View(result);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult Edit(int id, blog b, HttpPostedFileBase thumbnail)
        {
            var result = db.blogs.Where(m => m.blog_id == id).FirstOrDefault();
            result.blog_title = b.blog_title;
            result.blog_msg = b.blog_msg;
            result.blog_title_en = b.blog_title_en;
            result.blog_msg_en = b.blog_msg_en;
            if(thumbnail != null && thumbnail.ContentLength > 0)
            {
                var allowExtension = new[] {".png",".jpg",".jpeg"}; // array chua dinh dang hop le
                string extension = Path.GetExtension(thumbnail.FileName).ToLower();
                string name = DateTime.Now.ToFileTime().ToString(); // tranh trung lap ten anh
                name += extension;
                if (!allowExtension.Contains(name))
                {
                    ViewBag.Msg = "Dinh dang tep tin khong hop le, duoi anh phai la .PNG, .JPG, .JPEG";
                }
                else
                {
                    string folder = Path.Combine(Server.MapPath("~/images/thumbnail"), name);
                    thumbnail.SaveAs(folder);
                    result.thumbnail = name;
                }
            }
            db.SaveChanges();
            ViewBag.Success = "Cập nhật dữ liệu mới thành công !";
            return View();
        }
        // get nick name of user
        public string getNickname(int id=-1)
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