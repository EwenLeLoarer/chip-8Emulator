using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chip8Emulator
{
    public partial class Form1 : Form
    {
        PixelGrid screen = new PixelGrid();

        public Form1()
        {
            this.DoubleBuffered = true;
            this.Width = 64;
            this.Height = 32;
            this.Size = new Size(640, 32);
            InitializeComponent();

            screen.InitializePixel();

        }
        //TODO : faire le menu pause
        public void pause()
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            screen.UpdateScreen(sender, e);
            
        }
    }
}
