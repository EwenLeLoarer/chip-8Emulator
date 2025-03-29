using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chip8Emulator
{
    public class Pixel
    {
        public int x;
        public int y;
        public byte color;

        public Pixel(int x, int y, byte color)
        {
            this.x = x;
            this.y = y;
            this.color = color;
        }

        public Pixel()
        {
            this.x = 0;
            this.y = 0;
            this.color = 0;
        }
    }
}
