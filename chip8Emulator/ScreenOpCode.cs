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
        private const int l = 64; //number of pixel on x axis
        private const int L = 32; // number of pixel y axis
        private const int DimPixel = 16; // size of a pixel
        private const int Width = l * DimPixel; // x size of the screen
        private const int Height = L * DimPixel;// y size of the screen
        public bool NeedToBeUpdated = true;

        public byte[,] Screen = new byte[Width, Height];
        
        public void InitializePixel()
        {
            byte x = 0, y = 0;

            for(x = 0; x < l; x++)
            {
                for(y = 0; y < L; y++)
                {
                    if (x % (y + 1) == 0)
                        this.Screen[x, y] = 0;
                    else
                        this.Screen[x, y] = 1;
                }
            }
        }
        public void ResetScreen()
        {
            byte x = 0, y = 0;
            for(x = 0; x < l; x++)
            {
                for(y = 0; y < L;y++)
                {
                    Screen[x, y] = 0;
                }
            }

            this.NeedToBeUpdated = true;
        }

        public void UpdateScreen()
        {
            NeedToBeUpdated = true;
        }

        private void DrawScreen(byte b1, byte b2, byte b3, CPU cpu)
        {
            byte x = 0, y = 0, k = 0, codage = 0, j = 0, decalage = 0;
            cpu.V[0xF] = 0;
            for (k = 0; k < b1; k++)
            {
                codage = cpu.Memory[cpu.I + k]; //we get the codage of the line to draw

                y = (byte)((cpu.V[b2] + k) % L); //we calculate the y-axis of the line to not go over L

                for (j = 0, decalage = 7; j < 8; j++, decalage--)
                {
                    x = (byte)((cpu.V[b3] + j) % l); //we calculate the x-axis of the line to not go over l
                    if (((codage)& (0x1<<decalage))!=0) //we get the remaining bit
                    {
                        if (Screen[x, y] == 1)
                        {
                            Screen[x, y] = 0;
                            cpu.V[0xF] = 1;
                        }
                        else
                        {
                            Screen[x, y] = 1;
                        }
                    }
                }

                NeedToBeUpdated = true;
            }
        }

        private void WaitForInputs(byte b3, CPU  cpu)
        {


            if (!cpu.IsWaitingForKeys)
            {
                cpu.IsWaitingForKeys = true;

            }
            
            
        }



        public void InterpretOpCode(UInt16 opcode, CPU cpuObject)
        {
            byte b4;
            b4 = cpuObject.GetAction(opcode);
            byte b3, b2, b1;

            b3 = (byte)((opcode & (0x0F00)) >> 8);  //we take the 4 bits, b3 is x
            b2 = (byte)((opcode & (0x00F0)) >> 4);  //same, b2 is Y 
            b1 = (byte)((opcode & (0x000F)));     //we take the 4 bits remaining

            //to have NNN we need to do (b3<<8) + (b2<<4) + (b1) 


            switch (b4)
            {
                case 0: //unimplement
                    break;
                case 1: //00E0 : reset the screen
                    this.ResetScreen();
                    break;
                case 2: //00EE : go back from a jump
                     if(cpuObject.NbrJump>0)
                    {
                        cpuObject.NbrJump--;
                        cpuObject.Pc = cpuObject.Jump[cpuObject.NbrJump];
                    }
                    break;
                case 3: //1NNN : do a jump to the address 1NNN 

                    cpuObject.Pc = (UInt16)((b3 << 8) + (b2 << 4) + b1); 
                    cpuObject.Pc -= 2;
                    break;
                case 4: //2NNN : Execute subroutine starting at address NNN
                    cpuObject.Jump[cpuObject.NbrJump] = cpuObject.Pc;
                    
                    if(cpuObject.NbrJump<15)
                    {
                        cpuObject.NbrJump++;
                    }
                    cpuObject.Pc = (UInt16)((b3 << 8) + (b2 << 4) + b1)   ;
                    cpuObject.Pc -= 2; 

                    break;
                case 5: //3NNN: Skip the following instruction if the value of register VX equals NN
                    if (cpuObject.V[b3] == ((b2<<4) + b1))
                    {
                        cpuObject.Pc += 2;
                    }
                    break;
                case 6: //4XNN : Skip the following instruction if the value of register VX is not equal to NN
                    if (cpuObject.V[b3] != ((b2 << 4) + b1))
                    {
                        cpuObject.Pc += 2;
                    }
                    break;
                case 7:  //5XY0 : Skip the following instruction if the value of register VX is equal to the value of register VY
                    if (cpuObject.V[b3] == cpuObject.V[b2])
                    {
                        cpuObject.Pc += 2;
                    }
                    break;
                case 8:  //6XNN : Store number NN in register VX
                    cpuObject.V[b3] = (byte)((b2 << 4) + b1);
                    break;
                case 9: //7XNN : Add the value NN to register VX 
                    cpuObject.V[b3] += (byte)((b2 << 4) + b1);
                    break;
                case 10: //8XY0 : Store the value of register VY in register VX
                    cpuObject.V[b3] = cpuObject.V[b2];
                    break;
                case 11: //8XY1 : Set VX to VX OR VY
                    cpuObject.V[b3] = (byte)(cpuObject.V[b3] | cpuObject.V[b2]);
                    break;
                case 12: //8XY2 : Set VX to VX AND VY
                    cpuObject.V[b3] = (byte)(cpuObject.V[b3] & cpuObject.V[b2]);
                    break;
                case 13:  //8XY3 : Set VX to VX XOR VY
                    cpuObject.V[b3] = (byte)(cpuObject.V[b3] ^ cpuObject.V[b2]);
                    break;
                case 14: //8XY4 : Add the value of register VY to register VX
                         //Set VF to 01 if a carry occurs
                         //Set VF to 00 if a carry does not occur
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
                case 15:  //8XY5 : Subtract the value of register VY from register VX
                          //Set VF to 00 if a borrow occurs
                          //Set VF to 01 if a borrow does not occur
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
                case 16:  //8XY6: Store the value of register VY shifted right one bit in register VX
                          //Set register VF to the least significant bit prior to the shift
                          //VY is unchanged
                    cpuObject.V[0xF] = (byte)(cpuObject.V[b3] & (0x01));
                    cpuObject.V[b3] = (byte)(cpuObject.V[b3] >> 1);
                    break;
                case 17: //8XY7 : Set register VX to the value of VY minus VX
                         //Set VF to 00 if a borrow occurs
                         //Set VF to 01 if a borrow does not occur
                    if ((cpuObject.V[b2] < cpuObject.V[b3]))
                    {
                        cpuObject.V[0xF] = 0;
                    }
                    else
                    {
                        cpuObject.V[0xF] = 1;
                    }
                    cpuObject.V[b3] = (byte)(cpuObject.V[b2] - cpuObject.V[b3]);
                    break;
                case 18: //8XYE : Store the value of register VY shifted left one bit in register VX¹
                         //Set register VF to the most significant bit prior to the shift
                         //VY is unchanged
                    cpuObject.V[0xF] = (byte)(cpuObject.V[b3] & (0x01));
                    cpuObject.V[b3] = (byte)(cpuObject.V[b3] << 1);
                    break;
                case 19:  //9XY0 : Skip the following instruction if the value of register VX is not equal to the value of register VY
                    if (cpuObject.V[b3] != cpuObject.V[b2])
                    {
                        cpuObject.Pc += 2;
                    }
                    break;
                case 20: //ANNN : Store memory address NNN in register I
                    cpuObject.I = (UInt16)((b3 << 8) + (b2 << 4) + b1);
                    break;
                case 21:  //BNNN : Jump to address NNN + V0
                    cpuObject.Pc = (UInt16)((b3 << 8) + (b2 << 4) + b1 + cpuObject.V[0]);
                    break;
                case 22:  //CXNN : Set VX to a random number with a mask of NN
                    Random rand = new Random();
                    cpuObject.V[b3] =  (byte)rand.Next(0,((b2 << 4) + b1 + 1));
                    break;
                case 23: //DXYN : Draw a sprite at position VX, VY with N bytes of sprite data starting at the address stored in I
                         //Set VF to 01 if any set pixels are changed to unset, and 00 otherwise
                    DrawScreen(b1, b2, b3, cpuObject);
                    break;
                case 24: //EX9E : Skip the following instruction if the key corresponding to the hex value currently stored in register VX is pressed
                    if (cpuObject.Keys[cpuObject.V[b3]])
                    {
                        cpuObject.Pc += 2;
                    }
                    break;
                case 25:  //EXA1 : Skip the following instruction if the key corresponding to the hex value currently stored in register VX is not pressed

                    if (!cpuObject.Keys[cpuObject.V[b3]])
                    {
                        cpuObject.Pc += 2;
                    }
                    break;
                case 26: //FX07 : Store the current value of the delay timer in register VX
                    cpuObject.V[b3] = (byte)cpuObject.CounterGame;
                    break;
                case 27: //FX0A : Wait for a keypress and store the result in register VX
                    WaitForInputs(b3, cpuObject);
                    break;
                case 28:  //FX15 : Set the delay timer to the value of register VX
                    cpuObject.CounterGame = cpuObject.V[b3];
                    break;
                case 29: //FX18 : Set the sound timer to the value of register VX
                    cpuObject.CounterSound = cpuObject.V[b3];
                    break;
                case 30: //FX1E : Add the value stored in register VX to register I
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
                case 31: //FX29 : Set I to the memory address of the sprite data corresponding to the hexadecimal digit stored in register VX
                    cpuObject.I = (UInt16)(cpuObject.V[b3] * 5);
                    break;
                case 32: //FX33 : Store the binary-coded decimal equivalent of the value stored in register VX at addresses I, I + 1, and I + 2
                    cpuObject.Memory[cpuObject.I] = (byte)((cpuObject.V[b3] - cpuObject.V[b3] % 100) / 100); //the 100
                    cpuObject.Memory[cpuObject.I + 1] = (byte)(((cpuObject.V[b3] - cpuObject.V[b3] % 10) / 10) % 10);//the 10 
                    cpuObject.Memory[cpuObject.I + 2] = (byte)(cpuObject.V[b3] - cpuObject.Memory[cpuObject.I] * 100 - cpuObject.Memory[cpuObject.I + 1] * 10);//the 1
                    break;
                case 33: //FX55 : Store the values of registers V0 to VX inclusive in memory starting at address I
                         //I is set to I + X + 1 after operation
                    for (byte i = 0; i <= b3; i++)
                    {
                        cpuObject.Memory[cpuObject.I + i] = cpuObject.V[i];
                    }
                    break;
                case 34: //FX65 : Fill registers V0 to VX inclusive with the values stored in memory starting at address I
                         //I is set to I + X + 1 after operation
                    for (byte i = 0; i <= b3; i++)
                    {
                        cpuObject.V[i] = cpuObject.Memory[cpuObject.I + i];
                    }
                    break;
                default:
                    break;
            }
            if(!cpuObject.IsWaitingForKeys)
                cpuObject.Pc += 2; //we go to the next opcode
        }
    }
}
