using System.Collections.Generic;
using System.Linq;

namespace ImageToTiled
{
    public class ByteArrayComparer : EqualityComparer<byte[]>
    {
        public override bool Equals(byte[] first, byte[] second)
        {
            if (first == null || second == null)
                return first == second;
            else if (ReferenceEquals(first, second))
                return true;
            else if (first.Length != second.Length)
                return false;

            // Linq extension method is based on IEnumerable, must evaluate every item.
            return first.SequenceEqual(second);
        }
        public override int GetHashCode(byte[] obj) => obj.Length;
    }
}
