namespace Multicast {
    using Numerics;
    using NUnit.Framework;

    public class BigDoubleTests {
        [Test]
        [TestCase("0", 0)]
        [TestCase("1", 1)]
        [TestCase("1.23", 1.23)]
        [TestCase("1,23", 1.23)]
        [TestCase("1000000000000000000000000000000", 1e30)]
        public void Parse(string input, double expected) {
            var actual = BigDouble.Parse(input);
            Assert.IsTrue(actual == expected);
            Assert.IsTrue(BigDouble.Parse(actual.ToString()) == expected);
        }
    }
}