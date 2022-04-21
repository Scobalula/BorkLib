using Borks.Graphics3D;
using Borks.Graphics3D.SEModel;
using Borks.Graphics3D.SMD;

namespace Borks.Sandbox
{
    class Program
    {
        public static void Main(string[] args)
        {
            var smdTranslator     = new SMDTranslator();
            var seanimTranslator  = new SEAnimTranslator();
            var semodelTranslator = new SEModelTranslator();

            var output = new Graphics3DTranslatorIO();

            smdTranslator.Read(@"C:\Users\Admin\Documents\nigerion\anims\vm_ar_g3a3_reload_empty.smd", output);
            seanimTranslator.Write(@"C:\Users\Admin\Documents\nigerion\anims\vm_ar_g3a3_reload_empty.seanim", output);

        }
    }
}