namespace Multicast {
    using DropSystem;
    using Numerics;
    using NUnit.Framework;
    using UserData;

    public class UserDataTypesTests {
        [Test]
        public void Drop_Serialize_Int() {
            static UdValue<Drop> Factory(UdArgs args) => new UdValue<Drop>(args);

            var root = UdRoot.Create(Factory);

            var drop = Drop.Int("test_type", "test_key", 123);

            root.BeginTransaction(nameof(this.Drop_Serialize_Int));
            root.Value.Value = drop;
            root.CommitTransaction();

            var newRoot = UserDataTests.AssertIsCorrectlySerialized(root, Factory);

            Assert.AreEqual(DropAmountType.Int, newRoot.Value.Value.AmountType);
            Assert.AreEqual("test_type", newRoot.Value.Value.GetItemType());
            Assert.AreEqual("test_key", newRoot.Value.Value.ItemKey);
            Assert.AreEqual(123, newRoot.Value.Value.IntAmount);
        }

        [Test]
        public void Drop_Serialize_BigDouble() {
            static UdValue<Drop> Factory(UdArgs args) => new UdValue<Drop>(args);

            var root = UdRoot.Create(Factory);

            var drop = Drop.BigDouble("test_type", "test_key", 123.4);

            root.BeginTransaction(nameof(this.Drop_Serialize_BigDouble));
            root.Value.Value = drop;
            root.CommitTransaction();

            var newRoot = UserDataTests.AssertIsCorrectlySerialized(root, Factory);

            Assert.AreEqual(DropAmountType.BigDouble, newRoot.Value.Value.AmountType);
            Assert.AreEqual("test_type", newRoot.Value.Value.GetItemType());
            Assert.AreEqual("test_key", newRoot.Value.Value.ItemKey);
            Assert.AreEqual((BigDouble) 123.4, newRoot.Value.Value.BigDoubleAmount);
        }

        [Test]
        public void Drop_Serialize_LootBox() {
            static UdValue<Drop> Factory(UdArgs args) => new UdValue<Drop>(args);

            var root = UdRoot.Create(Factory);

            var drop = Drop.LootBox("test_type", "test_key", new[] {
                Drop.Int("inner_type", "inner_key", 456),
            });

            root.BeginTransaction(nameof(this.Drop_Serialize_LootBox));
            root.Value.Value = drop;
            root.CommitTransaction();

            var newRoot = UserDataTests.AssertIsCorrectlySerialized(root, Factory);

            Assert.AreEqual(DropAmountType.LootBox, newRoot.Value.Value.AmountType);
            Assert.AreEqual("test_type", newRoot.Value.Value.GetItemType());
            Assert.AreEqual("test_key", newRoot.Value.Value.ItemKey);
            Assert.AreEqual(1, newRoot.Value.Value.LootBoxDrops.Length);
            Assert.AreEqual("inner_type", newRoot.Value.Value.LootBoxDrops[0].GetItemType());
            Assert.AreEqual("inner_key", newRoot.Value.Value.LootBoxDrops[0].ItemKey);
            Assert.AreEqual(456, newRoot.Value.Value.LootBoxDrops[0].IntAmount);
        }
    }
}