using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class Intersection
    {
        public static bool AABB(BoundingBox box1, BoundingBox box2)
        {

            return
                box1.Min.x <= box2.Max.x &&
                box1.Max.x >= box2.Min.x &&
                box1.Min.y <= box2.Max.y &&
                box1.Max.y >= box2.Min.y &&
                box1.Min.z <= box2.Max.z &&
                box1.Max.z >= box2.Min.z;
        }

    }
}
