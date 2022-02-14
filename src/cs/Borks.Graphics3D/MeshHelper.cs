using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    /// <summary>
    /// Helper methods for working with <see cref="Mesh"/> objects.
    /// </summary>
    public class MeshHelper
    {
        public static void NormalizeWeights(MeshAttributeCollection<(int, float)> influences, int[] priority)
        {
            for (int i = 0; i < influences.ElementCount; i++)
            {
                var div = 1.0f;
                var sum = 0.0f;

                for (int w = 0; w < influences.Dimension; w++)
                {
                    var (index, influence) = influences[i, w];

                    if (priority.Contains(index))
                    {
                        div -= influence;
                    }
                    else
                    {
                        sum += influences[i, w].Item2;
                    }
                }

                var final = div / sum;

                for (int w = 0; w < influences.Dimension; w++)
                {
                    var (index, influence) = influences[i, w];

                    if (!priority.Contains(index))
                    {
                        influences[i, w] = (index, influence * final);
                    }
                }
            }
        }
    }
}
