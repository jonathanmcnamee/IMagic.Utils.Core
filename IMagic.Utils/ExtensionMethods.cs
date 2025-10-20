using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


public static class ExtensionMethods
{
    #region bool

    public static string ToStringYesNo(this bool item)
    {
        return item ? "Yes" : "No";
    }

    #endregion

    #region DateTime

    private const double TotalDaysInYear = 365.25;
    private const double AverateDaysInMonth = 30.4375;

    /// <summary>
    /// Returns date.ToShortTimeString() if today else date.ToShortDateString()
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static string ToShortDateStringOrShortTimeString(this DateTime item)
    {
        if (item.Date == DateTime.UtcNow.Date)
        {
            return item.ToShortTimeString();
        }
        return item.ToShortDateString();
    }


    public static string ToddMMyyyyString(this DateTime item)
    {
        return item.ToString("dd/MM/yyyy");
    }


    public static DateTime StartOfMonth(this DateTime item)
    {
        DateTime tmp = new DateTime(item.Year, item.Month, 1);

        return tmp.Date;
    }

    public static bool IsYearAndMonthInFuture(this DateTime item)
    {
        DateTime now = DateTime.Now;
        if (item.Year > now.Year) { return true; }
        else if (item.Year == now.Year && item.Month > now.Month) { return true; }
        return false;
    }

    public static bool IsThisMonth(this DateTime item)
    {
        DateTime now = DateTime.Now;
        return (item.Year == now.Year && item.Month == now.Month);
    }

    public static bool IsToday(this DateTime item)
    {
        DateTime now = DateTime.Now;
        return (item.Date == now.Date);
    }

    public static bool IsBirthdayOrAniversary(this DateTime item)
    {
        DateTime now = DateTime.Now.Date;
        DateTime tmp = new DateTime(now.Year, item.Month, item.Day);

        bool output = (now.Date == tmp.Date);

        return output;
    }

    public static bool IsOfBirthdayOrAniversaryThisWeek(this DateTime item)
    {
        DateTime now = DateTime.Now;

        DateTime tmp = new DateTime(now.Year, item.Month, item.Day);

        TimeSpan difference = now.Date.Subtract(tmp);

        bool output = (difference.Days >= 0) && (difference.Days < 8);

        return output;
    }

    public static bool IsBirthdayOrAniversaryTomorrow(this DateTime item)
    {
        DateTime now = DateTime.Now;

        DateTime tmp = new DateTime(now.Year, item.Month, item.Day);

        TimeSpan difference = now.Date.Subtract(tmp);

        bool isTomorrow = (difference.Days == 1);

        return isTomorrow;
    }


    public static string ToddMMyyyyHHmmString(this DateTime item)
    {
        return item.ToString("dd/MM/yyyy HH:mm");
    }

    public static string ToString_RFC822(this DateTime item)
    {
        //return item.ToString("ddd, dd MMM yyyy HH:mm:ss K");
        return item.ToString("R");
    }

    public static string ToString_DateOnly_UrlFriendly(this DateTime item)
    {
        return item.ToString("dd-MM-yyyy");
    }

    public static string ToStringMMMMyyyy(this DateTime item)
    {
        return item.ToString("MMMM yyyy");
    }

    public static string ToStringMMMM(this DateTime item)
    {
        return item.ToString("MMMM");
    }

    public static string ToStringMMMyyyy(this DateTime item)
    {
        return item.ToString("MMM yyyy");
    }

    public static string ToStringVerbose(this DateTime item)
    {
        return item.ToString("dddd, MMMM dd, yyyy");
    }

    public static string ToMMMMyyyyString(this DateTime item)
    {
        return item.ToString("MMMM yyyy");
    }

    public static string ToMicroFormatDateTime(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:sszzz");
    }

