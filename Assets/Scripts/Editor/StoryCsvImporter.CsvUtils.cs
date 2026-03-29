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

        List<List<string>> records = ParseCsvRecords(File.ReadAllText(path, Encoding.UTF8));
        if (records.Count == 0)
        {
            throw new Exception("CSV file is empty: " + path);
        }

        List<string> header = records[0];
        Dictionary<string, int> indexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < header.Count; i++)
        {
            string key = header[i].Trim();
            if (i == 0)
            {
                key = key.TrimStart('\uFEFF');
            }

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
        for (int recordIndex = 1; recordIndex < records.Count; recordIndex++)
        {
            List<string> values = records[recordIndex];
            bool hasAnyValue = false;
            for (int i = 0; i < values.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]))
                {
                    hasAnyValue = true;
                    break;
                }
            }

            if (!hasAnyValue)
            {
                continue;
            }

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

    private static List<List<string>> ParseCsvRecords(string content)
    {
        List<List<string>> records = new List<List<string>>();
        List<string> currentRecord = new List<string>();
        StringBuilder currentField = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < content.Length && content[i + 1] == '"')
                {
                    currentField.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                currentRecord.Add(currentField.ToString());
                currentField.Length = 0;
            }
            else if ((c == '\n' || c == '\r') && !inQuotes)
            {
                if (c == '\r' && i + 1 < content.Length && content[i + 1] == '\n')
                {
                    i++;
                }

                currentRecord.Add(currentField.ToString());
                currentField.Length = 0;

                records.Add(currentRecord);
                currentRecord = new List<string>();
            }
            else
            {
                currentField.Append(c);
            }
        }

        if (inQuotes)
        {
            throw new Exception("CSV parse error: unmatched quote.");
        }

        bool hasTrailingData = currentField.Length > 0 || currentRecord.Count > 0;
        if (hasTrailingData)
        {
            currentRecord.Add(currentField.ToString());
            records.Add(currentRecord);
        }

        return records;
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
