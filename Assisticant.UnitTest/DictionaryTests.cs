using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assisticant.Collections;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using Assisticant.UnitTest.CollectionData;

namespace Assisticant.UnitTest
{
    [TestClass]
    public class DictionaryTests
    {

        [TestMethod]
        public void VerifyNonGenericIDictionaryIndexOperator()
        {
            ObservableDictionary<string,string> genericDictionary = new ObservableDictionary<string, string>();
            IDictionary testDictionary = genericDictionary;
            
            testDictionary["stringKey1"] = "TestString";
            Assert.AreEqual("TestString", testDictionary["stringKey1"]);
        }

        [TestMethod]
        public void VerifyNonGenericIDictionaryGetEnumerator()
        {
            ObservableDictionary<string,string> genericDictionary = new ObservableDictionary<string, string>();
            IDictionary testDictionary = genericDictionary;

            testDictionary.Add("stringKey1", "TestString");

            foreach (KeyValuePair<string,string> item in testDictionary)
            {
                Assert.AreEqual("stringKey1", item.Key);
                Assert.AreEqual("TestString", item.Value);
            }
        }

        [TestMethod]
        public void VerifyNonGenericIDictionaryContainsMethod()
        {
            ObservableDictionary<string,string> genericDictionary = new ObservableDictionary<string, string>();
            IDictionary testDictionary = genericDictionary;

            testDictionary.Add("stringKey1", "TestString");
            Assert.IsTrue(testDictionary.Contains("stringKey1"));
        }

        [TestMethod]
        public void VerifyNonGenericIDictionaryAddMethod()
        {
            ObservableDictionary<string,string> genericDictionary = new ObservableDictionary<string, string>();
            IDictionary testDictionary = genericDictionary;

            testDictionary.Add("stringKey1", "TestString");
            Assert.AreEqual("TestString", testDictionary["stringKey1"]);

        }

        [TestMethod]
        public void VerifyNonGenericIDictionaryRemoveMethod()
        {
            ObservableDictionary<string,string> genericDictionary = new ObservableDictionary<string, string>();
            IDictionary testDictionary = genericDictionary;

            testDictionary.Add("stringKey1", "TestString");
            Assert.IsTrue(testDictionary.Contains("stringKey1"));

            testDictionary.Remove("stringKey1");
            Assert.IsFalse(testDictionary.Contains("stringKey1"));
        }

        [TestMethod]
        public void VerifyNonGenericIDictionaryIsFixedSizeProperty()
        {
            ObservableDictionary<string,string> genericDictionary = new ObservableDictionary<string, string>();
            IDictionary testDictionary = genericDictionary;

            Assert.IsFalse(testDictionary.IsFixedSize);
        }

        [TestMethod]
        public void VerifyNonGenericIDictionaryKeysProperty()
        {
            ObservableDictionary<string,string> genericDictionary = new ObservableDictionary<string, string>();
            IDictionary testDictionary = genericDictionary;

            testDictionary.Add("stringKey1", "TestString");

            ICollection keyCollection = testDictionary.Keys;
            foreach (var nextKey in keyCollection)
            {
                Assert.AreEqual("stringKey1", nextKey);
            }
        }

        [TestMethod]
        public void VerifyNonGenericIDictionaryValuesProperty()
        {
            ObservableDictionary<string,string> genericDictionary = new ObservableDictionary<string, string>();
            IDictionary testDictionary = genericDictionary;

            testDictionary.Add("stringKey1", "TestString");

            ICollection keyCollection = testDictionary.Values;
            foreach (var nextValue in keyCollection)
            {
                Assert.AreEqual("TestString", nextValue);
            }
        }

        [TestMethod]
        public void VerifyNonGenericIDictionaryCopyToMethod()
        {
            ObservableDictionary<string,string> genericDictionary = new ObservableDictionary<string, string>();
            IDictionary testDictionary = genericDictionary;
            
            testDictionary.Add("stringKey1", "TestString");

            Array array = new KeyValuePair<string, string>[5].ToArray();
            testDictionary.CopyTo(array, 0);

            Assert.AreEqual("stringKey1", ((KeyValuePair<string, string>) array.GetValue(0)).Key);
            Assert.AreEqual("TestString", ((KeyValuePair<string, string>) array.GetValue(0)).Value);
            Assert.IsNull(((KeyValuePair<string, string>) array.GetValue(1)).Key);
            Assert.IsNull(((KeyValuePair<string, string>) array.GetValue(1)).Value);

            array = new KeyValuePair<string, string>[5].ToArray();
            testDictionary.CopyTo(array, 1);

            Assert.AreEqual("stringKey1", ((KeyValuePair<string, string>) array.GetValue(1)).Key);
            Assert.AreEqual("TestString", ((KeyValuePair<string, string>) array.GetValue(1)).Value);
            Assert.IsNull(((KeyValuePair<string, string>) array.GetValue(0)).Key);
            Assert.IsNull(((KeyValuePair<string, string>) array.GetValue(0)).Value);
            Assert.IsNull(((KeyValuePair<string, string>) array.GetValue(2)).Key);
            Assert.IsNull(((KeyValuePair<string, string>) array.GetValue(2)).Value);

        }

    }
}
