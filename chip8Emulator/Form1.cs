using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chip8Emulator
{
    public partial class Form1 : Form
    {
        private PixelGrid _screen = new PixelGrid();
        private bool _isRunning = true;
        private static int _cpuSpeed = 4;
        public static int FPS = 16;
        private static int _scale = 8;
        private static int _baseHeight = 256;
        private static int _baseWidth = 512;
        private static int _offsetHeight = 24;
        private int _scalePixel = _scale;
        
        private string _romName;
        
        private OpenFileDialog _changeRom = new OpenFileDialog();
        CPU _cpu = new CPU();
        private Thread _gameLoopThread;

        public Form1()
        {
            this.DoubleBuffered = true;

            InitializeComponent();

            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            this.KeyPreview = true; 

            this._cpu.CpuInitialize();
            this._cpu.InitializeJump();
        }

        private void GameLoop()
        {
            const int targetFps = 60;
            const int frameTime = 1000 / targetFps;

            var stopwatch = new Stopwatch();

            while (_isRunning)
            {
                stopwatch.Restart();
                ExecuteCpuCycles();
                _cpu.Counter();

                if (_screen.NeedToBeUpdated)
                {
                    _screen.NeedToBeUpdated = false;
                    BeginInvoke((MethodInvoker)(() => Invalidate()));
                }

                int elapsed = (int)stopwatch.ElapsedMilliseconds;
                int delay = Math.Max(0, frameTime - elapsed);
            }
        }

        private void ExecuteCpuCycles()
        {
            for (int i = 0; i < _cpuSpeed; i++)
            {

                // if the opcode fx0A is running, we wait an input before continue
                if (_cpu.IsWaitingForKeys)
                    break;

                ushort opcode = _cpu.GetOpCode();
                _screen.InterpretOpCode(opcode, _cpu);

            }
        }

        private byte LoadGame(string romName)
        {
            try
            {
                // read the file's binary
                byte[] content = File.ReadAllBytes(romName);

                // check if the file is not too big
                if (content.Length > this._cpu.Memory.Length - 512)
                {
                    Console.Error.WriteLine("File too big for memory.");
                    return 0;
                }

                // copy the array at the start address 0x200
                Array.Copy(content, 0, this._cpu.Memory, 512, content.Length);

                return 1;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error loading the game : {e.Message}");
                return 0;
            }
        }

        public void RunGameLoop()
        {
            byte start = 0, counter = 0;
            start = LoadGame(_romName);

            if(start != 1)
            {
                Console.WriteLine("error launching the game :(");
                return;
            }
            while (_isRunning)
            {
                for (counter = 0; counter < _cpuSpeed; counter++)
                {
                    if (!this._cpu.IsWaitingForKeys)
                    {
                        this._screen.InterpretOpCode(this._cpu.GetOpCode(), this._cpu);
                        if (this._screen.NeedToBeUpdated)
                        {
                            this.Invalidate();
                            this._screen.NeedToBeUpdated = false;
                        }
                    }

                }

                while (_cpu.CounterSound != 0)
                {
                    Console.Beep();
                }

                this._cpu.Counter();
                Thread.Sleep(8);
            }              
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (_screen.Screen[x, y] == 1)
                    {
                        e.Graphics.FillRectangle(Brushes.White, x * _scalePixel, y * _scalePixel + _offsetHeight, _scalePixel, _scalePixel);
                    }
                    else
                    {
                        e.Graphics.FillRectangle(Brushes.Black, x * _scalePixel, y * _scalePixel + _offsetHeight, _scalePixel, _scalePixel);
                    }
                }
            }
            
        }

            private void Form1_KeyDown(object sender, KeyEventArgs e)
            {
                switch (e.KeyCode)
                {
                    case Keys.D1: this._cpu.Keys[0x1] = true; break;
                    case Keys.D2: this._cpu.Keys[0x2] = true; break;
                    case Keys.D3: this._cpu.Keys[0x3] = true; break;
                    case Keys.D4: this._cpu.Keys[0xC] = true; break;
                
                    case Keys.A: this._cpu.Keys[0x4] = true; break;
                    case Keys.Z: this._cpu.Keys[0x5] = true; break;
                    case Keys.E: this._cpu.Keys[0x6] = true; break;
                    case Keys.R: this._cpu.Keys[0xD] = true; break;
                
                    case Keys.Q: this._cpu.Keys[0x7] = true; break;
                    case Keys.S: this._cpu.Keys[0x8] = true; break;
                    case Keys.D: this._cpu.Keys[0x9] = true; break;
                    case Keys.F: this._cpu.Keys[0xE] = true; break;
                
                    case Keys.W: this._cpu.Keys[0xA] = true; break;
                    case Keys.X: this._cpu.Keys[0x0] = true; break;
                    case Keys.C: this._cpu.Keys[0xB] = true; break;
                    case Keys.V: this._cpu.Keys[0xF] = true; break;
                
                    default: break;
                }

                if(this._cpu.IsWaitingForKeys)
                {
                    this._cpu.IsWaitingForKeys = false;
                    this._cpu.Pc += 2;
                }
            }
        
            private void Form1_KeyUp(object sender, KeyEventArgs e)
            {
                switch (e.KeyCode)
                {
                    case Keys.D1: this._cpu.Keys[0x1] = false; break;
                    case Keys.D2: this._cpu.Keys[0x2] = false; break;
                    case Keys.D3: this._cpu.Keys[0x3] = false; break;
                    case Keys.D4: this._cpu.Keys[0xC] = false; break;
                
                    case Keys.A: this._cpu.Keys[0x4] = false; break;
                    case Keys.Z: this._cpu.Keys[0x5] = false; break;
                    case Keys.E: this._cpu.Keys[0x6] = false; break;
                    case Keys.R: this._cpu.Keys[0xD] = false; break;
                
                    case Keys.Q: this._cpu.Keys[0x7] = false; break;
                    case Keys.S: this._cpu.Keys[0x8] = false; break;
                    case Keys.D: this._cpu.Keys[0x9] = false; break;
                    case Keys.F: this._cpu.Keys[0xE] = false; break;
                
                    case Keys.W: this._cpu.Keys[0xA] = false; break;
                    case Keys.X: this._cpu.Keys[0x0] = false; break;
                    case Keys.C: this._cpu.Keys[0xB] = false; break;
                    case Keys.V: this._cpu.Keys[0xF] = false; break;
                }
            }

        private void changeRomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(_changeRom.ShowDialog() == DialogResult.OK)
            {
                _romName = _changeRom.FileName;

                if (_gameLoopThread != null && _gameLoopThread.IsAlive)
                {
                    _isRunning = false;
                    _gameLoopThread.Join(); // Wait for the thread to stop
                }

                // Reset emulator state here
                _cpu = new CPU(); // Reinitialize your CPU object
                _screen = new PixelGrid(); // Reinitialize your screen object
                this._cpu.CpuInitialize();
                this._cpu.InitializeJump();
                _isRunning = true;


                _gameLoopThread = new Thread(RunGameLoop)
                {
                    IsBackground = true
                };
                _gameLoopThread.Start();
            }

        }

        private void Scale(double scaleFactor)
        {
            _scalePixel = (int)(_scale * scaleFactor);
            this.Size = new Size((int)(_baseWidth * scaleFactor), (int)(_baseHeight * scaleFactor) + _offsetHeight);
            _screen.UpdateScreen();
        }
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Scale(0.5);
        }

        private void x1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Scale(1);
        }

        private void x15ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Scale(1.5);
        }

        private void x2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Scale(2);
        }

        private void x25ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Scale(2.5);
        }

        private void x3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Scale(3);
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Help help = new Help();
            help.Show();
        }
    }
}
