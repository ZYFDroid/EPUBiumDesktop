﻿using CefSharp.BrowserSubprocess;
using CefSharp.RenderProcess;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml.Linq;

namespace EPUBium_Desktop
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        /// 

        public static MyPakFile HtDocs = null;
        public static DBUtils DBUtils = null;
        public static ApiModel ApiModel = null;

        [STAThread]
        static int Main(string[] args)
        {



            if (args.Length > 2)
            {
                CefSharp.Cef.EnableHighDPISupport();
                try
                {
                    SelfHost.Main(args);
                }
                catch(Exception ex)
                {
                    File.WriteAllText("" + DateTime.Now.Ticks+".log", String.Join(" ", args) + Environment.NewLine + ex.ToString());
                }
                return 0;
            }
            try
            {
                ensureDirectoryExists(
                    "app", 
                    "app\\data", 
                    "app\\data\\app",
                    "app\\data\\appcache",
                    "app\\data\\appcache\\cover",
                    "app\\data\\cefdata");
                ensureDirectoryExists("app\\data\\cefdata\\data");
                ensureDirectoryExists("app\\data\\cefdata\\cache");
                ensureDirectoryExists("app\\data\\cefdata\\log");
                ensureDirectoryExists("app\\data\\cefdata\\data");
                HtDocs = new MyPakFile("htdocs.pak");
                HtDocs.files.ForEach(f => Console.WriteLine(f));
                DBUtils = new DBUtils();
                ApiModel = new ApiModel();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                File.WriteAllText("" + DateTime.Now.Ticks + ".log", String.Join(" ", args) + Environment.NewLine + ex.ToString());
                return 127;
            }
            return 0;
        }

        static void ensureDirectoryExists(params string[] paths) => paths.ToList().ForEach(p => { if (!Directory.Exists(p)) { Directory.CreateDirectory(p); } });

    }


    public class MyPakFile : IDisposable
    {
        public string Tag = "";
        public List<string> files = new List<string>();
        ZipFile src;
        public MyPakFile(string path)
        {
            ICSharpCode.SharpZipLib.Zip.ZipStrings.UseUnicode = true;
            src = new ZipFile(path);
            System.Collections.IEnumerator en = src.GetEnumerator();
            while (en.MoveNext())
            {
                ZipEntry ze = en.Current as ZipEntry;
                if(ze != null)
                {
                    files.Add(ze.Name);
                    Console.WriteLine(ze.Name);
                }
            }
        }

        public Stream OpenRead(string filename)
        {
            if(files.Contains(filename) && !filename.EndsWith("/"))
            {
                ZipEntry ze = src.GetEntry(filename);
                return src.GetInputStream(ze);
            }
            return null;
        }

        public void Dispose()
        {
            ((IDisposable)src).Dispose();
        }
    }

    public class ApiModel
    {
        private MyPakFile openingBook = null;

        public ApiResponse callApi(string pathWithUrl)
        {
            int ptr = pathWithUrl.IndexOf("?");
            if (ptr == -1)
            {
                return handleApiRequest(pathWithUrl, new Dictionary<string, string>());
            }
            string path = pathWithUrl.Substring(0, ptr);
            string param = pathWithUrl.Substring(ptr + 1, pathWithUrl.Length - ptr - 1);
            Dictionary<string, string> paramdict = new Dictionary<string, string>();
            string[] splitparam = param.Split('&','=');
            int len = splitparam.Length;
            if(len % 2 != 0) { len -=1; }
            for (int i = 0; i < len; i+=2)
            {
                paramdict.Add(splitparam[i], HttpUtility.UrlDecode(splitparam[i + 1]));
            }
            try
            {
                return handleApiRequest(path, paramdict);
            }catch(Exception ex)
            {
                return new ApiResponse(ApiResponseType.ErrorInternalError, "500 internal server error\n"+ex.ToString());
            }
        }

        public ApiResponse handleApiRequest(string apipath,Dictionary<string,string> param)
        {
            if (apipath.StartsWith("api/")) {
                string subpath = apipath.Substring(4, apipath.Length - 4);
                return handlePathApiRequest(subpath, param);
            }
            if (apipath.StartsWith("read/"))
            {
                string subpath = apipath.Substring(5, apipath.Length - 5);
                int bookptr = subpath.IndexOf('/');
                string bookuuid = subpath.Substring(0, bookptr);
                string bookapipath = subpath.Substring(bookptr + 1, subpath.Length - 1 - bookptr);
                return handlePathReadRequest(bookuuid,bookapipath, param);
            }
            return new ApiResponse(ApiResponseType.ErrorNotFound, "404 Not Found");
        }

        public ApiResponse handlePathApiRequest(string apipath, Dictionary<string, string> param)
        {
            if (apipath==("reportmode.js"))
            {
                return newFixedLengthResponse("var reportMessage=function(a,b){parent.reportMessage(a,b);}");
            }
            if (apipath=="devname")
            {
                return newFixedLengthResponse(Environment.UserName);
            }
            if (apipath == "scan")
            {
                return newFixedLengthResponse(new BookScanner().doInBackground());
            }
            List<BookEntry> bookEntries = null;
            if (apipath=="library")
            {
                bookEntries = Program.DBUtils.queryBooks("type=0 order by lastopen desc");
                return newFixedLengthResponse(JsonConvert.SerializeObject(bookEntries));
            }
            if (apipath=="folders")
            {
                bookEntries = Program.DBUtils.queryFoldersNotEmpty();
                return newFixedLengthResponse(JsonConvert.SerializeObject(bookEntries));
            }
            if (apipath.StartsWith("folder/"))
            {
                String targetFolder = replaceFirst(apipath, "folder/");
                bookEntries = Program.DBUtils.queryBooks("type=0 and parent_uuid = ? order by lastopen desc", targetFolder);
                return newFixedLengthResponse(JsonConvert.SerializeObject(bookEntries));
            }
            if (apipath.StartsWith("cover/"))
            {
                String uuid = replaceFirst(apipath, "cover/");
                bookEntries = Program.DBUtils.queryBooks("uuid = ?", uuid);
                BookEntry entry = bookEntries[0];
                string covertemppath = "app\\data\\appcache\\cover\\";
                if (!File.Exists(covertemppath + uuid + ".png"))
                {
                    extractCover(entry, covertemppath);
                }
                if (File.Exists(covertemppath + uuid + ".png"))
                {
                    return new ApiResponse(ApiResponseType.File, covertemppath + uuid + ".png");
                }
            }
            if (apipath.StartsWith("open/"))
            {
                String uuid = apipath.Substring(5,apipath.Length-5);
                bookEntries = Program.DBUtils.queryBooks("uuid = ?", uuid);
                BookEntry b = bookEntries[0];
                if (File.Exists(b.getPath()))
                {
                    ensureBookExtracted(b);
                    Program.DBUtils.execSql("update library set lastopen=? where uuid=?", JavaSystem.currentTimeMillis(), b.getUUID());
                    return newFixedLengthResponse("<script>window.location.replace('/read/" + b.getUUID() + "/read.html');</script>");
                }
            }
            return new ApiResponse(ApiResponseType.ErrorNotFound, "404 Not Found");
        }

        private void extractCover(BookEntry entry, string covertemppath)
        {
            string title = Path.GetFileNameWithoutExtension(entry.getPath());
            try
            {
                using (BookMetadata data = new BookMetadata(entry.getPath()))
                {
                    try
                    {
                        Console.WriteLine(data.coverpath);
                        using (Stream sr = data.bookdata.OpenRead(data.coverpath))
                        {
                            using (Image rawimg = Bitmap.FromStream(sr))
                            {
                                using (Image img2 = new Bitmap(rawimg, new Size(180, 260)))
                                {
                                    img2.Save(covertemppath + entry.getUUID() + ".png");
                                    return;
                                }
                            }
                        }
                    }catch(Exception ex)
                    {
                        Console.WriteLine(ex);
                        title = data.title;
                        if (data.author != "")
                        {
                            title += " - " + data.author;
                        }
                        if (title.Length > 32)
                        {
                            title = title.Substring(0, 32);
                        }
                    }
                } 
            }
            catch(Exception ex)
            {

                Console.WriteLine(ex);
            }
            using (Image img2 = new Bitmap(100, 150))
            {
                using (Graphics g = Graphics.FromImage(img2))
                {
                    g.Clear(Color.White);
                    g.DrawString(title, SystemFonts.DefaultFont, Brushes.Black, new RectangleF(0, 0, 100, 150), new StringFormat()
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    });
                }
                img2.Save(covertemppath + entry.getUUID() + ".png");
            }
        }

        public string replaceFirst(string str,string rep)
        {
            return str.Substring(rep.Length, str.Length - rep.Length);
        }

        public void ensureBookExtracted(BookEntry be)
        {
            string bookuuid = be.getUUID();
            if (openingBook != null)
            {
                if (openingBook.Tag != bookuuid)
                {
                    openingBook.Dispose();
                    openingBook = null;
                }
            }
            if (openingBook == null)
            {
                string path = Program.DBUtils.queryBooks("uuid = ?", bookuuid).First().getPath();
                openingBook = new MyPakFile(path)
                {
                    Tag = bookuuid
                };
            }
        }

        public ApiResponse handlePathReadRequest(string bookuuid,string apipath, Dictionary<string, string> param)
        {
            if (apipath.StartsWith("api/"))
            {
                string subpath = apipath.Substring(4, apipath.Length - 4);
                return handlePathReadApiRequest(bookuuid, subpath, param);
            }
            if (apipath.StartsWith("book/"))
            {
                string subpath = apipath.Substring(5, apipath.Length - 5);
                return handlePathReadBookRequest(bookuuid, subpath, param);
            }

            if (apipath == "") { apipath = "index.html"; }
            Stream staticres = Program.HtDocs.OpenRead("epubjs/" + apipath);
            if (staticres != null)
            {
                return new ApiResponse(ApiResponseType.Stream, MimeMapping.GetMimeMapping(apipath), staticres);
            }
            return new ApiResponse(ApiResponseType.ErrorNotFound, "404 Not Found");
        }
        Context appContext = null;
        private ApiResponse handlePathReadApiRequest(string bookuuid, string api, Dictionary<string, string> param)
        {
            if (api.StartsWith("bmload/"))
            {
                int requestId = int.Parse(api.Replace("bmload/",""));
                return newFixedLengthResponse(JsonConvert.SerializeObject(Program.DBUtils.queryBookmarks(appContext, bookuuid)[requestId]));
            }
            if (api==("bmloadall"))
            {
                return newFixedLengthResponse(JsonConvert.SerializeObject(Program.DBUtils.queryBookmarks(appContext, bookuuid)));
            }
            if (api.StartsWith("bmsave/"))
            {
                int requestId = int.Parse(api.Replace("bmsave/", ""));
                
                String name = param["name"];
                String cfi = param["cfi"];
                Program.DBUtils.setBookmark(bookuuid, requestId, name, cfi);
                return newFixedLengthResponse("OK");
            }
            if (api==("bookname"))
            {
                return newFixedLengthResponse(Program.DBUtils.queryBooks("uuid = ?", bookuuid).First().getDisplayName());
            }
            if (api == ("close"))
            {
                openingBook?.Dispose();
                openingBook = null;
                return newFixedLengthResponse("<script>window.location.href='/'</script>");
            }
            return new ApiResponse(ApiResponseType.ErrorNotFound, "404 Not Found");
        }
        private ApiResponse handlePathReadBookRequest(string bookuuid, string api, Dictionary<string, string> param)
        {
            if (openingBook != null)
            {
                if (openingBook.Tag != bookuuid)
                {
                    openingBook.Dispose();
                    openingBook = null;
                }
            }
            if (openingBook == null)
            {
                string path = Program.DBUtils.queryBooks("uuid = ?", bookuuid).First().getPath();
                openingBook = new MyPakFile(path)
                {
                    Tag = bookuuid
                };
            }
            api = HttpUtility.UrlDecode(api);
            Stream stream = openingBook.OpenRead(api);
            if (stream != null)
            {
                return new ApiResponse(ApiResponseType.Stream, MimeMapping.GetMimeMapping(api), stream);
            }
            return new ApiResponse(ApiResponseType.ErrorNotFound, "404 Not Found");
        }

        private ApiResponse newFixedLengthResponse(string msg)
        {
            return new ApiResponse(ApiResponseType.String, msg);
        }

    }

    public class ApiResponse
    {
        public ApiResponseType type;
        public String msg;
        public Stream stream;
        public ApiResponse(ApiResponseType type, string msg)
        {
            this.type = type;
            this.msg = msg;
        }
        public ApiResponse(ApiResponseType type, string mimetype,Stream stream)
        {
            this.type = type;
            this.msg = mimetype;
            this.stream = stream;
        }
    }
    public enum ApiResponseType
    {
        String,File,Stream,ErrorNotFound,ErrorInternalError
    }

    static class XmlUtils
    {
        public static XName withNS(this string localname, string ns) => XName.Get(localname, ns);

    }

    public class BookMetadata : IDisposable
    {
        public string filepath;
        public MyPakFile bookdata;
        public string title;
        public string author;
        public string coverpath;
        public BookMetadata(string filepath)
        {
            this.filepath = filepath;
            bookdata = new MyPakFile(filepath);
            parseBook();
        }

        string readStr(string file)
        {
            Stream tstream = bookdata.OpenRead(file);
            if (tstream != null)
            {
                using (Stream s = tstream)
                {
                    using(StreamReader sr = new StreamReader(s, Encoding.UTF8))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            return "";
        }

        
        void parseBook()
        {
            string containerns = "urn:oasis:names:tc:opendocument:xmlns:container";
            string opfns = "http://www.idpf.org/2007/opf";
            string dcns = "http://purl.org/dc/elements/1.1/";
            XDocument xld = XDocument.Parse(readStr("META-INF/container.xml"));
            containerns = xld.Root.GetDefaultNamespace().NamespaceName;
            string opfPath = xld.Root.Element("rootfiles".withNS(containerns)).Element("rootfile".withNS(containerns)).Attribute("full-path").Value;
            xld = XDocument.Parse(readStr(opfPath));
            opfns = xld.Root.GetDefaultNamespace().NamespaceName;
            title = string.Join(", ", xld.Root.Element("metadata".withNS(opfns)).Elements("title".withNS(dcns)).Select(c => c.Value).ToArray());
            author = string.Join(", ", xld.Root.Element("metadata".withNS(opfns)).Elements("creator".withNS(dcns)).Select(c => c.Value).ToArray());
            if (title == "")
            {
                throw new NullReferenceException("who the fuck invented XML namespace?");
            }
            if(xld.Root.Element("metadata".withNS(opfns)).Elements("meta".withNS(opfns)).Any(e => e.Attribute("name") !=null && e.Attribute("name").Value == "cover")){
                string coverentry = xld.Root.Element("metadata".withNS(opfns)).Elements("meta".withNS(opfns)).Where(e => e.Attribute("name") != null && e.Attribute("name").Value == "cover").First().Attribute("content").Value;
                string relativeCoverPath = xld.Root.Element("manifest".withNS(opfns)).Elements("item".withNS(opfns)).Where(i => i.Attribute("id") != null && i.Attribute("id").Value == coverentry)
                        .Select(i => i.Attribute("href").Value).FirstOrDefault();
                coverpath = relativeToAbsolute2(opfPath, relativeCoverPath);
            }
        }

        string relativeToAbsolute2(string rpath, string path)
        {
            if (path.StartsWith("/"))
            {
                return path.Substring(1);
            }
            Stack<string> dirStack = new Stack<string>();
            string[] cds = rpath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string[] paths = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < cds.Length-1; i++)
            {
                string dirEntry = cds[i];
                if (dirEntry == ".") { continue; }
                if (dirEntry == ".." && dirStack.Count > 0) { dirStack.Pop(); continue; }
                dirStack.Push(dirEntry);
            }
            for (int i = 0; i < paths.Length; i++)
            {
                string dirEntry = paths[i];
                if (dirEntry == ".") { continue; }
                if (dirEntry == ".." && dirStack.Count > 0) { dirStack.Pop(); continue; }
                dirStack.Push(dirEntry);
            }
            return string.Join("/", dirStack.Reverse().ToArray());
        }
        public void Dispose()
        {
            bookdata?.Dispose();
        }
    }
}
