using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chip8Emulator
{
    public class PixelGrid
    {
        public const int BLACK = 0;
        public const int WHITE = 1;

        public const int l = 64; //number of pixel on x axis
        public const int L = 32; // number of pixel y axis
        public const int DIMPIXEL = 8; // size of a pixel
        public const int WIDTH = l * DIMPIXEL; // x size of the screen
        public const int HEIGHT = L * DIMPIXEL;// y size of the screen

        public Pixel[,] Pixels = new Pixel[WIDTH, HEIGHT];

        public void initializePixel()
        {
            byte x = 0, y = 0;

            for(x = 0; x < l; x++)
            {
                for(y = 0; y < L; y++)
                {
                    Pixels[x,y].x = x * DIMPIXEL;
                    Pixels[x, y].y = y * DIMPIXEL;
                    Pixels[x, y].color = BLACK; // we set black as default color
                }
            }
        }
    }
}
