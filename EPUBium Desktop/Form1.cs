﻿
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
            this.Size = Properties.Settings.Default.windowsize;
            this.Icon = Properties.Resources.icon;

            webViewControl = new WebView2();
            Task<CoreWebView2Environment> createEnvTask = CoreWebView2Environment.CreateAsync(userDataFolder: Path.GetFullPath("app\\data\\cefdata"));
            createEnvTask.Wait();
            CoreWebView2Environment env = createEnvTask.Result;
            Controls.Add(webViewControl);
            webViewControl.Dock = DockStyle.Fill;
            webViewControl.EnsureCoreWebView2Async(env);
            webViewControl.CoreWebView2InitializationCompleted += WebView2_CoreWebView2InitializationCompleted;
            
            Disposed += Form1_Disposed;
            
        }

        private void WebView2_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                webView = webViewControl.CoreWebView2;
                initWebView2();
                webViewControl.Source = new Uri("http://epub.zyf-internal.com");
            }
            else
            {
                if(e.InitializationException is WebView2RuntimeNotFoundException)
                {
                    MessageBox.Show("此计算机上没有安装WebView2运行时。\r\n访问：https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/#download-section 获取运行时","",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    string runtimeDownloadUrl = "https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/#download-section";
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
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Properties.Settings.Default.windowsize = this.Size;
            Properties.Settings.Default.Save();
            base.OnFormClosing(e);
            Controls.Remove(webViewControl);
        }
        void initWebView2()
        {
            webView.DocumentTitleChanged += WebView_DocumentTitleChanged;
            webView.Settings.IsPinchZoomEnabled = false;
            webView.Settings.IsSwipeNavigationEnabled = true;
            webView.AddWebResourceRequestedFilter("http://epub.zyf-internal.com/*", CoreWebView2WebResourceContext.All);
            resourceHandler = new ResourceHandler(webView.Environment);
            webView.WebResourceRequested += WebView_WebResourceRequested;
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
