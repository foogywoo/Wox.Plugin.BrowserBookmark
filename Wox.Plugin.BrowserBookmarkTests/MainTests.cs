using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wox.Plugin.BrowserBookmark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wox.Plugin.BrowserBookmark.Tests
{
    [TestClass()]
    public class MainTests
    {
        [TestMethod()]
        public void FuzzyWordsMatchingCountTest()
        {
            string word = "test";
            string sentence = "This is a test sentence";

            int count = Wox.Plugin.BrowserBookmark.Main.FuzzyWordsMatchingCount(word, sentence);

            Assert.AreEqual(count, 1);
        }
    }
}