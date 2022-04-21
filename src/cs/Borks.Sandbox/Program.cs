using Borks.Graphics3D;
using Borks.Graphics3D.SEModel;
using Borks.Graphics3D.SMD;

namespace Borks.Sandbox
{
    class Program
    {
        public static void Main(string[] args)
        {
            var marven = new SMDTranslator();
            var dave = new SEAnim();
            var output = new Graphics3DTranslatorIO();

            marven.Read(@"D:\output\tank_reference.smd", output);

            dave.Write(@"D:\output\tank_reference.semodel", output);

        }
    }
}