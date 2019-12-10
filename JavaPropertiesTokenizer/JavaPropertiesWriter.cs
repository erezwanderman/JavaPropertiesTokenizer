using System.Collections.Generic;
using System.IO;

namespace JavaPropertiesTokenizer
{
    public static class JavaPropertiesWriter
    {
        public static void Write(StreamWriter sw, IEnumerable<JavaPropertiesToken> tokens)
        {
            foreach (var token in tokens)
            {
                switch (token)
                {
                    case JavaPropertiesTokenWhitespaceLine x:
                        sw.WriteLine(x.Contents);
                        break;
                    case JavaPropertiesTokenCommentLine x:
                        sw.WriteLine(x.Contents);
                        break;
                    case JavaPropertiesTokenKeyValuePair x:
                        sw.WriteLine(
                            string.Join("\\\n", x.BeforeKey) +
                            string.Join("\\\n", x.Key) +
                            string.Join("\\\n", x.AfterKey) +
                            string.Join("\\\n", x.KeyValueSeparator) +
                            string.Join("\\\n", x.AfterKeyValueSeparator) +
                            string.Join("\\\n", x.Value)
                        );
                        break;
                }
            }
        }
    }
}
