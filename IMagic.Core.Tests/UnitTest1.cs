using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.IO;

namespace IMagic.Core.Tests
{
    [TestClass]
    public class UnitTest1
    {
     


        [TestMethod]
        public void TestRandomUtil()
        {
            List<string> FakeMaleFirstNames = new List<string> { "Adam", "Adrian", "Alan", "Alexander", "Andrew", "Anthony", "Austin", "Benjamin", "Blake", "Boris", "Brandon", "Brian", "Cameron", "Carl", "Charles", "Christian", "Christopher", "Colin", "Connor", "Dan", "David", "Dominic", "Dylan", "Edward", "Eric", "Evan", "Frank", "Gavin", "Gordon", "Harry", "Ian", "Isaac", "Jack", "Jacob", "Jake", "James", "Jason", "Joe", "John", "Jonathan", "Joseph", "Joshua", "Julian", "Justin", "Keith", "Kevin", "Leonard", "Liam", "Lucas", "Luke", "Matt", "Max", "Michael", "Nathan", "Neil", "Nicholas", "Oliver", "Owen", "Paul", "Peter", "Phil", "Piers", "Richard", "Robert", "Ryan", "Sam", "Sean", "Sebastian", "Simon", "Stephen", "Steven", "Stewart", "Thomas", "Tim", "Trevor", "Victor", "Warren", "William" };
            List<string> randomOutput = new List<string>();
            for (int i = 0; i < FakeMaleFirstNames.Count; i++)
            {
                randomOutput.Add(FakeMaleFirstNames.RandomElement());
            }
            string output = string.Join(Environment.NewLine, randomOutput);

            Assert.IsTrue(randomOutput.Distinct().Count() > (randomOutput.Count / 2));//hoping for at least half the list to be distinct
        }

        [TestMethod]
        public void ReadLockedFile()
        {
            bool NoError = false;
            string BaseDirectoryBudgetLookup = @"D:\Dropbox\Presto\Research\Development\Lookups\PrestoGlobalRuleset.csv";//make sure this file exists and is open in excel which creates the lock
            try
            {
                string data = IOUtil.ReadTextFile(BaseDirectoryBudgetLookup);
                NoError = true;
            }
            catch { }

            Assert.IsTrue(NoError);
        }

        [TestMethod]
        public void Test_ToFirstLetterCapitalised()
        {
            string test1 = "apple";
            string expected1 = "Apple";
            string actual1 = test1.ToFirstLetterCapitalised();
            Assert.AreEqual(expected1, actual1);

            string test2 = "a";
            string expected2 = "A";
            string actual2 = test2.ToFirstLetterCapitalised();
            Assert.AreEqual(expected2, actual2);


            string test3 = "";
            string expected3 = "";
            string actual3 = test3.ToFirstLetterCapitalised();
            Assert.AreEqual(expected3, actual3);
        }
        [TestMethod]
        public void TestStringSplit()
        {
            string test = "cake|chocolate";
            List<string> expected = new List<string> { "cake", "chocolate" };
            List<string> actual = test.Split("|").ToList();
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void TestFileAppend()
        {
            string path = Path.Combine(Path.GetTempPath(), "test.txt");

            // ensure clean state
            if (File.Exists(path)) File.Delete(path);

            IOUtil.WriteTextFile(path, "line 1");
            IOUtil.WriteTextFile(path, "line 2", true);

            string contents = IOUtil.ReadTextFile(path);
            Assert.IsNotNull(contents);

            var lines = contents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(lines.Length >= 2);
            Assert.AreEqual("line 1", lines[0]);
            Assert.AreEqual("line 2", lines[1]);

            // cleanup
            try { File.Delete(path); } catch { }
        }


        [TestMethod]
        public void TestRandomise()
        {
            List<string> randomNames = FakeDataUtil.FirstNames_Male;
            List<string> randomOutput = randomNames.Randomise().ToList();
            Assert.IsTrue(randomNames.Count == randomOutput.Count);
            Assert.IsTrue(!randomNames.SequenceEqual(randomOutput));
        }



        [TestMethod]
        public void TestGetRandomString()
        {
            List<string> values = new List<string> { };
            for (int i = 0; i < 10; i++)
            {
                string x = RandomUtil.GetRandomString(20);
                values.Add(x);
            }
            Assert.IsTrue(values.Distinct().Count() == values.Count);
        }



        [TestMethod]
        public void TestRemove()
        {
            string input = "abcde";
            string expected = "acde";
            string actual = input.Remove("b");
            Assert.AreEqual(actual, expected);

            string actual2 = input.Remove("a", "bc");
            string expected2 = "de";
            Assert.AreEqual(actual2, expected2);
        }


        [TestMethod]
        public void TestRemoveAfter()
        {
            string input = "123456789";
            string expected = "789";
            string actual = input.RemoveAfter("6", true);
            Assert.AreEqual(actual, expected);

            string actual2 = input.RemoveAfter("6", false);
            string expected2 = "6789";
            Assert.AreEqual(actual2, expected2);
        }

        [TestMethod]
        public void TestRemoveBefore()
        {
            string input = "123456789";
            string expected = "12345";
            string actual = input.RemoveBefore("6", true);
            Assert.AreEqual(actual, expected);

            string actual2 = input.RemoveBefore("6", false);
            string expected2 = "123456";
            Assert.AreEqual(actual2, expected2);
        }

        [TestMethod]
        public void TestRemoveBetween()
        {
            string input = "123456789";
            string expected = "12349";
            string actual = input.RemoveBetween("5", "8", true);
            Assert.AreEqual(actual, expected);

            string actual2 = input.RemoveBetween("5", "8", false);
            string expected2 = "1234589";
            Assert.AreEqual(actual2, expected2);
        }

        [TestMethod]
        public void TestReplaceBetween()
        {
            string input = "123456789";
            string expected = "1234ABC9";
            string actual = input.ReplaceBetween("5", "8", "ABC", true);
            Assert.AreEqual(actual, expected);

            string actual2 = input.ReplaceBetween("5", "8", "ABC", false);
            string expected2 = "12345ABC89";
            Assert.AreEqual(actual2, expected2);
        }

        [TestMethod]
        public void Test_ToShortDateStringOrShortTimeString()
        {
            DateTime input = DateTime.UtcNow;
            string expected = DateTime.UtcNow.ToShortTimeString();
            string actual = input.ToShortDateStringOrShortTimeString();
            Assert.AreEqual(actual, expected);

            DateTime inputPast = DateTime.UtcNow.AddDays(-5);
            string expected2 = inputPast.ToShortDateString();
            string actual2 = inputPast.ToShortDateStringOrShortTimeString();
            Assert.AreEqual(actual2, expected2);

            DateTime inputFuture = DateTime.UtcNow.AddDays(5);
            string expected3 = inputFuture.ToShortDateString();
            string actual3 = inputFuture.ToShortDateStringOrShortTimeString();
            Assert.AreEqual(actual3, expected3);
        }

        [TestMethod]
        public void Test_StringCompareIgnoreCase()
        {
            string input = "abcDEFghi";
            Assert.IsFalse(input.Contains("def"));
            Assert.IsTrue(input.Contains("def", true));

        }
    }
}
