using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;

namespace Parsenok
{
    [Serializable]
    class Clubland : Prototype
    {
        public override Style[] InitStyles()
        {
            List<Style> items = new List<Style>();
            string text = Down._StreamtoText(Down._Openstream(new Uri(base.stylelink)));

            Regex rex = new Regex("(?<1>ul class=\"bluebox\">.+?<\\/ul>)");

            MatchCollection mc = rex.Matches(text);
            foreach (Match m in mc)
            {
                rex = new Regex("href\\s*=\\s*(?:\"(?<1>[^\"]*)\"|(?<1>\\S+))");

                MatchCollection mcc = rex.Matches(m.Value);
                foreach (Match mm in mcc)
                {
                    string temp = mm.Value.Replace("href=", "").Replace("\"", "").Replace("\'", "");
                    temp = temp.Substring(0, temp.Length - 1);
                    temp = temp.Substring(temp.LastIndexOf('/') + 1);
                    items.Add(new Style(temp, temp));
                }
            }

            return items.ToArray();
        }
        public override void InitPages(string styleidentificator, out int min, out int max)
        {
            try
            {
                string urla = base.masklink.Replace("*", styleidentificator).Replace("#", "" + 1);

                string text = Down._StreamtoText(Down._Openstream(new Uri(urla)));
                int pos = text.IndexOf("<span class='pages'>");
                int epos = text.IndexOf("</span>", pos);

                string sub = text.Substring(pos, epos - pos);

                for (int loop1 = sub.Length - 1; loop1 >= 0; loop1--)
                    if (!char.IsDigit(sub[loop1]))
                    {
                        sub = sub.Substring(loop1 + 1);
                        break;
                    }

                min = 1;
                max = int.Parse(sub);
            }
            catch { min = max = 0; }
        }

        public Clubland(string name,string masklink, string stylelink)
            : base(name,true, masklink, stylelink)
        { }
    }

    [Serializable]
    class Clubtone : Prototype
    {
        public override string GetLink(string pagelink)
        {
            string zippp = "http://*.zippyshare.com/v/#/file.html";
            string text = Down._StreamtoText(Down._Openstream(new Uri(pagelink), false));
            if (text.Length < 500) return "";
            int pos = text.IndexOf("www=\"www");
            if (pos == -1) return "Error";

            int epos = text.IndexOf('"', pos + 5);
            string reslink = zippp.Replace("*", text.Substring(pos + 5, epos - pos - 5));

            pos = text.IndexOf("file=", epos) + 6;
            epos = text.IndexOf('"', pos);

            reslink = reslink.Replace("#", text.Substring(pos, epos - pos));
            return reslink;
        }
        public override Style[] InitStyles()
        {
            List<Style> items = new List<Style>();
            string text = Down._StreamtoText(Down._Openstream(new Uri(base.masklink), false));
            int pos = text.IndexOf("<select size=\"1\" name=\"filter2\"");

            if (pos != -1)
            {

                int epos = text.IndexOf("</select>", pos);

                string sub = text.Substring(pos, epos - pos);

                /////////////////////////////////////////////////
                Regex rex = new Regex("<option value=\"(?<1>\\d+)\"[^>]*>(?<2>[^<]+)");

                MatchCollection mc = rex.Matches(sub);
                foreach (Match m in mc)
                {
                    items.Add(new Style((m.Groups[2].Value == "- Выберите стиль -" ? "Все стили" : m.Groups[2].Value.Replace("#39;", "'")), m.Groups[1].Value));
                }
            }

            return items.ToArray();
        }
        public override void InitPages(string styleidentificator, out int min, out int max)
        {
            try
            {
                string urla = base.masklink.Replace("*", styleidentificator).Replace("#", "" + 1);

                string text = Down._StreamtoText(Down._Openstream(new Uri(urla), false));
                Regex rex = new Regex("onclick=\"spages.'(?<1>[^']+)'");

                MatchCollection mc = rex.Matches(text);
                max = int.Parse(mc[mc.Count - 1].Groups[1].Value);
                min = 1;
            }
            catch { min = max = 0; }
        }

