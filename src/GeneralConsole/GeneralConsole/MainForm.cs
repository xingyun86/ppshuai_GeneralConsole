using Gdk;
using GLib;
using Gtk;
using NetCoreServer;
using System;
using System.Diagnostics;
using System.IO;

namespace GeneralConsole
{
    public class HexadecimalEncoding
    {
        public static string ToHexString(string str)
        {
            var sb = new System.Text.StringBuilder();

            var bytes = System.Text.Encoding.Unicode.GetBytes(str);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString(); // returns: "48656C6C6F20776F726C64" for "Hello world"
        }

        public static string FromHexString(string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return System.Text.Encoding.Unicode.GetString(bytes); // returns: "Hello world" for "48656C6C6F20776F726C64"
        }
    }
    class MainForm
    {
        const uint timeScreenshot = 1000;
        static uint timeScreenshotId = (0);
        const uint timeUpdateshow = 1000;
        static uint timeUpdateshowId = (0);
        static Image image = null;
        static bool timeout_screenshot_handler()
        {
            return screenshot();
        }
        static bool timeout_updateshow_handler()
        {
            return updateshow();
        }
        static bool screenshot()
        {
            if (image != null)
            {
                image.Pixbuf.Dispose();
                string tempName = "test.png";
                var stopWatch = new Stopwatch();

                var context = Gdk.CairoHelper.Create(Gdk.Screen.Default.RootWindow);
                var surface = context.GetTarget();
                stopWatch.Start();
                surface.WriteToPng(tempName);
                stopWatch.Stop();
                Console.WriteLine($"spend {stopWatch.Elapsed.TotalMilliseconds}ms");
                context.Dispose();
                surface.Dispose();

                if (Configure.GetInstance().IsServer == 0)
                {
                    TcpChatClient.Program.chatClient.SendAsync(File.ReadAllBytes(tempName));
                }
                //image.Pixbuf = (new Pixbuf(tempName)).ScaleSimple(600, 300, InterpType.Bilinear);
            }
            return true;
        }
        static bool updateshow()
        {
            /*// Now consume the blocking collection with foreach.
            // Use BlockingCollection.GetConsumingEnumerable() instead of just bc because the
            // former will block waiting for completion and the latter will
            // simply take a snapshot of the current state of the underlying collection.
            // for (var i = 0; i < TcpChatServer.Program.concurrentQueue.Count; i++)
            // foreach (var item in TcpChatServer.Program.concurrentQueue)
            {
                byte[] item;
                if(TcpChatServer.Program.concurrentQueue.TryDequeue(out item))
                {
                    File.WriteAllBytes("test2.png", item);
                    var pixbuf = new Pixbuf(item);
                    image.Pixbuf.Dispose();
                    image.Pixbuf = pixbuf.ScaleSimple(600, 300, InterpType.Bilinear);
                    pixbuf.Dispose();
                }
            }*/
            // Now consume the blocking collection with foreach.
            // Use BlockingCollection.GetConsumingEnumerable() instead of just bc because the
            // former will block waiting for completion and the latter will
            // simply take a snapshot of the current state of the underlying collection.
            // for (var i = 0; i < TcpChatServer.Program.blockingCollection.Count; i++)
            // foreach (var item in TcpChatServer.Program.blockingCollection.GetConsumingEnumerable())
            {
                byte[] item;
                if (TcpChatServer.Program.blockingCollection.TryTake(out item))
                {
                    File.WriteAllBytes("test2.png", item);
                    var pixbuf = new Pixbuf(item);
                    image.Pixbuf.Dispose();
                    image.Pixbuf = pixbuf.ScaleSimple(600, 300, InterpType.Bilinear);
                    pixbuf.Dispose();
                }
            }
            return true;
        }
        static void init_toolbar(HBox toolbar)
        {
            Button button;

            button = new Button("Monitor");
            button.Clicked += (s, e) =>
            {
                Button button = (Button)(s);

                button.Label = (button.Label == "Monitor") ? "Monitoring" : "Monitor";
            };
            button.SetSizeRequest(128, 128);
            toolbar.PackStart(button, false, false, 0);
            ////////////////////////////////////////////////////////////////////////////
            button = new Button("Lock");
            button.Clicked += (s, e) =>
            {
                Button button = (Button)(s);
                if (Configure.GetInstance().IsServer == 0)
                {
                    TcpChatClient.Program.chatClient.SendAsync("Hello World!");
                }
                button.Label = (button.Label == "Lock") ? "UnLock" : "Lock";
            };
            button.SetSizeRequest(128, 128);
            toolbar.PackStart(button, false, false, 0);
            ////////////////////////////////////////////////////////////////////////////
            button = new Button("StartSend");
            button.Clicked += (s, e) =>
            {
                Button button = (Button)(s);
                switch (button.Label)
                {
                    case "StartSend":
                        {
                            timeScreenshotId = Timeout.Add(timeScreenshot, timeout_screenshot_handler);
                            button.Label = "StopSend";
                        }
                        break;
                    case "StopSend":
                        {
                            Timeout.Remove(timeScreenshotId);
                            button.Label = "StartSend";
                        }
                        break;
                    default:
                        {
                            Timeout.Remove(timeScreenshotId);
                            button.Label = "StartSend";
                        }
                        break;
                }
            };
            button.SetSizeRequest(128, 128);
            toolbar.PackStart(button, false, false, 0);
            ////////////////////////////////////////////////////////////////////////////
            button = new Button("StartRecv");
            button.Clicked += (s, e) =>
            {
                Button button = (Button)(s);
                switch (button.Label)
                {
                    case "StartRecv":
                        {
                            timeUpdateshowId = Timeout.Add(timeUpdateshow, timeout_updateshow_handler);
                            button.Label = "StopRecv";
                        }
                        break;
                    case "StopRecv":
                        {
                            Timeout.Remove(timeUpdateshowId);
                            button.Label = "StartRecv";
                        }
                        break;
                    default:
                        {
                            Timeout.Remove(timeUpdateshowId);
                            button.Label = "StartRecv";
                        }
                        break;
                }
            };
            button.SetSizeRequest(128, 128);
            toolbar.PackStart(button, false, false, 0);
        }
        static void init_mainbody(HBox mainbody)
        {
            var treeView = new TreeView();

            var treeViewColumn = new TreeViewColumn();
            treeViewColumn.Title = "Room/Computer";
            var cellRenderText = new CellRendererText();
            treeViewColumn.PackStart(cellRenderText, true);
            treeViewColumn.AddAttribute(cellRenderText, "text", treeView.Columns.Length);
            treeView.InsertColumn(treeViewColumn, treeView.Columns.Length);

            treeViewColumn = new TreeViewColumn();
            treeViewColumn.Title = "User";
            cellRenderText = new CellRendererText();
            treeViewColumn.PackStart(cellRenderText, true);
            treeViewColumn.AddAttribute(cellRenderText, "text", treeView.Columns.Length);
            treeView.AppendColumn(treeViewColumn);

            var treeStore = new TreeStore(typeof(string), typeof(string));
            treeView.Model = treeStore;
            var treeIter = treeStore.AppendValues("room1");
            treeStore.AppendValues(treeIter, "PC01", "PC01");
            treeStore.AppendValues(treeIter, "PC02", "PC01");

            ScrolledWindow scrolledWindow = new ScrolledWindow();
            scrolledWindow.ShadowType = ShadowType.In;
            scrolledWindow.Add(treeView);
            scrolledWindow.SetSizeRequest(300, 0);
            mainbody.PackStart(scrolledWindow, false, true, 0);

            scrolledWindow = new ScrolledWindow();
            scrolledWindow.ShadowType = ShadowType.In;
            var frame = new Frame();

            scrolledWindow.Add(frame);
            mainbody.PackStart(scrolledWindow, true, true, 0);

            image = new Image();
            image.SetPadding(0, 0);
            frame.Add(image);

            {
                string tempName = "test.png";
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var context = Gdk.CairoHelper.Create(Gdk.Screen.Default.RootWindow);
                var surface = context.GetTarget();
                surface.WriteToPng(tempName);
                context.Dispose();
                surface.Dispose();
                stopWatch.Stop();
                Console.WriteLine("spend {0}ms", stopWatch.Elapsed.TotalMilliseconds);

                image.Pixbuf = (new Pixbuf(tempName)).ScaleSimple(600, 300, InterpType.Bilinear);
            }
        }
        static void init_statusbar(HBox statusbar)
        {
            Label label;
            label = new Label("Running");
            statusbar.PackStart(label, false, true, 0);
            label = new Label("Running");
            statusbar.PackStart(label, false, true, 0);
        }
        public static Widget init_main_form()
        {
            var layout = new VBox(false, 0);
            var toolbar = new HBox(false, 0);
            var mainbody = new HBox(false, 0);
            var statusbar = new HBox(false, 0);

            init_toolbar(toolbar);
            init_mainbody(mainbody);
            init_statusbar(statusbar);

            layout.PackStart(toolbar, false, true, 0);
            layout.PackStart(mainbody, true, true, 0);
            layout.PackStart(statusbar, false, true, 0);

            return layout;
        }
    }
}
