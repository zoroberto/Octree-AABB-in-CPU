using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class BoundingboxPair
    {
        public List<BoundingBox> Bound { get; set; }
        public List<int> Indices { get; set; }

        public BoundingboxPair(List<BoundingBox> b, List<int> i)
        {
            Bound = b;
            Indices = i;
        }


        public BoundingboxPair(List<BoundingBox> b)
        {
            Bound = b;
        }

    }
}
