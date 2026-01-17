namespace Multicast.Numerics {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using JetBrains.Annotations;
    using ExpressionParser;
    using Pool;

    [Serializable]
    public class IntCost : IEnumerable<KeyValuePair<string, int>>, IEquatable<IntCost> {
        private static readonly Dictionary<string, int> EmptyCost = new Dictionary<string, int>();

        public static Func<string, BigDouble> BalanceProvider = _ => -1;

        [CanBeNull] private Dictionary<string, int> cost;

        [PublicAPI]
        public static IntCost Empty => new IntCost();

        public IntCost() {
        }

        public IntCost([NotNull] IntCost source) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.cost != null) {
                this.cost = new Dictionary<string, int>(source.cost);
            }
        }

        public IntCost([NotNull] Dictionary<string, int> source) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }

            this.cost = new Dictionary<string, int>(source);
        }

        public IntCost(string currencyKey, int amount) {
            this.Add(currencyKey, amount);
        }

        [PublicAPI]
        public int CurrenciesCount => this.cost?.Count ?? 0;

        [PublicAPI]
        public void Add(string currencyKey, int amount) {
            this[currencyKey] += amount;
        }

        [PublicAPI]
        public void Add(IntCost other) {
            foreach (var (otherCurrencyKey, otherAmount) in other) {
                this[otherCurrencyKey] += otherAmount;
            }
        }

        [PublicAPI]
        public void Subtract(IntCost other) {
            foreach (var (otherCurrencyKey, otherAmount) in other) {
                this[otherCurrencyKey] -= otherAmount;
            }
        }

        [PublicAPI]
        public void Clear(string currencyKey) {
            this.cost?.Remove(currencyKey);
        }

        [PublicAPI]
        public void MultiplyBy(int value) {
            if (this.cost == null) {
                return;
            }

            using (ListPool<string>.Get(out var names)) {
                foreach (var (currencyKey, _) in this.cost) {
                    names.Add(currencyKey);
                }

                foreach (var name in names) {
                    this[name] *= value;
                }
            }
        }

        [PublicAPI]
        public void DivideBy(int value) {
            if (this.cost == null) {
                return;
            }

            using (ListPool<string>.Get(out var names)) {
                foreach (var (currencyKey, _) in this.cost) {
                    names.Add(currencyKey);
                }

                foreach (var name in names) {
                    this[name] /= value;
                }
            }
        }

        [PublicAPI]
        public static IntCost Create(Action<IntCost> builder) {
            var cost = new IntCost();
            builder?.Invoke(cost);
            return cost;
        }

        [PublicAPI]
        public static IntCost FromFormula([NotNull] Dictionary<string, FormulaInt> costFormula, [NotNull] FormulaContextCore<int> ctx) {
            if (costFormula == null) {
                throw new ArgumentNullException(nameof(costFormula));
            }

            if (ctx == null) {
                throw new ArgumentNullException(nameof(ctx));
            }

            var cost = new IntCost();
            foreach (var (key, value) in costFormula) {
                var amountValue = value.Calc(ctx);
                if (amountValue <= BigDouble.Zero) {
                    continue;
                }

                cost.Add(key, amountValue);
            }

            return cost;
        }

        [PublicAPI]
        public int this[string currencyKey] {
            get => this.cost != null && this.cost.TryGetValue(currencyKey, out var amount) ? amount : 0;
            set {
                this.cost ??= new Dictionary<string, int>();

                this.cost[currencyKey] = value;
            }
        }

        public bool Equals(IntCost other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            if (this.CurrenciesCount != other.CurrenciesCount) {
                return false;
            }

            if (this.cost == null || other.cost == null) {
                return this.cost == other.cost;
            }

            foreach (var (currencyKey, amount) in this.cost) {
                if (!other.cost.TryGetValue(currencyKey, out var otherAmount)) {
                    return false;
                }

                if (amount != otherAmount) {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((IntCost)obj);
        }

        public override int GetHashCode() {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return this.cost?.GetHashCode() ?? 0;
        }

        public override string ToString() {
            return this.ToString(" ");
        }

        public string ToString(string separator) {
            if (this.cost == null) {
                return string.Empty;
            }

            var str = new StringBuilder();
            foreach (var (currencyKey, amount) in this.cost) {
                if (str.Length > 0) {
                    str.Append(separator);
                }

                str.Append($"<sprite=\"{currencyKey}\">{BigString.ToString(amount)}");
            }

            return str.ToString();
        }

        public Dictionary<string, int>.Enumerator GetEnumerator() {
            if (this.cost == null) {
                return EmptyCost.GetEnumerator();
            }

            return this.cost.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, int>> IEnumerable<KeyValuePair<string, int>>.GetEnumerator() {
            if (this.cost == null) {
                return EmptyCost.GetEnumerator();
            }

            return this.cost.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public static IntCost operator +(IntCost a, IntCost b) {
            return new IntCost { a, b };
        }

        public static IntCost operator -(IntCost a, IntCost b) {
            var result = new IntCost { a };
            result.Subtract(b);
            return result;
        }

        public static IntCost operator *(IntCost a, int b) {
            var result = new IntCost { a };
            result.MultiplyBy(b);
            return result;
        }

        public static IntCost operator /(IntCost a, int b) {
            var result = new IntCost { a };
            result.DivideBy(b);
            return result;
        }

        public static implicit operator Cost(IntCost cost) {
            var result = new Cost();

            foreach (var (currencyKey, amount) in cost) {
                result.Add(currencyKey, amount);
            }

            return result;
        }
    }
}