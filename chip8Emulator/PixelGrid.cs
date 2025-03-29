using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chip8Emulator
{
    public class PixelGrid
    {
        public const byte BLACK = 0;
        public const byte WHITE = 1;

        public const int l = 64; //number of pixel on x axis
        public const int L = 32; // number of pixel y axis
        public const int DIMPIXEL = 8; // size of a pixel
        public const int WIDTH = l * DIMPIXEL; // x size of the screen
        public const int HEIGHT = L * DIMPIXEL;// y size of the screen

        public Pixel[,] Pixels = new Pixel[WIDTH, HEIGHT];

        public void InitializePixel()
        {
            byte x = 0, y = 0;

            for(x = 0; x < l; x++)
            {
                for(y = 0; y < L; y++)
                {
                    Pixel pixel = new Pixel();
                    Pixels[x,y] = pixel;
                    Pixels[x,y].x = x * DIMPIXEL;
                    Pixels[x,y].y = y * DIMPIXEL;
                    if (x % (y + 1) == 0)
                        Pixels[x, y].color = BLACK;
                    else
                        Pixels[x,y].color = WHITE;
                }
            }
        }

        public void DrawPixel(Pixel pixel, object sender, PaintEventArgs e)
        {
            using (Brush brush = new SolidBrush(pixel.color == 0 ? Color.Black : Color.White))
            {
                e.Graphics.FillRectangle(brush, pixel.x, pixel.y, DIMPIXEL, DIMPIXEL);
            }
        }

        public void ResetScreen(object sender, PaintEventArgs e)
        {
            byte x = 0, y = 0;
            for(x = 0; x < l; x++)
            {
                for(y = 0; y < L;y++)
                {
                    Pixels[x, y].color = BLACK;
                }
            }
            using (Brush brush = new SolidBrush(Color.Black))
            {
                e.Graphics.FillRectangle(brush, 0, 0, WIDTH, HEIGHT);
            }

        }

        public void UpdateScreen(object sender, PaintEventArgs e)
        {
            byte x = 0, y = 0;
            for(x = 0; x < l;x++)
            {
                for (y = 0; y < L; y++)
                {
                    DrawPixel(Pixels[x, y], sender, e);
                }
            }
        }
    }
}
