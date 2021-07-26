using CefSharp;
using CefSharp.Handler;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EPUBium_Desktop
{
    public partial class Form1 : Form
    {
        ChromiumWebBrowser webView;
        public Form1()
        {
            InitializeComponent();
            this.Size = Properties.Settings.Default.windowsize;
            this.Icon = Properties.Resources.icon;
            CefSettings setting = new CefSettings();
            
            setting.BrowserSubprocessPath = Application.ExecutablePath;
            setting.CachePath = Path.GetFullPath("app\\data\\cefdata\\cache");
            setting.LogFile = Path.GetFullPath("app\\data\\cefdata\\log\\cef" +DateTime.Now.Ticks+".log");
            setting.PersistUserPreferences = true;
            setting.UserDataPath = Path.GetFullPath("app\\data\\cefdata\\data");
            Disposed += Form1_Disposed;
            Cef.Initialize(setting);
            webView = new ChromiumWebBrowser("about:blank") { 
                RequestHandler = new LocalhostRequestHandler(),
                MenuHandler = new DevToolsMenuHandler()
            };
            webView.LoadUrlAsync("http://epub.zyf-internal.com");
            Controls.Add(webView);
        }

        private void Form1_Disposed(object sender, EventArgs e)
        {

            webView.Dispose();
            Cef.PreShutdown();
            Cef.Shutdown();
            int me = Process.GetCurrentProcess().Id;
            //Some process will not exit after close.
            //Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Application.ExecutablePath))
            //    .Where(f =>Path.GetFullPath(f.MainModule.FileName) == Path.GetFullPath(Application.ExecutablePath))
            //    .Where(f=>f.Id!=me)
            //    .ToList().ForEach(p => p.Kill());
            //Environment.Exit(0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {

            Properties.Settings.Default.windowsize = this.Size;
            Properties.Settings.Default.Save();
            base.OnFormClosing(e);
            Controls.Remove(webView);
        }


    }

    class LocalhostRequestHandler : RequestHandler
    {
        const string urlbase = "http://epub.zyf-internal.com";
        private LocalhostResourceHandler lhr = new LocalhostResourceHandler();
        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser,
            IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload,
            string requestInitiator, ref bool disableDefaultHandling)
        {
            if (request.Url.StartsWith(urlbase))
            {
                return lhr;
            }
            return base.GetResourceRequestHandler(chromiumWebBrowser, browser, frame, request, isNavigation, isDownload, requestInitiator, ref disableDefaultHandling);
        }

        protected override bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
        {
            if (targetUrl.StartsWith(urlbase))
            {
                return false;
            }
            else
            {
                if (userGesture)
                {
                    Process.Start("explorer", targetUrl);
                    return true;
                }
                return true;
            }
        }
    }

    class LocalhostResourceHandler : ResourceRequestHandler
    {
        const string urlbase = "http://epub.zyf-internal.com";
        protected override IResourceHandler GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            if (request.Url.StartsWith(urlbase))
            {
                string path = request.Url.Substring(urlbase.Length,request.Url.Length - urlbase.Length);
                if (path.StartsWith("/"))
                {
                    path = path.Substring(1);
                }
                if (path == "" || path.EndsWith("/")){
                    path += "index.html";
                }
                string mimetype = "text/html";
                if (path.Contains("."))
                {
                    string ext = path.Substring(path.LastIndexOf(".")).Replace(".","");
                    mimetype = System.Web.MimeMapping.GetMimeMapping(path);
                }
                if (path.StartsWith("api/") || path.StartsWith("read/"))
                {
                    ApiResponse resp = Program.ApiModel.callApi(path);
                    if(resp.type== ApiResponseType.ErrorInternalError) { return ResourceHandler.ForErrorMessage(resp.msg, System.Net.HttpStatusCode.InternalServerError); }
                    if(resp.type== ApiResponseType.ErrorNotFound) { return ResourceHandler.ForErrorMessage(resp.msg, System.Net.HttpStatusCode.NotFound); }
                    if(resp.type== ApiResponseType.Stream) {
                        return ResourceHandler.FromStream(resp.stream, resp.msg, true);
                    }
                    if (resp.type == ApiResponseType.File)
                    {
                        return ResourceHandler.FromFilePath(resp.msg, System.Web.MimeMapping.GetMimeMapping(resp.msg), true);
                    }
                    if (resp.type == ApiResponseType.String)
                    {
                        return ResourceHandler.FromString(resp.msg, Encoding.UTF8, false, "text/html");
                    }
                }
                Stream s = Program.HtDocs.OpenRead(path);
                if (s != null)
                {
                    return ResourceHandler.FromStream(s, mimetype, autoDisposeStream: true);
                }
            }
            return base.GetResourceHandler(chromiumWebBrowser, browser, frame, request);
        }
    }

    class DevToolsMenuHandler : ContextMenuHandler
    {
        protected override void OnBeforeContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            base.OnBeforeContextMenu(chromiumWebBrowser, browser, frame, parameters, model);
            model.Clear();
            model.AddItem(CefMenuCommand.SelectAll, "全选");
            model.AddItem(CefMenuCommand.Copy, "复制");
            model.AddItem(CefMenuCommand.Paste, "粘贴");
            model.AddSeparator();
            model.AddItem(CefMenuCommand.CustomFirst + 2, "回到主页");
            model.AddSeparator();
            model.AddItem(CefMenuCommand.CustomFirst + 1, "检查元素");
        }

        protected override bool OnContextMenuCommand(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            if (!parameters.IsCustomMenu)
            {
                if (commandId == CefMenuCommand.CustomFirst + 1)
                {
                    chromiumWebBrowser.ShowDevTools();
                }
                if (commandId == CefMenuCommand.CustomFirst + 2)
                {
                    chromiumWebBrowser.LoadUrlAsync("http://epub.zyf-internal.com");
                }
            }
            return base.OnContextMenuCommand(chromiumWebBrowser, browser, frame, parameters, commandId, eventFlags);
        }
    }
}
