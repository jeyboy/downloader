using System;
using System.Collections.Generic;

using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Parsenok
{
    [Serializable]
    public class Otstoy
    {
        public static int maxtry = 15;

        public string[] links;
        public string objname;
        public int cycles;

        public Otstoy(List<string> links, StylePair pair, int cycleready = 0)
        {
            this.links = links.ToArray();
            objname = pair.Name.Text;
            cycles = cycleready;
        }
    }

    [Serializable]
    public class Prototype
    {
        [NonSerialized]
        System.Windows.Forms.TextBox logs = null;

        /// <summary>Ожидание в милесекундах между закачками</summary>
        public int Pause { get; set; }

        public enum ProcType : int
        {
            NONE=-1,
            DOWNLOAD=0,
            SEARCH=1,
        }

        /// <summary>количество попыток поулчения ссылки и загрузки</summary>
        const int maxtry = 3;
        /// <summary>расширение по умолчанию</summary>
        public const string ext = ".(O_o)";

        bool pages = false;

        /// <summary>Ссылка - патерн позволяющая подставлять стиль и номер страницы</summary>
        protected string masklink = "";
        /// <summary>Ссылка по которой на странице мона спарсить стили</summary>
        protected string stylelink = "";
        /// <summary>Регулярка использущаяся для поиска ссылок на странице. По умолчанию ищются любые ссылки</summary>
        protected string mainlinkspattern;

        /// <summary>Сайты, ссылки на которые следует отложить для закачки</summary>
        public string[] sites = {/*"oron",*/"zippyshare" };
        protected string linkspattern { get { return mainlinkspattern; } set { mainlinkspattern = value; } }

        /// <summary>Отстойник для ссылок которые не прокачались</summary>
        public List<Otstoy> ots = new List<Otstoy>();
        /// <summary>Ссылки для текущего стиля</summary>
        public List<string> _findedresources = new List<string>();

        /// <summary>Флаг состояния класса</summary>
        public ProcType ptype;

        /// <summary>Индекс в списке закачки</summary>
        public int index=-1;
        /// <summary>Индекс в процессе поиска ссылок для прохода по номерам страниц</summary>
        int serindex=-1;
        /// <summary>Индекс в процессе поиска ссылок для прохода по ссылкам на след страницу</summary>
        string offserindex="";

        /// <summary>Имя объекта для которого парсятся стили</summary>
        public string Name {get; set;}

        /// <summary>Имя текущего обрабатываемого стиля</summary>
        public string currstyle = "";

        /// <summary>Инициализация стилей. Должна быть перегружена</summary>
        /// <returns>Массив стилей</returns>
        public virtual Style [] InitStyles()
        {
            throw new NotImplementedException();
            //return new[] { new Style("All","0") };
        }

        /// <summary>Инициализация количества страниц для стиля. Должна быть перегружена</summary>
        public virtual void InitPages(string styleidentificator, out int min, out int max)
        {
            throw new NotImplementedException();
        }

        /// <summary>Дополнительные манипуляции для получения ссылки. По умолчанию возвращает саму ссылку. При необходимости перегружается</summary>
        public virtual string GetLink(string pagelink)
        {
            return pagelink;
        }

        /// <summary>Получение ссылки на след страницу если парс идет не по номерам страниц. Должна быть перегружена</summary>
        /// <returns>Ссылка на новую страницу</returns>
        public virtual string GetNextPageLink(string currpagetext)
        {
            throw new NotImplementedException();
        }

        /// <summary>Конструктор</summary>
        /// <param name="Name">Имя объекта</param>
        /// <param name="parsebypages">флаг типа прохода при парсе (если установелн то постранично иначе на странице ищется ссылка на след страницу)</param>
        /// <param name="masklink">если parsebypages установлен то ссылка-маска для парса (* помечается место вставки идентификатора стиля, # место вставки номера страницы) иначе ссылка на страницу начала парса</param>
        /// <param name="stylelink">страница с которой производится парс стилей</param>
        public Prototype(string Name,bool parsebypages, string masklink,string stylelink="")
        {
            ptype = ProcType.NONE;
            this.Name = Name;
            pages = parsebypages;
            this.masklink = masklink;
            this.stylelink = stylelink;
            Pause = 0;
            linkspattern = "http\\://[^'^\"]*\\.html";
        }

        #region IO
            public static object Load(string path)
            {
                if (!File.Exists(path)) return null;

                try
                {
                    FileStream fs = new FileStream(path, FileMode.Open);
                    BinaryFormatter bf = new BinaryFormatter();
                    object res = bf.Deserialize(fs);
                    fs.Close();
                    return res;
                }
                catch
                {
                    return null;
                }
            }

            public void Save(string path)
            {
                if (File.Exists(path)) File.Delete(path);

                try
                {
                    FileStream fs = new FileStream(path, FileMode.Create);
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, this);
                    fs.Close();
                }
                catch
                {
                    return;
                }                
            }
        #endregion

        /// <summary>Поиск со смещением на следующую страницу</summary>
        /// <returns>Ссылка на следующую страницу</returns>
        string OffsetPatternSearch(string lastlink,string link,List<string> res, int deep = 0)
        {
            string text = Down._StreamtoText(Down._Openstream(new Uri(link)));

            Regex rex = new Regex(linkspattern);

            MatchCollection mc = rex.Matches(text);

            foreach (Match m in mc)
            {
                for (int beg = (m.Groups.Count > 1) ? 1 : 0; beg < m.Groups.Count; beg++)
                {
                    string temp = m.Groups[beg].Value;
                    bool sytesearchmatch = sites.Length == 0;

                    for (int loop1 = 0; loop1 < sites.Length; loop1++)
                        if (temp.IndexOf(sites[loop1]) != -1)
                        {
                            sytesearchmatch = true;
                            break;
                        }

                    if (sytesearchmatch)
                        if (lastlink != temp)
                            res.Add(temp);
                        else
                            return "";
                }
            }

            //InnerSearch(text, deep + 1);

            return (offserindex=GetNextPageLink(text));
        }
        /// <summary>Цикл поиска со смещением на следующую страницу</summary>
        /// <returns>Массив найденых ссылок</returns>
        void OffsetSearch(StylePair obj,string lnk)
        {
            string offserindex = lnk;
            int page = 0;

            while (true)
            {
                obj.Info.Text = "Поиск ( Страниц обработано : " + (++page)+" ) Найдено : ~"+_findedresources.Count;
                offserindex = OffsetPatternSearch(obj.lastlink, offserindex, _findedresources);
                if (offserindex.Length == 0) break;
            }

            offserindex = "";
        }

        /// <summary>Поиск по странице</summary>
        /// <returns>Флаг остановки поиска</returns>
        public virtual bool PagePatternSearch(string lastlink,string link, List<string> res, int deep = 0)
        {
            //if (deep > maxdeep && deep != 0) return true;
            bool boolka = true;

            string text = Down._StreamtoText(Down._Openstream(new Uri(link)));

            Regex rex = new Regex(linkspattern);
            MatchCollection mc = rex.Matches(text);

            foreach (Match m in mc)
            {
                for (int beg = (m.Groups.Count > 1) ? 1 : 0; beg < m.Groups.Count; beg++)
                {
                    string temp = m.Groups[beg].Value;
                    bool sytesearchmatch = sites.Length == 0;

                    for (int loop1 = 0; loop1 < sites.Length; loop1++)
                        if (temp.IndexOf(sites[loop1]) != -1)
                        {
                            sytesearchmatch = true;
                            break;
                        }

                    if (sytesearchmatch)
                        if (lastlink != temp)
                            res.Add(temp);
                        else
                            return false;
                }
            }

            if(Pause!=0) System.Threading.Thread.Sleep(Pause);

            //InnerSearch(text, deep + 1);
            return boolka;
        }
        /// <summary>Цикл поиска по страницам</summary>
        /// <returns>Массив найденых ссылок</returns>
        void PageSearch(StylePair obj,int min,int max)
        {
            if (max == 0) return;

            string link = masklink.Replace("*", obj.stylecode);

            for (serindex = min; serindex <= max; serindex++)
            {
                obj.Info.Text = "Поиск ( " + serindex + " из " + max + " ) Найдено : ~" + _findedresources.Count;

                string req = link.Replace("#", "" + serindex);
                if (!PagePatternSearch(obj.lastlink, req, _findedresources))
                    break;
            }

            serindex = -1;
        }

        /// <summary>Закачка файлов</summary>
        void Download(StylePair obj)
        {
            Down.DownloadFlag res = Down.DownloadFlag.Sucess;
            string title;

            for (; index >=0; index--)
            {
                if (_findedresources[index] == "") continue;
                obj.Info.Text = "Осталось : " + (index+1) + " (  ошибок : " + (_findedresources.Count - (index + 1)) + " )";

                try
                {
                    if (obj.Path.Text.Length == 0) {
                        _findedresources.Clear();
                        index = 0;
                        return;
                    }

                    string lnk = GetLink(_findedresources[index]);
                    string reff = lnk;

                    for (int tr = 0; tr < maxtry; tr++)
                    {
                        obj.SubInfo.Text = "Попытка получения ссылки № " + (tr + 1) + " из " + maxtry;
                        System.Windows.Forms.Application.DoEvents();
                        //string link = lnk == "Error" ? lnk : ((tr % 4 == 0) ? Down.__GetZippyShareLink4(lnk) : (tr % 4 == 1) ? Down.__GetZippyShareLink2(lnk) : (tr % 4 == 2) ? Down.__GetZippyShareLink1(lnk) : Down.__GetZippyShareLink3(lnk));
                        string link = lnk == "Error" ? lnk : Down.__GetZippyShareLinkUpdated(lnk);
                        res = Down.DownloadFlag.None;

                        if (link == "Deleted")
                        {
                            res = Down.DownloadFlag.Sucess;
                            logs.Text = "\t[" + link + "] " + lnk + "\r\n" + logs.Text;
                            break;
                        }

                        if (link == "Not exist")
                            continue;

                        if (link == "Error")
                        {
                            logs.Text = "\t\t[" + link + "] " + lnk + "\r\n" + logs.Text;
                            break;
                        }

                        res = Down._Downloadfile(300, reff, link, obj.Path.Text, obj, logs, out title);
                        obj.SubInfo.Text = "";
                        if (res == Down.DownloadFlag.Sucess)
                        {
                            logs.Text = "[Downloaded] " + title + "\r\n" + logs.Text;
                            break;
                        }
                        else if (res == Down.DownloadFlag.ConnectionClosed) {
                            logs.Text = "[Server fault] " + title + "\r\n" + logs.Text;
                            break;
                        }
                        else
                            logs.Text = "\t\t[" + link + "] " + lnk + "\r\n" + logs.Text;

                        #region Out of empty space
                        if (res == Down.DownloadFlag.EmptySpaceNeed)
                        {
                            //TODO: учесть возможность перехода на другой таск
                            DateTime date = DateTime.Now;
                            DriveInfo di = new DriveInfo(obj.Path.Text[0] + ":\\");
                            long tempfree = di.AvailableFreeSpace;
                            obj.SubInfo.Text = "Места нету. Ожидаю пока появится...";
                            while (true)
                            {
                                System.Threading.Thread.Sleep(10000);
                                di = new DriveInfo(obj.Path.Text[0] + ":\\");
                                if (tempfree != di.AvailableFreeSpace)
                                    break;
                                if ((DateTime.Now - date).TotalMinutes > 30)
                                    System.Windows.Forms.Application.Exit();
                            }
                        }
                        #endregion

                        if (tr + 1 == maxtry)
                            if (Pause != 0) System.Threading.Thread.Sleep(Pause);
                    }

                    if (res == Down.DownloadFlag.Sucess || res == Down.DownloadFlag.ConnectionClosed)
                        _findedresources.RemoveAt(index);
                }

                catch (Exception e) 
                {
                    /*if (e.Message == "Invalid URI: The URI is empty.")
                    {
                        if (_findedresources[0] == "")
                        {
                            obj.lastlink = (index + 1 < _findedresources.Count) ? _findedresources[index + 1] : _findedresources[index];
                            _findedresources.RemoveRange(0, index+1);
                            index = -1;
                        }
                        else
                            break;
                        
                    }
                    else System.Windows.Forms.MessageBox.Show(e.Message);*/ 
                }
            }

            obj.Info.Text = obj.SubInfo.Text = "";

            int ind = GetO(obj.Name.Text);
            if(ind!=-1) ots[ind].links = _findedresources.ToArray();
            else ots.Add(new Otstoy(_findedresources, obj));

            _findedresources.Clear();
        }

        /// <summary>Поиск ссылок</summary>
        void Search(StylePair obj)
        {
            obj.Info.Text = "Поиск";
            ptype = ProcType.SEARCH;

            if (pages)
            {
                int min, max;
                InitPages(obj.stylecode, out min, out max);

                //if (max < 1) max = 100;

                PageSearch(obj, serindex == -1 ? min : serindex, max);
            }
            else
                OffsetSearch(obj, offserindex.Length == 0 ? masklink : offserindex);

            for (int loop1 = 0; loop1 < _findedresources.Count; loop1++)
                for (int loop2 = loop1+1; loop2 < _findedresources.Count; loop2++)
                    if(_findedresources[loop2]==_findedresources[loop1])
                        _findedresources.RemoveAt(loop2--);

            obj.Info.Text = "";
        }

         /// <summary>Запуск новой закачки файлов</summary>
        public void Proc(StylePair obj)
        {
            if (logs == null)
                logs = (System.Windows.Forms.TextBox)Form1.logs;

            if (ptype != ProcType.DOWNLOAD)
            {
                currstyle = obj.Name.Text;
                Search(obj);

                if (_findedresources.Count > 0)
                {
                    obj.lastlink = _findedresources[0];
                    _findedresources.Insert(0, "");
                }

                AddO(GetO(currstyle));
                index = _findedresources.Count - 1;
            }
            else
            {
                //откат на несколько позиций в списке ссылок для избежания потерь
                if (index + 2 < _findedresources.Count)
                    index += 2;
                else index = _findedresources.Count - 1;
            }

            obj.Pro2.Visible = true;
            ptype = ProcType.DOWNLOAD;
            Download(obj);
            ptype = ProcType.NONE;

            obj.Pro2.Visible = false;
            currstyle=obj.Info.Text = "";
        }

        public int OtstoyCount()
        { 
            int ret=0;
            for (int loop1 = 0; loop1 < ots.Count; loop1++)
                ret += ots[loop1].links.Length;
            return ret;
        }

        public int GetO(string name)
        {
            for (int loop1 = 0; loop1 < ots.Count; loop1++)
                if (ots[loop1].objname == name)
                    return loop1;

            return -1;
        }
        public void AddO(int index)
        {
            if (index != -1)
            {
                for (int loop1 = 0; loop1 < ots[index].links.Length; loop1++)
                    if (_findedresources.IndexOf(ots[index].links[loop1]) == -1)
                        _findedresources.Add(ots[index].links[loop1]);
            }
        }

        public void Clear()
        {
            ptype = Prototype.ProcType.NONE;
            _findedresources.Clear();
            ots.Clear();
            index = -1;
            serindex = -1;
            offserindex = "";
        }
    }
}