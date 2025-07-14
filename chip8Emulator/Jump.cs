using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chip8Emulator
{
    public class Jump
    {
        public const byte nbrOPcode = 35;
        public UInt16[] masque = new UInt16[nbrOPcode];
        public UInt16[] id = new UInt16[nbrOPcode];
    }
}
