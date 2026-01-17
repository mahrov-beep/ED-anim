namespace Multicast {
    using NUnit.Framework;

    public class LinqExtensionsTests {
        [Test]
        [TestCase(new int[0], ExpectedResult         = 0)]
        [TestCase(new[] {1}, ExpectedResult          = 1)]
        [TestCase(new[] {1, 2}, ExpectedResult       = 0)]
        [TestCase(new[] {1, 2, 3, 4}, ExpectedResult = 0)]
        public int SingleOrDefaultIfMultiple(int[] arr) {
            return arr.SingleOrDefaultIfMultiple();
        }

        [Test]
        [TestCase(new int[0], ExpectedResult             = 0)]
        [TestCase(new[] {1}, ExpectedResult              = 1)]
        [TestCase(new[] {1, 2}, ExpectedResult           = 0)]
        [TestCase(new[] {1, 2, 3, 4}, ExpectedResult     = 0)]
        [TestCase(new[] {-1, 1}, ExpectedResult          = 1)]
        [TestCase(new[] {-1, 1, 2}, ExpectedResult       = 0)]
        [TestCase(new[] {-1, 1, 2, 3, 4}, ExpectedResult = 0)]
        public int SingleOrDefaultIfMultipleWithPredicate(int[] arr) {
            return arr.SingleOrDefaultIfMultiple(it => it > 0);
        }
    }
}