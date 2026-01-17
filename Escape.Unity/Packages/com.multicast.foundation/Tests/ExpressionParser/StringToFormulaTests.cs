namespace Multicast.ExpressionParser {
    using System;
    using System.Collections.Generic;
    using CodeWriter.ExpressionParser;
    using ExpressionParser;
    using NUnit.Framework;
    using UniMob;

    public class StringToFormulaTests {
        private LifetimeController lc;

        [SetUp]
        public void Setup() {
            this.lc = new LifetimeController();
        }

        [TearDown]
        public void TearDown() {
            this.lc.Dispose();
        }
        
        [Test]
        [TestCase("0", ExpectedResult = 0)]
        [TestCase("1", ExpectedResult = 1)]
        //[TestCase("+1", ExpectedResult   = 1)]
        [TestCase("-1", ExpectedResult  = -1)]
        [TestCase("(0)", ExpectedResult = 0)]
        [TestCase("(1)", ExpectedResult = 1)]
        //[TestCase("(+1)", ExpectedResult = 1)]
        [TestCase("(-1)", ExpectedResult = -1)]
        public int Primitive(string formula) {
            return ExpressionParserInt.Instance.Compile(formula, null, false).Invoke();
        }

        [Test]
        [TestCase("a", 2, 4, ExpectedResult           = 2)]
        [TestCase("b", 2, 4, ExpectedResult           = 4)]
        [TestCase("a + b", 2, 4, ExpectedResult       = 6)]
        [TestCase("a - b", 2, 4, ExpectedResult       = -2)]
        [TestCase("a * b", 2, 4, ExpectedResult       = 8)]
        [TestCase("a / b", 12, 4, ExpectedResult      = 3)]
        [TestCase("a ^ b", 2, 4, ExpectedResult       = 16)]
        [TestCase("a + b * 2", 2, 4, ExpectedResult   = 10)]
        [TestCase("a + (b * 2)", 2, 4, ExpectedResult = 10)]
        [TestCase("(a + b) * 2", 2, 4, ExpectedResult = 12)]
        public int Formula(string formula, int a, int b) {
            var context = new ExpressionContext<int>();
            context.RegisterVariable("a", () => a);
            context.RegisterVariable("b", () => b);

            return ExpressionParserInt.Instance.Compile(formula, context, false).Invoke();
        }

        [Test]
        public void FormulaThrowsWithUnknownParameters() {
            var context = new FormulaContext<float>(this.lc.Lifetime);
            context.RegisterVariable("x", () => 0f);

            var formula = new FormulaFloat("y + 1");

            Assert.Throws<VariableNotDefinedException>(() => formula.Calc(context));
        }

        [Test]
        public void MultiParameter() {
            var context = new FormulaContext<float>(this.lc.Lifetime);
            context.RegisterVariable("x", () => 5);
            context.RegisterVariable("y", () => 6);

            var formula = new FormulaFloat("x + y");

            Assert.AreEqual(11, formula.Calc(context));
        }

        [Test]
        public void MultiParameterMultiContext() {
            var ctx1 = new FormulaContext<float>(this.lc.Lifetime);
            var ctx2 = new FormulaContext<float>(this.lc.Lifetime);

            ctx1.RegisterVariable("x", () => 10);
            ctx2.RegisterVariable("x", () => 1);

            var formula = new FormulaFloat("x");

            Assert.AreEqual(10, formula.Calc(ctx1));
            Assert.AreEqual(1, formula.Calc(ctx2));

            Assert.AreEqual(10, formula.Calc(ctx1));
            Assert.AreEqual(1, formula.Calc(ctx2));
        }

        [Test]
        public void List() {
            var intList = new FormulaIntList(new List<string> {
                "11",
                "12",
                "n + 1 + 1 - 2",
            });

            Assert.AreEqual(11, intList[0]);
            Assert.AreEqual(10, intList[10]);
            Assert.AreEqual(12, intList[1]);
            Assert.AreEqual(2, intList[2]);
        }
    }
}