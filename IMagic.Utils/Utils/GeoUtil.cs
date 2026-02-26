using System;
using System.Globalization;

/// <summary>
/// Utilities for geographic coordinate parsing and float-array comparison.
/// </summary>
public static class GeoUtil
{
    /// <summary>
    /// Parses a coordinate string in decimal-degrees or DMS notation, with optional
    /// hemisphere suffix (N/S/E/W), into a signed decimal-degree <see cref="double"/>.
    /// Returns 0 on parse failure.
    /// </summary>
    public static double ParseCoordinateString(string coordinateString)
    {
        if (string.IsNullOrWhiteSpace(coordinateString))
            return 0;

        coordinateString = coordinateString.Trim().ToUpperInvariant();

        int sign = coordinateString.Contains('S') || coordinateString.Contains('W') ? -1 : 1;

        coordinateString = coordinateString
            .Replace("N", "").Replace("S", "")
            .Replace("E", "").Replace("W", "")
            .Trim();

        if (double.TryParse(coordinateString, NumberStyles.Float, CultureInfo.InvariantCulture, out double decimalDegrees))
            return sign * decimalDegrees;

        try
        {
            string cleaned = coordinateString
                .Replace("°", " ").Replace("'", " ").Replace("\"", " ")
                .Replace("  ", " ");

            string[] parts = cleaned.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
            {
                double degrees = double.Parse(parts[0], CultureInfo.InvariantCulture);
                double minutes = double.Parse(parts[1], CultureInfo.InvariantCulture);
                double seconds = parts.Length > 2 ? double.Parse(parts[2], CultureInfo.InvariantCulture) : 0;
                return sign * (degrees + minutes / 60.0 + seconds / 3600.0);
            }
        }
        catch
        {
            // fallback on parse error
        }

        return 0;
    }

    /// <summary>
    /// Converts a basic DMS string (e.g. "54° 31' 2.66\"") to decimal degrees.
    /// Returns 0 if the string does not contain at least three parts.
    /// </summary>
    public static double ParseDmsStringToDecimal(string dms)
    {
        string[] parts = dms.Split(new char[] { '°', '\'', '"' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
            return 0;

        double degrees = double.Parse(parts[0], CultureInfo.InvariantCulture);
        double minutes = double.Parse(parts[1], CultureInfo.InvariantCulture);
        double seconds = double.Parse(parts[2], CultureInfo.InvariantCulture);
        return degrees + minutes / 60.0 + seconds / 3600.0;
    }

    /// <summary>
    /// Returns <c>true</c> if the Euclidean distance between two float encodings is below
    /// <paramref name="threshold"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the arrays have different lengths.</exception>
    public static bool IsEncodingWithinTolerance(float[] encodingA, float[] encodingB, float threshold)
    {
        if (encodingA.Length != encodingB.Length)
            throw new ArgumentException("Encoding arrays must be the same length.");

        float sum = 0f;
        for (int i = 0; i < encodingA.Length; i++)
        {
            float diff = encodingA[i] - encodingB[i];
            sum += diff * diff;
        }

        return (float)Math.Sqrt(sum) < threshold;
    }
}
