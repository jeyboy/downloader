using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Parsenok
{
    public partial class Form1 : Form
    {
        public static TextBox logs;
        string path;
        string conv = "conv.(O_o)";
        Conveer c;

        Prototype[] pro = { new Clubland("Clubland", "http://clublandmp3.com/*/page/#/", "http://clublandmp3.com/") ,
                            new Clubtone("Clubtone","http://clubtone.net/load/club_music/2-#-2-0-0-0-*", "http://clubtone.net/load/club_music/2"),
                            new AllDj("All DJ","http://www.only-djmusic.com/"),
                            new RNB("RNB","http://musicbyrnblive.blogspot.com/"),
                            new MusicForDj("Music for DJ","http://www.music-for-dj.com/"),
                            new CoolMusic("Cool Music","http://coolmusic.at.ua/load/0-#","http://coolmusic.at.ua/load/0"),
                          };

        public Form1()
        {
            Form.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            logs = logs_collection;

            path = Application.StartupPath + '\\';
            c = Conveer.Load(path + conv, ground);
            if (c == null)  c = new Conveer(ground);

            for(int loop1=0;loop1<pro.Length;loop1++)
                c.AddObject(pro[loop1]);

            c.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            c.Stop();
            c.Save(path + conv);
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            if (c != null)
                if (c.actpair != null)
                    c.actpair.EnsureVisible();
        }
    }
}
