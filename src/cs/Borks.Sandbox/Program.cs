using Borks.Graphics3D;
using Borks.Graphics3D.SEAnim;
using Borks.Graphics3D.SEModel;
using Borks.Graphics3D.SMD;
using Borks.Graphics3D.CoDXAsset.Tokens;
using System.Security.Cryptography;
using System.Text;
using Borks.Graphics3D.Translator;
using System.Diagnostics;

namespace Borks.Sandbox
{
    class Program
    {
        public static void Main(string[] args)
        {
            var translatorFactory = new Graphics3DTranslatorFactory().WithDefaultTranslators();


            //translatorFactory.RegisterTranslator(new SEAnimTranslator());
            //translatorFactory.RegisterTranslator(new SEModelTranslator());
            //translatorFactory.RegisterTranslator(new CoDXAssetTranslator());


            //var md5 = IncrementalHash.CreateHash(HashAlgorithmName.MD5);

            //var smdTranslator     = new SMDTranslator();
            //var seanimTranslator  = new SEAnimTranslator();
            //var semodelTranslator = new SEModelTranslator();

            //var io = new Graphics3DTranslatorIO();

            //seanimTranslator.Read(@"C:\Users\Admin\Downloads\MMWX\exported_files\xanims\vm_sn_delta_reload.seanim", io);
            //seanimTranslator.Write(@"C:\Users\Admin\Downloads\MMWX\exported_files\xanims\vm_sn_delta_reload2.seanim", io);

            //var anim = io.Animations[0];

            //foreach (var action in anim.Actions)
            //{
            //    Console.WriteLine(action.Name);
            //}

            //var reader = new BinaryTokenReader(@"D:\SteamLibrary\steamapps\common\Call of Duty Black Ops III\model_export\skye_ports\t9_lc10\vm_t9_lc10_nt.xmodel_bin");

            var mdl = translatorFactory.Load<Model>(@"F:\_scobalula\ai\re3_zombies\ai_zom_re3zombie_01_fb.xmodel_bin");

            for (int i = 0; i < 8; i++)
            {
                var watch = Stopwatch.StartNew();
                Debugger.Break();
                translatorFactory.Save(@"D:\SteamLibrary\steamapps\common\Call of Duty Black Ops III\model_export\stock_LOD01.xmodel_bin", mdl);
                Debugger.Break();
                Console.WriteLine(watch.ElapsedMilliseconds / 1000.0f);
            }

            //var mdl = translatorFactory.Load<Model>(@"C:\Users\Admin\Downloads\stock_LOD0.smd");
            //var anim = translatorFactory.Load<Animation>(@"C:\Users\Admin\Downloads\inspect.smd");
            //var marv = AnimationHelper.ConvertTransformSpace(anim, mdl.Skeleton, TransformSpace.World);


            //translatorFactory.Save(@"D:\SteamLibrary\steamapps\common\Call of Duty Black Ops III\model_export\stock_LOD0.xmodel_bin", mdl);
            // translatorFactory.Save(@"C:\Users\Admin\Downloads\reload_empty.xanim_bin", marv);

            //var anim = translatorFactory.Load<Animation>(@"D:\SteamLibrary\steamapps\common\Call of Duty Black Ops III\xanim_export\t7_viewmodels\ap9\vm_ap9_reload_empty.XANIM_BIN");
            //GC.Collect();

            //// translatorFactory.Save(@"D:\SteamLibrary\steamapps\common\Call of Duty Black Ops III\model_export\_scobalula\character\iw7_zis_valleygirl\char_iw7_zis_valleygirl_viewhands.semodel", mdl);
            //translatorFactory.Save("test.xanim_bin", anim);


            //var test = translatorFactory.Load<Animation>("test.xanim_bin");
            //foreach (var token in reader.EnumerateTokens())
            //{
            //    Console.WriteLine(token.Token.Name);
            //}



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