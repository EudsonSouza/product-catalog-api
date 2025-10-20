namespace ProductCatalog.Data.Extensions;

public static class StringExtensions
{
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new System.Text.StringBuilder();

        for (int i = 0; i < input.Length; i++)
        {
            if (i > 0 && char.IsUpper(input[i]))
                result.Append('_');

            result.Append(char.ToLower(input[i]));
        }

        return result.ToString();
    }
}