        public Clubtone(string name,string masklink, string stylelink)
            : base(name,true, masklink, stylelink)
        {
            Pause = 15000;
            sites = new string [] { };
            linkspattern = "<div class=\"eTitle\"[^>]*><a href=\"(?<1>[^\"]*)\"";
        }
    }

    [Serializable]
    class AllDj : Prototype
    {
        public override string GetNextPageLink(string currpagetext)
        {
            string next = "<a class='blog-pager-older-link' href='";
            int pos = currpagetext.IndexOf(next);

            if (pos == -1) return "";
            else pos += next.Length;

            int epos = currpagetext.IndexOf('\'', pos);
            return currpagetext.Substring(pos, epos - pos);
        }
        public override Style[] InitStyles()
        {
            return new[] { new Style("All", "All") };
        }

        public AllDj(string name,string parselink)
            : base(name,false, parselink)
        { }
    }

    [Serializable]
    class RNB : Prototype
    {
        public override string GetNextPageLink(string currpagetext)
        {
            string next = "<a class='blog-pager-older-link' href='";
            int pos = currpagetext.IndexOf(next);

            if (pos == -1) return "";
            else pos += next.Length;

            int epos = currpagetext.IndexOf('\'', pos);
            return currpagetext.Substring(pos, epos - pos);
        }
        public override Style[] InitStyles()
        {
            return new[] { new Style("All", "All") };
        }

        public RNB(string name,string parselink)
            : base(name,false, parselink)
        { }
    }

    [Serializable]
    class MusicForDj : Prototype
    {
        public override string GetNextPageLink(string currpagetext)
        {
            string next = "<a class='blog-pager-older-link' href='";
            int pos = currpagetext.IndexOf(next);

            if (pos == -1) return "";
            else pos += next.Length;

            int epos = currpagetext.IndexOf('\'', pos);
            return currpagetext.Substring(pos, epos - pos);
        }
        public override Style[] InitStyles()
        {
            return new[] { new Style("All", "All") };
        }

        public MusicForDj(string name,string parselink)
            : base(name,false, parselink)
        { }
    }

    [Serializable]
    class CoolMusic : Prototype
    {
        public override void InitPages(string styleidentificator, out int min, out int max)
        {
            try
            {
                string urla = base.masklink.Replace("*", styleidentificator).Replace("#", "" + 1);

                string text = Down._StreamtoText(Down._Openstream(new Uri(urla), false));
                Regex rex = new Regex("onclick=\"spages.'(?<1>[^']+)'");

                MatchCollection mc = rex.Matches(text);
                max = int.Parse(mc[mc.Count - 1].Groups[1].Value);
                min = 1;
            }
            catch { min = max = 0; }
        }
        public override Style[] InitStyles()
        {
            return new[] { new Style("All", "All") };
        }

        public override bool PagePatternSearch(string lastlink, string link, List<string> res, int deep = 0)
        {
            string zippp = "http://*.zippyshare.com/v/#/file.html";

            string _link = "var zippywww=\"(?<1>[^\"]*)\";var zippyfile=\"(?<2>[^\"]*)\"";

            bool boolka = true;

            string text = Down._StreamtoText(Down._Openstream(new Uri(link)));

            Regex rex = new Regex(_link);
            MatchCollection mc = rex.Matches(text);

            foreach (Match m in mc)
            {
                string zlink = zippp.Replace("*", m.Groups[1].Value).Replace("#", m.Groups[2].Value);

                if (lastlink != zlink)
                {
                    if (res.IndexOf(zlink) == -1)
                        res.Add(zlink);
                }
                else
                    return false;
            }

            if (Pause != 0) System.Threading.Thread.Sleep(Pause);

            //InnerSearch(text, deep + 1);
            return boolka;
        }

        public CoolMusic(string name,string masklink, string stylelink)
            : base(name,true, masklink, stylelink)
        {
            
        }
    }
}