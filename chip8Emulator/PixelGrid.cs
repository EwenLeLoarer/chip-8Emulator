using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public const int l = 64; //number of pixel on x axis
        public const int L = 32; // number of pixel y axis
        public const int DIMPIXEL = 8; // size of a pixel
        public const int WIDTH = l * DIMPIXEL; // x size of the screen
        public const int HEIGHT = L * DIMPIXEL;// y size of the screen
        public bool NeedToBeUpdated = true;
        
        
        public PixelGrid()
        {

        }

        public byte[,] screen = new byte[WIDTH, HEIGHT];

        public void InitializePixel()
        {
            byte x = 0, y = 0;

            for(x = 0; x < l; x++)
            {
                for(y = 0; y < L; y++)
                {
                    if (x % (y + 1) == 0)
                        this.screen[x, y] = 0;
                    else
                        this.screen[x, y] = 1;
                }
            }
        }

        //TODO: change all pixel to 0
        public void ResetScreen()
        {
            byte x = 0, y = 0;
            for(x = 0; x < l; x++)
            {
                for(y = 0; y < L;y++)
                {
                    screen[x, y] = 0;
                }
            }

            this.NeedToBeUpdated = true;


        }

        public void UpdateScreen()
        {
            NeedToBeUpdated = true;
        }

        public void DrawScreen(byte b1, byte b2, byte b3, cpu ScreenCpu)
        {
            byte x = 0, y = 0, k = 0, codage = 0, j = 0, decalage = 0;
            ScreenCpu.V[0xF] = 0;
            for (k = 0; k < b1; k++)
            {
                codage = ScreenCpu.memory[ScreenCpu.I + k]; //on récupère le codage de la ligne à dessiner 

                y = (byte)((ScreenCpu.V[b2] + k) % L); //on calcule l'ordonnée de la ligne à dessiner, on ne doit pas dépasser L 

                for (j = 0, decalage = 7; j < 8; j++, decalage--)
                {
                    x = (byte)((ScreenCpu.V[b3] + j) % l); //on calcule l'abscisse, on ne doit pas dépasser l 
                    if (((codage)& (0x1<<decalage))!=0) //on récupère le bit correspondant 
                    {//si c'est blanc
                        if (screen[x, y] == 1)
                        {
                            screen[x, y] = 0;
                            ScreenCpu.V[0xF] = 1;
                        }
                        else
                        {
                            screen[x, y] = 1;
                            //test
                        }
                    }
                }

                NeedToBeUpdated = true;
            }
        }

        public void WaitForInputs(byte b3, cpu  ScreenCpu)
        {
            bool wait = true;
            while (wait)
            {
                for (byte i = 0; i < 16; i++)
                {
                    if (ScreenCpu.keys[i])
                    {
                        ScreenCpu.V[b3] = i;
                        wait = false;
                        ScreenCpu.pc += 2;
                    }
                }
            }
            
        }



        public void InterprateOpCode(UInt16 opcode, cpu cpuObject)
        {
            byte b4;
            b4 = cpuObject.getAction(opcode);
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
                    this.ResetScreen();
                    //cpuObject.pc += 2; // Avancer au prochain opcode
                    break;
                case 2: //00EE : revient du saut
                     if(cpuObject.nbrJump>0)
                    {
                        cpuObject.nbrJump--;
                        cpuObject.pc = cpuObject.jump[cpuObject.nbrJump];
                    }
                    break;
                case 3: //1NNN : effectue un saut à l'adresse 1NNN 

                    cpuObject.pc = (UInt16)((b3 << 8) + (b2 << 4) + b1); //on prend le nombre NNN (pour le saut) 
                    cpuObject.pc -= 2; //n'oublions pas le pc+=2 à la fin du bloc switch
                    break;
                case 4: //2NNN : appelle le sous-programme en NNN, mais on revient ensuite
                    cpuObject.jump[cpuObject.nbrJump] = cpuObject.pc;
                    
                    if(cpuObject.nbrJump<15)
                    {
                        cpuObject.nbrJump++;
                    }
                    cpuObject.pc = (UInt16)((b3 << 8) + (b2 << 4) + b1)   ; //on prend le nombre NNN (pour le saut) 
                    cpuObject.pc -= 2; //n'oublions pas le pc+=2 à la fin du block switch

                    break;
                case 5: //3XNN saute l'instruction suivante si VX est égal à NN. 
                    if (cpuObject.V[b3] == ((b2<<4) + b1))
                    {
                        cpuObject.pc += 2;
                    }
                    break;
                case 6: //4XNN saute l'instruction suivante si VX et NN ne sont pas égaux.
                    if (cpuObject.V[b3] != ((b2 << 4) + b1))
                    {
                        cpuObject.pc += 2;
                    }
                    break;
                case 7:  //5XY0 saute l'instruction suivante si VX et VY sont égaux. 
                    if (cpuObject.V[b3] == cpuObject.V[b2])
                    {
                        cpuObject.pc += 2;
                    }
                    break;
                case 8:  //6XNN définit VX à NN.
                    cpuObject.V[b3] = (byte)((b2 << 4) + b1);
                    break;
                case 9: //7XNN ajoute NN à VX. 
                    cpuObject.V[b3] += (byte)((b2 << 4) + b1);
                    break;
                case 10: //8XY0 définit VX à la valeur de VY. 
                    cpuObject.V[b3] = cpuObject.V[b2];
                    break;
                case 11: //8XY1 définit VX à VX OR VY.
                    cpuObject.V[b3] = (byte)(cpuObject.V[b3] | cpuObject.V[b2]);
                    break;
                case 12: //8XY2 définit VX à VX AND VY.
                    cpuObject.V[b3] = (byte)(cpuObject.V[b3] & cpuObject.V[b2]);
                    break;
                case 13:  //8XY3 définit VX à VX XOR VY. 
                    cpuObject.V[b3] = (byte)(cpuObject.V[b3] ^ cpuObject.V[b2]);
                    break;
                case 14: //8XY4 ajoute VY à VX. VF est mis à 1 quand il y a un dépassement de mémoire (carry), et à 0 quand il n'y en pas. 
                    if ((cpuObject.V[b3] + cpuObject.V[b2]) > 0xFF)
                    {
                        cpuObject.V[0xF] = 1; //V[15] 
                    }
                    else
                    {
                        cpuObject.V[0xF] = 0; //V[15] 
                    }
                    cpuObject.V[b3] += cpuObject.V[b2];
                    break;
                case 15:  //8XY5 VY est soustraite de VX. VF est mis à 0 quand il y a un emprunt, et à 1 quand il n'y a en pas. 
                    if (cpuObject.V[b3] < cpuObject.V[b2])
                    {
                        cpuObject.V[0xF] = 0;
                    }
                    else
                    {
                        cpuObject.V[0xF] = 1;
                    }
                    cpuObject.V[b3] -= cpuObject.V[b2];
                    break;
                case 16:  //8XY6 décale (shift) VX à droite de 1 bit. VF est fixé à la valeur du bit de poids faible de VX avant le décalage. 
                    cpuObject.V[0xF] = (byte)(cpuObject.V[b3] & (0x01));
                    cpuObject.V[b3] = (byte)(cpuObject.V[b3] >> 1);
                    break;
                case 17: //8XY7 VX = VY - VX. VF est mis à 0 quand il y a un emprunt et à 1 quand il n'y en a pas. 
                    if ((cpuObject.V[b2] < cpuObject.V[b3]))
                    {
                        cpuObject.V[0xF] = 0;//this.cpu.V[15]
                    }
                    else
                    {
                        cpuObject.V[0xF] = 1;
                    }
                    break;
                case 18: //8XYE décale (shift) VX à gauche de 1 bit. VF est fixé à la valeur du bit de poids fort de VX avant le décalage. 
                    cpuObject.V[0xF] = (byte)(cpuObject.V[b3] & (0x01));
                    cpuObject.V[b3] = (byte)(cpuObject.V[b3] << 1);
                    break;
                case 19:  //9XY0 saute l'instruction suivante si VX et VY ne sont pas égaux. 
                    if(b3 != b2)
                    {
                        cpuObject.pc += 2;
                    }
                    break;
                case 20: //ANNN affecte NNN à I. 
                    cpuObject.I = (UInt16)((b3 << 8) + (b2 << 4) + b1);
                    break;
                case 21:  //BNNN passe à l'adresse NNN + V0. 
                    cpuObject.pc = (UInt16)((b3 << 8) + (b2 << 4) + b1 + cpuObject.V[0]);
                    break;
                case 22:  //CXNN définit VX à un nombre aléatoire inférieur à NN.
                    Random rand = new Random();
                    cpuObject.V[b3] =  (byte)rand.Next(0,((b2 << 4) + b1 + 1));
                    break;
                case 23: //DXYN dessine un sprite aux coordonnées (VX, VY). 

                    DrawScreen(b1, b2, b3, cpuObject);
                    break;
                case 24: //EX9E saute l'instruction suivante si la clé stockée dans VX est pressée.
                    if (cpuObject.keys[cpuObject.V[b3]])
                    {
                        cpuObject.pc += 2;
                    }
                    break;
                case 25:  //EXA1 saute l'instruction suivante si la clé stockée dans VX n'est pas pressée. 
                    if (!cpuObject.keys[cpuObject.V[b3]])
                    {
                        cpuObject.pc += 2;
                    }
                    break;
                case 26: //FX07 définit VX à la valeur de la temporisation. 
                    cpuObject.V[b3] = (byte)cpuObject.counterGame;
                    break;
                case 27: //FX0A attend l'appui sur une touche et la stocke ensuite dans VX. 
                    WaitForInputs(b3, cpuObject);
                    break;
                case 28:  //FX15 définit la temporisation à VX. 
                    cpuObject.counterGame = cpuObject.V[b3];
                    break;
                case 29: //FX18 définit la minuterie sonore à VX. 
                    cpuObject.counterSound = cpuObject.V[b3];
                    break;
                case 30: //FX1E ajoute à VX I. VF est mis à 1 quand il y a overflow (I+VX>0xFFF), et à 0 si tel n'est pas le cas.
                    if ((cpuObject.I + cpuObject.V[b3]) > 0xFFF)
                    {
                        cpuObject.V[0xF] = 1;
                    }
                    else
                    {
                        cpuObject.V[0xF] = 0;
                    }
                    cpuObject.I += cpuObject.V[b3];
                    break;
                case 31: //FX29 définit I à l'emplacement du caractère stocké dans VX. Les caractères 0-F (en hexadécimal) sont représentés par une police 4x5. 
                    cpuObject.I = (UInt16)(cpuObject.V[b3] * 5);
                    break;
                case 32: //FX33 stocke dans la mémoire le code décimal représentant VX (dans I, I+1, I+2). 
                    cpuObject.memory[cpuObject.I] = (byte)((cpuObject.V[b3] - cpuObject.V[b3] % 100) / 100); //stocke les centaines 
                    cpuObject.memory[cpuObject.I + 1] = (byte)(((cpuObject.V[b3] - cpuObject.V[b3] % 10) / 10) % 10);//les dizaines 
                    cpuObject.memory[cpuObject.I + 2] = (byte)(cpuObject.V[b3] - cpuObject.memory[cpuObject.I] * 100 - cpuObject.memory[cpuObject.I + 1] * 10);//les unités
                    break;
                case 33: //FX55 stocke V0 à VX en mémoire à partir de l'adresse I.
                    for (byte i = 0; i <= b3; i++)
                    {
                        cpuObject.memory[cpuObject.I + i] = cpuObject.V[i];
                    }
                    break;
                case 34: //FX65 remplit V0 à VX avec les valeurs de la mémoire à partir de l'adresse I. 


                    for (byte i = 0; i <= b3; i++)
                    {
                        cpuObject.V[i] = cpuObject.memory[cpuObject.I + i];
                    }
                    break;
                default:
                    break;
            }
            //cpu.pc += 2; //on passe au prochain opcode
        }
    }
}
