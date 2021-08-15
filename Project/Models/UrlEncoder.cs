using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Project.Models
{
    public static class UrlEncoder
    {
        public static string ToFriendlyUrl(this string phrase, int maxLength = 100)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string str = phrase.ToLower().Normalize(NormalizationForm.FormD);
            str = regex.Replace(str, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"[\s-]+", " ").Trim();
            str = str.Substring(0, str.Length <= maxLength ? str.Length : maxLength).Trim();
            str = Regex.Replace(str, @"\s", "-");

            return str;
        }
    }
}