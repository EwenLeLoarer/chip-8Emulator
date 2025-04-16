using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chip8Emulator
{
    public partial class Form1 : Form
    {
        PixelGrid screen = new PixelGrid();
        public bool isRunning = true;
        cpu cpu = new cpu();
        public Form1()
        {
            this.DoubleBuffered = true;

            InitializeComponent();

            screen.InitializePixel();
            this.cpu.CPUInitialize();
            this.cpu.InitializeJump();
            this.cpu.getAction(0x8475);
            Thread gameLoop = new Thread(RunGameLoop)
            {
                IsBackground = true
            };
            
            
            gameLoop.Start();



        }
        //TODO : faire le menu pause
        public void pause()
        {

        }

        public void RunGameLoop()
        {
            const int targetFrameRate = 60;
            const int frameDelay = 250;

            while (isRunning)
            {
                if (this.IsHandleCreated) // ✅ Ensure the window handle exists
                {
                    this.BeginInvoke((MethodInvoker)(() => this.Invalidate()));
                }
                Thread.Sleep(frameDelay);
            }
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
