using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace CoubDownloader
{

    public class Downloader
    {
        public static void Save(string url , string save_file )
        {
            if(url == string.Empty || url == null || url == "")
                return;
            using (var client = new System.Net.WebClient())
            {
                //string[] str_array = url.Split(new char[] { '\\', '/' });
                client.DownloadFileAsync(new Uri(url), save_file);
                //client.DownloadProgressChanged += (o, e) =>
                //{
                //    Console.WriteLine($"[{e.ProgressPercentage}]");
                //};
                Log.add($"Download: {save_file}");
            }
        }
        public static string GetJson(string coub_url )
        {
            using (WebClient client = new WebClient() { Encoding = Encoding.UTF8 })
            {
                client.Headers["User-Agent"] = "Mozilla/5.0";
                return client.DownloadString(coub_url);
            }
        }
    }
    public class JsonCoub
    {
        public string JSON;
        public string URL;
        public string FindSound()
        {
            if (JSON == string.Empty)
                return string.Empty;

            URL = Regex.Match(JSON, "(\"audio\":{\"high\":{\"url\")(:[\\w\\W]+?high\\.mp3\")").Value;

            URL = Regex.Replace(URL, "(\"audio\":{\"high\":{\"url\":)|(\")", "");
            return URL;
        }
        public string getchanelid()
        {
            // "channel_id":2600478,

            return Regex.Replace(  Regex.Match(JSON, "(\"channel_id\":[0-9]+)").Value , "(\"channel_id\":)" , "");
        }
        public string FindMedia()
        {
            if (JSON == string.Empty)
                return string.Empty;
            URL = Regex.Match(JSON, "({\"higher\":{\"url\":\")([\\w\\W]+?muted_huge\\.mp4\")").Value;
            URL = Regex.Replace(URL, "({\"higher\":{\"url\":\")|(\")", "");
            return URL;
        }
        public string GetFormat()
        {
            return Regex.Match(URL, @"((\.mp3)|(\.mp4))").Value;
        }
    }
    public class Coub
    {
        public string FOLDER = "Downloader";
        public Coub()
        {
            if (!Directory.Exists(FOLDER))
                Directory.CreateDirectory(FOLDER);
        }
        public string GetPathSave()
        {
            return Path.Combine(FOLDER, GetPermalinkCoub());
        }
        public void CreateFonderCurCoub()
        {
            if(!Directory.Exists(GetPathSave()))
                Directory.CreateDirectory(GetPathSave());   
        }
        public string Url { get; set; }
        public JsonCoub GetDataJson()
        {
            JsonCoub jsonCoub = new JsonCoub();
            if (IsCoub())
            {
                jsonCoub.JSON  = Downloader.GetJson(ConvertCoubURlToApi());
                CreateFonderCurCoub();

                File.WriteAllText(Path.Combine(GetPathSave(), jsonCoub.getchanelid() +  "_jsondata.json"), jsonCoub.JSON);

                return jsonCoub;
            }
            return jsonCoub;
        }
        public string ConvertCoubURlToApi()
        {
            
            return Regex.Replace(Url, @"(https)([\w\W]+?\/view\/)", @"https://coub.com/api/v2/coubs/");
        }
        
        public string GetPermalinkCoub()
        {
            return Regex.Replace(Url, @"(https)([\w\W]+?\/view\/)", "");
        }
        public bool IsCoub()
        {
            return Regex.IsMatch(Url, @"(https)(\:\/\/)(coub\.com\/view\/)([\w]+)");
        }
    }

    public class CustomClipboard
    {
        public string Text { get; set; }

        public bool isUpdate()
        {
            if (Clipboard.ContainsText())
            {
                string sr_ = "";
                try
                {
                    sr_ = Clipboard.GetText();
                    if (Text != sr_)
                    {
                        Text = sr_;
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Log.add(e.Message);
                    return false;
                }
                
                

            }
            return false;
        }
    }
    public static class Log
    {
        public static void add(string str)
        {
            string l = $"[{DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss")}] {str}";
            Console.WriteLine(l);
            File.AppendAllText("coublog.log", l + "\n");
        }
    }
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Coub coub = new Coub();
            CustomClipboard customClipboard = new CustomClipboard();
            while (true)
            {
                try
                {
                    if (!customClipboard.isUpdate())
                        continue;
                    coub.Url = customClipboard.Text;

                    if (!coub.IsCoub())
                        continue;
                    coub.CreateFonderCurCoub();
                    Log.add($"Coub: {customClipboard.Text}");

                    JsonCoub jsonCoub = coub.GetDataJson();

                    

                    string url = jsonCoub.FindSound();

                    Downloader.Save(url, Path.Combine(coub.GetPathSave(), jsonCoub.getchanelid() + "_" + coub.GetPermalinkCoub() + jsonCoub.GetFormat()));

                    url = jsonCoub.FindMedia();

                    Downloader.Save(url, Path.Combine(coub.GetPathSave(), jsonCoub.getchanelid() + "_" + coub.GetPermalinkCoub() + jsonCoub.GetFormat()));

                }
                catch (Exception e)
                {
                    Log.add(e.Message);
                }
            }
        }
        
    }
}
