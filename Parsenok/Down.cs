using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Noesis.Javascript;

namespace Parsenok
{
    class Down
    {
        static int timeout = 20000;
        static string agent = "Eye/0.2";

        public enum DownloadFlag : int
        {
            None = -1,
            Sucess = 0,
            ReadStreamOpenError = 1,
            WriteStreamOpenError = 2,
            ContentToSmall = 3,
            OfflineError = 4,
            ReadWriteError = 5,
            WrongLink = 6,
            Deleted = 7,
            EmptySpaceNeed=8,
            ConnectionClosed = 9,
        }

        private string _Getpath(string path, string URL) { return path + "\\" + URL.Substring(URL.LastIndexOf('/') + 1); }
        private static string CLEAR(string URL)
        {
            string res = URL.Substring(URL.LastIndexOf('/') + 1);
            for (int loop1 = 0; loop1 < res.Length; loop1++)
                if (loop1 < res.Length - 1)
                    if (res[loop1] == '%' && char.IsDigit(res[loop1 + 1]))
                    {
                        while (true)
                        {
                            res = res.Remove(loop1, 1);
                            if (loop1 == res.Length) break;
                            if (!char.IsDigit(res[loop1])) break;
                        }
                        res = res.Insert(loop1, " ");
                    }

            return res;
        }

        static CookieCollection Cookies = null;

        public static string FileSizeView(long num,char delim=' ',int dec=3)
        {
            string[] level = { " B", "Kb", "Mb", "Gb", "Tb", "Pb" };

            int fictdec = dec < 0 ? 0 : -1;
            if (dec < 0) dec = 0;
            float c = num;
            int loop1;

            for (loop1 = 0; loop1 < level.Length && c > 1023; loop1++)
            {
                c /= 1024f;
                if(fictdec!=-1) fictdec++;
            }

            if (fictdec != -1)
            {
                fictdec--;
                string[] n = ("" + Math.Round(c, fictdec)).Split(new[] { ',' }, StringSplitOptions.None);
                if (n.Length > 1)
                    return n[0] + delim + n[1].PadRight(fictdec, '0') + " " + level[loop1];
                else
                    return n[0] + (fictdec > 0 ? "" + delim : "") + "".PadRight(fictdec, '0') + " " + level[loop1];
            }
            else
            {
                string[] n = ("" + Math.Round(c, dec)).Split(new[] { ',' }, StringSplitOptions.None);
                if (n.Length > 1)
                    return n[0] + delim + n[1].PadRight(dec, '0') + " " + level[loop1];
                else
                    return n[0] + (dec > 0 ? "" + delim : "") + "".PadRight(dec, '0') + " " + level[loop1];
            }
        }

