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


        public byte[] memory = new byte[MEMORYSIZE];

        public byte[] V = new byte[16]; // register
        public UInt16 I; // save a memory adress or draw adress
        public Jump jp = new Jump();
        public UInt16[] jump = new UInt16[16]; // to handle jump in memory, 16 at max
        public byte nbrJump; // save the number of jump done to not go over 16
        public byte counterGame; // counter for the syncronisation
        public byte counterSound; // counter for sound
        public UInt16 pc; // to go throught the array "memory"

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

        public void decompter()
        {
            if (counterGame > 0)
                counterGame--;

            if (counterSound > 0)
                counterSound--;
        }

        public UInt16 getOpCode()
        {
            return (UInt16)((memory[pc] << 8) | memory[pc + 1]);
        }

        public void InitializeJump()
        {
            jp.masque[0] = 0x0000; jp.id[0] = 0x0FFF;          /* 0NNN */
            jp.masque[1] = 0xFFFF; jp.id[1] = 0x00E0;          /* 00E0 */
            jp.masque[2] = 0xFFFF; jp.id[2] = 0x00EE;          /* 00EE */
            jp.masque[3] = 0xF000; jp.id[3] = 0x1000;          /* 1NNN */
            jp.masque[4] = 0xF000; jp.id[4] = 0x2000;          /* 2NNN */
            jp.masque[5] = 0xF000; jp.id[5] = 0x3000;          /* 3XNN */
            jp.masque[6] = 0xF000; jp.id[6] = 0x4000;          /* 4XNN */
            jp.masque[7] = 0xF00F; jp.id[7] = 0x5000;          /* 5XY0 */
            jp.masque[8] = 0xF000; jp.id[8] = 0x6000;          /* 6XNN */
            jp.masque[9] = 0xF000; jp.id[9] = 0x7000;          /* 7XNN */
            jp.masque[10] = 0xF00F; jp.id[10] = 0x8000;          /* 8XY0 */
            jp.masque[11] = 0xF00F; jp.id[11] = 0x8001;          /* 8XY1 */
            jp.masque[12] = 0xF00F; jp.id[12] = 0x8002;          /* 8XY2 */
            jp.masque[13] = 0xF00F; jp.id[13] = 0x8003;          /* BXY3 */
            jp.masque[14] = 0xF00F; jp.id[14] = 0x8004;          /* 8XY4 */
            jp.masque[15] = 0xF00F; jp.id[15] = 0x8005;          /* 8XY5 */
            jp.masque[16] = 0xF00F; jp.id[16] = 0x8006;          /* 8XY6 */
            jp.masque[17] = 0xF00F; jp.id[17] = 0x8007;          /* 8XY7 */
            jp.masque[18] = 0xF00F; jp.id[18] = 0x800E;          /* 8XYE */
            jp.masque[19] = 0xF00F; jp.id[19] = 0x9000;          /* 9XY0 */
            jp.masque[20] = 0xF000; jp.id[20] = 0xA000;          /* ANNN */
            jp.masque[21] = 0xF000; jp.id[21] = 0xB000;          /* BNNN */
            jp.masque[22] = 0xF000; jp.id[22] = 0xC000;          /* CXNN */
            jp.masque[23] = 0xF000; jp.id[23] = 0xD000;          /* DXYN */
            jp.masque[24] = 0xF0FF; jp.id[24] = 0xE09E;          /* EX9E */
            jp.masque[25] = 0xF0FF; jp.id[25] = 0xE0A1;          /* EXA1 */
            jp.masque[26] = 0xF0FF; jp.id[26] = 0xF007;          /* FX07 */
            jp.masque[27] = 0xF0FF; jp.id[27] = 0xF00A;          /* FX0A */
            jp.masque[28] = 0xF0FF; jp.id[28] = 0xF015;          /* FX15 */
            jp.masque[29] = 0xF0FF; jp.id[29] = 0xF018;          /* FX18 */
            jp.masque[30] = 0xF0FF; jp.id[30] = 0xF01E;          /* FX1E */
            jp.masque[31] = 0xF0FF; jp.id[31] = 0xF029;          /* FX29 */
            jp.masque[32] = 0xF0FF; jp.id[32] = 0xF033;          /* FX33 */
            jp.masque[33] = 0xF0FF; jp.id[33] = 0xF055;          /* FX55 */
            jp.masque[34] = 0xF0FF; jp.id[34] = 0xF065;          /* FX65 */
        }

        public byte getAction(UInt16 opcode)
        {
            byte action;
            UInt16 resultat;

            for (action = 0; action < Jump.nbrOPcode ; action++)
            {
                resultat = (UInt16)(jp.masque[action] & opcode);  /* On récupère les bits concernés par le test, l'identifiant de l'opcode */

                if (resultat == jp.id[action]) /* On a trouvé l'action à effectuer */
                    break; /* Plus la peine de continuer la boucle car la condition n'est vraie qu'une seule fois*/
            }
            Console.WriteLine(action);
            return action;  //on renvoie l'indice de l'action à effectuer 
        }
    }
}
