namespace Utils
{
    public static class StringManipulation
    {
        public static string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input; // Return as is if null or empty

            return char.ToUpper(input[0]) + input.Substring(1);
        }
    }
}