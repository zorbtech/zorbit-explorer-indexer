using System;
using System.Collections.Generic;

namespace Stratis.Bitcoin.Features.AzureIndexer
{
    internal class AnonymousEqualityComparer<T,TComparer> : IEqualityComparer<T>
    {
        private readonly Func<T, TComparer> _comparer;

        public AnonymousEqualityComparer(Func<T,TComparer> comparer)
        {
            this._comparer = comparer;
        }

        #region IEqualityComparer<T> Members

        public bool Equals(T x, T y)
        {
            return this._comparer(x).Equals(this._comparer(y));
        }

        public int GetHashCode(T obj)
        {
            return this._comparer(obj).GetHashCode();
        }

        #endregion
    }
}
