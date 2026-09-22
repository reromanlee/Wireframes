namespace reromanlee.Wireframes.Common
{

    internal static class ArrayExtensions
    {

        public static T[] Append<T>(this T[] array, params T[] elements)
        {
            T[] output = new T[array.Length + elements.Length];
            array.CopyTo(output, 0);
            elements.CopyTo(output, array.Length);
            return output;
        }

    }

}