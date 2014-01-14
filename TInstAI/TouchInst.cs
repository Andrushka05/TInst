using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Caching;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TInstAI
{
    internal sealed class TouchInst
    {
        private const string NameKeyCache = "imageInst";
        private const string ImageUrl = "http://media.omlet.ru/media/img/movies/hposter/magnit/";
        private readonly string m_pathFolderImages;
        private List<string> m_notDownloadIds;
        private List<WebClient> m_clientList;
        public event EventHandler<DownloadImageInstEventArgs> DownloadImageInst;
        private List<ImageInst> m_images;
        

        public TouchInst(int countImageInMemory = 16, int maxCountThreadDownload = 0)
        {
            CountImageInMemory = countImageInMemory;
            MaxCountThreadDownload = maxCountThreadDownload == 0 ? Environment.ProcessorCount:maxCountThreadDownload;
            m_pathFolderImages = Environment.CurrentDirectory + "\\Images\\";
            IsDownloadImage = false;
            m_notDownloadIds = new List<string>();
            m_clientList=new List<WebClient>();
            m_images=new List<ImageInst>();
        }

        
        public int CountImageInMemory { get; set; }
        public int MaxCountThreadDownload { get; set; }
        public bool IsDownloadImage { get; set; }
        public List<ImageInst> ImageInts{get { return m_images; }}

        protected void OnDownloadImageInsts(DownloadImageInstEventArgs e)
        {
            EventHandler<DownloadImageInstEventArgs> temp = Volatile.Read(ref DownloadImageInst);
            if (temp != null) temp(this, e);
        }

        public void CompletedDownloadImageInst(List<ImageInst> imageInsts)
        {
            DownloadImageInstEventArgs e=new DownloadImageInstEventArgs(imageInsts);
            OnDownloadImageInsts(e);
        }

        public void GetImageInsts(List<string> idList)
        {
            if (IsDownloadImage)
            {
                m_clientList.ForEach(x=>x.CancelAsync());
                m_clientList.Clear();
                IsDownloadImage = false;
                Console.WriteLine("Delete List webclient");
            }
            
            List<ImageInst> imageOnLoad;
            var memory = GetFromCache();
            
            if (memory != null&& memory.Any())
            {
                //ищем какие в памяти
                var joinList = memory.Join(idList, mem => mem.Id, id => id,
                    (m, d) => new ImageInst() { Id = m.Id, ImageBytes = m.ImageBytes });
                imageOnLoad = joinList.ToList();
                foreach (var join in joinList)
                {
                    idList.Remove(join.Id);
                }
            }
            else
                imageOnLoad = new List<ImageInst>();

            var notDownloadId = SearchImageIds(idList, false);
            if (notDownloadId.Any())
            {
                foreach (var imageInst in notDownloadId)
                {
                    //Новые добавляем в начало
                    m_notDownloadIds.Insert(0,imageInst);
                }
            }
            if (m_notDownloadIds.Count > 0)
            {
                DownloadImages(m_notDownloadIds.ToArray());
            }

            List<string> idInFolder = SearchImageIds(idList, true);

            if (idInFolder.Any())
            {
                foreach (var image in ReadFileImages(idInFolder))
                {
                    imageOnLoad.Insert(0, image);
                }
            }
            
            if (imageOnLoad.Any())
            {
                var count = imageOnLoad.Count < CountImageInMemory
                    ? imageOnLoad.Count
                    : CountImageInMemory;
                for(int i=0;i<count;i++)
                    m_images.Insert(0,imageOnLoad[i]);
                
                if(m_images.Count>CountImageInMemory)
                    m_images.RemoveRange(CountImageInMemory-1,m_images.Count);

                SaveInCach(m_images);
            }

        }
        /// <summary>
        /// Поиск файлов на диске
        /// </summary>
        /// <param name="list"></param>
        /// <param name="isDownload">загруженные?</param>
        /// <returns></returns>
        private List<string> SearchImageIds(List<string> list, bool isDownload)
        {

            foreach (var id in list.Reverse<string>())
            {
                var pathFile = m_pathFolderImages + id + "_min.jpg";
                if (isDownload)
                {
                    if (!File.Exists(pathFile))
                    {
                        list.Remove(id);
                    }
                }
                else
                {
                    if (File.Exists(pathFile))
                    {
                        list.Remove(id);
                    }
                }
            }
            return list;
        }

        public List<ImageInst> ReadFileImages(IEnumerable<string> ids)
        {
            if (ids != null && ids.Any())
            {
                List<ImageInst> result = new List<ImageInst>();
                foreach (var id in ids)
                {
                    var pathFile = m_pathFolderImages + id + "_min.jpg";
                    result.Add(new ImageInst() { Id = id, ImageBytes = File.ReadAllBytes(pathFile) });
                }
                return result;
            }
            return null;
        }

        public void RemoveFromCache()
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove(NameKeyCache);
        }

        private List<ImageInst> GetFromCache()
        {
            ObjectCache cache = MemoryCache.Default;
            var memory = cache[NameKeyCache] as List<ImageInst>;
            return memory;
        }

        private void SaveInCach(List<ImageInst> value)
        {
            ObjectCache cache = MemoryCache.Default;
            var policy = new CacheItemPolicy();
            var filePaths = new List<string>
                    {
                        Environment.CurrentDirectory+"\\XmlFiles\\ImageInst.xml"
                    };
            policy.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths));
            cache.Set(NameKeyCache, value, policy);
        }

        private async Task<ImageInst> _GetImageToIdTask(string id)
        {
            byte[] result=null;
            try
            {
                var client = new WebClient();
                m_clientList.Add(client);
                result=await client.DownloadDataTaskAsync(ImageUrl + id + "/760_308.jpg");
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    var resp = (HttpWebResponse)e.Response;
                    switch (resp.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            Console.WriteLine("Error 404: id=" + id);
                            break;
                        case HttpStatusCode.Forbidden:
                            Console.WriteLine("Error 403: id=" + id);
                            break;
                        case HttpStatusCode.InternalServerError:
                            Console.WriteLine("Error 500: id=" + id);
                            break;
                        default:
                            Console.WriteLine("Error response: id=" + id);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error response: id=" + id);
            }
            
            if (result != null)
            {
                File.WriteAllBytes(m_pathFolderImages + id + "_min.jpg", result);
                Console.WriteLine(id+" the end");
                return new ImageInst() {Id = id, ImageBytes = result};
            }
            return null;
        }

        private async void DownloadImages(string[] ids)
        {
            int count = ids.Count();
            IsDownloadImage = true;
            
            var tasks = new List<Task<ImageInst>>();
            
            for (int i = 0; i < count; i++)
            {
                tasks.Add(_GetImageToIdTask(ids[i]));
            }

            await Task.WhenAll(tasks);

            if (m_images.Count < CountImageInMemory)
            {
                int countImage = CountImageInMemory - m_images.Count;
                if (countImage > tasks.Count)
                    countImage = tasks.Count;
                for(int i=0;;i++)
                {
                    if (countImage == 0)
                        break;
                    m_images.Insert(0,tasks[i].Result);
                    countImage--;
                }
            }
            IsDownloadImage = false;
            m_clientList.Clear();
            m_notDownloadIds.Clear();
            CompletedDownloadImageInst(m_images);
            SaveInCach(m_images);
        }
    }
}
