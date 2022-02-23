using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LicentaApp
{
    public sealed class SHAClass
    {
        private SHAClass() { }

        private static SHAClass instance = null;

        public static SHAClass Instance
        {
            get {
                if(instance == null)
                {
                    instance = new SHAClass();
                }
                return instance;
            }
        }

        public byte[] getSHA256Hash(string file)
        {
            string text = System.IO.File.ReadAllText(file);

            var data = Encoding.UTF8.GetBytes(text);
            SHA256 shaM = new SHA256Managed();
            var result = shaM.ComputeHash(data);

            return result;
        }

        public byte[] getSHA384Hash(string file)
        {
            string text = System.IO.File.ReadAllText(file);

            var data = Encoding.UTF8.GetBytes(text);
            SHA384 shaM = new SHA384Managed();
            var result = shaM.ComputeHash(data);

            return result;
        }

        public byte[] getSHA512Hash(string file)
        {
            string text = System.IO.File.ReadAllText(file);

            var data = Encoding.UTF8.GetBytes(text);
            SHA512 shaM = new SHA512Managed();
            var result = shaM.ComputeHash(data);

            return result;
        }


       
    }
}
