// ReSharper disable CollectionNeverUpdated.Global

#pragma warning disable 649 // Unused fields

namespace Multicast {
    using System;
    using System.Collections.Generic;
    using DirtyDataEditor;
    using Numerics;
    using NUnit.Framework;

    public class DirtyDataEditorParserTests {
        [SetUp]
        public void SetUp() {
            DirtyDataParser.Errors.Clear();
            DirtyDataParser.Errors.Clear();
        }

        [Test]
        public void ParseJsonArray() {
            var result = DirtyDataParser.ParseList<DirtyDataEditorParserTestClass>(@"[
                {
                    boolProp:true, 
                    intProp:123,
                    longProp:12345678910,
                    floatProp: 123.456,
                    doubleProp: 123.456789,
                    stringProp: ""qwerty"",
                },
                {
                    boolProp:true, 
                    intProp:123,
                    longProp:12345678910,
                    floatProp: 123.456,
                    doubleProp: 123.456789,
                    stringProp: ""qwerty"",
                },
            ]");

            Assert.AreEqual(0, DirtyDataParser.Errors.Count);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result[0].BoolProp);
            Assert.AreEqual(123, result[0].IntProp);
            Assert.AreEqual(12345678910, result[0].LongProp);
            Assert.AreEqual(123.456f, result[0].FloatProp);
            Assert.AreEqual(123.456789, result[0].DoubleProp);
            Assert.AreEqual("qwerty", result[0].StringProp);
        }

