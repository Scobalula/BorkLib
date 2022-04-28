using Borks.Graphics3D;
using Borks.Graphics3D.SEModel;
using Borks.Graphics3D.SMD;

namespace Borks.Sandbox
{
    class Program
    {
        public static void Main(string[] args)
        {
            var translatorFactory = new Graphics3DTranslatorFactory();

            translatorFactory.RegisterTranslator(new SMDTranslator());
            translatorFactory.RegisterTranslator(new SEAnimTranslator());
            translatorFactory.RegisterTranslator(new SEModelTranslator());

            var smdTranslator     = new SMDTranslator();
            var seanimTranslator  = new SEAnimTranslator();
            var semodelTranslator = new SEModelTranslator();

            var io = new Graphics3DTranslatorIO();

            seanimTranslator.Read(@"C:\Users\Admin\Downloads\MMWX\exported_files\xanims\vm_sn_delta_reload.seanim", io);
            seanimTranslator.Write(@"C:\Users\Admin\Downloads\MMWX\exported_files\xanims\vm_sn_delta_reload2.seanim", io);

            var anim = io.Animations[0];

            foreach (var action in anim.Actions)
            {
                Console.WriteLine(action.Name);
            }

            //Printer.WriteLine("INIT", "------------------------");
            //Printer.WriteLine("INIT", "SMD to SEModel/SEAnim");
            //Printer.WriteLine("INIT", "Butterbloc Edition");
            //Printer.WriteLine("INIT", "------------------------");

            //var io = new Graphics3DTranslatorIO();

            //foreach (var file in args)
            //{
            //    var ext = Path.GetExtension(file);

            //    if(!string.IsNullOrEmpty(ext) && ext.Equals(".smd", StringComparison.CurrentCultureIgnoreCase))
            //    {
            //        Printer.WriteLine("MARV", $"Converting: {Path.GetFileName(file)}");

            //        smdTranslator.Read(file, io);

            //        // Check did we get an animation from this
            //        if(io.Animations.Count > 0)
            //        {
            //            seanimTranslator.Write(Path.ChangeExtension(file, ".seanim"), io);
            //        }
            //        else
            //        {
            //            semodelTranslator.Write(Path.ChangeExtension(file, ".semodel"), io);
            //        }

            //        Printer.WriteLine("MARV", $"Converted: {Path.GetFileName(file)}");
            //    }

            //    io.Models.Clear();
            //    io.Skeletons.Clear();
            //    io.Animations.Clear();
            //}

            //Printer.WriteLine("DONE", "Execution complete.");

            //Console.ReadLine();
        }
    }
}