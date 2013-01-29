using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace Parsenok
{
    public class Conveer
    {
        public static int Top = 0;
        volatile bool work = false;
        Timer timer;

        System.Threading.Thread th = null;
        int localindex = 0,globalindex=0;
        
        public StylePair actpair = null;
        public StylesList actlist = null;

        public Panel objectscontainer = null;

        List<StylesList> styles=new List<StylesList>();

        public Conveer(Panel container)
        {
            InitTimer();
            objectscontainer = container;
        }

        void InitStyles(Prototype pt, StylesList sl)
        {
            Style[] st = pt.InitStyles();

            for (int loop1 = 0; loop1 < st.Length; loop1++)
                if (sl.GetPos(st[loop1].name) == -1)
                    sl.Add(st[loop1]);
        }

        public void AddObject(Prototype obj)
        {
            for (int loop1 = 0; loop1 < styles.Count; loop1++)
                if (styles[loop1].prototype.Name == obj.Name)
                {
                    InitStyles(obj,styles[loop1]);
                    return;
                }

            StylesList te = new StylesList(this,obj,styles.Count==0 ? Top : styles[styles.Count-1].Top);
            InitStyles(obj, te);

            styles.Add(te);
        }

        public void InitTimer() 
        {
            timer = new Timer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = 500;
        }
        public void Start() 
        {
            timer.Enabled = true; 
        }
        public void Stop() 
        {
            timer.Enabled = false;
            try { th.Abort(); }
            catch { }
        }

        void Routine()
        {
            this.work = true;
            this.Save(Application.StartupPath + '\\' + "amongstyle.(^_^)");
            DateTime maint = DateTime.Now;

            for (; localindex < actlist.Count; localindex++)
            {
                try
                {
                    if (actlist[localindex].Path.Text.Length > 0)
                    {
                        DateTime t = DateTime.Now;
                        actlist[localindex].EnsureVisible();
                        actlist.prototype.Proc((actpair = actlist[localindex]));

                        actlist.SetName("В отстойнике : " + actlist.prototype.OtstoyCount());
                        if ((DateTime.Now - t).TotalSeconds > 60) Save(Application.StartupPath + "\\conv.(O_o)");

                        Application.DoEvents();
                    }
                }

                catch (Exception e) { 
                    MessageBox.Show(e.Message,"Routine"); 
                }
                //catch { Save(Application.StartupPath + "\\crash.(O_o)"); }
            }

            if (actlist.OnOffState == CheckState.Indeterminate)
                actlist.OnOffState = CheckState.Unchecked;

            localindex = 0;
            actpair = null;
            if (++this.globalindex >= styles.Count) this.globalindex = 0;
            if ((DateTime.Now - maint).TotalSeconds > 60) Save(Application.StartupPath + "\\conv.(O_o)");
            this.work = false;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (!work)
            {
                if (styles[globalindex].OnOffState != CheckState.Unchecked)
                {
                    work = true;
                    actlist = styles[globalindex];

                    th = new System.Threading.Thread(Routine);

                    th.Priority = System.Threading.ThreadPriority.Highest;
                    th.Start();
                }
                else
                {
                    if (++this.globalindex >= styles.Count)
                        this.globalindex = 0;
                }
            }

            Application.DoEvents();    
        }

        #region IO
            public static Conveer Load(string path, Panel p)
            {
                if (!File.Exists(path)) return null;
                Conveer c = new Conveer(p);

                try
                {
                    StreamReader fs = new StreamReader(path);
                    BinaryFormatter bf = new BinaryFormatter();

                    c.globalindex = (int)bf.Deserialize(fs.BaseStream);
                    c.localindex = (int)bf.Deserialize(fs.BaseStream);
                    int lim = (int)bf.Deserialize(fs.BaseStream);

                    for (int loop1 = 0; loop1 < lim; loop1++)
                    {
                        Prototype prot = ((Prototype)bf.Deserialize(fs.BaseStream));

                        //int loo = 0;
                        //prot.ots.Clear();
                        //int y = 0;

                        StylesList stli = new StylesList(c, prot, c.styles.Count == 0 ? Top : c.styles[c.styles.Count - 1].Top);
                        stli.Load(fs);
                        stli.SetName("В отстойнике : "+prot.OtstoyCount());
                        c.styles.Add(stli);
                        //prot.ots[1].links = new string[] { };

                        //c.globalindex = c.localindex = 0;

                        //stli.prototype.sites = new string[] { };

                        //stli.prototype.sites = new [] {/*"oron",*/"zippyshare" };
                        //stli.DropLastLinkFields();
                        //stli.prototype.Clear();
                    }

                    fs.Close();
                }

                catch { return null; }

                return c;
            }

            public void Save(string path)
            {
                if (File.Exists(path))
                {
                    if (File.Exists(path + ".lastbackup"))
                        File.Delete(path + ".lastbackup");

                    File.Move(path,path+".lastbackup");
                    File.Delete(path);
                }

                StreamWriter wri = new StreamWriter(path);
                BinaryFormatter bf = new BinaryFormatter();

                bf.Serialize(wri.BaseStream, globalindex);
                bf.Serialize(wri.BaseStream, localindex);
                bf.Serialize(wri.BaseStream, styles.Count);

                for (int loop1 = 0; loop1 < styles.Count; loop1++)
                {
                    bf.Serialize(wri.BaseStream, styles[loop1].prototype);
                    styles[loop1].Save(wri);
                }

                wri.Close();
            }
        #endregion
    }
}