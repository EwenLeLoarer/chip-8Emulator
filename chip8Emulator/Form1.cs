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
        PixelGrid screen = new PixelGrid();
        public bool isRunning = true;
        public static int VITESSECPU = 4;
        public static int FPS = 16;
        public static int SCALE = 8;
        cpu cpu = new cpu();
        public Form1()
        {
            this.DoubleBuffered = true;

            InitializeComponent();

            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            this.KeyPreview = true; 
            

            //screen.InitializePixel();
            this.cpu.CPUInitialize();
            this.cpu.InitializeJump();


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
        private void Start()
        {
            if (LoadGame("7-beep.ch8") == 0)
            {
                Console.WriteLine("Failed to load ROM.");
                return;
            }

            Task.Run(() => GameLoop());
        }

        private void GameLoop()
        {
            const int targetFps = 60;
            const int frameTime = 1000 / targetFps;

            var stopwatch = new Stopwatch();

            while (isRunning)
            {
                stopwatch.Restart();
                ExecuteCpuCycles();
                cpu.decompter();

                if (screen.NeedToBeUpdated)
                {
                    screen.NeedToBeUpdated = false;
                    BeginInvoke((MethodInvoker)(() => Invalidate()));
                }

                int elapsed = (int)stopwatch.ElapsedMilliseconds;
                int delay = Math.Max(0, frameTime - elapsed);
                Thread.Sleep(delay);
            }
        }

        private void ExecuteCpuCycles()
        {
            for (int i = 0; i < VITESSECPU; i++)
            {

                // Si opcode Fx0A, gestion d'attente clavier déjà intégrée dans InterprateOpCode()
                if (cpu.isWaitingForKeys)
                    break;

                ushort opcode = cpu.getOpCode();
                screen.InterprateOpCode(opcode, cpu);

            }
        }

        public byte LoadGame(string nomJeu)
        {
            try
            {
                // Lire le fichier ROM en binaire
                byte[] contenu = File.ReadAllBytes(nomJeu);

                // Vérifie que le fichier ne déborde pas la mémoire disponible
                if (contenu.Length > this.cpu.memory.Length - 512)
                {
                    Console.Error.WriteLine("Fichier trop grand pour la mémoire.");
                    return 0;
                }

                // Copie dans la mémoire à partir de l'adresse de départ (ex: 0x200)
                Array.Copy(contenu, 0, this.cpu.memory, 512, contenu.Length);

                return 1;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Erreur lors du chargement du jeu : {e.Message}");
                return 0;
            }
        }

        public void RunGameLoop()
        {
            const int targetFrameRate = 60;
            const int frameDelay = 250;


                

            byte start = 0, compteur = 0;
            start = LoadGame("7-beep.ch8");

            if(start != 1)
            {
                Console.WriteLine("error launching the game :(");
                return;
            }
            while (isRunning)
            {
                for (compteur = 0; compteur < VITESSECPU; compteur++)
                {
                    if (!this.cpu.isWaitingForKeys)
                    {
                        this.screen.InterprateOpCode(this.cpu.getOpCode(), this.cpu);
                        if (this.screen.NeedToBeUpdated)
                        {
                            this.Invalidate();
                            this.screen.NeedToBeUpdated = false;
                        }
                    }

                }

                while (cpu.counterSound != 0)
                {
                    Console.Beep();
                }

                this.cpu.decompter();
                Thread.Sleep(16);
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
                    if (screen.screen[x, y] == 1)
                    {
                        e.Graphics.FillRectangle(Brushes.White, x * SCALE, y * SCALE, SCALE, SCALE);
                    }
                    else
                    {
                        e.Graphics.FillRectangle(Brushes.Black, x * SCALE, y * SCALE, SCALE, SCALE);
                    }
                }
            }
            
        }

            private void Form1_KeyDown(object sender, KeyEventArgs e)
            {
                switch (e.KeyCode)
                {
                    case Keys.D1: this.cpu.keys[0x1] = true; break;
                    case Keys.D2: this.cpu.keys[0x2] = true; break;
                    case Keys.D3: this.cpu.keys[0x3] = true; break;
                    case Keys.D4: this.cpu.keys[0xC] = true; break;
                
                    case Keys.A: this.cpu.keys[0x4] = true; break;
                    case Keys.Z: this.cpu.keys[0x5] = true; break;
                    case Keys.E: this.cpu.keys[0x6] = true; break;
                    case Keys.R: this.cpu.keys[0xD] = true; break;
                
                    case Keys.Q: this.cpu.keys[0x7] = true; break;
                    case Keys.S: this.cpu.keys[0x8] = true; break;
                    case Keys.D: this.cpu.keys[0x9] = true; break;
                    case Keys.F: this.cpu.keys[0xE] = true; break;
                
                    case Keys.W: this.cpu.keys[0xA] = true; break;
                    case Keys.X: this.cpu.keys[0x0] = true; break;
                    case Keys.C: this.cpu.keys[0xB] = true; break;
                    case Keys.V: this.cpu.keys[0xF] = true; break;
                
                    default: break;
                }

                if(this.cpu.isWaitingForKeys)
                {
                    this.cpu.isWaitingForKeys = false;
                    this.cpu.pc += 2;
                }
            }
        
            private void Form1_KeyUp(object sender, KeyEventArgs e)
            {
                switch (e.KeyCode)
                {
                    case Keys.D1: this.cpu.keys[0x1] = false; break;
                    case Keys.D2: this.cpu.keys[0x2] = false; break;
                    case Keys.D3: this.cpu.keys[0x3] = false; break;
                    case Keys.D4: this.cpu.keys[0xC] = false; break;
                
                    case Keys.A: this.cpu.keys[0x4] = false; break;
                    case Keys.Z: this.cpu.keys[0x5] = false; break;
                    case Keys.E: this.cpu.keys[0x6] = false; break;
                    case Keys.R: this.cpu.keys[0xD] = false; break;
                
                    case Keys.Q: this.cpu.keys[0x7] = false; break;
                    case Keys.S: this.cpu.keys[0x8] = false; break;
                    case Keys.D: this.cpu.keys[0x9] = false; break;
                    case Keys.F: this.cpu.keys[0xE] = false; break;
                
                    case Keys.W: this.cpu.keys[0xA] = false; break;
                    case Keys.X: this.cpu.keys[0x0] = false; break;
                    case Keys.C: this.cpu.keys[0xB] = false; break;
                    case Keys.V: this.cpu.keys[0xF] = false; break;
                }
            }
    }
}