    public static string ToLongDateTimeWithSuffix(this DateTime dateTime)
    {
        int day = dateTime.Day;
        string suffix = string.Empty;

        switch (day)
        {
            case 1:
            case 21:
            case 31:
                suffix = "st";
                break;

            case 2:
            case 22:
                suffix = "nd";
                break;

            case 3:
            case 23:
                suffix = "rd";
                break;

            default:
                suffix = "th";
                break;
        }

        return string.Format("{0}<sup>{1}</sup> {2}", dateTime.Day, suffix, dateTime.ToString("MMM yyyy"));
    }

    public static string ToStringFuzzyTime2(this DateTime CreateDate)
    {
        TimeSpan ts = DateTime.Now.Subtract(CreateDate);
        if (CreateDate.Date == DateTime.Now.Date)
        {
            if (ts.TotalSeconds < 60)
            {
                if (ts.Seconds == 1)
                {
                    return "1 second ago";
                }
                return string.Format("{0} seconds ago", ts.Seconds);
            }
            else if (ts.TotalMinutes < 2)
            {
                return "1 minute ago";
            }
            else if (ts.TotalMinutes < 60)
            {
                return ts.Minutes.ToString() + " minutes ago";
            }
            else if (ts.TotalHours < 2)
            {
                return "an hour ago";
            }
            else if (ts.Hours < DateTime.Now.Hour)
            {
                return string.Format("{0} hours ago", ts.Hours);
            }
        }
        else if (CreateDate.Date >= DateTime.Now.AddDays(-1).Date)
        {
            return "yesterday";
        }
        else
        {
            //reset date to include date only portion
            ts = DateTime.Now.Date.Subtract(CreateDate.Date);

            if (ts.TotalDays < 7)
            {
                return Math.Ceiling(ts.TotalDays) + " days ago";
            }
            else if (ts.Days <= 14)
            {
                return "a week ago";
            }
            else if (ts.Days <= 50)
            {
                int weeks = ts.Days / 7;
                return string.Format("{0} weeks ago", weeks);
            }
            else if (ts.Days <= 365)
            {
                int monthsBetween = (int)Math.Ceiling(ts.Days / 31.00M);
                return string.Format("{0} months ago", monthsBetween);
            }
            else if (CreateDate.Year == DateTime.Now.AddYears(-1).Year)
            {
                return string.Format("last year");
            }
            else
            {
                return string.Format("{0} years ago", (DateTime.Now.Year - CreateDate.Year));
            }
        }
        return "Unknown";
    }

    public static string ToStringAge(this DateTime d)
    {
        string output = string.Empty;
        if (d <= DateTime.Now)
        {
            TimeSpan ts = DateTime.Now.Subtract(d);
            if (ts.TotalMinutes < 2)
            {
                output = "1 minute";
            }
            else if (ts.TotalHours < 1)
            {
                output = ts.Minutes.ToString() + " minutes";
            }
            else if (ts.TotalHours < 2)
            {
                output = "1 hour";
            }
            else if (ts.TotalHours >= 1 && ts.TotalDays < 1)
            {
                output = ts.Hours + " hours";
            }
            else if (ts.TotalHours > 1 && ts.TotalDays < 1)
            {
                output = "1 day";
            }
            else if ((int)ts.Days < 7)
            {
                output = ts.Days + " days";
            }
            else if ((ts.Days >= 7) && (ts.Days <= 14))
            {
                output = "1 week";
            }
            else if (ts.Days < 58)//we want to display weeks up to 2 months[58 is total of 2 shortest months, feb-28 days + a 30 day month]
            {
                double m = ((double)ts.Days / (double)7);
                output = Convert.ToInt32(m) + " weeks";
            }
            else if (ts.Days < TotalDaysInYear)
            {
                double m = ((double)ts.Days / (double)AverateDaysInMonth);
                output = Convert.ToInt32(m) + " months";
            }
            else if (ts.Days < (TotalDaysInYear * 2))
            {
                output = "1 year";
            }
            else
            {
                double m = ((double)ts.Days / (double)TotalDaysInYear);
                output = Convert.ToInt32(m) + " years";
            }

            output = string.Format("{0} old", output);
        }
        else
        {
            output = "[date is in future]";
        }
        return output;
    }


