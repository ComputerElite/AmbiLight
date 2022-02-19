using ComputerUtils.Logging;
using ComputerUtils.Webserver;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;

namespace AmbiLightFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Computing");
            int width = Screen.PrimaryScreen.Bounds.Size.Width;
            int height = Screen.PrimaryScreen.Bounds.Size.Height;
            Bitmap bmpScreenshot = new Bitmap(width,
                                           height,
                                           System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            Graphics g = Graphics.FromImage(bmpScreenshot);


            Config config = Config.Load();
            HttpServer server = new HttpServer();
            server.AddRoute("GET", "/ambilight", new Func<ServerRequest, bool>(request =>
            {
                request.SendString(JsonSerializer.Serialize(AmbiLightController.GetAmbiLight(config.top, config.right, config.bottom, config.left, width, height, config.sample, config.normalise, bmpScreenshot, g)));
                return true;
            }));
            server.AddRouteFile("/viewer", "viewer.html");
            server.AddRouteFile("/config", "config.html");
            server.AddRoute("POST", "/configjson", new Func<ServerRequest, bool>(request =>
            {
                config = JsonSerializer.Deserialize<Config>(request.bodyString);
                config.Save();
                request.SendString("");
                return true;
            }));
            server.AddRoute("GET", "/configjson", new Func<ServerRequest, bool>(request =>
            {
                request.SendString(JsonSerializer.Serialize(config));
                return true;
            }));
            server.AddRouteFile("/ambi.js", "ambi.js");
            server.AddRouteFile("/viewerreversed", "viewerR.html");
            server.AddRouteFile("/", "index.html");
            server.StartServer(503);
        }
    }

    public class Config
    {
        public int top { get; set; } = 40;
        public int right { get; set; } = 40;
        public int bottom { get; set; } = 40;
        public int left { get; set; } = 40;
        public int sample { get; set; } = 500;
        public bool normalise { get; set; } = true;

        public void Save()
        {
            File.WriteAllText("config.json", JsonSerializer.Serialize(this));
        }

        public static Config Load()
        {
            if (!File.Exists("config.json")) return new Config();
            return JsonSerializer.Deserialize<Config>(File.ReadAllText("config.json"));
        }
    }

    public class AmbiLightOutput
    {
        public List<AmbiLightColor> top { get; set; } = new List<AmbiLightColor>();
        public List<AmbiLightColor> right { get; set; } = new List<AmbiLightColor>();
        public List<AmbiLightColor> bottom { get; set; } = new List<AmbiLightColor>();
        public List<AmbiLightColor> left { get; set; } = new List<AmbiLightColor>();
    }

    public class AmbiLightColor
    {
        public int r { get; set; }
        public int g { get; set; }
        public int b { get; set; }
        public string hex { get; set; }

        public override string ToString()
        {
            return "r: " + r + " g: " + g + " b: " + b + " hex: #" + hex;
        }

        public void SetHex()
        {
            hex = r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
        }
    }

    public class AmbiLightController
    {
        public static AmbiLightOutput GetAmbiLight(int top, int right, int bottom, int left, int width, int height, int sample, bool normalise, Bitmap bmpScreenshot, Graphics gfxScreenshot)
        {
            AmbiLightOutput output = new AmbiLightOutput();
            //Create a new bitmap.
            
            gfxScreenshot.CopyFromScreen(0,
                                            0,
                                            0,
                                            0,
                                            new Size(width, height),
                                            CopyPixelOperation.SourceCopy);

            //bmpScreenshot.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb); ;

            int spacing = 10;

            // top
            int topSpacing = width / top;
            for(int x = 0; x < width; x += topSpacing)
            {
                output.top.Add(ColorCalculator.GetColorOfPixel(x, 0, topSpacing, sample, spacing, spacing, normalise, bmpScreenshot));
            }

            //  right
            int rightSpacing = height / right;
            for (int y = 0; y < height; y += rightSpacing)
            {
                output.right.Add(ColorCalculator.GetColorOfPixel(width, y, sample, rightSpacing, spacing, spacing, normalise, bmpScreenshot));
            }

            // bottom
            int bottomSpacing = width / bottom;
            for (int x = 0; x < width; x += bottomSpacing)
            {
                output.bottom.Add(ColorCalculator.GetColorOfPixel(x, height, bottomSpacing, sample, spacing, spacing, normalise, bmpScreenshot));
            }

            //  left
            int leftSpacing = height / left;
            for (int y = 0; y < height; y += leftSpacing)
            {
                output.left.Add(ColorCalculator.GetColorOfPixel(0, y, sample, leftSpacing, spacing, spacing, normalise, bmpScreenshot));
            }
            return output;
        }
    }

    public class ColorCalculator
    {
        public static AmbiLightColor Normalise(AmbiLightColor input)
        {
            double multiply = 255.0;
            if (255 / (double)input.r < multiply) multiply = 255 / (double)input.r;
            if (255 / (double)input.g < multiply) multiply = 255 / (double)input.g;
            if (255 / (double)input.b < multiply) multiply = 255 / (double)input.b;
            input.r = (int)(input.r * multiply);
            input.g = (int)(input.g * multiply);
            input.b = (int)(input.b * multiply);
            input.SetHex();
            return input;
        }
        public static AmbiLightColor GetColorOfPixel(int xS, int yS, int width, int height, int xIntervall, int yIntervall, bool normalise, Bitmap source)
        {
            int yMin = yS - height;
            int yMax = yS + height;
            int xMin = xS - width;
            int xMax = xS + width;
            if (yMin < 0) yMin = 0;
            if(yMax > source.Height) yMax = source.Height;
            if(xMin < 0) xMin = 0;
            if(xMax > source.Width) xMax = source.Width;

            int r = 0;
            int g = 0;
            int b = 0;
            int total = 1;

            Color p;

            for(int x = xMin; x < xMax; x += xIntervall)
            {
                for (int y = yMin; y < yMax; y += yIntervall)
                {
                    p = source.GetPixel(x, y);
                    if (p.R + p.G + p.B < 10) continue;
                    r += p.R;
                    g += p.G;
                    b += p.B;
                    total++;
                }
            }
            r /= total;
            g /= total;
            b /= total;
            if(normalise)
            {
                return ColorCalculator.Normalise(new AmbiLightColor() { r = r, g = g, b = b, hex = r.ToString("X2") + g.ToString("X2") + b.ToString("X2") });
            }
            return new AmbiLightColor() { r = r, g = g, b = b, hex = r.ToString("X2") + g.ToString("X2") + b.ToString("X2") };
        }
    }
}