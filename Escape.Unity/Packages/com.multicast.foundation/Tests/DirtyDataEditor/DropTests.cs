namespace Multicast {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DirtyDataEditor;
    using DropSystem;
    using Numerics;
    using NUnit.Framework;
    using UnityEngine;

    public class DropTests {
        [Test]
        public void ParseDrop() {
            DirtyDataParser.Errors.Clear();

            var dropText = @"{ ""type"" : ""test"", ""value"" : 123 }";

            var text = @$"---
drop: {dropText}
drop_optional_value: {dropText}
drop_list: [ {dropText}, {dropText} ]
drop_optional_list_empty: [ ]
drop_optional_list_values: [ {dropText} ]
";

            var obj = DirtyDataParser.ParseList<DirtyDataDropTestObject>(text).Single();

            foreach (var error in DirtyDataParser.Errors) {
                Debug.LogError(error);
            }

            Assert.IsTrue(obj.drop is DirtyDataTestDropDef);

            Assert.IsTrue(obj.dropOptionalNone is null);
            Assert.IsTrue(obj.dropOptionalValue is DirtyDataTestDropDef);

            Assert.IsTrue(obj.dropList.Count == 2);
            Assert.IsTrue(obj.dropOptionalListNone.Count == 0);
            Assert.IsTrue(obj.dropOptionalListEmpty.Count == 0);
            Assert.IsTrue(obj.dropOptionalListValues.Count == 1);

            Assert.IsTrue(obj.dropList[0] is DirtyDataTestDropDef);
            Assert.IsTrue(obj.dropList[1] is DirtyDataTestDropDef);
            Assert.IsTrue(obj.dropOptionalListValues[0] is DirtyDataTestDropDef);
        }
    }

    [Serializable, DDEObject]
    public class DirtyDataDropTestObject {
        [DDE("drop")] public DropDef drop;

        [DDE("drop_optional_none", null)]  public DropDef dropOptionalNone;
        [DDE("drop_optional_value", null)] public DropDef dropOptionalValue;

        [DDE("drop_list")]                            public List<DropDef> dropList;
        [DDE("drop_optional_list_none", DDE.Empty)]   public List<DropDef> dropOptionalListNone;
        [DDE("drop_optional_list_empty", DDE.Empty)]  public List<DropDef> dropOptionalListEmpty;
        [DDE("drop_optional_list_values", DDE.Empty)] public List<DropDef> dropOptionalListValues;
    }

    [Serializable, DDEObject, DDEImpl(typeof(DropDef), "test")]
    public sealed class DirtyDataTestDropDef : DropDef {
        [DDE("value", null)] public ProtectedBigDouble? value;
    }
}