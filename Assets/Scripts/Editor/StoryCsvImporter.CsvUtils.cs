using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

public static partial class StoryCsvImporter
{
    private static List<Dictionary<string, string>> ReadCsvAsMaps(string path, string[] requiredColumns)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("CSV file not found", path);
        }

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);
        if (lines.Length == 0)
        {
            throw new Exception("CSV file is empty: " + path);
        }

        List<string> header = ParseCsvLine(lines[0]);
        Dictionary<string, int> indexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < header.Count; i++)
        {
            string key = header[i].Trim();
            if (!indexMap.ContainsKey(key))
            {
                indexMap.Add(key, i);
            }
        }

        for (int i = 0; i < requiredColumns.Length; i++)
        {
            if (!indexMap.ContainsKey(requiredColumns[i]))
            {
                throw new Exception("Missing required column '" + requiredColumns[i] + "' in " + path);
            }
        }

        List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();
        for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
        {
            string line = lines[lineIndex];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            List<string> values = ParseCsvLine(line);
            Dictionary<string, string> row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, int> pair in indexMap)
            {
                string value = pair.Value < values.Count ? values[pair.Value].Trim() : string.Empty;
                row[pair.Key] = value;
            }

            rows.Add(row);
        }

        return rows;
    }

    private static List<string> ParseCsvLine(string line)
    {
        List<string> values = new List<string>();
        StringBuilder current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Length = 0;
            }
            else
            {
                current.Append(c);
            }
        }

        values.Add(current.ToString());
        return values;
    }

    private static string Get(Dictionary<string, string> row, string key)
    {
        string value;
        if (!row.TryGetValue(key, out value))
        {
            return string.Empty;
        }

        return value;
    }

    private static string Require(Dictionary<string, string> row, string key, string path)
    {
        string value = Get(row, key);
        if (string.IsNullOrEmpty(value))
        {
            throw new Exception("Required value is empty: column='" + key + "' file='" + path + "'");
        }

        return value;
    }

    private static int ParseInt(string text, int defaultValue)
    {
        if (string.IsNullOrEmpty(text))
        {
            return defaultValue;
        }

        int value;
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
        {
            return value;
        }

        return defaultValue;
    }

    private static float ParseFloat(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0f;
        }

        float value;
        if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            return value;
        }

        return 0f;
    }
}
