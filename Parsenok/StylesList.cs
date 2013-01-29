using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Parsenok
{
    public class Style
    {
        public string name;
        public string code;

        public Style(string Name, string Code)
        {
            name = Name;
            code = Code;
        }
    }

    public class StylePair
    {
        StylesList p;

        int SetPathWidth = 50;
        int PathLeft = 120;
        int PathWidth = 300;

        public int Top
        {
            get { return Name.Top; }
            set
            {
                Name.Top = Info.Top = value;
                Path.Top = value - 2;
                SetPath.Top = Canc.Top = value - 4;
                SubInfo.Top = Name.Top - SubInfo.Height - 5;
            }
        }

        public Label Name = new Label();
        public TextBox Path = new TextBox();
        public Button SetPath = new Button();
        public Button Canc = new Button();
        public Progress2 Pro2;
        public Label Info = new Label();
        public Label SubInfo = new Label();

        public string stylecode;
        public string lastlink = "";

        public StylePair(StylesList container, Style st, string path)
        {
            p = container;
            p.owner.objectscontainer.AutoScrollMargin = new System.Drawing.Size(1, 1);

            p.owner.objectscontainer.Controls.Add(Name);
            p.owner.objectscontainer.Controls.Add(Path);
            p.owner.objectscontainer.Controls.Add(SetPath);
            p.owner.objectscontainer.Controls.Add(Canc);
            p.owner.objectscontainer.Controls.Add(Info);
            p.owner.objectscontainer.Controls.Add(SubInfo);

            stylecode = st.code;
            Name.Text = st.name;
            SetPath.Text = "...";
            SetPath.Width = SetPathWidth / 2 - 2;
            Canc.Text = "Clr";
            Canc.Width = SetPathWidth / 2;

            Name.Left = 0;
            Path.Left = PathLeft + Name.Left;
            Path.Width = PathWidth;
            SetPath.Left = 5 + Path.Left + Path.Width;
            Canc.Left = SetPath.Left + SetPath.Width + 2;

            Info.Left = SetPathWidth + SetPath.Left + 5;
            SubInfo.AutoSize=Info.AutoSize = true;

            SubInfo.Left = Path.Left;
            SubInfo.Font = new System.Drawing.Font(SubInfo.Font,System.Drawing.FontStyle.Bold);

            Path.Text = path;
            p.tipok.SetToolTip(Path, path);

            Path.Enabled = false;

            Pro2 = new Progress2(Path,0, 100, 1, (int)(1024 * 1024 * 1.5f));

            SetPath.Click += new EventHandler(SetPath_Click);
            Canc.Click += new EventHandler(Canc_Click);

            p.tipok.SetToolTip(SetPath, "Установить путь\r\nТекущий путь : "+Path.Text);
            p.tipok.SetToolTip(Canc, "Очистить путь");
        }

        public void EnsureVisible()
        {
            if (p.owner.objectscontainer.VerticalScroll.Visible)
                p.owner.objectscontainer.AutoScrollPosition = new System.Drawing.Point(0, Top - p.owner.objectscontainer.DisplayRectangle.Y - 30);
        }
        public void SetPathDialog()        {SetPath_Click(this, null);}

        void Canc_Click(object sender, EventArgs e)
        {
            Path.Text = "";
            p.tipok.SetToolTip(Path, "Путь не указан");
            if (p.prototype.currstyle == Name.Text)
                p.prototype.index = -1;
        }

        void SetPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog f = new FolderBrowserDialog();

            if (f.ShowDialog() == DialogResult.OK)
            {
                Path.Text = f.SelectedPath;
                p.tipok.SetToolTip(SetPath, "Установить путь\r\nТекущий путь : "+Path.Text);
            }

            f.Dispose();
        }
    }

    public class StylesList
    {
        public Conveer owner;
        int topmargin = 20;
        int currtop = 0;
        CheckBox onoff;
        public ToolTip tipok = new ToolTip();

        List<StylePair> pairs = new List<StylePair>();

        public string Name { get; set; }
        public Prototype prototype = null;

        public int Top { get { return currtop; } }
        public int Count { get { return pairs.Count; } }
        public CheckState OnOffState { get { return onoff.CheckState; } set { onoff.CheckState = value; } }

        public StylesList(Conveer conv,Prototype proto,int top=0)
        {
            tipok.IsBalloon = true;
            owner = conv;
            prototype = proto;

            currtop = top;

            onoff = new CheckBox();
            onoff.ThreeState = true;
            onoff.Checked = false;
            Name = prototype.Name;
            onoff.Text=Name;
            onoff.Left = 5;
            onoff.Top = top + 5;
            onoff.AutoSize = false;
            onoff.Width = owner.objectscontainer.Width - 20;
            onoff.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            onoff.BackColor = System.Drawing.Color.LightBlue;

            currtop += 10 + onoff.Height;

            owner.objectscontainer.Controls.Add(onoff);
        }

        public void SetName(string addtext)
        {
            onoff.Text = Name + " ( " + addtext + " )";
        }

        public StylePair this[int index]
        {
            get { return index < Count && index > -1 ? pairs[index] : null; }
        }

        public void Add(Style style, string path = "")
        {
            pairs.Add(new StylePair(this, style, path));
            pairs[pairs.Count - 1].Top = (currtop += topmargin);
            currtop += 30;
        }

        public void Save(StreamWriter wri)
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(wri.BaseStream, onoff.CheckState == CheckState.Indeterminate ? 2 : onoff.CheckState == CheckState.Checked ? 1 : 0);
            bf.Serialize(wri.BaseStream, pairs.Count);

            for (int loop1 = 0; loop1 < pairs.Count; loop1++)
            {
                bf.Serialize(wri.BaseStream, pairs[loop1].Name.Text);
                bf.Serialize(wri.BaseStream, pairs[loop1].stylecode);
                bf.Serialize(wri.BaseStream, pairs[loop1].Path.Text);
                bf.Serialize(wri.BaseStream, pairs[loop1].lastlink);
            }
        }

        public void Load(StreamReader read)
        {
            string n, p, c,ll;
            BinaryFormatter bf = new BinaryFormatter();

            int ch = (int)bf.Deserialize(read.BaseStream);
            onoff.CheckState = (ch==0) ? CheckState.Unchecked : ch==1 ? CheckState.Checked : CheckState.Indeterminate;

            int lim = (int)bf.Deserialize(read.BaseStream);

            for(int loop1=0;loop1<lim;loop1++)
            {
                n = (string)bf.Deserialize(read.BaseStream);
                c = (string)bf.Deserialize(read.BaseStream);
                p = (string)bf.Deserialize(read.BaseStream);
                ll = (string)bf.Deserialize(read.BaseStream);
                Add(new Style(n, c), p);
                pairs[pairs.Count - 1].lastlink = ll;
            }
        }
        
        public int GetPos(string Name)
        {
            for (int loop1 = 0; loop1 < pairs.Count; loop1++)
                if (pairs[loop1].Name.Text == Name)
                    return loop1;

            return -1;
        }

        public void DropLastLinkFields()
        {
            for (int loop1 = 0; loop1 < pairs.Count; loop1++)
                pairs[loop1].lastlink = "";
        }
    }
}
