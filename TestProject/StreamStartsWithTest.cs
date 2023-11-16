using Game1;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace TestProject
{
    [TestClass]
    public class StreamStartsWithTest
    {
        [TestMethod]
        public void StreamTooShortReturnsFalse()
        {
            using StreamReader streamReader = new(StreamFromString("foo"));
            Assert.AreEqual
            (
                expected: false,
                actual: Algorithms.StreamStartsWith
                (
                    streamReader: streamReader,
                    tokens: ["foo", "bar"]
                )
            );
        }

        [TestMethod]
        public void StreamWithMuchWhitespace()
        {
            using StreamReader streamReader = new(StreamFromString("\tfoo\r\n\nbar baz"));
            Assert.AreEqual
            (
                expected: true,
                actual: Algorithms.StreamStartsWith
                (
                    streamReader: streamReader,
                    tokens: ["foo", "bar", "baz"]
                )
            );
        }

        [TestMethod]
        public void StreamWithNoWhitespace()
        {
            using StreamReader streamReader = new(StreamFromString("foobarbaz"));
            Assert.AreEqual
            (
                expected: true,
                actual: Algorithms.StreamStartsWith
                (
                    streamReader: streamReader,
                    tokens: ["foo", "bar", "baz"]
                )
            );
        }

        [TestMethod]
        public void StreamWithSpecialCharacters()
        {
            using StreamReader streamReader = new(StreamFromString("\"foo\" \'bar\' \\baz\\"));
            Assert.AreEqual
            (
                expected: true,
                actual: Algorithms.StreamStartsWith
                (
                    streamReader: streamReader,
                    tokens: ["\"foo\"", "\'bar\'", "\\baz\\"]
                )
            );
        }

        [TestMethod]
        public void StreamProceedsDifferentlyReturnsFalseWithoutReadingTooMuch()
        {
            using StreamReader streamReader = new(StreamFromString("foo babaz"));
            Assert.AreEqual
            (
                expected: false,
                actual: Algorithms.StreamStartsWith
                (
                    streamReader: streamReader,
                    tokens: ["foo", "bar", "baz"]
                )
            );
            Assert.AreEqual
            (
                streamReader.ReadToEnd(),
                "az"
            );
        }

        private static Stream StreamFromString(string value)
            => new MemoryStream(Encoding.UTF8.GetBytes(value));
    }
}
