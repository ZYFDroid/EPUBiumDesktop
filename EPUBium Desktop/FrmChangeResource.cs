using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace EPUBium_Desktop
{
    public partial class FrmChangeResource : Form
    {
        public FrmChangeResource()
        {
            InitializeComponent();
            doDGVAutoScale();
        }

        void doDGVAutoScale()
        {
            SizeF designTimeScale = new SizeF(192,192);// 200%缩放
            SizeF currentScale = CurrentAutoScaleDimensions;
            float mulX = currentScale.Width / designTimeScale.Width;
            float mulY = currentScale.Height / designTimeScale.Height;
            tblItems.Columns[1].Width = scale(tblItems.Columns[1].Width, mulX);
            tblItems.Columns[3].Width = scale(tblItems.Columns[3].Width, mulX);
            tblItems.RowTemplate.Height = scale(tblItems.RowTemplate.Height, mulY);
            tblItems.Columns[1].DefaultCellStyle.Padding = new Padding(scale(tblItems.Columns[1].DefaultCellStyle.Padding.Right,mulX));
            tblItems.Columns[3].DefaultCellStyle.Padding = new Padding(scale(tblItems.Columns[3].DefaultCellStyle.Padding.Right,mulX));
        }

        int scale(int i,float f)
        {
            return (int)((float)i * f);
        }

        Point scale(Point p,float fx,float fy)
        {
            Point s = new Point(scale(p.X,fx), scale(p.Y,fy));
            return s;
        }

        private void FrmChangeResource_Load(object sender, EventArgs e)
        {
            loadData();
        }
        Image defaultImage = Properties.Resources._default;
        void loadData()
        {
            string basePath = Path.Combine(".", "app", "respack");
            string[] list = Directory.GetFiles(basePath, "*.zip");
            List<ResPackInfo> resPackInfos = new List<ResPackInfo>();
            foreach (string file in list)
            {
                ResPackInfo resPackInfo = new ResPackInfo();
                resPackInfo.key = Path.GetFileName(file);
                try
                {
                    using (MyPakFile myPakFile = new MyPakFile(file))
                    {
                        try
                        {
                            Stream imageStream = myPakFile.OpenRead("pack.png");
                            if (imageStream != null)
                            {
                                using (Stream s = imageStream)
                                {
                                    resPackInfo.thumbnail = Image.FromStream(s);
                                }
                            }
                        }
                        catch { }
                        try
                        {
                            Stream metaStream = myPakFile.OpenRead("meta.json");
                            if (metaStream != null)
                            {
                                using (StreamReader s = new StreamReader(metaStream))
                                {
                                    resPackInfo.metadata = JsonConvert.DeserializeObject<ResPackMeta>(s.ReadToEnd());
                                    if (resPackInfo.metadata == null)
                                    {
                                        resPackInfo.metadata = new ResPackMeta();
                                    }
                                }
                            }
                            else
                            {
                                resPackInfo.metadata = new ResPackMeta();
                                
                            }

                            resPackInfo.metadata.safeValue(file);
                        }
                        catch { }
                    }
                }
                catch {
                    resPackInfo.thumbnail = null;
                    resPackInfo.metadata.name = Path.GetFileName(file);
                }
                resPackInfos.Add(resPackInfo);
            }

            tblItems.Rows.Clear();
            resPackInfos.ForEach(resPackInfo => {
                tblItems.Rows.Add(resPackInfo.key, resPackInfo.thumbnail, resPackInfo.metadata.ToString(), "链接");
                tblItems.Rows[tblItems.RowCount - 1].Cells[3].Tag = resPackInfo.metadata.link;
            });
        }

        class ResPackInfo
        {
            public Image thumbnail;
            public string key;
            public ResPackMeta metadata;
        }
        class ResPackMeta
        {
            public string name = "";
            public string description = "";
            public string author = "";
            public string version = "";
            public string link = "";

            public void safeValue(string filename)
            {
                name = name ?? Path.GetFileName(filename);
                description = description ?? "";
                author = author ?? "";
                version = version ?? "";
                link = link ?? "";

                name = name.Trim() == "" ? Path.GetFileName(filename) : name;


            }

            public override string ToString()
            {
                if(version == "" || author == "")
                {
                    return $"{name}\r\n \r\n ";
                }
                return $"{name} v{version}\r\n{description}\r\n作者：{author}";
            }
        }

        private void tblItems_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.ColumnIndex == 3 && e.RowIndex >= 0)
            {
                string link = (tblItems[e.ColumnIndex, e.RowIndex].Tag ?? (object)"").ToString();
                if(link.Trim() != "")
                {
                    if (link.StartsWith("http") && link.Contains("://"))
                    {
                        if (MessageBox.Show(this, "是否打开链接：\r\n" + link, "安全警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                        {
                            try
                            {
                                System.Diagnostics.Process.Start(link);
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        MessageBox.Show("未知的链接格式。按Ctrl+C可以复制内容\r\n"+link);
                    }
                }
            }
        }

        string selected = null;

        private void tblItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                selected = tblItems[0, e.RowIndex].Value.ToString();
            }
        }

        void setTheme(string key)
        {
            Properties.Settings.Default.respack = key;
            Properties.Settings.Default.Save();
            if(key == null || key.Length == 0)
            {

                Program.HtDocs.Overrides?.Dispose();
                Program.HtDocs.Overrides = null;
            }
            else
            {

                string path = Path.GetFullPath(Path.Combine(".", "app", "respack", key));
                Program.HtDocs.Overrides = new MyPakFile(path);
            }
            MessageBox.Show(this,"主题包已应用。在界面上右键刷新生效");
        }

        private void btnOpenDir_Click(object sender, EventArgs e)
        {
            string path = Path.GetFullPath(Path.Combine(".", "app", "respack"));
            if(path.Contains(" "))
            {
                path = "\"" + path + "\"";
            }
            System.Diagnostics.Process.Start("explorer",path);
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            setTheme("");
        }

        private void btnUse_Click(object sender, EventArgs e)
        {
            if(selected != null)
            {
                setTheme(selected);
            }
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "开发模式可以指定本地的一个目录作为资源包的根目录，应用后刷新生效，重启或点击恢复默认按钮可还原。");
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                Program.HtDocs.Overrides = new LocalDirRoot(Path.GetDirectoryName(openFileDialog1.FileName));
                MessageBox.Show(this, "主题包已应用。在界面上右键刷新生效");
            }
        }
    }
}
