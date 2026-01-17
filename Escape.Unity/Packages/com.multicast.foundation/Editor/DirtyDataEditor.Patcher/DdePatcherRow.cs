namespace Multicast.DirtyDataEditor.Patcher {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using JetBrains.Annotations;

    public readonly struct DdePatcherRow<TItem> {
        private readonly DdePatchBuilder builder;

        private readonly string tableName;
        private readonly string key;

        public DdePatcherRow(DdePatchBuilder builder, string tableName, string key) {
            this.builder   = builder;
            this.tableName = tableName;
            this.key       = key;
        }

        [PublicAPI]
        public DdePatcherValue<T> For<T>(string property) {
            return new DdePatcherValue<T>(this.builder, this.tableName, this.key, property);
        }

        [PublicAPI]
        public DdePatcherValue<T> For<T>(Expression<Func<TItem, T>> selector) {
            return new DdePatcherValue<T>(this.builder, this.tableName, this.key, BuildComplexProperty(
                this.GetDdePropertyFromSelector(selector)
            ));
        }

        [PublicAPI]
        public DdePatcherValue<T1> For<T1>(
            Expression<Func<TItem, Dictionary<string, T1>>> selector1, string key1) {
            return new DdePatcherValue<T1>(this.builder, this.tableName, this.key, BuildComplexProperty(
                this.GetDdePropertyFromSelector(selector1),
                key1
            ));
        }

        [PublicAPI]
        public DdePatcherValue<T2> For<T1, T2>(
            Expression<Func<TItem, Dictionary<string, T1>>> selector1, string key1,
            Expression<Func<T1, T2>> selector2) {
            return new DdePatcherValue<T2>(this.builder, this.tableName, this.key, BuildComplexProperty(
                this.GetDdePropertyFromSelector(selector1),
                key1,
                this.GetDdePropertyFromSelector(selector2)
            ));
        }

        [PublicAPI]
        public DdePatcherValue<T2> For<T1, T2>(
            Expression<Func<TItem, Dictionary<string, T1>>> selector1, string key1,
            Expression<Func<T1, Dictionary<string, T2>>> selector2, string key2) {
            return new DdePatcherValue<T2>(this.builder, this.tableName, this.key, BuildComplexProperty(
                this.GetDdePropertyFromSelector(selector1),
                key1,
                this.GetDdePropertyFromSelector(selector2),
                key2
            ));
        }

        [PublicAPI]
        public DdePatcherValue<T3> For<T1, T2, T3>(
            Expression<Func<TItem, Dictionary<string, T1>>> selector1, string key1,
            Expression<Func<T1, Dictionary<string, T2>>> selector2, string key2,
            Expression<Func<T2, T3>> selector3
        ) {
            return new DdePatcherValue<T3>(this.builder, this.tableName, this.key, BuildComplexProperty(
                this.GetDdePropertyFromSelector(selector1),
                key1,
                this.GetDdePropertyFromSelector(selector2),
                key2,
                this.GetDdePropertyFromSelector(selector3)
            ));
        }

        [PublicAPI]
        public DdePatcherValue<T3> For<T1, T2, T3>(
            Expression<Func<TItem, Dictionary<string, T1>>> selector1, string key1,
            Expression<Func<T1, Dictionary<string, T2>>> selector2, string key2,
            Expression<Func<T2, Dictionary<string, T3>>> selector3, string key3
        ) {
            return new DdePatcherValue<T3>(this.builder, this.tableName, this.key, BuildComplexProperty(
                this.GetDdePropertyFromSelector(selector1),
                key1,
                this.GetDdePropertyFromSelector(selector2),
                key2,
                this.GetDdePropertyFromSelector(selector3),
                key3
            ));
        }

        private static string BuildComplexProperty(params string[] args) {
            return string.Join(':', args);
        }

        private string GetDdePropertyFromSelector<A, B>(Expression<Func<A, B>> selector) {
            if (selector.Body is MemberExpression {Member: FieldInfo fieldInfo} &&
                fieldInfo.GetCustomAttribute<DDEAttribute>() is var ddeAttribute &&
                ddeAttribute != null) {
                return ddeAttribute.Key;
            }

            throw new Exception($"Selector '{selector.Body}' is invalid at table='{this.tableName}' key='{this.key}'");
        }
    }
}