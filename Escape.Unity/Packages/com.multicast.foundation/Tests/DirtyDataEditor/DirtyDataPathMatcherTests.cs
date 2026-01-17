namespace Multicast {
    using DirtyDataEditor;
    using NUnit.Framework;

    public class DirtyDataPathMatcherTests {
        [Test]
        // simple
        [TestCase("Configs/Default0/achievements", "Configs/Default0/achievements", ExpectedResult           = true)]
        [TestCase("Configs/Default0/achievements", "Configs/Default0/achievements$test", ExpectedResult      = true)]
        [TestCase("Configs/Default0/achievements", "Configs/Default0/adachievements", ExpectedResult          = false)]
        [TestCase("Configs/Default0/achievements", "Configs/Default0/adachievements$test", ExpectedResult     = false)]
        [TestCase("Configs/Default0/achievements", "Configs/Default0/achievementsad_new", ExpectedResult      = false)]
        [TestCase("Configs/Default0/achievements", "Configs/Default0/achievementsad_new$test", ExpectedResult = false)]
        // regex
        [TestCase("Configs/*/achievements", "Configs/Default0/achievements", ExpectedResult       = true)]
        [TestCase("Configs/*/achievements", "Configs/Default0/achievements$test", ExpectedResult  = true)]
        [TestCase("Configs/*/achievements", "Configs/Default0/adachievements", ExpectedResult      = false)]
        [TestCase("Configs/*/achievements", "Configs/Default0/adachievements$test", ExpectedResult = false)]
        [TestCase("Configs/*/achievements", "Configs/Default0/achievementsad", ExpectedResult      = false)]
        [TestCase("Configs/*/achievements", "Configs/Default0/achievementsad$test", ExpectedResult = false)]
        // regex subfolder
        [TestCase("Configs/*/achievements", "Configs/Default0/sub123/achievements", ExpectedResult       = false)]
        [TestCase("Configs/*/achievements", "Configs/Default0/sub123/achievements$test", ExpectedResult  = false)]
        [TestCase("Configs/*/achievements", "Configs/Default0/sub123/adachievements", ExpectedResult      = false)]
        [TestCase("Configs/*/achievements", "Configs/Default0/sub123/adachievements$test", ExpectedResult = false)]
        [TestCase("Configs/*/achievements", "Configs/Default0/sub123/achievementsad", ExpectedResult      = false)]
        [TestCase("Configs/*/achievements", "Configs/Default0/sub123/achievementsad$test", ExpectedResult = false)]
        // invalid regex
        [TestCase("Configs/*/map(test)/achievements", "Configs/default/map(test)/achievements", ExpectedResult = true)]
        [TestCase("Configs/*/map(test)/achievements", "Configs/default/maptest/achievements", ExpectedResult = false)]
        public bool Match(string regex, string str) {
            var matcher = DirtyDataCacheExtensions.Match(regex);
            return matcher.Invoke(str);
        }
    }
}