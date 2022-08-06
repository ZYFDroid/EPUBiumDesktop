
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EPUBium_Desktop
{
    public partial class Form1 : Form
    {


        CoreWebView2 webView;
        WebView2 webViewControl;
        public Form1()
        {
            InitializeComponent();
            this.BackColor = Color.White;
            this.Icon = Properties.Resources.icon;
            try
            {
                webViewControl = new WebView2();
                Task<CoreWebView2Environment> createEnvTask = CoreWebView2Environment.CreateAsync(userDataFolder: Path.GetFullPath("app\\data\\cefdata"));
                createEnvTask.Wait();
                CoreWebView2Environment env = createEnvTask.Result;
                Controls.Add(webViewControl);
                webViewControl.Dock = DockStyle.Fill;
                webViewControl.EnsureCoreWebView2Async(env);
                webViewControl.CoreWebView2InitializationCompleted += WebView2_CoreWebView2InitializationCompleted;
            }catch (Exception ex)
            {
                if(ex.InnerException is WebView2RuntimeNotFoundException)
                {
                    MessageBox.Show("此计算机上没有安装WebView2运行时。\r\n访问：https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/consumer/ 获取运行时", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    string runtimeDownloadUrl = "https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/consumer/";
                    Process.Start(runtimeDownloadUrl);
                    Application.Exit();
                }
            }
            Disposed += Form1_Disposed;
            
        }

        private string startUrl = null;

        public Form1(string startUrl) : this()
        {
            this.startUrl = startUrl;
        }

        private void WebView2_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                webView = webViewControl.CoreWebView2;
                initWebView2();
                webViewControl.Source = new Uri(startUrl ?? "http://epub.zyf-internal.com");
                webViewControl.NavigationCompleted += WebViewControl_NavigationCompleted;
            }
            else
            {
                if(e.InitializationException is WebView2RuntimeNotFoundException)
                {
                    MessageBox.Show("此计算机上没有安装WebView2运行时。\r\n访问：https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/consumer/ 获取运行时", "",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    string runtimeDownloadUrl = "https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/consumer/";
                    Process.Start(runtimeDownloadUrl);
                    Application.Exit();
                }
                else
                {
                    MessageBox.Show(e.InitializationException.ToString(),"Web页面初始化失败");
                    Application.Exit();
                }
            }
        }

        private void WebViewControl_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            
        }

        private void WebView_DocumentTitleChanged(object sender, object e)
        {
            Invoke(new Action(() => {
                this.Text = webView.DocumentTitle + " - EPUBium Desktop " + Application.ProductVersion;
            }));
        }

        private void Form1_Disposed(object sender, EventArgs e)
        {
            webViewControl.Dispose();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Size = Properties.Settings.Default.windowsize;
            this.Location = Properties.Settings.Default.windowloc;
            this.WindowState = Properties.Settings.Default.ismaxium ? FormWindowState.Maximized : FormWindowState.Normal;
            Rectangle workingArea = Screen.FromControl(this).WorkingArea;
            if (!workingArea.Contains(this.Location))
            {
                this.Location = workingArea.Location;
            } 
        }

        void handleEvent(string eventType)
        {
            switch (eventType)
            {
                case "respack":
                    BeginInvoke(new Action(() => { new FrmChangeResource().ShowDialog(this); }));
                    break;
                default:
                    MessageBox.Show("未知Event："+eventType);
                    break;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Properties.Settings.Default.windowsize = this.Size;
            Properties.Settings.Default.windowloc = this.Location;
            Properties.Settings.Default.ismaxium = this.WindowState == FormWindowState.Maximized;
            Properties.Settings.Default.Save();
            base.OnFormClosing(e);
            Controls.Remove(webViewControl);
        }
        void initWebView2()
        {
            webView.DocumentTitleChanged += WebView_DocumentTitleChanged;
            webView.Settings.IsPinchZoomEnabled = false;
            webView.Settings.IsSwipeNavigationEnabled = false;
            webView.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            resourceHandler = new ResourceHandler(webView.Environment);
            webView.WebResourceRequested += WebView_WebResourceRequested;
            webView.FrameNavigationStarting += WebView_FrameNavigationStarting;
            webView.ContextMenuRequested += WebView_ContextMenuRequested;
            webView.NewWindowRequested += WebView_NewWindowRequested;
        }

        private void WebView_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            if (e.Uri.StartsWith(urlbase))
            {
                e.Handled = true;
                new Form1(e.Uri).Show();
            }
        }

        private void WebView_ContextMenuRequested(object sender, CoreWebView2ContextMenuRequestedEventArgs e)
        {
            CoreWebView2ContextMenuItem changeSkin = webView.Environment.CreateContextMenuItem("更换主题包", null, CoreWebView2ContextMenuItemKind.Command);
            changeSkin.CustomItemSelected += onChangeSkinClicked;
            e.MenuItems.Add(changeSkin);
        }

        private void onChangeSkinClicked(object sender, object e)
        {
            new FrmChangeResource().ShowDialog(this);
        }

        private void WebView_FrameNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (!e.Uri.StartsWith(urlbase))
            {
                e.Cancel = true;
            }
        }

        const string urlbase = "http://epub.zyf-internal.com";
        private void WebView_WebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            e.Response = handleRequest(e.Request);
            e.GetDeferral().Complete();
        }
        
        public CoreWebView2WebResourceResponse handleRequest(CoreWebView2WebResourceRequest request)
        {
            if (request.Uri.StartsWith(urlbase))
            {
                string path = request.Uri.Substring(urlbase.Length, request.Uri.Length - urlbase.Length);
                if (path.StartsWith("/"))
                {
                    path = path.Substring(1);
                }
                if (path == "" || path.EndsWith("/"))
                {
                    path += "index.html";
                }
                string mimetype = "text/html";
                if (path.Contains("."))
                {
                    string ext = path.Substring(path.LastIndexOf(".")).Replace(".", "");
                    mimetype = System.Web.MimeMapping.GetMimeMapping(path);
                }
                if (path.StartsWith("api/") || path.StartsWith("read/"))
                {
                    ApiResponse resp = Program.ApiModel.callApi(path);
                    if (resp.type == ApiResponseType.ErrorInternalError) { return resourceHandler.ForErrorMessage(resp.msg, System.Net.HttpStatusCode.InternalServerError); }
                    if (resp.type == ApiResponseType.ErrorNotFound) { return resourceHandler.ForErrorMessage(resp.msg, System.Net.HttpStatusCode.NotFound); }
                    if (resp.type == ApiResponseType.Stream)
                    {
                        return resourceHandler.FromStream(resp.stream, resp.msg, true);
                    }
                    if (resp.type == ApiResponseType.File)
                    {
                        return resourceHandler.FromFilePath(resp.msg, System.Web.MimeMapping.GetMimeMapping(resp.msg), true);
                    }
                    if (resp.type == ApiResponseType.String)
                    {
                        return resourceHandler.FromString(resp.msg, Encoding.UTF8, "text/html");
                    }
                    if(resp.type == ApiResponseType.Event)
                    {
                        handleEvent(resp.msg);
                        return resourceHandler.FromString("OK",Encoding.UTF8, "text/html");
                    }
                }
                Stream s = Program.HtDocs.OpenRead(path);
                if (s != null)
                {
                    return resourceHandler.FromStream(s, mimetype, autoDisposeStream: true);
                }
            }

            return webView.Environment.CreateWebResourceResponse(null, 403, "Forbidden", "");
            
        }

        ResourceHandler resourceHandler;
    }

    internal class ResourceHandler
    {
        private CoreWebView2Environment env;

        public ResourceHandler(CoreWebView2Environment env)
        {
            this.env = env;
        }

        internal CoreWebView2WebResourceResponse ForErrorMessage(string errMsg, HttpStatusCode errCode)
        {
            return env.CreateWebResourceResponse(null, (int)errCode, errMsg, "");
        }

        internal CoreWebView2WebResourceResponse FromFilePath(string path, string mimeType, bool autoClose)
        {
            FileStream fileStream = File.OpenRead(path);
            return env.CreateWebResourceResponse(fileStream, 200, "OK", "Content-Type: " + mimeType);
        }

        internal CoreWebView2WebResourceResponse FromStream(Stream s, string mimetype, bool autoDisposeStream)
        {
            return env.CreateWebResourceResponse(s, 200, "OK", "Content-Type: " + mimetype);
        }

        internal CoreWebView2WebResourceResponse FromString(string msg, Encoding encoding, string contentType)
        {
            MemoryStream ms = new MemoryStream(encoding.GetBytes(msg));
            return env.CreateWebResourceResponse(ms, 200, "OK", "Content-Type: " + contentType+", charset="+encoding.EncodingName);
        }
    }
}
