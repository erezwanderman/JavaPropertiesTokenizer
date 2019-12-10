using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JavaPropertiesTokenizer
{
    public abstract class JavaPropertiesToken
    {
    }
    public class JavaPropertiesTokenWhitespaceLine : JavaPropertiesToken
    {
        public readonly string Contents;
        public JavaPropertiesTokenWhitespaceLine(string contents) => Contents = contents;

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return this == obj as JavaPropertiesTokenWhitespaceLine;
        }

        public override int GetHashCode() => 0;

        public static bool operator ==(JavaPropertiesTokenWhitespaceLine lhs, JavaPropertiesTokenWhitespaceLine rhs)
        {
            return
                lhs.Contents == rhs.Contents;
        }

        public static bool operator !=(JavaPropertiesTokenWhitespaceLine lhs, JavaPropertiesTokenWhitespaceLine rhs)
        {
            return !(lhs == rhs);
        }
    }
    public class JavaPropertiesTokenCommentLine : JavaPropertiesToken
    {
        public readonly string Contents;
        public JavaPropertiesTokenCommentLine(string contents) => Contents = contents;

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return this == obj as JavaPropertiesTokenCommentLine;
        }

        public override int GetHashCode() => 0;

        public static bool operator ==(JavaPropertiesTokenCommentLine lhs, JavaPropertiesTokenCommentLine rhs)
        {
            return
                lhs.Contents == rhs.Contents;
        }

        public static bool operator !=(JavaPropertiesTokenCommentLine lhs, JavaPropertiesTokenCommentLine rhs)
        {
            return !(lhs == rhs);
        }
    }
    public class JavaPropertiesTokenKeyValuePair : JavaPropertiesToken
    {
        public readonly List<string> BeforeKey;
        public readonly List<string> Key;
        public readonly List<string> AfterKey;
        public readonly List<string> KeyValueSeparator;
        public readonly List<string> AfterKeyValueSeparator;
        public readonly List<string> Value;
        public JavaPropertiesTokenKeyValuePair(
            List<string> beforeKey,
            List<string> key,
            List<string> afterKey,
            List<string> keyValueSeparator,
            List<string> afterKeyValueSeparator,
            List<string> value
            )
        {
            BeforeKey = beforeKey;
            Key = key;
            AfterKey = afterKey;
            KeyValueSeparator = keyValueSeparator;
            AfterKeyValueSeparator = afterKeyValueSeparator;
            Value = value;
            ParsedKey = ParseRawString(key);
            ParsedValue = ParseRawString(value);
        }

        public string ParsedKey { get; }
        public string ParsedValue { get; }

        private string ParseRawString(List<string> raw)
        {
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < raw.Count; i++)
            {
                for (int j = 0; j < raw[i].Length; j++)
                {
                    if (raw[i][j] != '\\')
                    {
                        ret.Append(raw[i][j]);
                    }
                    else
                    {
                        j++;
                        /*while (j >= raw[i].Length)
                        {
                            i++;
                            j = 0;
                        }*/
                        switch (raw[i][j])
                        {
                            case 't':
                                ret.Append('\t');
                                break;
                            case 'n':
                                ret.Append('\n');
                                break;
                            case 'f':
                                ret.Append('\f');
                                break;
                            case 'r':
                                ret.Append('\r');
                                break;
                            case '\\':
                                ret.Append('\\');
                                break;
                            case 'u':
                                string hex = "";
                                while (hex.Length < 4)
                                {
                                    j++;
                                    while (j == raw[i].Length)
                                    {
                                        i++;
                                        j = 0;
                                    }
                                    hex += raw[i][j];
                                }
                                ret.Append((char)Int16.Parse(hex, System.Globalization.NumberStyles.AllowHexSpecifier));
                                break;
                            default:
                                ret.Append(raw[i][j]);
                                break;
                        }
                    }
                }
            }
            return ret.ToString();
        }

        public override string ToString()
        {
            return
                string.Join("\\\n", BeforeKey) +
                string.Join("\\\n", Key) +
                string.Join("\\\n", AfterKey) +
                string.Join("\\\n", KeyValueSeparator) +
                string.Join("\\\n", AfterKeyValueSeparator) +
                string.Join("\\\n", Value);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return this == obj as JavaPropertiesTokenKeyValuePair;
        }

        public override int GetHashCode() => 0;

        public static bool operator ==(JavaPropertiesTokenKeyValuePair lhs, JavaPropertiesTokenKeyValuePair rhs)
        {
            return
                (lhs.BeforeKey == rhs.BeforeKey || lhs.BeforeKey.SequenceEqual(rhs.BeforeKey)) &&
                (lhs.Key == rhs.Key || lhs.Key.SequenceEqual(rhs.Key)) &&
                (lhs.AfterKey == rhs.AfterKey || lhs.AfterKey.SequenceEqual(rhs.AfterKey)) &&
                (lhs.KeyValueSeparator == rhs.KeyValueSeparator || lhs.KeyValueSeparator.SequenceEqual(rhs.KeyValueSeparator)) &&
                (lhs.AfterKeyValueSeparator == rhs.AfterKeyValueSeparator || lhs.AfterKeyValueSeparator.SequenceEqual(rhs.AfterKeyValueSeparator)) &&
                (lhs.Value == rhs.Value || lhs.Value.SequenceEqual(rhs.Value));
        }

        public static bool operator !=(JavaPropertiesTokenKeyValuePair lhs, JavaPropertiesTokenKeyValuePair rhs)
        {
            return !(lhs == rhs);
        }
    }
}
