namespace Common.Extensions
{
    public static class ListExtension
    {
        public static bool ContainsAllItems<T>(this IEnumerable<T> a, IEnumerable<T> b)
        {
            return !b.Except(a).Any();
        }

        public static List<T> GetRandomItems<T>(this List<T> list, int n = 0)
        {
            if (n == 0)
            {
                n = list.Count;
            }
            Random rng = new Random();
            return list.OrderBy(x => rng.Next()).Take(n).ToList();
        }
    }
}
