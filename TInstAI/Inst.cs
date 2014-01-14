using System;
using System.Collections.Generic;
using System.IO;
using HtmlAgilityPack;

namespace TInstAI
{
    internal static class Inst
    {
        /// <summary>
        /// Получаем содержимое странице по указанной ссылки
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public static HtmlDocument OpenLink(string link)
        {
            var web = new HtmlWeb();
            return web.Load(link);
        }

        /// <summary>
        /// Получаем количество страниц с фильмами
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static int CountPageFilms(HtmlDocument page)
        {
            var liPaginator=page.DocumentNode.SelectNodes("//ul[contains(concat(' ', @class, ' '), ' paginator ')]/li");
            if (liPaginator == null) return 0;
            var count = liPaginator[liPaginator.Count - 2].ChildNodes[0].InnerText.Trim();
            return string.IsNullOrEmpty(count) ? 0 : Convert.ToInt32(count);
        }

        public static List<string> GetIdFilms(string link)
        {
            var page = OpenLink(link);
            var ids = new List<string>();
            //*[id="films"]/ul[class="item-list"]/li/div[class="description"]/a
            var aFilms = page.DocumentNode.SelectNodes("//section[contains(concat(' ', @id, ' '), ' films ')]/ul[contains(concat(' ', @class, ' '), ' item-list ')]/li/div[contains(concat(' ', @class, ' '), ' description ')]/a");
            foreach (var aFilm in aFilms)
            {
                var id = GetIdFilm(aFilm.Attributes["href"].Value);
                if(!string.IsNullOrEmpty(id)) ids.Add(id);
            }
            return ids;
        }

        public static string GetIdFilm(string linkFilm)
        {
            linkFilm = linkFilm.Replace("/", "");
            var numberId = linkFilm.IndexOf("id");
            if (numberId > 0)
            {
                var id = linkFilm.Substring(numberId + 2);
                return id;
            }
            return null;
        }

         
        public static int ConvertStringToInt(string str,int maxValue)
        {
            
            if (!string.IsNullOrEmpty(str))
            {
                int i = 0;
                try
                {
                    i = Convert.ToInt32(str);
                }
                catch (Exception e)
                {
                    i = 0;
                }
                
                if (i > 0 && i <= maxValue)
                {
                    return i;
                }
                
            }
            
            return 0;
        }

        
    }
}
