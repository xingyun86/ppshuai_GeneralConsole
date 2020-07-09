using Gtk;
using System;

namespace GeneralConsole
{
    class Program
    {
        static Gtk.Window init_window()
        {
            //新建窗体，标题是Hello World
            var win = new Gtk.Window("Hello World");

            win.SetDefaultSize(800, 600);
            //win.SetSizeRequest(800, 600);

            //窗体关闭后退出应用
            win.DeleteEvent += (s, e) =>
            {
                Gtk.Application.Quit();
            };

            win.WindowPosition = WindowPosition.Center;
            //win.Resizable = false;
            
            return win;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            if(args.Length > 0)
            {
                Configure.Init(args[0]);
            }
            if (Configure.GetInstance().IsServer == 1)
            {
                Console.WriteLine("This is server!");
                TcpChatServer.Program.Start(new string[] { Configure.GetInstance().Port.ToString() });
            }
            else
            {
                Console.WriteLine("This is client!");
                TcpChatClient.Program.Start(new string[] { Configure.GetInstance().Host, Configure.GetInstance().Port.ToString() });
            }
            Gtk.Application.Init();//初始化
            
            // Load the Theme
            var css_provider = new Gtk.CssProvider();
            //css_provider.LoadFromPath("themes/DeLorean-3.14/gtk-3.0/gtk.css")
            //css_provider.LoadFromPath("themes/DeLorean-Dark-3.14/gtk-3.0/gtk.css");
            css_provider.LoadFromData("#myWindow,button,treeview {" +
                "background-image: url('bg.jpg');" +
                "background-repeat: no-repeat;" +
                "background-position: left bottom;" +
                "}" +
                "#myWindow,button,treeview {" +
                "background-image: radial-gradient(ellipse at center, yellow 0%, green 100%);" +
                "}" +
                "label0 {" +
                "background-image: radial-gradient(circle farthest-side at left bottom, red, yellow 50px, green);" +
                "}");
            Gtk.StyleContext.AddProviderForScreen(Gdk.Screen.Default, css_provider, 800);
            /*using (ImageSurface draw = new ImageSurface(Format.Argb32, 70, 150))
            {
                using (Cairo.Context gr = new Cairo.Context(draw))
                {
                    gr.Antialias = Antialias.Subpixel;    // sets the anti-aliasing method
                    gr.LineWidth = 9;          // sets the line width
                    gr.SetSourceColor(new Cairo.Color(0, 0, 0, 1));   // red, green, blue, alpha
                    gr.MoveTo(10, 10);          // sets the Context's start point.
                    gr.LineTo(40, 60);          // draws a "virtual" line from 5,5 to 20,30
                    gr.Stroke();          //stroke the line to the image surface

                    gr.Antialias = Antialias.Gray;
                    gr.LineWidth = 8;
                    gr.SetSourceColor(new Cairo.Color(1, 0, 0, 1));
                    gr.LineCap = LineCap.Round;
                    gr.MoveTo(10, 50);
                    gr.LineTo(40, 100);
                    gr.Stroke();

                    gr.Antialias = Antialias.None;    //fastest method but low quality
                    gr.LineWidth = 7;
                    gr.MoveTo(10, 90);
                    gr.LineTo(40, 140);
                    gr.Stroke();

                    draw.WriteToPng("antialias.png");  //save the image as a png image.
                }
            }
            {
                var screen = Gdk.Screen.Default;
                var root_window = Gdk.Global.DefaultRootWindow;
                int w = screen.Width;
                int h = screen.Height;                

                var stopWatch = new Stopwatch();
                stopWatch.Start();
                Gdk.CairoHelper.Create(root_window).GetTarget().WriteToPng("test.png");
                stopWatch.Stop();
                Console.WriteLine("spend {0}ms", stopWatch.Elapsed.TotalMilliseconds);

                var imageSurface = new Cairo.ImageSurface(Format.ARGB32, w, h);
                var context = new Cairo.Context(imageSurface);
                context.Antialias = Antialias.Default;
                context.SetSourceColor(new Cairo.Color(1, 0, 0, 1));
                // Select a color (blue)
                context.SetSourceRGB(0, 0, 1);
                // Select a font to draw with
                context.SelectFontFace("serif", FontSlant.Normal, FontWeight.Bold);
                context.SetFontSize(30.0);
                context.LineWidth = 10.0;
                context.MoveTo(0, 0);
                context.LineTo(600.0, 600.0);
                context.ShowText("Message");
                context.Stroke();

                context.Rectangle(700, 700, 200, 200);
                context.Fill();
                context.Stroke();

                double xc = 0.5;
                double yc = 0.5;
                double radius = 0.4;
                double angle1 = 45.0 * (Math.PI / 180.0);  // angles are specified
                double angle2 = 180.0 * (Math.PI / 180.0);  // in radians
                context.Scale(w, h);
                context.LineWidth = 0.04;

                context.Arc(xc, yc, radius, angle1, angle2);
                context.Stroke();

                // draw helping lines
                context.SetSourceColor(new Cairo.Color(1, 0.2, 0.2, 0.6));
                context.Arc(xc, yc, 0.05, 0, 2 * Math.PI);
                context.Fill();
                context.LineWidth = 0.03;
                context.Arc(xc, yc, radius, angle1, angle1);
                context.LineTo(new PointD(xc, yc));
                context.Arc(xc, yc, radius, angle2, angle2);
                context.LineTo(new PointD(xc, yc));
                context.Stroke();

                context.SetSourceColor(new Cairo.Color(1, 0.2, 0.2, 0.6));
                context.Translate(100, 100);
                context.Scale(Math.Min(w / 640.0, h / 480.0), Math.Min(w / 640.0, h / 480.0));
                context.Arc(0, 0, 10.0, 0.0, 2 * Math.PI);
                context.Fill();
                context.Stroke();

                context.Save();
                imageSurface.WriteToPng("aa.png");
            }*/
            
            var win = init_window();
            win.Name = "myWindow";
            //将标签加入到窗体
            win.Add(MainForm.init_main_form());

            win.ShowAll();//显示窗体

            Gtk.Application.Run();//运行窗体

            if (Configure.GetInstance().IsServer == 0)
            {
                TcpChatClient.Program.Stop();
            }
            else
            {
                TcpChatServer.Program.Stop();
            }
        }
    }
}
