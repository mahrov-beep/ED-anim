namespace Multicast {
    using Numerics;
    using NUnit.Framework;

    public class GameTimeTests {
        [Test]
        [TestCase("2025-01-13 00:01")]
        [TestCase("2010-08-30 00:00")]
        [TestCase("2012-05-13 23:59")]
        [TestCase("2050-12-01 09:06")]
        public void Parse(string input) {
            Assert.IsTrue(GameTime.TryParse(input, out var gameTime));
            Assert.AreEqual(input, gameTime.ToString());
        }
    }
}