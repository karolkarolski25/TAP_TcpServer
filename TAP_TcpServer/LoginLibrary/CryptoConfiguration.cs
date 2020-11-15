using System;
using System.Collections.Generic;
using System.Text;

namespace LoginLibrary
{
    public class CryptoConfiguration
    {
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }
    }
}
