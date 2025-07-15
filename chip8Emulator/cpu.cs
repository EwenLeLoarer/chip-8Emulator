using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chip8Emulator
{
    public class CPU
    {
        private const int MEMORYSIZE = 4096;
        private const int STARTADRESS = 512;


        public byte[] Memory = new byte[MEMORYSIZE];

        public byte[] V = new byte[16]; // register
        public UInt16 I = 0x200; // save a memory address or draw address
        private Jump _jp = new Jump();
        public UInt16[] Jump = new UInt16[16]; // to handle jump in memory, 16 at max
        public byte NbrJump; // save the number of jump done to not go over 16
        public byte CounterGame; // counter for the synchronisation
        public byte CounterSound; // counter for sound
        public UInt16 Pc; // to go through the array "Memory"
        public bool[] Keys = new bool[16];

        public bool IsWaitingForKeys = false;

        public void CpuInitialize()
        {
            for(UInt16 i = 0; i < MEMORYSIZE; i++)
            {
                Memory[i] = 0;
            }

            for (UInt16 i = 0; i < 16; i++)
            {
                V[i] = 0;
                Jump[i] = 0;
            }

            Pc = STARTADRESS;
            NbrJump = 0;
            CounterGame = 0;
            CounterSound = 0;
            I = 0;
            this.InitializeJump();
        }

        public void Counter()
        {
            if (CounterGame > 0)
                CounterGame--;

            if (CounterSound > 0)
                CounterSound--;
        }

        public UInt16 GetOpCode()
        {
            return (UInt16)((Memory[Pc] << 8) | Memory[Pc + 1]);
        }

        public void InitializeJump()
        {
            _jp.Masque[0] = 0x0000; _jp.Id[0] = 0x0FFF;          /* 0NNN */
            _jp.Masque[1] = 0xFFFF; _jp.Id[1] = 0x00E0;          /* 00E0 */
            _jp.Masque[2] = 0xFFFF; _jp.Id[2] = 0x00EE;          /* 00EE */
            _jp.Masque[3] = 0xF000; _jp.Id[3] = 0x1000;          /* 1NNN */
            _jp.Masque[4] = 0xF000; _jp.Id[4] = 0x2000;          /* 2NNN */
            _jp.Masque[5] = 0xF000; _jp.Id[5] = 0x3000;          /* 3XNN */
            _jp.Masque[6] = 0xF000; _jp.Id[6] = 0x4000;          /* 4XNN */
            _jp.Masque[7] = 0xF00F; _jp.Id[7] = 0x5000;          /* 5XY0 */
            _jp.Masque[8] = 0xF000; _jp.Id[8] = 0x6000;          /* 6XNN */
            _jp.Masque[9] = 0xF000; _jp.Id[9] = 0x7000;          /* 7XNN */
            _jp.Masque[10] = 0xF00F; _jp.Id[10] = 0x8000;          /* 8XY0 */
            _jp.Masque[11] = 0xF00F; _jp.Id[11] = 0x8001;          /* 8XY1 */
            _jp.Masque[12] = 0xF00F; _jp.Id[12] = 0x8002;          /* 8XY2 */
            _jp.Masque[13] = 0xF00F; _jp.Id[13] = 0x8003;          /* 8XY3 */
            _jp.Masque[14] = 0xF00F; _jp.Id[14] = 0x8004;          /* 8XY4 */
            _jp.Masque[15] = 0xF00F; _jp.Id[15] = 0x8005;          /* 8XY5 */
            _jp.Masque[16] = 0xF00F; _jp.Id[16] = 0x8006;          /* 8XY6 */
            _jp.Masque[17] = 0xF00F; _jp.Id[17] = 0x8007;          /* 8XY7 */
            _jp.Masque[18] = 0xF00F; _jp.Id[18] = 0x800E;          /* 8XYE */
            _jp.Masque[19] = 0xF00F; _jp.Id[19] = 0x9000;          /* 9XY0 */
            _jp.Masque[20] = 0xF000; _jp.Id[20] = 0xA000;          /* ANNN */
            _jp.Masque[21] = 0xF000; _jp.Id[21] = 0xB000;          /* BNNN */
            _jp.Masque[22] = 0xF000; _jp.Id[22] = 0xC000;          /* CXNN */
            _jp.Masque[23] = 0xF000; _jp.Id[23] = 0xD000;          /* DXYN */
            _jp.Masque[24] = 0xF0FF; _jp.Id[24] = 0xE09E;          /* EX9E */
            _jp.Masque[25] = 0xF0FF; _jp.Id[25] = 0xE0A1;          /* EXA1 */
            _jp.Masque[26] = 0xF0FF; _jp.Id[26] = 0xF007;          /* FX07 */
            _jp.Masque[27] = 0xF0FF; _jp.Id[27] = 0xF00A;          /* FX0A */
            _jp.Masque[28] = 0xF0FF; _jp.Id[28] = 0xF015;          /* FX15 */
            _jp.Masque[29] = 0xF0FF; _jp.Id[29] = 0xF018;          /* FX18 */
            _jp.Masque[30] = 0xF0FF; _jp.Id[30] = 0xF01E;          /* FX1E */
            _jp.Masque[31] = 0xF0FF; _jp.Id[31] = 0xF029;          /* FX29 */
            _jp.Masque[32] = 0xF0FF; _jp.Id[32] = 0xF033;          /* FX33 */
            _jp.Masque[33] = 0xF0FF; _jp.Id[33] = 0xF055;          /* FX55 */
            _jp.Masque[34] = 0xF0FF; _jp.Id[34] = 0xF065;          /* FX65 */
        }

        public byte GetAction(UInt16 opcode)
        {
            byte action;
            UInt16 resultat = 0;

            for (action = 0; action < chip8Emulator.Jump.NbrOPcode ; action++)
            {
                resultat = (UInt16)(_jp.Masque[action] & opcode);  /* we apply a bit mask */

                if (resultat == _jp.Id[action]) /* if we found the correct action we stop */
                    break; 
            }
            return action;  //we return the index of the action
        }
    }
}
