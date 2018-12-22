using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

class PointComparer : IEqualityComparer<int2> {
    public bool Equals(int2 a, int2 b) {
        return a.x == b.x && a.y == b.y;
    }

    public int GetHashCode(int2 obj) {
        // Perfect hash for practical bitmaps, their width/height is never >= 65536
        return Util.GetHashCode(obj.x, obj.y);
    }
}
