using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

            while (isRunning)
            {
                if (this.IsHandleCreated) // ✅ Ensure the window handle exists
                {
                    byte start = 0, continuer = 1, compteur = 0;
                    start = LoadGame("testIBM.ch8");
                    if (start == 1)
                    {
                        do
                        {
                            continuer = 1; //afin de pouvoir quitter l'émulateur 

                            for (compteur = 0; compteur < VITESSECPU; compteur++)
                            {
                                this.screen.InterprateOpCode(this.cpu.getOpCode(), this.cpu);
                                if (this.screen.NeedToBeUpdated)
                                {
                                    this.Invalidate();
                                    this.screen.NeedToBeUpdated = false;
                                }

                                this.cpu.pc += 2;
                            }

                            //screen.UpdateScreen();
                            this.cpu.decompter();
                            Thread.Sleep(16);

                        } while (continuer == 1);
                    }
                    this.BeginInvoke((MethodInvoker)(() => this.Invalidate()));
                }

                Console.WriteLine("resetDisplay");
                Thread.Sleep(frameDelay);
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
    }
}
