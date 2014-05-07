using System;
using System.Linq;

namespace Orleans.Bus
{
    public static class Identity
    {
        public static string Id(this IGrain grain)
        {
            string id;
            grain.GetPrimaryKeyLong(out id);
            return id;
        }
    }
}
