namespace Multicast.Numerics {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using JetBrains.Annotations;
    using ExpressionParser;
    using Pool;

    [Serializable]
    public class Cost : IEnumerable<KeyValuePair<string, BigDouble>>, IEquatable<Cost> {
        private static readonly Dictionary<string, BigDouble> EmptyCost = new Dictionary<string, BigDouble>();

        public static Func<string, BigDouble> BalanceProvider = _ => -1;

        [CanBeNull] private Dictionary<string, BigDouble> cost;

        [PublicAPI]
        public static Cost Empty => new Cost();

        public Cost() {
        }

        public Cost([NotNull] Cost source) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.cost != null) {
                this.cost = new Dictionary<string, BigDouble>(source.cost);
            }
        }

        public Cost([NotNull] Dictionary<string, BigDouble> source) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }

            this.cost = new Dictionary<string, BigDouble>(source);
        }

        public Cost(string currencyKey, BigDouble amount) {
            this.Add(currencyKey, amount);
        }

        [PublicAPI]
        public int CurrenciesCount => this.cost?.Count ?? 0;

        [PublicAPI]
        public void Add(string currencyKey, BigDouble amount) {
            this[currencyKey] += amount;
        }

        [PublicAPI]
        public void Add(Cost other) {
            foreach (var (otherCurrencyKey, otherAmount) in other) {
                this[otherCurrencyKey] += otherAmount;
            }
        }

        [PublicAPI]
        public void Subtract(Cost other) {
            foreach (var (otherCurrencyKey, otherAmount) in other) {
                this[otherCurrencyKey] -= otherAmount;
            }
        }

        [PublicAPI]
        public void Clear(string currencyKey) {
            this.cost?.Remove(currencyKey);
        }

        [PublicAPI]
        public void MultiplyBy(BigDouble value) {
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
        public void DivideBy(BigDouble value) {
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
        public static Cost Create(Action<Cost> builder) {
            var cost = new Cost();
            builder?.Invoke(cost);
            return cost;
        }

        [PublicAPI]
        public static Cost FromFormula([NotNull] Dictionary<string, FormulaBigDouble> costFormula, [NotNull] FormulaContextCore<BigDouble> ctx) {
            if (costFormula == null) {
                throw new ArgumentNullException(nameof(costFormula));
            }

            if (ctx == null) {
                throw new ArgumentNullException(nameof(ctx));
            }

            var cost = new Cost();
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
        public BigDouble this[string currencyKey] {
            get => this.cost != null && this.cost.TryGetValue(currencyKey, out var amount) ? amount : 0;
            set {
                this.cost ??= new Dictionary<string, BigDouble>();

                this.cost[currencyKey] = value;
            }
        }

        public bool Equals(Cost other) {
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

            return this.Equals((Cost)obj);
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

        public Dictionary<string, BigDouble>.Enumerator GetEnumerator() {
            if (this.cost == null) {
                return EmptyCost.GetEnumerator();
            }

            return this.cost.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, BigDouble>> IEnumerable<KeyValuePair<string, BigDouble>>.GetEnumerator() {
            if (this.cost == null) {
                return EmptyCost.GetEnumerator();
            }

            return this.cost.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public static Cost operator +(Cost a, Cost b) {
            return new Cost { a, b };
        }

        public static Cost operator -(Cost a, Cost b) {
            var result = new Cost { a };
            result.Subtract(b);
            return result;
        }

        public static Cost operator *(Cost a, BigDouble b) {
            var result = new Cost { a };
            result.MultiplyBy(b);
            return result;
        }

        public static Cost operator /(Cost a, BigDouble b) {
            var result = new Cost { a };
            result.DivideBy(b);
            return result;
        }
    }
}