        public static DownloadFlag _Downloadfile(int pingvalue, string referer, string URL, string path, StylePair style, System.Windows.Forms.TextBox logs, out string clear)
        {
            HttpWebRequest req = null;
            HttpWebResponse res = null;
            Uri url = new Uri(URL);
            Stream stream = null;
            FileStream file = null;
            clear = CLEAR(url.LocalPath);
            clear = (clear == "") ? CLEAR(URL) : clear;
            clear = clear.Replace("+", " ").Replace(":", "").Replace("?", "").Replace("<", "").Replace(">", "").Replace("*", "");


            path = path + (path[path.Length - 1] != '\\' ? "\\" : "") + clear;

            byte[] buf;
            long temp = 12000, summ = 0, len = -1;
            int read = 0, tr = 0;
            style.Pro2.Clear();

            try
            {
                req = (HttpWebRequest)WebRequest.Create(url);
                req.Timeout = timeout;
                req.UserAgent = agent;
                //req.UserAgent = "Mozila/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; MyIE2;";
                req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                req.Headers.Add("Accept-Language", "ru-ru,ru;q=0.8,en-us;q=0.5,en;q=0.3");
                req.Headers.Add("Accept-Encoding", "gzip,deflate");
                req.Headers.Add("Accept-Charset", "windows-1251,utf-8;q=0.7,*;q=0.7");
                req.KeepAlive = true;


                //req.ReadWriteTimeout = pingvalue;//200;// 500;//1000;
                req.Referer = referer;

                if (Cookies != null && Cookies.Count > 0)
                    (req.CookieContainer = new CookieContainer()).Add(Cookies);

                res = (HttpWebResponse)req.GetResponse();
                len = res.ContentLength;

                if (len < 0) return DownloadFlag.WrongLink;
                style.Pro2.Max = (int)len;

                string inf = FileSizeView(len) + "  *  " + clear;
                style.SubInfo.Text = inf;
                System.Windows.Forms.Application.DoEvents();

                if (File.Exists(path))
                {
                    FileInfo f = new FileInfo(path);
                    if (f.Length != len)
                        File.Delete(path);
                    else
                    {
                        res.Close();
                        return DownloadFlag.Sucess;
                    }
                }

                //////////////////////////////////////////////////

                DriveInfo di = new DriveInfo(path[0]+":\\");
                if (di.AvailableFreeSpace <= len)
                {
                    while (true)
                    {
                        if (System.Windows.Forms.MessageBox.Show(style.Path, "На колотушке нету места однако.\r\n Выбрать другую папку ?\r\n В случае отказа ваше домашнее животное\r\n будет подвергнуто жестоким пыткам...", "Интимное предложение...", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        {
                            style.SetPathDialog();
                            di = new DriveInfo(style.Path.Text[0] + ":\\");
                            if (di.AvailableFreeSpace > len)
                            {
                                path = style.Path.Text + '\\' + clear;
                                break;
                            }
                        }
                        else
                        {
                            //фикс на случай если пользователь очистит место на диске пока высвечено окно запроса
                            di = new DriveInfo(style.Path.Text[0] + ":\\");
                            if (di.AvailableFreeSpace < len)
                                return DownloadFlag.EmptySpaceNeed;
                            else
                                break;
                        }
                    }
                }
                //////////////////////////////////////////////////

                file = new FileStream(path, FileMode.Create);
                if (file == null) return DownloadFlag.WriteStreamOpenError;

                stream = res.GetResponseStream();

                if (stream == null) return DownloadFlag.ReadStreamOpenError;
                DateTime d = DateTime.Now;
                double dod = 1;
                int alread = 0, uy = 1;
                double timelowlevel = 0.5d;

                while (true)
                {
                    System.Threading.Thread.Sleep(40); //25
                    if (temp < 6000) temp = 6000;
                    buf = new byte[temp];

                    try
                    {
                        if (dod >= timelowlevel)
                        {
                            d = DateTime.Now;
                            alread = 0;
                        }

                        alread+=(read = stream.Read(buf, 0, buf.Length));
                        file.Write(buf, 0, read);
                        //file.Flush();

                        dod =  (DateTime.Now - d).TotalSeconds;
                        if (dod >= timelowlevel)
                        {
                            uy = alread == 0 ? 0 : (int)(alread / dod);
                            style.SubInfo.Text = inf + "  ~ " + (uy == 0 ? "0" : FileSizeView(uy, ',', -1)) + "/сек";
                            System.Windows.Forms.Application.DoEvents();
                        }

                        if ((summ += read) >= len)
                            break;

                        try { style.Pro2.SetValue((int)summ, uy); }
                        catch { }

                        temp = (int)(temp * ((read == temp) ? 1.1f : 0.98f));
                        tr = 0;
                    }
                    catch (Exception e)
                    {
                        //System.Windows.Forms.MessageBox.Show(e.Message);
                        tr++;
                        logs.Text = " try(" + tr + "/40) " + e.Message + "\r\n" + logs.Text;
                        Boolean cc = e.Message == "Не удается прочитать данные из транспортного соединения: Соединение разорвано.";
                        if (tr > 40 || cc)
                        {
                            if (file.SafeFileHandle != null) file.Close();
                            stream.Close();
                            File.Delete(path);
                            return cc ? DownloadFlag.ConnectionClosed : DownloadFlag.ReadWriteError;
                        }
                    }
                }

                file.Flush();
                if (file.SafeFileHandle != null) file.Close();
                stream.Close();
                res.Close();
                style.Pro2.Clear();
            }

            catch (Exception e)
            {
                //System.Windows.Forms.MessageBox.Show(e.Message);
                try
                {
                    if (file != null) file.Close();
                    if (stream != null) stream.Close();
                }
                catch { }
                return DownloadFlag.ReadWriteError;
            }

            if (len > 0 && summ < len) return DownloadFlag.OfflineError;
            if (summ < 8 * 1024) return DownloadFlag.ContentToSmall;
            return DownloadFlag.Sucess;
        }

        static public Stream _Openstream(Uri uri, bool addcookie = false)
        {
            HttpWebRequest loHttp = (HttpWebRequest)WebRequest.Create(uri);
            //loHttp.Timeout = 50000;     // 10 secs
            //loHttp.UserAgent = "Web Client";

            loHttp.Timeout = timeout;
            loHttp.UserAgent = agent;
            //loHttp.UserAgent = "Mozila/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; MyIE2;";
            //loHttp.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            //loHttp.Headers.Add("Accept-Language", "ru-ru,ru;q=0.8,en-us;q=0.5,en;q=0.3");
            //loHttp.Headers.Add("Accept-Encoding", "gzip,deflate");
            //loHttp.Headers.Add("Accept-Charset", "windows-1251,utf-8;q=0.7,*;q=0.7");
            //loHttp.Referer = "https://www.google.com.ua/";


            loHttp.CookieContainer = new CookieContainer();

            if (Cookies != null && Cookies.Count > 0)
                loHttp.CookieContainer.Add(Cookies);

            try
            {
                HttpWebResponse loWebResponse = (HttpWebResponse)loHttp.GetResponse();
                if (addcookie)
                    Cookies.Add(loWebResponse.Cookies);
                else
                    Cookies = loWebResponse.Cookies;

                return loWebResponse.GetResponseStream();
            }
            catch(Exception e) { return null; }
        }

        static public string _StreamtoText(Stream s)
        {
            string text = "";
            if (s == null) return text;
            StreamReader w = new StreamReader(s, Encoding.UTF8);//Encoding.GetEncoding(1251));

            while (!w.EndOfStream)
            {
                try
                {
                    text += w.ReadLine();
                }

                catch
                {
                    s.Close();
                    return "Error";
                }
            }

            w.Close();
            return text.ToString();
        }

        static public string _GetInfo(string URL)
        {
            string res;
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URL);
            HttpWebResponse myHttpWebResponse = (HttpWebResponse)req.GetResponse();
            StreamReader myStreamReader = new StreamReader(myHttpWebResponse.GetResponseStream(), Encoding.GetEncoding(1251));
            res = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myHttpWebResponse.Close();
            req.Abort();
            return res;
        }

        static public void AddCookie(string name, string value, string domain, string path = "/")
        {
            if (Cookies == null)
                Cookies = new CookieCollection();

            Cookies.Add(new Cookie(name, value, path, domain));
        }

        /// <summary></summary>
        /// <param name="cookie">string viewed as 'name=value'</param>
        static public void AddCookie(string cookie, string domain, string path = "/")
        {
            string[] s = cookie.Split(new[] { '=' }, StringSplitOptions.None);
            AddCookie(s[0], s[1], domain, path);
        }

        #region ZippyShare
        static public string proceedJava(string block) {
            using (JavascriptContext context = new JavascriptContext())
            {

                // Setting external parameters for the context
                //context.SetParameter("message", "Hello World !");
                //context.SetParameter("number", 1);

                // Script
                context.Run(block);
                return (string)context.GetParameter("href");
            }
        }

        static public string __GetZippyShareLinkUpdated(string link)
        {
            Cookies = null;
            Stream s = Down._Openstream(new Uri(link));

            if (s == null) return "Error";

            string text = Down._StreamtoText(s).ToLower();
            //string text = "<a id=\"dlbutton\" href=\"#\"><img src=\"/images/download.png\" alt=\"download\" border=\"0\"></a>                                <span id=\"omg\" class=\"2\" style=\"display:none;\"></span><script type=\"text/javascript\">    document.getelementbyid('dlbutton').omg = \"abcdef\";    var a = 170724%1000;    var b = math.pow(a, 2);    document.getelementbyid('dlbutton').href = \"/d/98517114/\"+(b + document.getelementbyid('dlbutton').omg.length + 170724%93)+\"/domenico%20pepe%20feat.%20stella%20-%20il%20mio%20angelo%20blu%20%28dj%20spampy%20engel%20remix%29%20%28www.musicdjsmp3.com%29.mp3\";</script>                </div>";
            if (!ValidPage(text)) return "Deleted";

            Regex rex = new Regex("<script[^>]*>(?<all>[^<]*dlbutton[^<]*)</script>", RegexOptions.Singleline & RegexOptions.IgnoreCase);
            MatchCollection mc = rex.Matches(text);
            if (mc.Count == 0) return "Not exist";

            string lin;
            try { lin = proceedJava(mc[0].Groups["all"].Value.Replace("document.getelementbyid('dlbutton').", "")); }
            catch { return "Not exist";  }

            Uri r = new Uri(link);
            return r.Scheme + "://" + r.Host + lin;
        }

        static public bool ValidPage(string text) {
            return text.IndexOf("file does not exist on this server") == -1 && text.IndexOf("file has expired and does not exist anymore on this server") == -1;
        }
        #endregion
    }
}