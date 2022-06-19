using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Borks
{
    public class NativeHelpers
    {
        public static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            var path = $"Native\\{RuntimeInformation.ProcessArchitecture}{libraryName}";

            try
            {
                return NativeLibrary.Load(path, assembly, searchPath);
            }
            // If we didn't find the exception, we don't want a fatal 
            // error here, we want to fall back to default resolver
            catch (DllNotFoundException)
            {
                return IntPtr.Zero;
            }
            // Everything else, we might have something more serious
            catch
            {
                throw;
            }
        }
    }
}
