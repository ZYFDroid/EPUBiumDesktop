using CefSharp.BrowserSubprocess;
using CefSharp.RenderProcess;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EPUBium_Desktop
{

    public class DBUtils
    {

        SQLiteConnection conn;
        public String getBookRoot()
        {
            return ".";
        }
    
        public DBUtils()
        {
            bool createNew = false;
            if (!File.Exists("app\\data\\app\\data.db"))
            {
                createNew = true;
            }
            conn = new SQLiteConnection("DataSource=app/data/app/data.db");
            conn.Open();
            if (createNew)
            {
                execSql("create table _sqlver(ver integer)");
                execSql("insert into _sqlver(ver) values(-1)");
                onCreate();
                execSql("update _sqlver set ver=?", SQL_VERSION);
            }
            else
            {
                Cursor c = rawQuery("select ver from _sqlver");
                c.moveToFirst();
                int currentver = c.getInt(0);
                c.close();
                if (currentver != SQL_VERSION)
                {
                    onUpgrade(currentver, SQL_VERSION);
                }
            }
        }

        const int SQL_VERSION = 1;
        public void onCreate()
        {
            onUpgrade(-1, SQL_VERSION);
        }
    
        public void onUpgrade(int oldVersion, int newVersion)
        {
            int curversion = oldVersion + 1;
            while (curversion <= newVersion)
            {
                if (curversion == 1)
                {
                    //type=0: this is a book; type=1: this is a folder/bookshelf, query sub uuid for more books. the Root uuid is 0;
                    execSql("create table library(id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE,uuid text,type integer,parent_uuid text,display_name text,path text,lastopen bigint default 0)");
                    execSql("create table bookmarks(id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE,bookid text,epubcfi text,slot integer default 0,name text,savetime bigint default 0)");
                    execSql("insert into library(uuid,type,display_name,path) values(?,?,?,?)", new Object[] { "0", 1, "图书馆", getBookRoot() });
                    
                }
                curversion++;
            }
        }
    
        public void execSql(String sql, params object[] args)
        {
            SQLiteCommand cmd = conn.CreateCommand();
            string paramPrefix = "arg_";
            string[] sqlbreaks = sql.Split('?');
            StringBuilder sqlBuilder = new StringBuilder();
            for (int i = 0; i < sqlbreaks.Length; i++)
            {
                sqlBuilder.Append(sqlbreaks[i]);
                if (i != sqlbreaks.Length - 1)
                {
                    String paramname = "$" + paramPrefix + i;
                    sqlBuilder.Append(paramname);
                    cmd.Parameters.AddWithValue(paramname, args[i]);
                }
            }
            cmd.CommandText = sqlBuilder.ToString();
            cmd.ExecuteNonQuery();
        }
    
        public int getCount(String sql, params string[] args)
        {
            Cursor c = rawQuery("select count(*) from library where " + sql, args);
            c.moveToFirst();
            int count = c.getInt(0); 
            c.close();
            return count;
        }
        public int getCount2(String sql, params string[] args)
        {
            Cursor c = rawQuery("select count(*) from bookmarks where " + sql, args);
            c.moveToFirst();
            int count = c.getInt(0);
            c.close();
            return count;
        }
    
    
        public void InsertBooks(List<BookEntry> books)
        {
            foreach (BookEntry b in books)
            {
                execSql("insert into library(uuid,type,parent_uuid,display_name,path,lastopen) values(?,?,?,?,?,?)",
                        b.getUUID(),
                        b.getType(),
                        b.getParentUUID(),
                        b.getDisplayName(),
                        b.getPath(),
                        b.getLastOpenTime()
                );
            }
        }
    
        public Cursor rawQuery(String sql, params string[] args)
        {
            SQLiteCommand cmd = conn.CreateCommand();
            string paramPrefix = "arg_";
            string[] sqlbreaks = sql.Split('?');
            StringBuilder sqlBuilder = new StringBuilder();
            for (int i = 0; i < sqlbreaks.Length; i++)
            {
                sqlBuilder.Append(sqlbreaks[i]);
                if (i != sqlbreaks.Length - 1)
                {
                    String paramname = "$" + paramPrefix + i;
                    sqlBuilder.Append(paramname);
                    cmd.Parameters.AddWithValue(paramname, args[i]);
                }
            }
            cmd.CommandText = sqlBuilder.ToString();
            return new Cursor(cmd.ExecuteReader());
        }
    
        public List<BookEntry> queryBooks(String sql, params string[] args)
        {
            Cursor c = rawQuery("select id,uuid,type,parent_uuid,display_name,path,lastopen from library where " + sql + "", args);
            List<BookEntry> books = new List<BookEntry>();
            if (c.moveToFirst())
            {
                do
                {
                    books.Add(BookEntry.readFromDB(c.getInt(0), c.getString(1), c.getInt(2), c.getString(3), c.getString(4), c.getString(5), c.getLong(6)));
                } while (c.moveToNext());
            }
            c.close();
            return books;
        }
    
        public List<BookEntry> queryFoldersNotEmpty()
        {
            Cursor c = rawQuery("select id,uuid,type,parent_uuid,display_name,path,lastopen from library as lib where 0 < (select count(*) from library where parent_uuid=lib.uuid and type=0)  order by display_name");
            List<BookEntry> books = new List<BookEntry>();
            if (c.moveToFirst())
            {
                do
                {
                    books.Add(BookEntry.readFromDB(c.getInt(0), c.getString(1), c.getInt(2), c.getString(3), c.getString(4), c.getString(5), c.getLong(6)));
                } while (c.moveToNext());
            }
            c.close();
            return books;
        }
    
    
        public List<BookMark> queryBookmarks(Context ctx, String bookid)
        {
            Cursor c = rawQuery("select id,bookid,slot,epubcfi,name,savetime from bookmarks where bookid=? order by slot", bookid);
            List<BookMark> bookmarks = new List<BookMark>();
            for (int i = 0; i <= 11; i++)
            {
                bookmarks.Add(new BookMark(-1, bookid, i, "", "无存档", -1));
            }
            if (c.moveToFirst())
            {
                do
                {
                    BookMark bm = new BookMark(c.getInt(0), c.getString(1), c.getInt(2), c.getString(3), c.getString(4), c.getLong(5));
                    bookmarks[bm.slot] = bm;
                } while (c.moveToNext());
            }
            return bookmarks;
        }
    
    
        public void autoSave(String bookId, String cfi, String title)
        {
            setBookmark(bookId, 0, "" + title, cfi);
        }
    
        public BookMark autoLoad(Context ctx, String bookId)
        {
            return queryBookmarks(ctx, bookId)[0];
        }
    
        public void quickSave(String bookId, String cfi, String title)
        {
            setBookmark(bookId, 1, "" + title, cfi);
        }
    
        public BookMark quickLoad(Context ctx, String bookId)
        {
            return queryBookmarks(ctx, bookId)[1];
        }
    
        public void setBookmark(String bookId, int slot, String name, String epubCfi)
        {
            if (getCount2("bookid=? and slot=?", "" + bookId, "" + slot) > 0)
            {
                execSql("update bookmarks set name=? where bookid=? and slot=?", name, "" + bookId, "" + slot);
                execSql("update bookmarks set epubcfi=? where bookid=? and slot=?", epubCfi, "" + bookId, "" + slot);
                execSql("update bookmarks set savetime=? where bookid=? and slot=?", JavaSystem.currentTimeMillis() + "", "" + bookId, "" + slot);
            }
            else
            {
                execSql("insert into bookmarks(bookid,slot,name,epubcfi,savetime) values(?,?,?,?,?)", bookId + "", slot + "", name, epubCfi, JavaSystem.currentTimeMillis());
            }
        }
    }

    //Code copied from java
    public class BookMark
    {
        public int id;
        public String bookid;
        public int slot;
        public String epubcft, name;
        public long saveTime;

        public long getSaveTime()
        {
            return saveTime;
        }

        public void setSaveTime(long saveTime)
        {
            this.saveTime = saveTime;
        }

        public BookMark(int id, String bookid, int slot, String epubcft, String name, long savetime)
        {
            this.id = id;
            this.bookid = bookid;
            this.slot = slot;
            this.epubcft = epubcft;
            this.name = name;
            this.saveTime = savetime;
        }

        public int getId()
        {
            return id;
        }

        public void setId(int id)
        {
            this.id = id;
        }

        public String getBookid()
        {
            return bookid;
        }

        public void setBookid(String bookid)
        {
            this.bookid = bookid;
        }

        public int getSlot()
        {
            return slot;
        }

        public void setSlot(int slot)
        {
            this.slot = slot;
        }

        public String getEpubcft()
        {
            return epubcft;
        }

        public void setEpubcft(String epubcft)
        {
            this.epubcft = epubcft;
        }

        public String getName()
        {
            return name;
        }

        public void setName(String name)
        {
            this.name = name;
        }
    }

    //Code copied from java
    public class BookEntry
    {
        public const String ROOT_UUID = "0";
        public const int TYPE_BOOK = 0;
        public const int TYPE_FOLDER = 1;
        public int id = -1;
        public String UUID;
        public int type;
        public String parentUUID;
        public String displayName;
        public String path;
        public long lastOpenTime;

        private BookEntry(int id, String UUID, int type, String parentUUID, String displayName, String path, long lastOpenTime)
        {
            this.UUID = UUID;
            this.type = type;
            this.parentUUID = parentUUID;
            this.displayName = displayName;
            this.path = path;
            this.lastOpenTime = lastOpenTime;
        }

        public static BookEntry createBook(String parentUUID, String title, String path)
        {
            return new BookEntry(-1, md5(path), TYPE_BOOK, parentUUID, title, path, JavaSystem.currentTimeMillis());
        }

        public static BookEntry createFolder(String parentUUID, String path)
        {
            FileInfo f = new FileInfo(path);
            return new BookEntry(-1, md5(path), TYPE_FOLDER, parentUUID, f.Name, path, JavaSystem.currentTimeMillis());
        }

        public static BookEntry readFromDB(int id, String UUID, int type, String parentUUID, String displayName, String path, long lastOpenTime)
        {
            return new BookEntry(id, UUID, type, parentUUID, displayName, path, lastOpenTime);
        }

        //region Getter and setter

        public int getId()
        {
            return id;
        }

        public void setId(int id)
        {
            this.id = id;
        }

        public String getUUID()
        {
            return UUID;
        }

        public void setUUID(String UUID)
        {
            this.UUID = UUID;
        }

        public int getType()
        {
            return type;
        }

        public void setType(int type)
        {
            this.type = type;
        }

        public String getParentUUID()
        {
            return parentUUID;
        }

        public void setParentUUID(String parentUUID)
        {
            this.parentUUID = parentUUID;
        }

        public String getDisplayName()
        {
            return displayName;
        }

        public void setDisplayName(String displayName)
        {
            this.displayName = displayName;
        }

        public String getPath()
        {
            return path;
        }

        public void setPath(String path)
        {
            this.path = path;
        }

        public long getLastOpenTime()
        {
            return lastOpenTime;
        }

        public void setLastOpenTime(long lastOpenTime)
        {
            this.lastOpenTime = lastOpenTime;
        }

        private static System.Security.Cryptography.MD5 md5impl = System.Security.Cryptography.MD5.Create();
        private static string md5(string strin)
        {
            if (strin == null || strin == "") { return ""; }
            byte[] hash = md5impl.ComputeHash(Encoding.UTF8.GetBytes(strin));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        //endregion
    }

    public class Cursor
    {
        internal SQLiteDataReader reader;

        public Cursor(SQLiteDataReader reader)
        {
            this.reader = reader;
        }

        public string getString(int index)
        {
            if (reader.IsDBNull(index))
            {
                return "";
            }
            return reader.GetString(index);
        }
        public int getInt(int index)
        {
            return reader.GetInt32(index);
        }
        public long getLong(int index)
        {
            return reader.GetInt64(index);
        }

        internal bool moveToFirst()
        {
            if (reader.HasRows)
            {
                return reader.Read();
            }
            return false;
        }

        internal bool moveToNext()
        {
            return reader.Read();
        }

        internal void close()
        {
            reader.Close();
        }

    }

    public class Context { }

    public class JavaSystem
    {
        private static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0) + (DateTime.Now - DateTime.UtcNow);
        public static long currentTimeMillis()
        {
            return (long)((DateTime.Now - epoch).TotalMilliseconds);
        }
        public static long currentTimeMillis(DateTime dt)
        {
            return (long)((dt - epoch).TotalMilliseconds);
        }

    }

    class TextUtils
    {
        public static bool isEmpty(string str) => str != null && str != "";
    }

    public class BookScanner
    {
        Dictionary<String, String> pathToUUID = new Dictionary<String, String>();
        List<BookEntry> bookEntries = new List<BookEntry>();
        List<BookEntry> folderEntries = new List<BookEntry>();

        List<String> path = new List<String>();
        List<String> bookPath = new List<String>();
        void rescurePath(String root)
        {
            Directory.EnumerateDirectories(root).ToList().ForEach(f =>
            {
                rescurePath(f);
                path.Add(toRelativePath(f));
            });
            Directory.EnumerateFiles(root,"*.epub").ToList().ForEach(f =>
            {
                bookPath.Add(toRelativePath(f));
                Console.WriteLine("Added " + f);
            });
        }

        public string toRelativePath(string path)
        {
            String basepath = Environment.CurrentDirectory;
            String targetpath = Path.GetFullPath(path);
            String rpath = targetpath.Substring(basepath.Length);
            if (rpath.StartsWith("\\"))
            {
                rpath = rpath.Substring(1);
            }
            return ".\\"+rpath;
        }

        void delay(int mill)
        {
            
        }

        public String doInBackground()
        {
            try
            {
                rescurePath(".");
                List<BookEntry> pathInDb = Program.DBUtils.queryBooks("type=?", (BookEntry.TYPE_FOLDER).ToString());
                foreach (BookEntry pid in pathInDb)
                {
                    pathToUUID.Add(pid.getPath(), pid.getUUID());
                }
                List<BookEntry> newPaths = new List<BookEntry>();
                foreach (String p in path)
                {
                    if (!pathToUUID.ContainsKey(p))
                    {
                        newPaths.Add(BookEntry.createFolder("", p));
                    }
                }
                foreach (BookEntry pid in newPaths)
                {
                    pathToUUID.Add(pid.getPath(), pid.getUUID());
                }
                foreach (BookEntry pid in newPaths)
                {
                    if (TextUtils.isEmpty(pid.getParentUUID()))
                    {
                        String parentPath = toRelativePath(pid.getPath());
                        String path2uuid = null;
                        pathToUUID.TryGetValue(parentPath,out path2uuid);
                        pid.setParentUUID(path2uuid != null ? path2uuid : BookEntry.ROOT_UUID);
                    }
                }
                folderEntries.AddRange(newPaths);
                List<BookEntry> bookInDb =Program.DBUtils.queryBooks("type=?", (BookEntry.TYPE_BOOK).ToString());

                List<String> bookPathInDb = new List<String>();
                foreach (BookEntry bk in
                            bookInDb)
                {
                    bookPathInDb.Add(bk.getPath());
                }

                List<String> newBookPathList = new List<String>();
                foreach (String newBookPath in bookPath)
                {
                    if (!bookPathInDb.Contains(newBookPath))
                    {
                        newBookPathList.Add(newBookPath);
                    }
                }
               
                int success = 0, deleted = 0;

                for (int i = 0; i < newBookPathList.Count; i++)
                {

                   
                    try
                    {
                        FileInfo bf = new FileInfo(newBookPathList[i]);
                        String parentPath = toRelativePath(Path.GetDirectoryName(newBookPathList[i]));
                        String displayName = Path.GetFileNameWithoutExtension(newBookPathList[i]);
                        using (BookMetadata readinfo = new BookMetadata(newBookPathList[i]))
                        {
                            displayName = readinfo.title;
                            if (readinfo.author != "")
                            {
                                displayName += " - " + readinfo.author;
                            }
                        }
                        String path2uuid = pathToUUID[parentPath];
                        BookEntry tmpEntry = BookEntry.createBook(path2uuid != null ? path2uuid : BookEntry.ROOT_UUID, displayName, toRelativePath(newBookPathList[i]));
                        tmpEntry.lastOpenTime = JavaSystem.currentTimeMillis(bf.LastWriteTime);
                        bookEntries.Add(tmpEntry);
                        success++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                Program.DBUtils.InsertBooks(folderEntries);
                Program.DBUtils.InsertBooks(bookEntries);
                deleted = cleanDB();
                int removeEmptyPath = cleanEmptyDir();
                while (removeEmptyPath > 0)
                {
                    removeEmptyPath = cleanEmptyDir();
                }
                
                return $"扫描完成，添加了{success}本书，删除了{deleted}本书";
            }
            catch (Exception ex)
            {
                return "扫描出错:" +ex.GetType().FullName+":"+ ex.Message;
            }
        }

        int cleanDB()
        {
            int deleted = 0;
            List<String> deletionUUIDs = new List<String>();
            List<BookEntry> bookInDb = Program.DBUtils.queryBooks("1=1");
            foreach (BookEntry bk in bookInDb)
            {
                if (bk.getType() == BookEntry.TYPE_BOOK)
                {
                    if (!File.Exists(bk.getPath()))
                    {
                        deletionUUIDs.Add(bk.getUUID());
                        deleted++;
                    }
                }
            }
            foreach (String delete in deletionUUIDs)
            {
                Program.DBUtils.execSql("delete from library where uuid=?", delete);
                if (File.Exists("app\\data\\appcache\\cover\\"+delete+".png"))
                {
                    File.Delete("app\\data\\appcache\\cover\\" + delete + ".png");
                }
            }
            return deleted;
        }

        int cleanEmptyDir()
        {
            int deleted = 0;
            List<String> deletionUUIDs = new List<String>();
            List<BookEntry> bookInDb = Program.DBUtils.queryBooks("type=?", (BookEntry.TYPE_FOLDER).ToString());
            foreach (BookEntry bk in bookInDb)
            {
                if (bk.getUUID()==(BookEntry.ROOT_UUID)) { continue; }
                if (Program.DBUtils.getCount("parent_uuid=?", bk.getUUID()) == 0)
                {
                    deletionUUIDs.Add(bk.getUUID());
                }
            }
            foreach (String delete in deletionUUIDs)
            {
                Program.DBUtils.execSql("delete from library where uuid=?", delete);
                deleted++;
            }
            return deleted;
        }



    }

}