using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

int size = 512;
var bmp = new Bitmap(size, size);
using (var g = Graphics.FromImage(bmp))
{
    g.SmoothingMode = SmoothingMode.AntiAlias;
    g.Clear(Color.Transparent);

    int m = 40;
    var rect = new Rectangle(m, m, size - 2*m, size - 2*m);

    using var shadow = new SolidBrush(Color.FromArgb(60, 0, 0, 0));
    g.FillEllipse(shadow, rect.X + 6, rect.Y + 6, rect.Width, rect.Height);

    using var bg = new LinearGradientBrush(rect, Color.FromArgb(0, 210, 255), Color.FromArgb(0, 100, 220), LinearGradientMode.ForwardDiagonal);
    g.FillEllipse(bg, rect);

    using var pen = new Pen(Color.FromArgb(0, 70, 180), 4);
    g.DrawEllipse(pen, rect);

    using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
    using var font = new Font("Segoe UI", 220, FontStyle.Bold, GraphicsUnit.Pixel);
    using var textBrush = new SolidBrush(Color.White);
    g.DrawString("S", font, textBrush, new RectangleF(rect.X, rect.Y - 4, rect.Width, rect.Height), sf);
}

using var ms = new MemoryStream();
bmp.Save(ms, ImageFormat.Png);
byte[] pngBytes = ms.ToArray();

int numIcons = 1;
using var fs = new FileStream(args[0], FileMode.Create);
using var bw = new BinaryWriter(fs);
bw.Write((ushort)0);       // reserved
bw.Write((ushort)1);       // ICO type
bw.Write((ushort)numIcons);
bw.Write((byte)0);         // width (0=256)
bw.Write((byte)0);         // height (0=256)
bw.Write((byte)0);         // color palette
bw.Write((byte)0);         // reserved
bw.Write((ushort)1);       // color planes
bw.Write((ushort)32);      // bits per pixel
bw.Write((uint)pngBytes.Length);
bw.Write((uint)(6 + 16 * numIcons));

bw.Write(pngBytes);
Console.WriteLine($"ICO created: {args[0]} ({pngBytes.Length} bytes, {bmp.Width}x{bmp.Height})");