    #endregion

    #region double

    public static string ToStringCurrencyMajorPartOnly(this double d)
    {
        return d.ToString("##");
    }

    public static string ToStringCurrency(this double item)
    {
        string trailingZeros = ".00";
        string output = item.ToString("C");
        //need to trim trailing zeros in a clean way
        if (output.EndsWith(trailingZeros))
        {
            output = output.Substring(0, output.IndexOf(trailingZeros));
        }
        return output;
    }

    /// <summary>
    /// defaults to 2 decimal places
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static string ToStringDecimalPlaces(this double item)
    {
        return item.ToStringDecimalPlaces(2);
    }

    public static string ToStringDecimalPlaces(this double item, int totalDecimalPlaces)
    {
        string suffix = "";
        for (int i = 0; i < totalDecimalPlaces; i++)
        {
            suffix += "0";
        }
        string formatString = string.Format("0.{0}", suffix);
        return item.ToString(formatString);
    }
    #endregion

    #region enum

    public static string EnumDescription(this Enum enumValue)
    {
        if (enumValue == null || enumValue.ToString() == "0")
        {
            return string.Empty;
        }
        FieldInfo enumInfo = enumValue.GetType().GetField(enumValue.ToString());
        DescriptionAttribute[] enumAttributes = (DescriptionAttribute[])enumInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (enumAttributes.Length > 0)
        {
            return enumAttributes[0].Description;
        }
        else
        {
            return enumValue.ToString();
        }
    }

    public static string FlagsDescription(this Enum enumValue)
    {
        List<Enum> enums = new List<Enum>();

        foreach (string name in enumValue.ToString().Split(','))
        {
            enums.Add((Enum)Enum.Parse(enumValue.GetType(), name));
        }

        return enums.Select(p => p.EnumDescription()).ToStringCommaSeperated();
    }


    #endregion

    #region IEnumerable<T>

