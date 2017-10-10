using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MandelBrot
{
    public class MandelbrotForm : Form
    {
        public const int ADD = 125, SIZE = 200;
        private Bitmap bmp;

        private ColorBox Color_True, Color_False;
        private InputBox<float> Mid_X, Mid_Y, Scalar;
        private InputBox<int> Max;
        private BoolBox Zoom;
        private PictureBox picture;

        public MandelbrotForm()
        {
            this.SuspendLayout();

            LabeledInputBox.OFFSET = 10;
            LabeledInputBox.LABEL_WIDTH = 65;

            Mid_X  = new InputBox<float>(this,     "x", "X Mid:",   0);
            Mid_Y  = new InputBox<float>(this,     "y", "Y Mid:",   0);
            Scalar = new InputBox<float>(this, "scale", "Scale:",   1);
            Max    = new InputBox<int>  (this,   "max",   "Max:", 100);

            Color_True  = new ColorBox(this,  "true", "Inner Color:", KnownColor.White);
            Color_False = new ColorBox(this, "false", "Outer Color:", KnownColor.Black);

            Zoom = new BoolBox(this, "zoom", "Zooming:", true);

            picture = new PictureBox();
            picture.Name = "picture";
            picture.Location = new Point(
                Program.Max(Mid_X.box.Right, Mid_Y.box.Right, Scalar.box.Right, Max.box.Right, Color_True.box.Right, Color_False.box.Right, Zoom.box.Right) + LabeledInputBox.OFFSET,
                LabeledInputBox.OFFSET);
            picture.MouseClick += new MouseEventHandler(this.picture_click);
            Controls.Add(picture);

            this.Name = "Mandelbrot";
            this.Text = "MandelBrot";
            this.ClientSize = new Size(picture.Left + Zoom.box.Bottom, picture.Top + Zoom.box.Bottom);
            this.MinimumSize = this.Size;

            this.ResizeEnd += (o, e) => AfterResize();
            this.Shown += (o, e) => AfterResize();

            this.ResumeLayout(true);
            this.PerformLayout();
        }

        public void Draw()
        {
            if (bmp == null) return;
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            int bpp = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
            int width = data.Width * bpp;

            unsafe
            {
                Parallel.For(0, data.Height, (y) => {
                    byte* currentLine = (byte*)data.Scan0 + (y * data.Stride);
                    for (int x = 0; x < width; x += bpp)
                    {
                        float newx = ((float)x * 4 / width - 2) * Scalar.Value - Mid_X.Value;
                        float newy = ((float)y * 4 / data.Height - 2) * Scalar.Value - Mid_Y.Value;
                        Color color = MandelNumber(newx, newy) ? Color_True.Value : Color_False.Value;

                        currentLine[x] = color.B;
                        currentLine[x + 1] = color.G;
                        currentLine[x + 2] = color.R;
                        currentLine[x + 3] = color.A;
                    }
                });
            }
            bmp.UnlockBits(data);
            picture.Invalidate();
        }

        private bool MandelNumber(float x0, float y0)
        {
            float x = 0, y = 0, xtemp;
            int iteration = 0;

            while (x * x + y * y <= 4 && iteration < Max.Value)
            {
                xtemp = x * x - y * y + x0;
                y = 2 * x * y + y0;
                x = xtemp;
                iteration++;
            }
            return (iteration & 1) == 0;
        }

        private void AfterResize()
        {
            //picture.Size = ClientSize - Program.SquareSize(LabeledInputBox.OFFSET) - (Size)picture.Location;
            picture.Size = Program.SquareSize(Math.Min(ClientSize.Width - picture.Left - LabeledInputBox.OFFSET, ClientSize.Height - 2 * LabeledInputBox.OFFSET));
            ClientSize = new Size(picture.Right + LabeledInputBox.OFFSET, picture.Bottom + LabeledInputBox.OFFSET);

            bmp = new Bitmap(picture.Width, picture.Height);

            Draw();
            picture.Image = bmp;
        }

        private void picture_click(object sender, MouseEventArgs e)
        {
            Mid_X.Value -= ((float)e.X * 4 / picture.Width - 2) * Scalar.Value;

            Mid_Y.Value -= ((float)e.Y * 4 / picture.Height - 2) * Scalar.Value;

            if (Zoom.Value)
                Scalar.Value /= 2;

            Draw();
        }
    }
}
