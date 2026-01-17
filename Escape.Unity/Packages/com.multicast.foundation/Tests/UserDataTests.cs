namespace Multicast {
    using System;
    using System.Buffers;
    using System.Linq;
    using MessagePack;
    using NUnit.Framework;
    using UniMob;
    using UniMob.Core;
    using UserData;

    public class UserDataTests {
        [Test]
        public void UdValue_InitialValue() {
            var root = UdRoot.Create(arg => new UdValue<int>(arg));

            Assert.AreEqual(0, root.Value.Value);
        }

        [Test]
        public void UdValue_SetOutsideOfTransactionFails() {
            var root = UdRoot.Create(arg => new UdValue<int>(arg));

            Assert.Throws<InvalidOperationException>(() => root.Value.Value = 12);
        }

        [Test]
        public void UdValue_EmptyCommit() {
            var root = UdRoot.Create(arg => new UdValue<int>(arg));

            root.BeginTransaction(nameof(this.UdValue_EmptyCommit));
            root.CommitTransaction();

            Assert.Pass();
        }

        [Test]
        public void UdValue_SetAndCommit() {
            var root = UdRoot.Create(arg => new UdValue<int>(arg));

            root.BeginTransaction(nameof(this.UdValue_SetAndCommit));

            root.Value.Value = 12;

            Assert.AreEqual(12, root.Value.Value);

            root.CommitTransaction();

            Assert.AreEqual(12, root.Value.Value);
        }

        [Test]
        public void UdValue_CommitTriggersAtom() {
            var root = UdRoot.Create(arg => new UdValue<int>(arg));

            var lc = new LifetimeController();

            int sum = 0;
            Atom.Reaction(lc.Lifetime, () => root.Value.Value, v => sum += v);

            root.BeginTransaction(nameof(this.UdValue_CommitTriggersAtom));
            root.Value.Value = 123;
            root.CommitTransaction();

            AtomScheduler.Sync();

            Assert.AreEqual(123, sum);

            lc.Dispose();
        }

        [Test]
        public void UdLocalValue_ReadWithoutTransaction() {
            var root = UdRoot.Create(args => new UdLocalValue<int>(args));

            Assert.AreEqual(0, root.Value.Value);
        }

        [Test]
        public void UdLocalValue_WriteWithoutTransaction() {
            var root = UdRoot.Create(args => new UdLocalValue<int>(args));

            root.Value.Value = 123;

            Assert.AreEqual(123, root.Value.Value);
        }

        [Test]
        public void UdLocalValue_Serialize() {
            static UdLocalValue<int> Factory(UdArgs args) => new UdLocalValue<int>(args);

            var root = UdRoot.Create(Factory);

            root.Value.Value = 123;

            AssertIsCorrectlySerialized(root, Factory);
        }

        [Test]
        public void UdDict_CanReadOutsideOfTransaction() {
            var root = UdRoot.Create(args => new UdDict<TestUserData>(args, a => new TestUserData(a)));

            Assert.AreEqual(0, root.Value.Count);
            Assert.IsFalse(root.Value.ContainsKey("none"));
            Assert.IsFalse(root.Value.TryGetValue("none", out _));
        }

        [Test]
        public void UdDict_CannotEditOutsideOfTransaction() {
            var root = UdRoot.Create(args => new UdDict<TestUserData>(args, a => new TestUserData(a)));

            Assert.Throws<InvalidOperationException>(() => root.Value.Create("some"));
            Assert.Throws<InvalidOperationException>(() => root.Value.Remove("some"));
        }

        [Test]
        public void UdDict_CreateItemAndCommit() {
            var root = UdRoot.Create(args => new UdDict<TestUserData>(args, a => new TestUserData(a)));

            root.BeginTransaction(nameof(this.UdDict_CreateItemAndCommit));
            var item = root.Value.Create("some");
            root.CommitTransaction();

            Assert.AreEqual(1, root.Value.Count);
            Assert.IsTrue(root.Value.ContainsKey("some"));
            Assert.IsTrue(root.Value.TryGetValue("some", out var item2));
            Assert.AreEqual(item, item2);
        }

        [Test]
        public void UdDict_ModificationsInTransaction() {
            var root = UdRoot.Create(args => new UdDict<TestUserData>(args, a => new TestUserData(a)));

            root.BeginTransaction(nameof(this.UdDict_ModificationsInTransaction));
            root.Value.Create("initial");
            root.CommitTransaction();

            root.BeginTransaction(nameof(this.UdDict_ModificationsInTransaction));

            root.Value.Create("one");
            Assert.Throws<ArgumentException>(() => root.Value.Create("one"));
            Assert.AreEqual(2, root.Value.Count);
            Assert.IsTrue(root.Value.ContainsKey("one"));

            Assert.IsTrue(root.Value.Remove("one"));
            Assert.IsFalse(root.Value.Remove("one"));
        }

        /*
        [Test]
        public void UdLookup_GetWithoutTransaction() {
            var root = UdRoot.Create(args => new UdLookup<TestUserData>(args, a => new TestUserData(a)));

            var testItem = root.Value.Get("test");

            root.BeginTransaction();
            testItem.InnerInt.Value = 123;
            root.CommitTransaction();

            Assert.AreEqual(123, root.Value.Get("test").InnerInt.Value);
        }
        */

        /*
        [Test]
        public void UdLookup_Serialize() {
            static UdLookup<TestUserData> Factory(UdArgs args) => new UdLookup<TestUserData>(args, a => new TestUserData(a));

            var root = UdRoot.Create(Factory);

            root.BeginTransaction();
            root.Value.Get("demo").InnerInt.Value = 123;
            root.CommitTransaction();

            AssertIsCorrectlySerialized(root, Factory);
        }
        */

        [Test]
        public void UdObject_MultipleChildrenWithSameKeyFails() {
            Assert.Throws<UdDuplicatedPropertyKeyException>(() => UdRoot.Create(args => new TestWithDuplicateKeysUserData(args)));
        }

        [Test]
        public void UdObject_Serialize() {
            static TestUserData Factory(UdArgs args) => new TestUserData(args);

            var root = UdRoot.Create(Factory);

            root.BeginTransaction(nameof(this.UdObject_Serialize));
            root.Value.InnerInt.Value = 123;
            var item1 = root.Value.InnerDict.Create("one");
            item1.InnerInt.Value = 1;
            var item2 = root.Value.InnerDict.Create("two");
            item2.InnerInt.Value = 2;
            var item3 = item2.InnerDict.Create("three");
            item3.InnerInt.Value = 3;
            root.CommitTransaction();

            AssertIsCorrectlySerialized(root, Factory);
        }

        internal static UdRoot<T> AssertIsCorrectlySerialized<T>(UdRoot<T> root, Func<UdArgs, T> factory) where T : UdObjectBase {
            var memory1Value = SerializeValue(root);
            var root2        = UdRoot.FromMemory(factory, SerializeRoot(root), MessagePackSerializer.DefaultOptions);
            var memory2Value = SerializeValue(root2);

            Assert.IsTrue(memory1Value.ToArray().SequenceEqual(memory2Value.ToArray()));

            return root2;

            static ReadOnlyMemory<byte> SerializeRoot(UdRoot<T> root) {
                var stream = new ArrayBufferWriter<byte>();
                UdRoot.Serialize(root, stream, MessagePackSerializer.DefaultOptions);
                return stream.WrittenMemory;
            }

            static ReadOnlyMemory<byte> SerializeValue(UdRoot<T> root) {
                var stream = new ArrayBufferWriter<byte>();
                UdRoot.Serialize(root.Value, stream, MessagePackSerializer.DefaultOptions);
                return stream.WrittenMemory;
            }
        }

        private class TestUserData : UdObject {
            public UdValue<int> InnerInt { get; }

            public UdDict<TestUserData> InnerDict { get; }

            public TestUserData(UdArgs args) : base(args) {
                this.InnerInt  = new UdValue<int>(this.Child("int"));
                this.InnerDict = new UdDict<TestUserData>(this.Child("dict"), a => new TestUserData(a));
            }
        }

        private class TestWithDuplicateKeysUserData : UdObject {
            public UdValue<int> A { get; }
            public UdValue<int> B { get; }

            public TestWithDuplicateKeysUserData(UdArgs args) : base(args) {
                this.A = new UdValue<int>(this.Child("a"));
                this.B = new UdValue<int>(this.Child("a"));
            }
        }
    }
}