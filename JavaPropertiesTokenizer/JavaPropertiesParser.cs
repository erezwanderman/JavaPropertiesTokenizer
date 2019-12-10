using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JavaPropertiesTokenizer
{
    public static class JavaPropertiesParser
    {
        public static Encoding DefaultEncoding { get; } = Encoding.GetEncoding("ISO-8859-1");
        public static List<JavaPropertiesToken> Parse(Stream stream, Encoding encoding = null)
        {
            List<JavaPropertiesToken> tokens = new List<JavaPropertiesToken>();
            using var sr = new StreamReader(stream, encoding ?? DefaultEncoding);
            foreach (var ll in EnumerateLogicalLines(sr))
            {
                if (ll.IsBlank)
                    tokens.Add(new JavaPropertiesTokenWhitespaceLine(ll.Parts[0]));
                else if (ll.IsComment)
                    tokens.Add(new JavaPropertiesTokenCommentLine(ll.Parts[0]));
                else
                    tokens.Add(ParseKeyValuePair(ll.Parts));
            }
            return tokens;
        }

        private static JavaPropertiesTokenKeyValuePair ParseKeyValuePair(List<string> parts)
        {
            int i = 0, j = 0;
            List<string> beforeKey = GetWhitespace(parts, ref i, ref j);
            List<string> key = GetKey(parts, ref i, ref j);
            (List<string> afterKey, List<string> keyValueSeparator) = GetAfterKeyAndKeyValueSeparator(parts, ref i, ref j);
            List<string> afterKeyValueSeparator = GetWhitespace(parts, ref i, ref j);
            List<string> value = GetValue(parts, ref i, ref j);



            return new JavaPropertiesTokenKeyValuePair(beforeKey, key, afterKey, keyValueSeparator, afterKeyValueSeparator, value);
        }

        private static List<string> GetWhitespace(List<string> parts, ref int i, ref int j)
        {
            if (i == parts.Count)
                return new List<string> { "" };
            int startI = i, startJ = j;
            List<string> beforeKey = new List<string>();
            for (; i < parts.Count; i++)
            {
                bool foundNonWhitespace = false;
                for (; j < parts[i].Length; j++)
                {
                    if (parts[i][j] != ' ' && parts[i][j] != '\t' && parts[i][j] != '\f')
                    {
                        foundNonWhitespace = true;
                        break;
                    }
                }
                if (foundNonWhitespace)
                {
                    if (i == startI)
                        beforeKey.Add(parts[i].Substring(startJ, j - startJ));
                    else
                        beforeKey.Add(parts[i].Remove(j));
                    break;
                }
                if (i == startI)
                    beforeKey.Add(parts[i].Substring(startJ, j - startJ));
                else
                    beforeKey.Add(parts[i].Remove(j));
                j = 0;
            }
            return beforeKey;
        }

        private static List<string> GetKey(List<string> parts, ref int i, ref int j)
        {
            List<string> key = new List<string>();
            if (parts[i][j] != '=' && parts[i][j] != ':')
            {
                bool foundCharAfterKey = false;
                for (; i < parts.Count; i++)
                {
                    StringBuilder keyDecoded = new StringBuilder();
                    for (; j < parts[i].Length; j++)
                    {
                        if (parts[i][j] == ' ' || parts[i][j] == '\t' || parts[i][j] == '\f' || parts[i][j] == '=' || parts[i][j] == ':')
                        {
                            foundCharAfterKey = true;
                            break;
                        }
                        if (parts[i][j] == '\\')
                        {
                            switch (parts[i][j + 1])
                            {
                                case 't':
                                    keyDecoded.Append('\t');
                                    j += 1;
                                    break;
                                case 'r':
                                    keyDecoded.Append('\r');
                                    j += 1;
                                    break;
                                case 'n':
                                    keyDecoded.Append('\n');
                                    j += 1;
                                    break;
                                case 'f':
                                    keyDecoded.Append('\f');
                                    j += 1;
                                    break;
                                case 'u':
                                    j += 2;
                                    string strCharsChecked = @"\u";
                                    keyDecoded.Append(strCharsChecked);
                                    for (; i < parts.Count; key.Add(keyDecoded.ToString()), keyDecoded = new StringBuilder(), j = 0, i++)
                                    {
                                        for (; j < parts[i].Length; j++)
                                        {
                                            strCharsChecked += parts[i][j];
                                            keyDecoded.Append(parts[i][j]);
                                            if (strCharsChecked.Length == 6)
                                                break;
                                        }
                                        if (strCharsChecked.Length == 6)
                                            break;
                                    }
                                    if (strCharsChecked.Length < 6)
                                        throw new Exception("Invalid unicode escape " + strCharsChecked + " (unexpected end of stream)");
                                    if (!int.TryParse(strCharsChecked.Substring(2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out _))
                                        throw new Exception("Invalid unicode escape " + strCharsChecked + " (not hex digits)");
                                    break;
                                default:
                                    keyDecoded.Append(parts[i][j + 1]);
                                    j += 1;
                                    break;
                            }
                        }
                        else
                        {
                            keyDecoded.Append(parts[i][j]);
                        }
                    }
                    key.Add(keyDecoded.ToString());
                    if (foundCharAfterKey)
                        break;
                    j = 0;
                }
            }
            return key;
        }

        private static (List<string>, List<string>) GetAfterKeyAndKeyValueSeparator(List<string> parts, ref int i, ref int j)
        {
            var whitespace = GetWhitespace(parts, ref i,  ref j);
            if (i < parts.Count && j < parts[i].Length && (parts[i][j] == '=' || parts[i][j] == ':'))
            {
                List<string> keyValueSeparator = new List<string> { "" + parts[i][j] };
                j++;
                if (j == parts[i].Length)
                {
                    i++;
                    j = 0;
                }
                return (whitespace, keyValueSeparator);
            }
            return (new List<string> { "" }, whitespace);
        }

        private static List<string> GetValue(List<string> parts, ref int i, ref int j)
        {
            List<string> value = new List<string>();
            if (i == parts.Count || j == parts[i].Length)
            {
                value.Add("");
                return value;
            }
            for (; i < parts.Count; i++)
            {
                StringBuilder keyDecoded = new StringBuilder();
                for (; j < parts[i].Length; j++)
                {
                    if (parts[i][j] == '\\')
                    {
                        switch (parts[i][j + 1])
                        {
                            case 't':
                                keyDecoded.Append('\t');
                                j += 1;
                                break;
                            case 'r':
                                keyDecoded.Append('\r');
                                j += 1;
                                break;
                            case 'n':
                                keyDecoded.Append('\n');
                                j += 1;
                                break;
                            case 'f':
                                keyDecoded.Append('\f');
                                j += 1;
                                break;
                            case 'u':
                                j += 2;
                                string strCharsChecked = @"\u";
                                keyDecoded.Append(strCharsChecked);
                                for (; i < parts.Count; value.Add(keyDecoded.ToString()), keyDecoded = new StringBuilder(), j = 0, i++)
                                {
                                    for (; j < parts[i].Length; j++)
                                    {
                                        strCharsChecked += parts[i][j];
                                        keyDecoded.Append(parts[i][j]);
                                        if (strCharsChecked.Length == 6)
                                            break;
                                    }
                                    if (strCharsChecked.Length == 6)
                                        break;
                                }
                                if (strCharsChecked.Length < 6)
                                    throw new Exception("Invalid unicode escape " + strCharsChecked + " (unexpected end of stream)");
                                if (!int.TryParse(strCharsChecked.Substring(2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out _))
                                    throw new Exception("Invalid unicode escape " + strCharsChecked + " (not hex digits)");
                                break;
                            default:
                                keyDecoded.Append(parts[i][j + 1]);
                                j += 1;
                                break;
                        }
                    }
                    else
                    {
                        keyDecoded.Append(parts[i][j]);
                    }
                }
                value.Add(keyDecoded.ToString());
                j = 0;
            }
            return value;
        }


        public static Dictionary<string, string> ToDictionary(List<JavaPropertiesToken> entries, bool throwOnDuplicateKey = false)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            foreach (var kvp in entries.OfType<JavaPropertiesTokenKeyValuePair>())
            {
                string key = kvp.ParsedKey;
                string value = kvp.ParsedValue;
                if (throwOnDuplicateKey && ret.ContainsKey(key))
                    throw new Exception("Duplicate key " + key);
                ret[key] = value;
            }
            return ret;
        }

        public static List<JavaPropertiesToken> ParseFile(string path, Encoding encoding = null)
        {
            using var stream = new FileStream(path, FileMode.Open);
            return Parse(stream, encoding);
        }

        public static Dictionary<string, string> ParseFileToDictionary(string path, Encoding encoding = null, bool throwOnDuplicateKey = false)
        {
            using var stream = new FileStream(path, FileMode.Open);
            return ToDictionary(Parse(stream, encoding), throwOnDuplicateKey);
        }

        /// <summary>
        /// A backslash can be used to join multiple lines in a properties file (but not in comment lines).
        /// A logical line is a combination of consecutive lines joined in this way. This method returns the
        /// logical lines in the stream. It does not decode escape sequences and does not remove whitespace
        /// </summary>
        private static IEnumerable<LogicalLine> EnumerateLogicalLines(StreamReader sr)
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line == null)
                    yield break;
                int i = 0;


                while (i < line.Length && (line[i] == ' ' || line[i] == '\t' || line[i] == '\f'))
                    i++;

                if (i == line.Length)
                {
                    yield return new LogicalLine { Parts = new List<string> { line }, IsComment = false, IsBlank = true };
                    continue;
                }
                if (line[i] == '#' || line[i] == '!')
                {
                    yield return new LogicalLine { Parts = new List<string> { line }, IsComment = true, IsBlank = false };
                    continue;
                }

                bool oddNumberOfBackslashes = false;
                for (int j = i; j < line.Length; j++)
                    oddNumberOfBackslashes = line[j] == '\\' && !oddNumberOfBackslashes;

                if (oddNumberOfBackslashes)
                {
                    List<string> parts = new List<string> { line.Remove(line.Length - 1) };
                    while ((line = sr.ReadLine()) != null)
                    {
                        oddNumberOfBackslashes = false;
                        for (int j = 0; j < line.Length; j++)
                            oddNumberOfBackslashes = line[j] == '\\' && !oddNumberOfBackslashes;
                        if (oddNumberOfBackslashes)
                        {
                            parts.Add(line.Remove(line.Length - 1));
                        }
                        else
                        {
                            parts.Add(line);
                            break;
                        }
                    }
                    yield return new LogicalLine { Parts = parts, IsComment = false, IsBlank = false };
                }
                else
                {
                    yield return new LogicalLine { Parts = new List<string> { line }, IsComment = false, IsBlank = false };
                }
            }
        }

        private class LogicalLine
        {
            public List<string> Parts;
            public bool IsComment;
            public bool IsBlank;

            // override object.Equals
            public override bool Equals(object obj)
            {
                //       
                // See the full list of guidelines at
                //   http://go.microsoft.com/fwlink/?LinkID=85237  
                // and also the guidance for operator== at
                //   http://go.microsoft.com/fwlink/?LinkId=85238
                //

                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }

                var objc = obj as LogicalLine;
                return this.IsComment == objc.IsComment && this.IsBlank == objc.IsBlank && (this.Parts == objc.Parts || this.Parts.SequenceEqual(objc.Parts));
            }

            // override object.GetHashCode
            public override int GetHashCode()
            {
                // TODO: write your implementation of GetHashCode() here
                throw new NotImplementedException();
                return base.GetHashCode();
            }
        }
    }
}
