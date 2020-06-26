namespace Assets.Scripts
{
    public static class StringExtensions
    {
        /// From https://stackoverflow.com/questions/8784517/counting-number-of-words-in-c-sharp
        /// <summary>
        /// Counts words without the overhead of Split().
        /// </summary>
        public static int CountWords(this string @this)
        {
            int wordCount = 0, index = 0;

            // skip whitespace until first word
            while (index < @this.Length && char.IsWhiteSpace(@this[index]))
                index++;

            while (index < @this.Length)
            {
                // check if current char is part of a word
                while (index < @this.Length && !char.IsWhiteSpace(@this[index]))
                    index++;

                wordCount++;

                // skip whitespace until next word
                while (index < @this.Length && char.IsWhiteSpace(@this[index]))
                    index++;
            }

            return wordCount;
        }
    }
}
