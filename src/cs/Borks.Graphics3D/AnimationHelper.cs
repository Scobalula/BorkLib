using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    public class AnimationHelper
    {
        /// <summary>
        /// Gets the frame pair indices for the given animation frame list
        /// </summary>
        public static (int, int) GetFramePairIndex<T>(List<AnimationFrame<T>>? list, float time, float startTime, float minTime = float.MinValue, float maxTime = float.MaxValue, int cursor = 0)
        {
            // Early quit for lists that we can't "pair"
            if (list == null)
                return (-1, -1);
            if (list.Count == 0)
                return (-1, -1);
            if (list.Count == 1)
                return (0, 0);
            if (time > (startTime + list.Last().Time))
                return (list.Count - 1, list.Count - 1);
            if (time < (startTime + list.First().Time))
                return (0, 0);

            int i;

            // First pass from cursor
            for (i = 0; i < list.Count - 1; i++)
            {
                if (time < (startTime + list[i + 1].Time))
                    return (i, i + 1);
            }

            // Second pass up to cursor
            for (i = 0; i < list.Count - 1 && i < cursor; i++)
            {
                if (time < (startTime + list[i + 1].Time))
                    return (i, i + 1);
            }

            return (list.Count - 1, list.Count - 1);
        }
    }
}
