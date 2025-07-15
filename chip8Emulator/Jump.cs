using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chip8Emulator
{
    public class Jump
    {
        public const byte NbrOPcode = 35;
        public UInt16[] Masque = new UInt16[NbrOPcode];
        public UInt16[] Id = new UInt16[NbrOPcode];
    }
}