    /// <summary>
    /// Provides a generic method to execute a given method on every item of a list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="action"></param>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (T element in source)
        {
            action(element);
        }
    }

    public static bool None<TSource>(this IEnumerable<TSource> source)
    {
        return !source.Any();
    }


    public static bool None<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        return !source.Any(predicate);
    }

    #region IEnumerable<T> Extensions

    /// <summary>
    /// works out if there are at least x items in a collection without invoking count method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="minimum"></param>
    /// <returns></returns>
    public static bool AtLeast<T>(this IEnumerable<T> items, int minimum)
    {
        IEnumerator<T> iEnumerator = items.GetEnumerator();
        int count = 0;
        while (iEnumerator.MoveNext())
        {
            count++;
            if (count >= minimum)
            {
                return true;
            }
        }
        return (count >= minimum);
    }

    /// <summary>
    /// works out if there are at most x items in a collection without invoking count method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="minimum"></param>
    /// <returns></returns>
    public static bool AtMost<T>(this IEnumerable<T> items, int maximum)
    {
        //after extensive testing, it appears that you only want this to return true if the exact count matches, and false above or below this number,
        //meaning that its implementation is effectively the Exactly method
        return items.Exactly(maximum);
    }

    /// <summary>
    /// works out if there are exactly x items in a collection without invoking count method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="minimum"></param>
    /// <returns></returns>
    public static bool Exactly<T>(this IEnumerable<T> items, int exactAmount)
    {
        IEnumerator<T> iEnumerator = items.GetEnumerator();
        int count = 0;
        while (iEnumerator.MoveNext())
        {
            count++;
            if (count > exactAmount)
            {
                return false;
            }
        }
        return (count == exactAmount);
    }



    public static T RandomElement<T>(this IEnumerable<T> items)// where T : class
    {
        int randomIndex = RandomUtil.Next(items.Count());
        return items.ElementAt(randomIndex);
    }

    /// <summary>
    /// returns iEnumerable in a random order
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> Randomise<T>(this IEnumerable<T> items)// where T : class
    {
        return items.RandomElements(items.Count(), null);
    }

    public static IEnumerable<T> RandomElements<T>(this IEnumerable<T> items, int count)// where T : class
    {
        return items.RandomElements(count, null);
    }

    public static IEnumerable<T> RandomElements<T>(this IEnumerable<T> items, int count, Func<T, bool> predicate) //where T : class
    {
        if (predicate != null)
        {
            items = items.Where(predicate);
        }

        int itemCount = items.Count();
        if (count > 20 && itemCount < 5000)//upload upfront if you're going to be taking at least half the list back as random elements
        {
            items = items.ToList();
        }

        List<T> output = new List<T>();
        List<int> usedIndices = new List<int>();

        if (count > 0)
        {
            for (int i = 0; i < count && i < itemCount; i++)
            {
                int index = -1;
                do
                {
                    index = RandomUtil.Next(itemCount);
                }
                while (usedIndices.Contains(index));
                usedIndices.Add(index);


                output.Add(items.ElementAt(index));
            }
        }

        return output;
    }

    public static IEnumerable<T> RandomElementsAndRemainingItems<T>(this IEnumerable<T> items, int count, out List<T> remainingItems)// where T : class
    {
        List<T> output = new List<T>();
        List<int> randomIndices = new List<int>();

        if (count > 0)
        {
            remainingItems = new List<T>();
            int itemCount = items.Count();

            for (int i = 0; i < count && i < itemCount; i++)//generate random indices
            {
                int index = -1;
                do
                {
                    index = RandomUtil.Next(itemCount);
                }
                while (randomIndices.Contains(index));
                randomIndices.Add(index);
                output.Add(items.ElementAt(index));
            }

            //I could use except method, but this relies on having Equals method overridden on custom items

            int inputCount = items.Count();
            for (int i = 0; i < inputCount; i++)
            {
                if (randomIndices.Contains(i))
                {
                    i++;
                }
                else
                {
                    remainingItems.Add(items.ElementAt(i));
                }
            }
        }
        else
        {
            remainingItems = items.ToList();
        }

        return output;
    }

    //public static T RandomElement<T>(this IEnumerable<T> items) where T : class
    //{
    //    Random r = new Random();
    //    return items.ElementAt(r.Next(items.Count()));
    //}

    //public static IEnumerable<T> RandomElements<T>(this IEnumerable<T> items, int count) where T : class
    //{
    //    int itemCount = items.Count();
    //    List<int> usedIndices = new List<int>();
    //    List<T> output = new List<T>();
    //    Random r = new Random();

    //    for (int i = 0; i < count && i < itemCount; i++)
    //    {
    //        int index = -1;
    //        do
    //        {
    //            index = r.Next(itemCount);
    //        }
    //        while (usedIndices.Contains(index));
    //        usedIndices.Add(index);


    //        output.Add(items.ElementAt(index));
    //    }




    //    return output;
    //}




    //todo: eventually remove - but for now keep for reference as a way of running a predicate against an item using a temp list
    //public static ListItem[] ToListItemArray<T>(this IEnumerable<T> items, Func<T, string> textPredicate, Func<T, string> valuePredicate, bool includeSelect = true, string selectText = "Please Select")
    //{
    //    //todo: extend this method to allow pre-selection of items, eg: checkbox list, or a multiselect dropdown

    //    List<ListItem> output = new List<ListItem>();

    //    if (includeSelect)
    //    {
    //        output.Add(new ListItem(selectText, ""));
    //    }

    //    foreach (T item in items)
    //    {
    //        List<T> tmp = new List<T>() { item };//create temporary 1 item list so we can do select :)
    //        ListItem listItem = new ListItem(tmp.Select(textPredicate).FirstOrDefault(), tmp.Select(valuePredicate).FirstOrDefault());
    //        output.Add(listItem);
    //    }

    //    return output.ToArray();
    //}

    #endregion

    #region IEnumerable<string> Extensions

    public static string ToStringCommaSeperated(this IEnumerable<string> t)
    {
        return string.Join(",", t.ToArray());
    }

    public static string ToStringSeperated(this IEnumerable<string> t, string seperator)
    {
        return string.Join(seperator, t.ToArray());
    }

    #endregion


    #region int

    public static string ToStringOrdinal(this int value)
    {
        if (value % 100 > 10 && value % 100 < 20)
        {
            return string.Format("{0}th", value);
        }
        else
        {
            int remainder = value % 10;
            switch (remainder)
            {
                case 1:
                    {
                        return string.Format("{0}st", value);
                    }
                case 2:
                    {
                        return string.Format("{0}nd", value);
                    }
                case 3:
                    {
                        return string.Format("{0}rd", value);
                    }
                default:
                    {
                        return string.Format("{0}th", value);
                    }
            }
        }
    }

    public static string ToStringLeadingZero(this int value)
    {
        return value.ToString("d2");
    }

    public static string ToStringLeadingZero(this int value, int leadingZeros)
    {
        return value.ToString(string.Format("d{0}", leadingZeros));
    }

    public static string ToStringNumber(this int i)
    {
        return i.ToString("###,###,###");
    }
    #endregion   

    #region string

    public static string ToBase64EncodedString(this string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }

    public static string FromBase64EncodedString(this string base64EncodedData)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }

    public static string ToUrlFriendlyString(this string text)
    {
        string output = text.Trim()
               .Replace("&", "and")
               .Replace(".", "-")
               .Replace(" ", "-")
               .RemoveAccent()
               .StripNonAplhaNumeric();
        return output;
    }

    public static string UrlFriendly(this string text)
    {
        string str = text.RemoveAccent().ToLower();

        str = Regex.Replace(str, @"[^a-z0-9\s-]", ""); // remove invalid chars      
        str = Regex.Replace(str, @"[\s-]+", "-"); // convert spaces and hyphens into one hyphen
        str = str.Trim("- ".ToCharArray()); // trim any unecessary spaces or hyphens

        return str;
    }


    public static bool IsValidEmailAddress(this string text)
    {
        if (text != null)
        {
            string emailRegex = @"^([a-zA-Z0-9_\-\.\+]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Match match = Regex.Match(text, emailRegex, RegexOptions.IgnoreCase);
            return match.Success;
        }
        return false;
    }

    public static string ToTitleCase(this string s)
    {
        CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
        TextInfo textInfo = cultureInfo.TextInfo;
        return textInfo.ToTitleCase(s);
    }

    public static byte[] ToByteArray(this string s)
    {
        System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
        Byte[] bytes = encoding.GetBytes(s);
        return bytes;
    }


    public static bool HasValue(this string s)
    {
        bool output = !string.IsNullOrWhiteSpace(s);
        return output;
    }


    public static string ToMaxLength(this string s, int maxLength)
    {
        return s.ToMaxLength(maxLength, true);
    }

    public static string ToMaxLength(this string s, int maxLength, bool htmlElipsis)
    {
        string elipsis = htmlElipsis ? "&#8230;" : "...";
        int elipsisLength = 3;
        if ((s.Length + elipsisLength) > maxLength)
        {
            s = s.Substring(0, (maxLength - elipsisLength)) + elipsis;
        }
        return s;
    }




    public static string StripHTML(this string s)
    {
        string pattern = @"<(.|\n)*?>";
        return Regex.Replace(s, pattern, string.Empty);
    }

    //fails on .net Standard :(
    public static string RemoveAccent(this string txt)
    {
        byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(txt);
        return System.Text.Encoding.ASCII.GetString(bytes);
    }


    public static string StripNonAplhaNumeric(this string s)
    {
        s = Regex.Replace(s, @"[^a-zA-Z0-9]", string.Empty);
        return s;
    }

    public static string StripNonAplhaNumericDash(this string s)
    {
        s = Regex.Replace(s, @"[^a-zA-Z0-9\-]", string.Empty);
        return s;
    }

    public static string RemoveMultipleDashes(this string s)
    {
        string doubleDash = "--";
        while (s.Contains(doubleDash))
        {
            s = s.Replace(doubleDash, "-");
        }
        return s;
    }

    public static bool ContainsAny(this string s, IEnumerable<string> searchStrings)
    {
        if (s.HasValue())
        {
            s = s.ToLower();
            foreach (string searchString in searchStrings)
            {
                if (s.Contains(searchString.ToLower()))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static int IndexOfNth(this string value, string search, int n)
    {
        int output = -1;
        int current = 0;

        for (int i = 1; i <= n && i < value.Length; i++)
        {
            if (i == n)
            {
                output = value.IndexOf(search, current);
            }
            else
            {
                current = value.IndexOf(search, current) + 1;
            }
        }

        return output;
    }

    public static T ConvertToEnum<T>(this string value)
    {
        Type t = typeof(T);

        if (!t.IsEnum)
        {
            throw new ArgumentException("Type provided must be an Enum.", "T");
        }

        T item = (T)Enum.Parse(t, value, true);

        return item;
    }
    #endregion

    #region string - topping and tailing

    /// <summary>
    /// removes all test from input from post onwards, including post itself
    /// </summary>
    /// <param name="input"></param>
    /// <param name="post"></param>
    /// <returns></returns>
    public static string RemovePost(this string input, string post)
    {
        if (input.IndexOf(post) > -1)
        {
            return input.Substring(0, input.IndexOf(post));
        }
        return input;
    }
    /// <summary>
    /// removes all test from input from post onwards, but leaves post intact
    /// </summary>
    /// <param name="input"></param>
    /// <param name="post"></param>
    /// <param name="removePost"></param>
    /// <returns></returns>
    public static string RemovePost(this string input, string post, bool removePostString)
    {
        if (removePostString)
        {
            return input.RemovePost(post);
        }
        else
        {
            if (input.IndexOf(post) > -1)
            {
                return input.Substring(0, input.IndexOf(post + post.Length));
            }
        }
        return input;
    }

    #endregion


    #region TimeSpan

    public static string ToStringFuzzyTime(this TimeSpan ts)
    {
        return string.Format("{0:00}:{1:00}:{2:00}", (int)ts.TotalHours, ts.Minutes, ts.Seconds);
    }

    public static string ToStringFuzzyTimeMillis(this TimeSpan ts)
    {
        return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", (int)ts.TotalHours, ts.Minutes, ts.Seconds, ts.Milliseconds);
    }

    #endregion


    #region Is & As conversion

    public static bool IsValueInRange(this int input, int lowerBound, int upperBound)
    {
        if (input >= lowerBound && input <= upperBound)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// tests a given string to see if it is a valid integer
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static bool IsInt(this string s)
    {
        if (s.HasValue())
        {
            int output = -1;
            return int.TryParse(s, out output);
        }
        return false;
    }

    /// <summary>
    /// if string is a valid integer (IsInteger) integer value is returned, else -1
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static int AsInt(this string s)
    {
        if (s.IsInt())
        {
            int output = -1;
            int.TryParse(s, out output);
            return output;
        }
        return -1;
    }

    public static bool IsDouble(this string s)
    {
        if (s.HasValue())
        {
            double output = 0;
            return double.TryParse(s, out output);
        }
        return false;
    }

    public static double AsDouble(this string s)
    {
        if (s.HasValue())
        {
            double output = 0;
            double.TryParse(s, out output);
            return output;
        }
        return -1;
    }

    public static bool IsBool(this string s)
    {
        if (s.HasValue())
        {
            bool output = false;
            return bool.TryParse(s, out output);
        }
        return false;
    }

    public static bool AsBool(this string s)
    {
        if (s.HasValue())
        {
            bool output = false;
            bool.TryParse(s, out output);
            return output;
        }
        return false;
    }
    #endregion

    #region Guid
    /// <summary>
    /// tests a given string to see if it is a valid Guid
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static bool IsGuid(this string s)
    {
        if (s.HasValue())
        {
            Guid output = Guid.Empty;
            return Guid.TryParse(s, out output);
        }
        return false;
    }

    /// <summary>
    /// if string is a valid integer (IsGuid) integer value is returned, else Guid.Empty
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static Guid AsGuid(this string s)
    {
        if (s.IsGuid())
        {
            Guid output = Guid.Empty;
            Guid.TryParse(s, out output);
            return output;
        }
        return Guid.Empty;
    }
    #endregion

    #region Properties
    //http://handcraftsman.wordpress.com/2008/11/11/how-to-get-c-property-names-without-magic-strings/
    //https://github.com/mvba/MvbaCore/blob/master/src/MvbaCore/Reflection.cs#L248

    public static string GetPropertyName(MemberExpression memberExpression)
    {
        if (memberExpression == null)
        {
            return null;
        }
        List<string> names = GetNames(memberExpression);
        string name = string.Join(".", names.ToArray());
        return name;
    }

    public static string GetPropertyName(UnaryExpression unaryExpression)
    {
        if (unaryExpression == null)
        {
            return null;
        }
        var memberExpression = unaryExpression.Operand as MemberExpression;
        return GetPropertyName(memberExpression);
    }

    private static List<string> GetNames(MemberExpression memberExpression)
    {
        var names = new List<string>
{
memberExpression.Member.Name
};
        while (memberExpression.Expression as MemberExpression != null)
        {
            memberExpression = (MemberExpression)memberExpression.Expression;
            names.Insert(0, memberExpression.Member.Name);
        }
        return names;
    }

    #endregion






    #endregion

    #region string

    /// <summary>
    /// the missing string.split method that just takes a string without the new string[] { splitString } nonsense
    /// </summary>
    /// <param name="text"></param>
    /// <param name="splitString"></param>
    /// <param name="stringSplitOptions"></param>
    /// <returns></returns>
    public static string[] Split(this string text, string splitString, StringSplitOptions stringSplitOptions = StringSplitOptions.None)
    {
        string[] output = text.Split(new string[] { splitString }, stringSplitOptions);
        return output;
    }

    public static string DefaultIfEmpty(this string text, string defaultText)
    {
        if (!text.HasValue())
        {
            return defaultText;
        }
        return text;
    }

    /// <summary>
    /// this extension method is a simple wrapper for text.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase);
    /// <remarks>It will not work on a sql based linq query, call .ToList() first</remarks>
    /// </summary>
    /// <param name="text"></param>
    /// <param name="searchTerm"></param>
    /// <returns></returns>
    [Obsolete("Use Contains(value,false) instead")]
    public static bool ContainsIgnoreCase(this string text, string searchTerm)
    {
        bool found = false;
        if (text != null)
        {
            int index = text.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase);
            found = (index > -1);
        }
        return found;
    }

    [Obsolete("Use Contains(value,false) instead")]
    public static bool ContainsCaseInsensitive(this string s, string compare)
    {
        bool equal = s.Trim().ToLower().Contains(compare.Trim().ToLower());

        return equal;
    }

    public static bool Contains(this string text, string searchTerm, bool ignoreCase)
    {
        bool found = false;
        if (text != null)
        {
            int index = text.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase);
            found = (index > -1);
        }
        return found;
    }

    public static string Remove(this string s, params string[] stringsToRemove)
    {
        string output = s;

        if (stringsToRemove != null)
        {
            foreach (string key in stringsToRemove)
            {
                output = output.Replace(key, string.Empty);
            }
        }

        return output;
    }


    public static string RemoveAfter(this string s, string key, bool removekey = true)
    {
        int index = s.IndexOf(key);

        if (index > -1)
        {
            int trimIndex = index;
            if (removekey)
            {
                trimIndex = index + key.Length;
            }
            s = s.Substring(trimIndex);
        }

        return s;
    }

    public static string RemoveBefore(this string s, string key, bool removeKey = true)
    {
        int index = s.IndexOf(key);

        if (index > -1)
        {
            int trimIndex = index + key.Length;
            if (removeKey)
            {
                trimIndex = index;
            }
            s = s.Substring(0, trimIndex);
        }

        return s;
    }

    public static string RemoveBetween(this string s, string before, string after, bool replaceKeys = false)
    {
        string pre = s.RemoveBefore(before, replaceKeys);
        string post = s.RemoveAfter(after, replaceKeys);
        string output = pre + post;
        return output;
    }

    public static string ReplaceBetween(this string s, string before, string after, string textToInject, bool replaceKeys = false)
    {
        string pre = s.RemoveBefore(before, replaceKeys);
        string post = s.RemoveAfter(after, replaceKeys);
        string output = pre + textToInject + post;
        return output;
    }


    public static string Append(this string s, string secondString)
    {
        return s.Append(secondString, true);
    }
    public static string Append(this string s, string secondString, bool includeSpace)
    {
        string space = includeSpace ? " " : "";
        string output = string.Format("{0}{1}{2}", s, space, secondString);
        return output;
    }




    public static string ToMaxLengthNoElipsis(this string s, int maxLength)
    {
        if (s != null)
        {
            s = s.Trim();
            if (s.Length > maxLength)
            {
                s = s.Substring(0, maxLength);
            }
        }
        return s;
    }




    public static string RemoveNonAplhabetic(this string s)
    {
        s = Regex.Replace(s, @"[^a-zA-Z]", string.Empty);
        return s;
    }

    public static string RemoveNonAplhaNumeric(this string s)
    {
        s = Regex.Replace(s, @"[^a-zA-Z0-9\-]", string.Empty);
        return s;
    }

    public static string RemoveNonAplhaNumericDash(this string s)
    {
        s = Regex.Replace(s, @"[^a-zA-Z0-9\-]", string.Empty);
        return s;
    }



    public static string TidyName(this string name)
    {
        string tidyName = string.Empty;

        foreach (string namePart in name.Split(' '))
        {
            tidyName += namePart[0].ToString().ToUpper();
            tidyName += namePart.Substring(1) + " ";
        }

        return tidyName.Trim();
    }
    public static string TidyName(this string name, bool alsoLower)
    {
        string tidyName = string.Empty;

        foreach (string namePart in name.Split(' '))
        {
            tidyName += namePart[0].ToString().ToUpper();

            if (alsoLower)
            {
                tidyName += namePart.Substring(1).ToLower() + " ";
            }
            else
            {
                tidyName += namePart.Substring(1) + " ";
            }
        }

        return tidyName.Trim();
    }

    public static string ToFirstLetterCapitalised(this string text)
    {
        string capitalisedText = string.Empty;

        if (text != null && text.Length > 0)
        {
            capitalisedText = text[0].ToString().ToUpper() + text.Substring(1);
        }

        return capitalisedText;
    }

    public static List<string> SplitQuoted(this string s)
    {
        Regex regex = new Regex(@"((""((?<token>.*?)(?<!\\)"")|(?<token>[\w]+))(\s)*)", RegexOptions.None);
        List<string> result = (from Match m in regex.Matches(s)
                               where m.Groups["token"].Success
                               select m.Groups["token"].Value.Trim()).ToList();

        return result;
    }


    #endregion

    #region DirectoryInfo
    /// <summary>
    /// extension method to remove all files and folders within a directory
    /// </summary>
    /// <param name="directory"></param>
    public static void ClearDirectory(this DirectoryInfo directory)
    {
        foreach (FileInfo file in directory.GetFiles())
        {
            file.Delete();
        }

        foreach (DirectoryInfo subDirectory in directory.GetDirectories())
        {
            subDirectory.Delete(true);
        }
    }
    #endregion
}
