using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public cpu cpu = new cpu();
        
        public PixelGrid()
        {
            this.cpu.CPUInitialize();
            this.cpu.InitializeJump();
        }

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

        public void DrawScreen(byte b1, byte b2, byte b3)
        {
            byte x = 0, y = 0, k = 0, codage = 0, j = 0, decalage = 0;
            this.cpu.V[0xF] = 0;
            for (k = 0; k < b1; k++)
            {
                codage = cpu.memory[this.cpu.I + k]; //on récupère le codage de la ligne à dessiner 

                y = (byte)((this.cpu.V[b2] + k) % L); //on calcule l'ordonnée de la ligne à dessiner, on ne doit pas dépasser L 

                for (j = 0; j < b3; j++)
                {
                    x = (byte)((this.cpu.V[b3] + j) % l); //on calcule l'abscisse, on ne doit pas dépasser l 
                    if (((codage)& (0x1<<decalage))!=0) //on récupère le bit correspondant 
                    {//si c'est blanc
                        if (Pixels[x, y].color == WHITE)
                        {
                            Pixels[x, y].color = BLACK; // on l'eteint
                            this.cpu.V[0xF] = 1;
                        }
                        else
                        {
                            Pixels[x, y].color = WHITE;
                        }
                    }
                }
            }
        }



        public void InterprateOpCode(UInt16 opcode, object sender, PaintEventArgs e)
        {
            byte b4;
            b4 = this.cpu.getAction(opcode);
            byte b3, b2, b1;

            b3 = (byte)((opcode & (0x0F00)) >> 8);  //on prend les 4 bits, b3 représente X 
            b2 = (byte)((opcode & (0x00F0)) >> 4);  //idem, b2 représente Y 
            b1 = (byte)((opcode & (0x000F)));     //on prend les 4 bits de poids faible

            /* 
                Pour obtenir NNN par exemple, il faut faire (b3<<8) + (b2<<4) + (b1) 

            */

            switch (b4)
            {
                case 0: //non implementer
                    break;
                case 1: //00E0 : efface l'ecran
                    this.ResetScreen(sender, e);
                    //this.cpu.pc += 2; // Avancer au prochain opcode
                    break;
                case 2: //00EE : revient du saut
                     if(this.cpu.nbrJump>0)
                    {
                        this.cpu.nbrJump--;
                        this.cpu.pc = this.cpu.jump[this.cpu.nbrJump];
                    }
                    break;
                case 3: //1NNN : effectue un saut à l'adresse 1NNN 

                    cpu.pc = (UInt16)((b3 << 8) + (b2 << 4) + b1); //on prend le nombre NNN (pour le saut) 
                    cpu.pc -= 2; //n'oublions pas le pc+=2 à la fin du bloc switch
                    break;
                case 4: //2NNN : appelle le sous-programme en NNN, mais on revient ensuite
                    this.cpu.jump[this.cpu.nbrJump] = this.cpu.pc;
                    
                    if(this.cpu.nbrJump<15)
                    {
                        this.cpu.nbrJump++;
                    }
                    cpu.pc = (UInt16)((b3 << 8) + (b2 << 4) + b1)   ; //on prend le nombre NNN (pour le saut) 
                    cpu.pc -= 2; //n'oublions pas le pc+=2 à la fin du block switch

                    break;
                case 5: //3XNN saute l'instruction suivante si VX est égal à NN. 
                    if (cpu.V[b3] == ((b2<<4) + b1))
                    {
                        cpu.pc += 2;
                    }
                    break;
                case 6: //4XNN saute l'instruction suivante si VX et NN ne sont pas égaux.
                    if (cpu.V[b3] != ((b2 << 4) + b1))
                    {
                        cpu.pc += 2;
                    }
                    break;
                case 7:  //5XY0 saute l'instruction suivante si VX et VY sont égaux. 
                    if (cpu.V[b3] == cpu.V[b2])
                    {
                        cpu.pc += 2;
                    }
                    break;
                case 8:  //6XNN définit VX à NN.
                    this.cpu.V[b3] = (byte)((b2 << 4) + b1);
                    break;
                case 9: //7XNN ajoute NN à VX. 
                    this.cpu.V[b3] += (byte)((b2 << 4) + b1);
                    break;
                case 10: //8XY0 définit VX à la valeur de VY. 
                    this.cpu.V[b3] = this.cpu.V[b2];
                    break;
                case 11: //8XY1 définit VX à VX OR VY.
                    this.cpu.V[b3] = (byte)(this.cpu.V[b3] | this.cpu.V[b2]);
                    break;
                case 12: //8XY2 définit VX à VX AND VY.
                    this.cpu.V[b3] = (byte)(this.cpu.V[b3] & this.cpu.V[b2]);
                    break;
                case 13:  //8XY3 définit VX à VX XOR VY. 
                    this.cpu.V[b3] = (byte)(this.cpu.V[b3] ^ this.cpu.V[b2]);
                    break;
                case 14: //8XY4 ajoute VY à VX. VF est mis à 1 quand il y a un dépassement de mémoire (carry), et à 0 quand il n'y en pas. 
                    if ((this.cpu.V[b3] + this.cpu.V[b2]) > 0xFF)
                    {
                        this.cpu.V[0xF] = 1; //V[15] 
                    }
                    else
                    {
                        this.cpu.V[0xF] = 0; //V[15] 
                    }
                    this.cpu.V[b3] += this.cpu.V[b2];
                    break;
                case 15:  //8XY5 VY est soustraite de VX. VF est mis à 0 quand il y a un emprunt, et à 1 quand il n'y a en pas. 
                    if (this.cpu.V[b3] < this.cpu.V[b2])
                    {
                        this.cpu.V[0xF] = 0;
                    }
                    else
                    {
                        this.cpu.V[0xF] = 1;
                    }
                    this.cpu.V[b3] -= this.cpu.V[b2];
                    break;
                case 16:  //8XY6 décale (shift) VX à droite de 1 bit. VF est fixé à la valeur du bit de poids faible de VX avant le décalage. 
                    this.cpu.V[0xF] = (byte)(this.cpu.V[b3] & (0x01));
                    this.cpu.V[b3] = (byte)(this.cpu.V[b3] >> 1);
                    break;
                case 17: //8XY7 VX = VY - VX. VF est mis à 0 quand il y a un emprunt et à 1 quand il n'y en a pas. 
                    if ((this.cpu.V[b2] < this.cpu.V[b3]))
                    {
                        this.cpu.V[0xF] = 0;//this.cpu.V[15]
                    }
                    else
                    {
                        this.cpu.V[0xF] = 1;
                    }
                    break;
                case 18: //8XYE décale (shift) VX à gauche de 1 bit. VF est fixé à la valeur du bit de poids fort de VX avant le décalage. 
                    this.cpu.V[0xF] = (byte)(this.cpu.V[b3] & (0x01));
                    this.cpu.V[b3] = (byte)(this.cpu.V[b3] << 1);
                    break;
                case 19:  //9XY0 saute l'instruction suivante si VX et VY ne sont pas égaux. 
                    if(b3 != b2)
                    {
                        this.cpu.pc += 2;
                    }
                    break;
                case 20: //ANNN affecte NNN à I. 
                    this.cpu.I = (byte)((b3 << 8) + (b2 << 4) + b1);
                    break;
                case 21:  //BNNN passe à l'adresse NNN + V0. 
                    this.cpu.pc = (byte)((b3 << 8) + (b2 << 4) + b1 + this.cpu.V[0]);
                    break;
                case 22:  //CXNN définit VX à un nombre aléatoire inférieur à NN.
                    Random rand = new Random();
                    this.cpu.V[b3] =  (byte)rand.Next(0,((b2 << 4) + b1 + 1));
                    break;
                case 23: //DXYN dessine un sprite aux coordonnées (VX, VY). 

                    DrawScreen(b1, b2, b3);
                    break;
                case 24: //EX9E saute l'instruction suivante si la clé stockée dans VX est pressée.
                         //
                    break;
                case 25:  //EXA1 saute l'instruction suivante si la clé stockée dans VX n'est pas pressée. 
                    break;
                case 26: //FX07 définit VX à la valeur de la temporisation. 
                    this.cpu.V[b3] = (byte)this.cpu.counterGame;
                    break;
                case 27: //FX0A attend l'appui sur une touche et la stocke ensuite dans VX. 
                    break;
                case 28:  //FX15 définit la temporisation à VX. 
                    this.cpu.counterGame = this.cpu.V[b3];
                    break;
                case 29: //FX18 définit la minuterie sonore à VX. 
                    this.cpu.counterSound = this.cpu.V[b3];
                    break;
                case 30: //FX1E ajoute à VX I. VF est mis à 1 quand il y a overflow (I+VX>0xFFF), et à 0 si tel n'est pas le cas.
                    if ((this.cpu.I + this.cpu.V[b3]) > 0xFFF)
                    {
                        this.cpu.V[0xF] = 1;
                    }
                    else
                    {
                        this.cpu.V[0xF] = 0;
                    }
                    this.cpu.I += this.cpu.V[b3];
                    break;
                case 31: //FX29 définit I à l'emplacement du caractère stocké dans VX. Les caractères 0-F (en hexadécimal) sont représentés par une police 4x5. 
                    this.cpu.I = (byte)(cpu.V[b3] * 5);
                    break;
                case 32: //FX33 stocke dans la mémoire le code décimal représentant VX (dans I, I+1, I+2). 
                    this.cpu.memory[cpu.I] = (byte)((this.cpu.V[b3] - this.cpu.V[b3] % 100) / 100); //stocke les centaines 
                    this.cpu.memory[cpu.I + 1] = (byte)(((cpu.V[b3] - cpu.V[b3] % 10) / 10) % 10);//les dizaines 
                    this.cpu.memory[cpu.I + 2] = (byte)(cpu.V[b3] - this.cpu.memory[cpu.I] * 100 - this.cpu.memory[cpu.I + 1] * 10);//les unités
                    break;
                case 33: //FX55 stocke V0 à VX en mémoire à partir de l'adresse I.
                    for (byte i = 0; i <= b3; i++)
                    {
                        this.cpu.memory[cpu.I + i] = cpu.V[i];
                    }
                    break;
                case 34: //FX65 remplit V0 à VX avec les valeurs de la mémoire à partir de l'adresse I. 


                    for (byte i = 0; i <= b3; i++)
                    {
                        cpu.V[i] = this.cpu.memory[cpu.I + i];
                    }
                    break;
                default:
                    break;
            }
            cpu.pc += 2; //on passe au prochain opcode
        }
    }
}
