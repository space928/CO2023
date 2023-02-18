// Military time converter by Thomas M. - Team 16

using System.Text;

public static class MilitaryTimeConverter
{
    private static readonly string[] numberNames =
    {
        "zero",
        "one",
        "two",
        "three",
        "four",
        "five",
        "six",
        "seven",
        "eight",
        "nine",
        "ten",
        "eleven",
        "twelve",
        "thirteen",
        "fourteen",
        "fifteen",
        "sixteen",
        "seventeen",
        "eighteen",
        "nineteen",
        "twenty",
        "twenty one",
        "twenty two",
        "twenty three",
    };

    private static readonly string[] tensNames =
    {
        "zero",
        "ten",
        "twenty",
        "thirty",
        "forty",
        "fifty",
    };

    private static void Main(string[] args)
    {
#if DEBUG
        // Run unit tests
        Assert(ParseTime("1:00PM") == (13,0));
        Assert(ParseTime("1:00AM") == (1,0));
        Assert(ParseTime("01:00AM") == (1,0));
        Assert(ParseTime("12:00AM") == (0,0));
        Assert(ParseTime("11:00PM") == (23,0));

        Assert(ConvertTime("4:00PM") == "sixteen hundred hours");
        Assert(ConvertTime("11:00AM") == "eleven hundred hours");
        Assert(ConvertTime("11:23AM") == "eleven twenty three");
        Assert(ConvertTime("6:45PM") == "eighteen forty five");
        Assert(ConvertTime("7:45AM") == "zero seven forty five");
        Assert(ConvertTime("5:05PM") == "seventeen zero five");
        Assert(ConvertTime("4:09AM") == "zero four zero nine");
        Assert(ConvertTime("6:45PM") == "eighteen forty five");
#else
        string inputTime = Console.ReadLine();
        Console.WriteLine(ConvertTime(inputTime));
#endif
    }

    private static void Assert(bool value, string msg = "")
    {
        if (!value)
            throw new Exception("Assertion triggered! " + msg);
    }

    /// <summary>
    /// Converts a time string from 12 hour format (X:XX[PM/AM] or XX:XX[PM/AM]) to spoken
    /// military time format.
    /// </summary>
    /// <param name="inputTime">Time string to convert</param>
    /// <returns>Converted time</returns>
    private static string ConvertTime(string inputTime)
    {
        (int hours, int minutes) = ParseTime(inputTime);

        StringBuilder sb = new();
        if (hours < 10)
        {
            sb.Append(numberNames[0]);
            sb.Append(' ');
        }
        sb.Append(numberNames[hours]);
        sb.Append(' ');

        // Early exit for hundred hours
        if (minutes == 0)
        {
            sb.Append("hundred hours");
            return sb.ToString();
        }

        // Minutes leading zero
        if (minutes < 10)
        {
            sb.Append(numberNames[0]);
            sb.Append(' ');
        }
        if (minutes < 24)
        {
            // For minutes < 24 use the cached name
            sb.Append(numberNames[minutes]);
        }
        else
        {
            // Otherwise construct the name
            sb.Append(tensNames[minutes / 10]);
            sb.Append(' ');
            sb.Append(numberNames[minutes % 10]);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Parses a 12 hour time string in the form X:XX[PM/AM] or XX:XX[PM/AM].
    /// </summary>
    /// <param name="inputTime"></param>
    /// <returns>Returns the number of hours and minutes as integers in 24 hour format</returns>
    /// <exception cref="ArgumentException">throws if the input time is not in the correct format</exception>
    private static (int hours, int minutes) ParseTime(string inputTime)
    {
        if (string.IsNullOrWhiteSpace(inputTime))
            throw new ArgumentException("No time was input!");

        if (inputTime.Length < 4)
            throw new ArgumentException("Time string must be in the form [x]x:xx[PM/AM]!");

        // To avoid costly heap allocations we're going to split the string manually,
        // given that the colon must be at index 1 or 2.
        int hours;
        int colon = 0;
        if (inputTime[1] == ':')
        {
            if (!int.TryParse(inputTime[0..1], out hours))
                throw new ArgumentException($"'{inputTime[0]}' is not a valid number of hours");

        }
        else if (inputTime[2] == ':')
        {
            if (!int.TryParse(inputTime[0..2], out hours))
                throw new ArgumentException($"'{inputTime[0..2]}' is not a valid number of hours");
            colon++;
        }
        else
        {
            throw new ArgumentException("Time string must contain a colon!");
        }

        // Parse minutes
        if (!int.TryParse(inputTime[(2 + colon)..(4 + colon)], out int minutes))
            throw new ArgumentException($"'{inputTime[(2 + colon)..(4 + colon)]}' is not a valid number of minutes");

        // Check that hours and minutes are within range
        if (hours > 12 || hours < 1 || minutes < 0 || minutes > 59)
            throw new ArgumentException("Time string out of range!");

        // Wrap hours to 0
        if (hours == 12)
            hours = 0;

        // Apply AM/PM
        if (inputTime.ToLowerInvariant()[4 + colon] == 'p')
            hours += 12;

        return (hours, minutes);
    }
}
