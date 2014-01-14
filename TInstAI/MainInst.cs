using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace TInstAI
{
    public class MainInst
    {
        private TouchInst m_touchInst;
        
        public MainInst()
        {
            RunMain();
        }

        private void RunMain()
        {
            string link = "http://stream.ru/films/";
            int countPageFilms = 0;

            try
            {
                var web = new HtmlWeb();
                var page = web.Load(link);
                countPageFilms = Inst.CountPageFilms(page);
            }
            catch (Exception e)
            {
                try
                {
                    var ya = Inst.OpenLink("google.com");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Подключитесь к интернету!");
                    Console.ReadLine();
                    return;
                }
                Console.WriteLine("Доступ к сайту временно приостановлен!");
                Console.ReadLine();
                return;
            }

            m_touchInst=new TouchInst();
            m_touchInst.DownloadImageInst += CompletedDownloadImg;

            for (int i = 0; i < 10; i++)
            {
                int selectNumberPage = 0;
                while (true)
                {
                    Console.Write("Введите страницу загрузки картинок диапазон от 1 до {0}: ", countPageFilms);
                    var numberPage = Console.ReadLine();
                    selectNumberPage = Inst.ConvertStringToInt(numberPage, countPageFilms);
                    if (selectNumberPage > 0)
                        break;
                    else
                        Console.WriteLine("Повторите попытку, вы ввели не корректное число");
                }
                //if (i!=0&&m_touchInst.IsDownloadImage)
                //{
                //    Console.WriteLine("Сначала загружаем новые изображения");
                //    m_touchInst.DownloadImageInst -= CompletedDownloadImg;
                //}
                //получаем id фильмов, для скачивания, с указанной страницы
                var ids = Inst.GetIdFilms(link + "?page=" + selectNumberPage);
                
                m_touchInst.GetImageInsts(ids);
                
                if (m_touchInst.IsDownloadImage)
                {
                    ////событие
                    //m_touchInst.DownloadImageInst += CompletedDownloadImg;
                }
                else
                {
                    Console.WriteLine("Изображения загружены с диска");
                }

            }

            Console.WriteLine("Всё");
            Console.ReadLine();
        }

        private void CompletedDownloadImg(Object sender, DownloadImageInstEventArgs e)
        {
            Console.WriteLine("Все изображения загружены на диск (событие)");
        }
    }
}
