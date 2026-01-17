namespace Multicast {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DirtyDataEditor;
    using NUnit.Framework;
    using UnityEngine;

    public class BoolTests {
        [Test]
        public void ParseBool() {
            DirtyDataParser.Errors.Clear();

            var text = @"---
bool_true: true
bool_false: false
bool_optional_true: true
bool_optional_false: false
bool_list: [ true, false ]
bool_optional_list_empty: [ ]
bool_optional_list_values: [ false, true ]
";

            var obj = DirtyDataParser.ParseList<DirtyDataBoolTestObject>(text).Single();

            foreach (var error in DirtyDataParser.Errors) {
                Debug.LogError(error);
            }

            Assert.AreEqual(true, obj.boolTrue);
            Assert.AreEqual(false, obj.boolFalse);

            Assert.AreEqual(true, obj.boolOptionalTrue);
            Assert.AreEqual(false, obj.boolOptionalFalse);

            Assert.AreEqual(null, obj.boolOptionalNone);

            Assert.AreEqual(2, obj.boolList.Count);
            Assert.AreEqual(true, obj.boolList[0]);
            Assert.AreEqual(false, obj.boolList[1]);

            Assert.AreEqual(0, obj.boolOptionalListNone.Count);
            Assert.AreEqual(0, obj.boolOptionalListEmpty.Count);

            Assert.AreEqual(2, obj.boolOptionalListValues.Count);
            Assert.AreEqual(false, obj.boolOptionalListValues[0]);
            Assert.AreEqual(true, obj.boolOptionalListValues[1]);
        }
    }

    [DDEObject, Serializable]
    public class DirtyDataBoolTestObject {
        [DDE("bool_true")]  public bool boolTrue;
        [DDE("bool_false")] public bool boolFalse;

        [DDE("bool_optional_true")]  public bool? boolOptionalTrue;
        [DDE("bool_optional_false")] public bool? boolOptionalFalse;

        [DDE("bool_optional_none", null)] public bool? boolOptionalNone;

        [DDE("bool_list")]                            public List<bool> boolList;
        [DDE("bool_optional_list_none", DDE.Empty)]   public List<bool> boolOptionalListNone;
        [DDE("bool_optional_list_empty", DDE.Empty)]  public List<bool> boolOptionalListEmpty;
        [DDE("bool_optional_list_values", DDE.Empty)] public List<bool> boolOptionalListValues;
    }
}