using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chip8Emulator
{
    public class cpu
    {
        public const int MEMORYSIZE = 4096;
        public const int STARTADRESS = 512;

        byte[] memory = new byte[MEMORYSIZE];
        byte[] V = new byte[16]; // register
        UInt16 I; // save a memory adress or draw adress
        UInt16[] jump = new UInt16[16]; // to handle jump in memory, 16 at max
        byte nbrJump; // save the number of jump done to not go over 16
        byte counterGame; // counter for the syncronisation
        byte counterSound; // counter for sound
        UInt16 pc; // to go throught the array "memory"

        public void CPUInitialize()
        {
            UInt16 i = 0;

            for(i = 0; i < MEMORYSIZE; i++)
            {
                memory[i] = 0;
            }

            for (i = 0; i < 16; i++)
            {
                V[i] = 0;
                jump[i] = 0;
            }

            pc = STARTADRESS;
            nbrJump = 0;
            counterGame = 0;
            counterSound = 0;
            I = 0;
        }

        void decompter()
        {
            if (counterGame > 0)
                counterGame--;

            if (counterSound > 0)
                counterSound--;
        }

    }
}