        [Test]
        public void ParseJsonArrayWithAuto() {
            var result = DirtyDataParser.ParseList<DirtyDataEditorParserAutoTestClass>(@"[
                {
                    boolProp:true, 
                },
                {
                    floatProp: 123.456,
                },
            ]");

            Assert.AreEqual(0, DirtyDataParser.Errors.Count);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result[0].BoolProp);
            Assert.AreEqual("", result[1].StringProp);
            Assert.AreEqual(123.456, result[1].FloatProp, 0.01);
        }

        [Test]
        public void ParseYamlArray() {
            var result = DirtyDataParser.ParseList<DirtyDataEditorParserTestClass>(@"
---
boolProp:true
intProp:123
longProp:12345678910
floatProp: 123.456
doubleProp: 123.456789
stringProp: ""qwerty""
---
boolProp:true
intProp:123
longProp:12345678910
floatProp: 123.456
doubleProp: 123.456789
stringProp: ""qwerty""
            ");

            Assert.AreEqual(0, DirtyDataParser.Errors.Count);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result[0].BoolProp);
            Assert.AreEqual(123, result[0].IntProp);
            Assert.AreEqual(12345678910, result[0].LongProp);
            Assert.AreEqual(123.456f, result[0].FloatProp);
            Assert.AreEqual(123.456789, result[0].DoubleProp);
            Assert.AreEqual("qwerty", result[0].StringProp);
        }

        [Test]
        public void ParsePolymorphic() {
            var result = DirtyDataParser.Parse<DdeTestDrop>(@"
{
  type: ""Chest"",
  drops: [
    { type: ""Item"", item: ""CoolSword"" },
    { type: ""Item"", item: ""OldShield"" },
  ]
}
");

            Assert.AreEqual(0, DirtyDataParser.Errors.Count);

            Assert.IsTrue(result is DdeTestChestDrop);

            var outer = (DdeTestChestDrop) result;
            Assert.AreEqual(2, outer.Drops.Count);
            Assert.IsTrue(outer.Drops[0] is DdeTestItemDropClass);
            Assert.IsTrue(outer.Drops[1] is DdeTestItemDropClass);

            var inner1 = (DdeTestItemDropClass) outer.Drops[0];
            var inner2 = (DdeTestItemDropClass) outer.Drops[1];

            Assert.AreEqual("CoolSword", inner1.Item);
            Assert.AreEqual("OldShield", inner2.Item);
        }

        [Test]
        public void ValueParse() {
            // ReSharper disable RedundantTypeArgumentsOfMethod
            DoTest<bool>("true", true);
            DoTest<long>("123", 123);
            DoTest<int>("123", 123);
            DoTest<float>("123.4", 123.4f);
            DoTest<double>("123.4", 123.4);
            DoTest<string>("\"123abc\"", "123abc");
            DoTest<BigDouble>("123", BigDouble.Create(123));
            DoTest<BigDouble>("123.4", BigDouble.Create(123.4));
            DoTest<ProtectedBigDouble>("123", BigDouble.Create(123));
            DoTest<ProtectedBigDouble>("123.4", BigDouble.Create(123.4));
            DoTest<ProtectedInt>("123", 123);
            DoTest<GameTime>("\"2023-03-13 18:25\"", GameTime.FromUtcDateTime_UNSAFE(new DateTime(ticks: 638143287000000000)));
            // ReSharper restore RedundantTypeArgumentsOfMethod

            static void DoTest<T>(string input, T expectedResult) {
                var actualResult = DirtyDataParser.Parse<T>(input);

                Assert.AreEqual(expectedResult?.GetType(), actualResult?.GetType());
                Assert.AreEqual(expectedResult, actualResult);
            }
        }

        [Test]
        public void ListsParse() {
            // ReSharper disable RedundantTypeArgumentsOfMethod
            DoTest<bool>("[true, false]", new List<bool> {true, false});
            DoTest<long>("[123, 456]", new List<long> {123, 456});
            DoTest<int>("[123, 456]", new List<int> {123, 456});
            DoTest<float>("[123.4, 567.8]", new List<float> {123.4f, 567.8f});
            DoTest<double>("[123.4, 567.8]", new List<double> {123.4, 567.8});
            DoTest<string>("[\"abc\", \"123\"]", new List<string> {"abc", "123"});
            DoTest<ProtectedInt>("[123, 456]", new List<ProtectedInt> {123, 456});
            DoTest<ProtectedBigDouble>("[123.4, 567.8]", new List<ProtectedBigDouble> {123.4f, 567.8f});
            DoTest<BigDouble>("[123.4, 567.8]", new List<BigDouble> {123.4f, 567.8f});
            // ReSharper restore RedundantTypeArgumentsOfMethod

            static void DoTest<T>(string input, List<T> expectedResult) {
                var actualResult = DirtyDataParser.ParseList<T>(input);

                Assert.AreEqual(expectedResult?.GetType(), actualResult?.GetType());
                Assert.AreEqual(expectedResult, actualResult);
            }
        }

        [Test]
        public void NullablesParse() {
            DoTest<bool?>("true", true);
            DoTest<long?>("123", 123);
            DoTest<int?>("123", 123);
            DoTest<float?>("123.4", 123.4f);
            DoTest<double?>("123.4", 123.4);
            DoTest<BigDouble?>("123", BigDouble.Create(123));
            DoTest<BigDouble?>("123.4", BigDouble.Create(123.4));
            DoTest<ProtectedBigDouble?>("123", BigDouble.Create(123));
            DoTest<ProtectedBigDouble?>("123.4", BigDouble.Create(123.4));
            DoTest<ProtectedInt?>("123", 123);
            DoTest<GameTime?>("\"2023-03-13 18:25\"", GameTime.FromUtcDateTime_UNSAFE(new DateTime(ticks: 638143287000000000)));

            static void DoTest<T>(string input, T expectedResult) {
                var actualResult = DirtyDataParser.Parse<T>(input);

                Assert.AreEqual(expectedResult?.GetType(), actualResult?.GetType());
                Assert.AreEqual(expectedResult, actualResult);
            }
        }
    }

    [DDEObject]
    internal class DirtyDataEditorParserTestClass {
        [DDE("boolProp")]   public bool   BoolProp;
        [DDE("intProp")]    public int    IntProp;
        [DDE("longProp")]   public long   LongProp;
        [DDE("floatProp")]  public float  FloatProp;
        [DDE("doubleProp")] public double DoubleProp;
        [DDE("stringProp")] public string StringProp;
    }

    [DDEObject]
    internal class DirtyDataEditorParserAutoTestClass {
        [DDE("boolProp", false)] public bool   BoolProp;
        [DDE("intProp", 0)]      public int    IntProp;
        [DDE("longProp", 0)]     public long   LongProp;
        [DDE("floatProp", 0)]    public float  FloatProp;
        [DDE("doubleProp", 0)]   public double DoubleProp;
        [DDE("stringProp", "")]  public string StringProp;
    }

    [DDEObject, DDEBase("type")]
    internal class DdeTestDrop {
        [DDE("type")] public string Type;
    }

    [DDEObject, DDEImpl(typeof(DdeTestDrop), "Chest")]
    internal class DdeTestChestDrop : DdeTestDrop {
        [DDE("drops", null)] public List<DdeTestDrop> Drops;
    }

    [DDEObject, DDEImpl(typeof(DdeTestDrop), "Item")]
    internal class DdeTestItemDropClass : DdeTestDrop {
        [DDE("item")] public string Item;
    }
}