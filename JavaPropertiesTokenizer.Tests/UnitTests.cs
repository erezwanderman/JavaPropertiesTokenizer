using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JavaPropertiesTokenizer.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestGetLogicalLinesEmpty()
        {
            Assert.AreEqual(
                RunEnumerateLogicalLines(""),
                CreateListLogicalLine(new List<(bool isBlank, bool isComment, List<string> parts)>
                {
                }));
        }

        [Test]
        public void TestReadLineSimple()
        {
            Assert.AreEqual(
                RunEnumerateLogicalLines("ABC"),
                CreateListLogicalLine(new List<(bool isBlank, bool isComment, List<string> parts)>
                {
                    (isBlank: false, isComment: false, parts: new List<string>{ "ABC" } ),
                }));
        }

        [Test]
        public void TestReadLineMultiLine()
        {
            Assert.AreEqual(
                RunEnumerateLogicalLines(@"ABC
DEF"),
                CreateListLogicalLine(new List<(bool isBlank, bool isComment, List<string> parts)>
                {
                    (isBlank: false, isComment: false, parts: new List<string>{ "ABC" } ),
                    (isBlank: false, isComment: false, parts: new List<string>{ "DEF" } ),
                }));
        }

        [Test]
        public void TestReadLineMultiLineWithEmpties()
        {
            Assert.AreEqual(
                RunEnumerateLogicalLines(@"
ABC


DEF"),
                CreateListLogicalLine(new List<(bool isBlank, bool isComment, List<string> parts)>
                {
                    (isBlank: true, isComment: false, parts: new List<string>{ "" } ),
                    (isBlank: false, isComment: false, parts: new List<string>{ "ABC" } ),
                    (isBlank: true, isComment: false, parts: new List<string>{ "" } ),
                    (isBlank: true, isComment: false, parts: new List<string>{ "" } ),
                    (isBlank: false, isComment: false, parts: new List<string>{ "DEF" } ),
                }));
        }

        [Test]
        public void TestReadLineCombine()
        {
            Assert.AreEqual(
                RunEnumerateLogicalLines(@"ABC\
DEF"),
                CreateListLogicalLine(new List<(bool isBlank, bool isComment, List<string> parts)>
                {
                    (isBlank: false, isComment: false, parts: new List<string>{ "ABC", "DEF" } ),
                }));
        }

        [Test]
        public void TestReadLineCommentLinesCannotBeCombined1()
        {
            Assert.AreEqual(
                RunEnumerateLogicalLines(@"#ABC\
DEF"),
                CreateListLogicalLine(new List<(bool isBlank, bool isComment, List<string> parts)>
                {
                    (isBlank: false, isComment: true, parts: new List<string>{ "#ABC\\" } ),
                    (isBlank: false, isComment: false, parts: new List<string>{ "DEF" } ),
                }));
        }

        [Test]
        public void TestReadLineCommentLinesCannotBeCombined2()
        {
            Assert.AreEqual(
                RunEnumerateLogicalLines(@" #ABC\
DEF"),
                CreateListLogicalLine(new List<(bool isBlank, bool isComment, List<string> parts)>
                {
                    (isBlank: false, isComment: true, parts: new List<string>{ " #ABC\\" } ),
                    (isBlank: false, isComment: false, parts: new List<string>{ "DEF" } ),
                }));
        }

        [Test]
        public void TestReadLineCommentLinesCannotBeCombined3()
        {
            Assert.AreEqual(
                RunEnumerateLogicalLines(@"# ABC\
DEF"),
                CreateListLogicalLine(new List<(bool isBlank, bool isComment, List<string> parts)>
                {
                    (isBlank: false, isComment: true, parts: new List<string>{ "# ABC\\" } ),
                    (isBlank: false, isComment: false, parts: new List<string>{ "DEF" } ),
                }));
        }

        [Test]
        public void TestReadLineCommentLinesCannotBeCombined4()
        {
            Assert.AreEqual(
                RunEnumerateLogicalLines(@"!ABC\
DEF"),
                CreateListLogicalLine(new List<(bool isBlank, bool isComment, List<string> parts)>
                {
                    (isBlank: false, isComment: true, parts: new List<string>{ "!ABC\\" } ),
                    (isBlank: false, isComment: false, parts: new List<string>{ "DEF" } ),
                }));
        }

        [Test]
        public void TestReadLineCommentLinesCannotBeCombined5()
        {
            Assert.AreEqual(
                RunEnumerateLogicalLines(@" !ABC\
DEF"),
                CreateListLogicalLine(new List<(bool isBlank, bool isComment, List<string> parts)>
                {
                    (isBlank: false, isComment: true, parts: new List<string>{ " !ABC\\" } ),
                    (isBlank: false, isComment: false, parts: new List<string>{ "DEF" } ),
                }));
        }

        [Test]
        public void TestReadLineCommentLinesCannotBeCombined6()
        {
            Assert.AreEqual(
                RunEnumerateLogicalLines(@"! ABC\
DEF"),
                CreateListLogicalLine(new List<(bool isBlank, bool isComment, List<string> parts)>
                {
                    (isBlank: false, isComment: true, parts: new List<string>{ "! ABC\\" } ),
                    (isBlank: false, isComment: false, parts: new List<string>{ "DEF" } ),
                }));
        }

        [Test]
        public void TestReadLinePreserveWhitespace1()
        {
            Assert.AreEqual(
                RunEnumerateLogicalLines(" \t\f"),
                CreateListLogicalLine(new List<(bool isBlank, bool isComment, List<string> parts)>
                {
                    (isBlank: true, isComment: false, parts: new List<string>{ " \t\f" } ),
                }));
        }

        [Test]
        public void TestReadLinePreserveWhitespace2()
        {
            Assert.AreEqual(
                RunEnumerateLogicalLines(" \t\f # \t\t gaga\n  \t  AB = DE"),
                CreateListLogicalLine(new List<(bool isBlank, bool isComment, List<string> parts)>
                {
                    (isBlank: false, isComment: true , parts: new List<string>{ " \t\f # \t\t gaga" } ),
                    (isBlank: false, isComment: false , parts: new List<string>{ "  \t  AB = DE" } ),
                }));
        }

        [Test]
        public void TestReadLinePreserveWhitespace3()
        {
            Assert.AreEqual(
                RunEnumerateLogicalLines(" \t\f # \t\t gaga\n  \t  AB = DE   \\\n"),
                CreateListLogicalLine(new List<(bool isBlank, bool isComment, List<string> parts)>
                {
                    (isBlank: false, isComment: true , parts: new List<string>{ " \t\f # \t\t gaga" } ),
                    (isBlank: false, isComment: false , parts: new List<string>{ "  \t  AB = DE   " } ),
                }));
        }

        [Test]
        public void TestReadLinePreserveWhitespace4()
        {
            Assert.AreEqual(
                RunEnumerateLogicalLines(" \t\f # \t\t gaga\n  \t  AB = DE   \\\n\\\n"),
                CreateListLogicalLine(new List<(bool isBlank, bool isComment, List<string> parts)>
                {
                    (isBlank: false, isComment: true , parts: new List<string>{ " \t\f # \t\t gaga" } ),
                    (isBlank: false, isComment: false , parts: new List<string>{ "  \t  AB = DE   ", "" } ),
                }));
        }

        [Test]
        public void TestReadLinePreserveWhitespace5()
        {
            Assert.AreEqual(
                RunEnumerateLogicalLines(" \t\f # \t\t gaga\n  \t  AB = DE   \\"),
                CreateListLogicalLine(new List<(bool isBlank, bool isComment, List<string> parts)>
                {
                    (isBlank: false, isComment: true , parts: new List<string>{ " \t\f # \t\t gaga" } ),
                    (isBlank: false, isComment: false , parts: new List<string>{ "  \t  AB = DE   " } ),
                }));
        }

        [Test]
        public void TestReadLinePreserveWhitespace6()
        {
            Assert.AreEqual(
                RunEnumerateLogicalLines(" \t\f # \t\t gaga\n  \t  AB = DE   \\\n h  =  d "),
                CreateListLogicalLine(new List<(bool isBlank, bool isComment, List<string> parts)>
                {
                    (isBlank: false, isComment: true , parts: new List<string>{ " \t\f # \t\t gaga" } ),
                    (isBlank: false, isComment: false , parts: new List<string>{ "  \t  AB = DE   ", " h  =  d " } ),
                }));
        }

        private object RunEnumerateLogicalLines(string str)
        {
            using var testStream = new MemoryStream(Encoding.UTF8.GetBytes(str));
            using var testStreamReader = new StreamReader(testStream);
            var enumeratorLogicalLines = typeof(JavaPropertiesParser).GetMethod("EnumerateLogicalLines", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).Invoke(null, new object[] { testStreamReader });

            var typeLogicalLine = typeof(JavaPropertiesParser).GetNestedType("LogicalLine", System.Reflection.BindingFlags.NonPublic);
            var typeIEnumerableLogicalLine = typeof(IEnumerable<>).MakeGenericType(typeLogicalLine);

            var meth = typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(typeLogicalLine);
            var listLogicalLines = meth.Invoke(enumeratorLogicalLines, new object[] { enumeratorLogicalLines });
            return listLogicalLines;
        }

        private object CreateListLogicalLine(List<(bool isBlank, bool isComment, List<string> parts)> logicalLines)
        {
            var typeLogicalLine = typeof(JavaPropertiesParser).GetNestedType("LogicalLine", System.Reflection.BindingFlags.NonPublic);
            var typeListLogicalLine = typeof(List<>).MakeGenericType(typeLogicalLine);

            var list = (System.Collections.IList)Activator.CreateInstance(typeListLogicalLine);
            var fieldIsBlank = typeLogicalLine.GetField("IsBlank");
            var fieldIsComment = typeLogicalLine.GetField("IsComment");
            var fieldParts = typeLogicalLine.GetField("Parts");
            foreach (var logicalLine in logicalLines)
            {
                var logicalLineObj = Activator.CreateInstance(typeLogicalLine);
                fieldIsBlank.SetValue(logicalLineObj, logicalLine.isBlank);
                fieldIsComment.SetValue(logicalLineObj, logicalLine.isComment);
                fieldParts.SetValue(logicalLineObj, logicalLine.parts);
                list.Add(logicalLineObj);
            }
            return list;
        }


        private static List<string> ListOfEmptyString = new List<string> { "" };

        [Test]
        public void TestParsing()
        {
            var r = RunParse(@"ABC
A=B
 A=B
A =B
A= B
A=B 
ABC\
A=B
ABC \
A=B
ABC\ 
A=B

ABC
A:B
 A:B
A :B
A: B
A:B 
ABC\
A:B
ABC \
A=B
ABC\ 
A:B

k\t\n\f=e\t\n\f
\t\n\fk=\t\n\fe
\a\b\cdef\g\h
ZAG\u1234def=g
abcdef=\u1234g\u2345
a b c
a b c 
#I AM Comment
  #  SO AM I  
# NEXT LINES AREN'T

 	
!BLABLA
TestNewlineIn\u00\
8\
\
0\
UnicodeEscape=InValue\u00\
80
# \u12 Comment lines do not use escape sequences
");
            Assert.AreEqual(new List<JavaPropertiesToken> {
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "ABC" }, ListOfEmptyString, ListOfEmptyString, ListOfEmptyString, ListOfEmptyString),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "A" }, ListOfEmptyString, new List<string>{ "=" }, ListOfEmptyString, new List<string>{ "B" }),
                new JavaPropertiesTokenKeyValuePair(new List<string>{ " " }, new List<string>{ "A" }, ListOfEmptyString, new List<string>{ "=" }, ListOfEmptyString, new List<string>{ "B" }),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "A" }, new List<string>{ " " }, new List<string>{ "=" }, ListOfEmptyString, new List<string>{ "B" }),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "A" }, ListOfEmptyString, new List<string>{ "=" }, new List<string>{ " " }, new List<string>{ "B" }),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "A" }, ListOfEmptyString, new List<string>{ "=" }, ListOfEmptyString, new List<string>{ "B " }),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "ABC", "A" }, ListOfEmptyString, new List<string>{ "=" }, ListOfEmptyString, new List<string>{ "B" }),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "ABC" }, ListOfEmptyString, new List<string>{ " ", "" }, ListOfEmptyString, new List<string>{ "A=B" }),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "ABC " }, ListOfEmptyString, ListOfEmptyString, ListOfEmptyString, ListOfEmptyString),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "A" }, ListOfEmptyString, new List<string>{ "=" }, ListOfEmptyString, new List<string>{ "B" }),
                new JavaPropertiesTokenWhitespaceLine(""),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "ABC" }, ListOfEmptyString, ListOfEmptyString, ListOfEmptyString, ListOfEmptyString),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "A" }, ListOfEmptyString, new List<string>{ ":" }, ListOfEmptyString, new List<string>{ "B" }),
                new JavaPropertiesTokenKeyValuePair(new List<string>{ " " }, new List<string>{ "A" }, ListOfEmptyString, new List<string>{ ":" }, ListOfEmptyString, new List<string>{ "B" }),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "A" }, new List<string>{ " " }, new List<string>{ ":" }, ListOfEmptyString, new List<string>{ "B" }),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "A" }, ListOfEmptyString, new List<string>{ ":" }, new List<string>{ " " }, new List<string>{ "B" }),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "A" }, ListOfEmptyString, new List<string>{ ":" }, ListOfEmptyString, new List<string>{ "B " }/*, new List<string>{ " " }*/),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "ABC", "A" }, ListOfEmptyString, new List<string>{ ":" }, ListOfEmptyString, new List<string>{ "B" }),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "ABC" }, ListOfEmptyString, new List<string>{ " ", "" }, ListOfEmptyString, new List<string>{ "A=B" }),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "ABC " }, ListOfEmptyString, ListOfEmptyString, ListOfEmptyString, ListOfEmptyString),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "A" }, ListOfEmptyString, new List<string>{ ":" }, ListOfEmptyString, new List<string>{ "B" }),
                new JavaPropertiesTokenWhitespaceLine(""),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "k\t\n\f" }, ListOfEmptyString, new List<string>{ "=" }, ListOfEmptyString, new List<string>{ "e\t\n\f" }),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "\t\n\fk" }, ListOfEmptyString, new List<string>{ "=" }, ListOfEmptyString, new List<string>{ "\t\n\fe" }),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "abcdefgh" }, ListOfEmptyString, ListOfEmptyString, ListOfEmptyString, ListOfEmptyString),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ @"ZAG\u1234def" }, ListOfEmptyString, new List<string>{ "=" }, ListOfEmptyString, new List<string>{ "g" }),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "abcdef" }, ListOfEmptyString, new List<string>{ "=" }, ListOfEmptyString, new List<string>{ @"\u1234g\u2345" }),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "a" }, ListOfEmptyString, new List<string>{ " " }, ListOfEmptyString, new List<string>{ "b c" }),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ "a" }, ListOfEmptyString, new List<string>{ " " }, ListOfEmptyString, new List<string>{ "b c " }),
                new JavaPropertiesTokenCommentLine("#I AM Comment"),
                new JavaPropertiesTokenCommentLine("  #  SO AM I  "),
                new JavaPropertiesTokenCommentLine("# NEXT LINES AREN'T"),
                new JavaPropertiesTokenWhitespaceLine(""),
                new JavaPropertiesTokenWhitespaceLine(" \t"),
                new JavaPropertiesTokenCommentLine("!BLABLA"),
                new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string>{ @"TestNewlineIn\u00", @"8", @"", @"0", @"UnicodeEscape" }, ListOfEmptyString, new List<string>{ "=" }, ListOfEmptyString, new List<string>{ @"InValue\u00", @"80" }),
                new JavaPropertiesTokenCommentLine("# \\u12 Comment lines do not use escape sequences"),
            }, r);

            Assert.Throws<Exception>(() => RunParse("\\u123Z"));
            Assert.Throws<Exception>(() => RunParse("\\u123"));
            Assert.Throws<Exception>(() => RunParse("\\u12\\\n3"));
        }

        [Test]
        public void TestToDictionary()
        {
            var r = RunParse(@"A=B
CC=DD
#COMMENT
CC=DDD
E\
E\
=FF\
F
ESC=\b\t\r\n\f\u1234\u\
1\
2\
\
3\
4");
            Assert.Throws<Exception>(() => JavaPropertiesParser.ToDictionary(r, true));
            Assert.DoesNotThrow(() => JavaPropertiesParser.ToDictionary(new List<JavaPropertiesToken> { new JavaPropertiesTokenKeyValuePair(ListOfEmptyString, new List<string> { "A" }, ListOfEmptyString, new List<string> { "=" }, ListOfEmptyString, new List<string> { "B" }) }, true));
            var dict = JavaPropertiesParser.ToDictionary(r, false);
            Assert.AreEqual(dict.Count, 4);
            Assert.AreEqual(dict["A"], "B");
            Assert.AreEqual(dict["CC"], "DDD");
            Assert.AreEqual(dict["EE"], "FFF");
            Assert.AreEqual(dict["ESC"], "b\t\r\n\f\u1234\u1234");
        }

        private List<JavaPropertiesToken> RunParse(string str)
        {
            using var testStream = new MemoryStream(JavaPropertiesParser.DefaultEncoding.GetBytes(str));
            return JavaPropertiesParser.Parse(testStream);
        }
    }
}