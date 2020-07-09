using Gdk;
using GLib;
using Gtk;
using NetCoreServer;
using System;
using System.Diagnostics;
using System.IO;

namespace GeneralConsole
{
    class ChatForm
    {
        public static TextView RecvChatWidget;
        public static TextView SendChatWidget;
        public void ScrollToBottom(object sender, Gtk.SizeAllocatedArgs e)
        {
            RecvChatWidget.ScrollToIter(RecvChatWidget.Buffer.EndIter, 0, false, 0, 0);
        }
        public static Widget show_chat_form()
        {
            //新建窗体，标题是Hello World
            var win = new Gtk.Window("聊天");

            win.SetDefaultSize(800, 600);
            //win.SetSizeRequest(800, 600);

            //窗体关闭后退出应用
            //win.DeleteEvent += (s, e) =>
            //{
            //    Gtk.Application.Quit();
            //};

            win.WindowPosition = WindowPosition.Center;
            //win.Resizable = false;
            win.BorderWidth = 10;

            var paned1 = new VPaned();
            win.Add(paned1);
            var paned2 = new HPaned();
            paned1.SetSizeRequest(200, -1);
            paned2.SetSizeRequest(200, -1);
            paned1.Add(paned2);
            var button1 = new Button("按钮一");
            paned1.Pack1(button1, true, false);
            var button2 = new Button("按钮二");
            paned2.Pack1(button2, true, false);
            var paned3 = new VPaned();
            paned3.SetSizeRequest(200, -1);
            paned2.Pack2(paned3, true, false);
            //var button3 = new Button("按钮三");
            RecvChatWidget = new TextView();
            var recvScrollView = new ScrolledWindow();
            recvScrollView.Add(RecvChatWidget);
            //RecvChatWidget.SizeAllocated += new SizeAllocatedHandler(ScrollToBottom);
            paned3.Pack1(recvScrollView, true, false);
            //var button4 = new Button("按钮四");
            SendChatWidget = new TextView();
            var sendScrollView = new ScrolledWindow();
            sendScrollView.Add(SendChatWidget);
            var vbox = new VBox(false, 0);
            var hbox = new VBox(true, 0);
            var sendMsgBtn = new Button("发送消息");
            sendMsgBtn.Clicked += (s, e) =>
            {
                TextIter start, end;
                RecvChatWidget.Buffer.GetBounds(out start, out end);
                RecvChatWidget.Buffer.Insert(ref end, SendChatWidget.Buffer.Text);
                RecvChatWidget.Buffer.Insert(ref end, "\n");
                RecvChatWidget.ScrollToIter(RecvChatWidget.Buffer.EndIter, 0.0, false, 0.0, 0.0);
            };
            hbox.PackStart(sendMsgBtn, false, false, 0);
            vbox.PackStart(hbox, false, false, 0);
            vbox.PackEnd(sendScrollView, true, true, 0);
            paned3.Pack2(vbox, true, false);

            win.ShowAll();

            return paned1;
        }
    }
}
