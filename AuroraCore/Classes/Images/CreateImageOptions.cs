using Microsoft.AspNetCore.Hosting;
using System.Drawing;
using System.Drawing.Imaging;

namespace AuroraCore.Classes.Images
{
    public class CreateImageOptions
    {
        readonly IWebHostEnvironment _env = null;
        public CreateImageOptions(IWebHostEnvironment env)
        {
            _env = env;
            Path = _env.WebRootPath + @"\images\lights\";
        }
        public ImageFormat Type { get; set; } = ImageFormat.Png;
        public string Path { get; private set; }
        public string Extension => Type.ToString().ToLower();
        public Color Background { get; set; } = Color.Transparent;
        public Color BorderColor { get; set; } = Color.Red;
        public int ResizeFactor { get; set; } = 3;
        public int Border { get; set; } = 100;
        public int BorderResized => ResizeFactor * Border;


    }
}
