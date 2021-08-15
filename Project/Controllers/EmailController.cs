using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project.Models;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Project.Controllers
{
    public class EmailController : Controller
    {
        NewspaperEntities db = new NewspaperEntities();
        // GET: Email
        public ActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Index(string user, string email)
        {
            System.Threading.Thread.Sleep(1000);
            string msg = ""; // return text for client 
            var result = db.Users.Where(m => m.username == user && m.email == email).FirstOrDefault();
            if(result == null) // no result
            {
                msg = "Tài khoản và email không trùng khớp !";
                return Json(new { msg = msg,success="" },JsonRequestBehavior.AllowGet);
            }
            else
            {
                MailMessage mail = new MailMessage("huahoangduy.cusc@gmail.com", email);
                mail.Subject = "Reset Password - CTY BÁO CHÍ THỜI ĐẠI MỚI - HHD";
                string password = GetRandomPassword();
                mail.Body = "Mật khẩu mới của bạn là <b>"+password+"</b> <br>Vui lòng không tiết lộ mật khẩu với bất kỳ ai kể cả admin.";
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                NetworkCredential nc = new NetworkCredential("huahoangduy.cusc@gmail.com", "lanhuthedo");
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = nc;
                smtp.Send(mail);
                result.password = encrypt(password); // ma hoa 
                db.SaveChanges(); // save new password
                msg = "Chúng tôi đã gửi đến địa chỉ email bạn mật khẩu mới, vui lòng kiểm tra hộp thư !";
                return Json(new { success = msg,msg="" }, JsonRequestBehavior.AllowGet);
            }
            // end if
        }
        // tao mat khau ngau nhien
        public string GetRandomPassword()
        {
            int length = 8;
            const string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder sb = new StringBuilder();
            Random rnd = new Random();
            for (int i = 0; i < length; i++)
            {
                int index = rnd.Next(chars.Length);
                sb.Append(chars[index]);
            }
            return sb.ToString();
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
    }
}