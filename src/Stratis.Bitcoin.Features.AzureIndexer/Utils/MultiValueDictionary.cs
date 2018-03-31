using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Stratis.Bitcoin.Features.AzureIndexer
{
    /// <summary>
    /// Indicates to Code Analysis that a method validates a particular parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class ValidatedNotNullAttribute : Attribute
    {
    }

    internal static class Requires
    {
        /// <summary>
        /// Throws an exception if the specified parameter's value is null.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="value">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <returns>The value of the parameter.</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if <paramref name="value" /> is <c>null</c></exception>
        [DebuggerStepThrough]
        public static T NotNull<T>([ValidatedNotNull] T value, string parameterName) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            return value;
        }
        /// <summary>
        /// Throws an exception if the specified parameter's value is IntPtr.Zero.
        /// </summary>
        /// <param name="value">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <returns>The value of the parameter.</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if <paramref name="value" /> is <c>null</c></exception>
        [DebuggerStepThrough]
        public static IntPtr NotNull([ValidatedNotNull] IntPtr value, string parameterName)
        {
            if (value == IntPtr.Zero)
            {
                throw new ArgumentNullException(parameterName);
            }
            return value;
        }
        /// <summary>
        /// Throws an exception if the specified parameter's value is null.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="value">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <returns>The value of the parameter.</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if <paramref name="value" /> is <c>null</c></exception>
        /// <remarks>
        /// This method exists for callers who themselves only know the type as a generic parameter which
        /// may or may not be a class, but certainly cannot be null.
        /// </remarks>
        [DebuggerStepThrough]
        public static T NotNullAllowStructs<T>([ValidatedNotNull] T value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            return value;
        }
        /// <summary>
        /// Throws an <see cref="T:System.ArgumentOutOfRangeException" /> if a condition does not evaluate to true.
        /// </summary>
        [DebuggerStepThrough]
        public static void Range(bool condition, string parameterName, string message = null)
        {
            if (!condition)
            {
                Requires.FailRange(parameterName, message);
            }
        }
        /// <summary>
        /// Throws an <see cref="T:System.ArgumentOutOfRangeException" /> if a condition does not evaluate to true.
        /// </summary>
        /// <returns>Nothing.  This method always throws.</returns>
        [DebuggerStepThrough]
        public static Exception FailRange(string parameterName, string message = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
            throw new ArgumentOutOfRangeException(parameterName, message);
        }
        /// <summary>
        /// Throws an ArgumentException if a condition does not evaluate to true.
        /// </summary>
        [DebuggerStepThrough]
        public static void Argument(bool condition, string parameterName, string message)
        {
            if (!condition)
            {
                throw new ArgumentException(message, parameterName);
            }
        }
        /// <summary>
        /// Throws an ArgumentException if a condition does not evaluate to true.
        /// </summary>
        [DebuggerStepThrough]
        public static void Argument(bool condition)
        {
            if (!condition)
            {
                throw new ArgumentException();
            }
        }
    }
    internal sealed class Sr : Exceptions
    {
        private Sr()
        {
        }
        internal static string GetString(string text)
        {
            return text;
        }
    }

    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), DebuggerNonUserCode, CompilerGenerated]
    internal class Exceptions
    {
        private static ResourceManager _resourceMan;
        private static CultureInfo _resourceCulture;
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(Exceptions._resourceMan, null))
                {
                    var resourceManager = new ResourceManager("Stratis.Bitcoin.Features.AzureIndexer.Exceptions", typeof(Exceptions).GetTypeInfo().Assembly);
                    Exceptions._resourceMan = resourceManager;
                }
                return Exceptions._resourceMan;
            }
        }
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get
            {
                return Exceptions._resourceCulture;
            }
            set
            {
                Exceptions._resourceCulture = value;
            }
        }
        /// <summary>
        ///   Looks up a localized string similar to Destination array is not long enough to copy all the items in the collection. Check array index and length..
        /// </summary>
        internal static string CopyToArgumentsTooSmall
        {
            get
            {
                return Exceptions.ResourceManager.GetString("CopyTo_ArgumentsTooSmall", Exceptions._resourceCulture);
            }
        }
        /// <summary>
        ///   Looks up a localized string similar to The specified TValueCollection creates collections that have IsReadOnly set to true by default. TValueCollection must be a mutable ICollection..
        /// </summary>
        internal static string CreateTValueCollectionReadOnly
        {
            get
            {
                return Exceptions.ResourceManager.GetString("Create_TValueCollectionReadOnly", Exceptions._resourceCulture);
            }
        }
        /// <summary>
        ///   Looks up a localized string similar to Enumeration has already completed..
        /// </summary>
        internal static string EnumeratorAfterCurrent
        {
            get
            {
                return Exceptions.ResourceManager.GetString("Enumerator_AfterCurrent", Exceptions._resourceCulture);
            }
        }
        /// <summary>
        ///   Looks up a localized string similar to Enumeration has not started. Call MoveNext() before Current..
        /// </summary>
        internal static string EnumeratorBeforeCurrent
        {
            get
            {
                return Exceptions.ResourceManager.GetString("Enumerator_BeforeCurrent", Exceptions._resourceCulture);
            }
        }
        /// <summary>
        ///   Looks up a localized string similar to Collection was modified; enumeration operation may not execute.
        /// </summary>
        internal static string EnumeratorModification
        {
            get
            {
                return Exceptions.ResourceManager.GetString("Enumerator_Modification", Exceptions._resourceCulture);
            }
        }
        /// <summary>
        ///   Looks up a localized string similar to The given key was not present..
        /// </summary>
        internal static string KeyNotFound
        {
            get
            {
                return Exceptions.ResourceManager.GetString("KeyNotFound", Exceptions._resourceCulture);
            }
        }
        /// <summary>
        ///   Looks up a localized string similar to The collection is read-only.
        /// </summary>
        internal static string ReadOnlyModification
        {
            get
            {
                return Exceptions.ResourceManager.GetString("ReadOnly_Modification", Exceptions._resourceCulture);
            }
        }
        internal Exceptions()
        {
        }
    }

    /// <summary>
    /// A MultiValueDictionary can be viewed as a <see cref="T:System.Collections.IDictionary" /> that allows multiple 
    /// values for any given unique key. While the MultiValueDictionary API is 
    /// mostly the same as that of a regular <see cref="T:System.Collections.IDictionary" />, there is a distinction
    /// in that getting the value for a key returns a <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" /> of values
    /// rather than a single value associated with that key. Additionally, 
    /// there is functionality to allow adding or removing more than a single
    /// value at once. 
    ///
    /// The MultiValueDictionary can also be viewed as a IReadOnlyDictionary&lt;TKey,IReadOnlyCollection&lt;TValue&gt;t&gt;
    /// where the <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" /> is abstracted from the view of the programmer.
    ///
    /// For a read-only MultiValueDictionary, see <see cref="T:System.Linq.ILookup`2" />.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    internal class MultiValueDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, IReadOnlyCollection<TValue>>, IReadOnlyCollection<KeyValuePair<TKey, IReadOnlyCollection<TValue>>>, IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>>, IEnumerable
    {
        /// <summary>
        /// The Enumerator class for a <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />
        /// that iterates over <typeparamref name="TKey" />-<see cref="T:System.Collections.Generic.IReadOnlyCollection`1" />
        /// pairs.
        /// </summary>
        private class Enumerator : IEnumerator<KeyValuePair<TKey, IReadOnlyCollection<TValue>>>, IEnumerator, IDisposable
        {
            private enum EnumerationState
            {
                BeforeFirst,
                During,
                AfterLast
            }
            private readonly MultiValueDictionary<TKey, TValue> _multiValueDictionary;
            private readonly int _version;
            private KeyValuePair<TKey, IReadOnlyCollection<TValue>> _current;
            private Dictionary<TKey, MultiValueDictionary<TKey, TValue>.InnerCollectionView>.Enumerator _enumerator;
            private MultiValueDictionary<TKey, TValue>.Enumerator.EnumerationState _state;
            public KeyValuePair<TKey, IReadOnlyCollection<TValue>> Current
            {
                get
                {
                    return this._current;
                }
            }
            object IEnumerator.Current
            {
                get
                {
                    switch (this._state)
                    {
                        case MultiValueDictionary<TKey, TValue>.Enumerator.EnumerationState.BeforeFirst:
                            throw new InvalidOperationException(Sr.GetString(Exceptions.EnumeratorBeforeCurrent));
                        case MultiValueDictionary<TKey, TValue>.Enumerator.EnumerationState.AfterLast:
                            throw new InvalidOperationException(Sr.GetString(Exceptions.EnumeratorAfterCurrent));
                    }
                    return this._current;
                }
            }
            /// <summary>
            /// Constructor for the enumerator
            /// </summary>
            /// <param name="multiValueDictionary">A MultiValueDictionary to iterate over</param>
            internal Enumerator(MultiValueDictionary<TKey, TValue> multiValueDictionary)
            {
                this._multiValueDictionary = multiValueDictionary;
                this._version = multiValueDictionary._version;
                this._current = default(KeyValuePair<TKey, IReadOnlyCollection<TValue>>);
                this._enumerator = multiValueDictionary._dictionary.GetEnumerator();
                this._state = MultiValueDictionary<TKey, TValue>.Enumerator.EnumerationState.BeforeFirst;
            }
            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
            public bool MoveNext()
            {
                if (this._version != this._multiValueDictionary._version)
                {
                    throw new InvalidOperationException(Sr.GetString(Exceptions.EnumeratorModification));
                }
                if (this._enumerator.MoveNext())
                {
                    var keyValuePair = this._enumerator.Current;
                    var arg570 = keyValuePair.Key;
                    var keyValuePair2 = this._enumerator.Current;
                    this._current = new KeyValuePair<TKey, IReadOnlyCollection<TValue>>(arg570, keyValuePair2.Value);
                    this._state = MultiValueDictionary<TKey, TValue>.Enumerator.EnumerationState.During;
                    return true;
                }
                this._current = default(KeyValuePair<TKey, IReadOnlyCollection<TValue>>);
                this._state = MultiValueDictionary<TKey, TValue>.Enumerator.EnumerationState.AfterLast;
                return false;
            }
            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
            public void Reset()
            {
                if (this._version != this._multiValueDictionary._version)
                {
                    throw new InvalidOperationException(Sr.GetString(Exceptions.EnumeratorModification));
                }
                this._enumerator.Dispose();
                this._enumerator = this._multiValueDictionary._dictionary.GetEnumerator();
                this._current = default(KeyValuePair<TKey, IReadOnlyCollection<TValue>>);
                this._state = MultiValueDictionary<TKey, TValue>.Enumerator.EnumerationState.BeforeFirst;
            }
            /// <summary>
            /// Frees resources associated with this Enumerator
            /// </summary>
            public void Dispose()
            {
                this._enumerator.Dispose();
            }
        }
        /// <summary>
        /// An inner class that functions as a view of an ICollection within a MultiValueDictionary
        /// </summary>
        private class InnerCollectionView : ICollection<TValue>, IReadOnlyCollection<TValue>, IGrouping<TKey, TValue>, IEnumerable<TValue>, IEnumerable
        {
            private readonly TKey _key;
            private readonly ICollection<TValue> _collection;
            public int Count
            {
                get
                {
                    return this._collection.Count;
                }
            }
            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }
            public TKey Key
            {
                get
                {
                    return this._key;
                }
            }
            public InnerCollectionView(TKey key, ICollection<TValue> collection)
            {
                this._key = key;
                this._collection = collection;
            }
            public void AddValue(TValue item)
            {
                this._collection.Add(item);
            }
            public bool RemoveValue(TValue item)
            {
                return this._collection.Remove(item);
            }
            public bool Contains(TValue item)
            {
                return this._collection.Contains(item);
            }
            public void CopyTo(TValue[] array, int arrayIndex)
            {
                Requires.NotNullAllowStructs<TValue[]>(array, "array");
                Requires.Range(arrayIndex >= 0, "arrayIndex", null);
                Requires.Range(arrayIndex <= array.Length, "arrayIndex", null);
                Requires.Argument(array.Length - arrayIndex >= this._collection.Count, "arrayIndex", Sr.GetString(Exceptions.CopyToArgumentsTooSmall));
                this._collection.CopyTo(array, arrayIndex);
            }
            public IEnumerator<TValue> GetEnumerator()
            {
                return this._collection.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
            void ICollection<TValue>.Add(TValue item)
            {
                throw new NotSupportedException(Sr.GetString(Exceptions.ReadOnlyModification));
            }
            void ICollection<TValue>.Clear()
            {
                throw new NotSupportedException(Sr.GetString(Exceptions.ReadOnlyModification));
            }
            bool ICollection<TValue>.Remove(TValue item)
            {
                throw new NotSupportedException(Sr.GetString(Exceptions.ReadOnlyModification));
            }
        }
        /// <summary>
        /// A view of a <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> as a read-only 
        /// <see cref="T:System.Linq.ILookup`2" /> object
        /// </summary>
        private class MultiLookup : ILookup<TKey, TValue>, IEnumerable<IGrouping<TKey, TValue>>, IEnumerable
        {
            private class Enumerator : IEnumerator<IGrouping<TKey, TValue>>, IEnumerator, IDisposable
            {
                private enum EnumerationState
                {
                    BeforeFirst,
                    During,
                    AfterLast
                }
                private readonly MultiValueDictionary<TKey, TValue> _multiValueDictionary;
                private IGrouping<TKey, TValue> _current;
                private readonly int _version;
                private readonly IEnumerator<KeyValuePair<TKey, MultiValueDictionary<TKey, TValue>.InnerCollectionView>> _enumerator;
                private MultiValueDictionary<TKey, TValue>.MultiLookup.Enumerator.EnumerationState _state;
                IGrouping<TKey, TValue> IEnumerator<IGrouping<TKey, TValue>>.Current
                {
                    get
                    {
                        return this._current;
                    }
                }
                object IEnumerator.Current
                {
                    get
                    {
                        switch (this._state)
                        {
                            case MultiValueDictionary<TKey, TValue>.MultiLookup.Enumerator.EnumerationState.BeforeFirst:
                                throw new InvalidOperationException(Sr.GetString(Exceptions.EnumeratorBeforeCurrent));
                            case MultiValueDictionary<TKey, TValue>.MultiLookup.Enumerator.EnumerationState.AfterLast:
                                throw new InvalidOperationException(Sr.GetString(Exceptions.EnumeratorAfterCurrent));
                        }
                        return this._current;
                    }
                }
                internal Enumerator(MultiValueDictionary<TKey, TValue> multiValueDictionary)
                {
                    this._multiValueDictionary = multiValueDictionary;
                    this._enumerator = multiValueDictionary._dictionary.GetEnumerator();
                    this._version = multiValueDictionary._version;
                    var keyValuePair = this._enumerator.Current;
                    this._current = keyValuePair.Value;
                    this._state = MultiValueDictionary<TKey, TValue>.MultiLookup.Enumerator.EnumerationState.BeforeFirst;
                }
                public bool MoveNext()
                {
                    if (this._version != this._multiValueDictionary._version)
                    {
                        throw new InvalidOperationException(Sr.GetString(Exceptions.EnumeratorModification));
                    }
                    if (this._enumerator.MoveNext())
                    {
                        var keyValuePair = this._enumerator.Current;
                        this._current = keyValuePair.Value;
                        this._state = MultiValueDictionary<TKey, TValue>.MultiLookup.Enumerator.EnumerationState.During;
                        return true;
                    }
                    this._state = MultiValueDictionary<TKey, TValue>.MultiLookup.Enumerator.EnumerationState.AfterLast;
                    return false;
                }
                public void Reset()
                {
                    if (this._version != this._multiValueDictionary._version)
                    {
                        throw new InvalidOperationException(Sr.GetString(Exceptions.EnumeratorModification));
                    }
                    this._state = MultiValueDictionary<TKey, TValue>.MultiLookup.Enumerator.EnumerationState.BeforeFirst;
                    this._enumerator.Reset();
                    var keyValuePair = this._enumerator.Current;
                    this._current = keyValuePair.Value;
                }
                public void Dispose()
                {
                    this._enumerator.Dispose();
                }
            }
            private readonly MultiValueDictionary<TKey, TValue> _multiValueDictionary;
            public int Count
            {
                get
                {
                    return this._multiValueDictionary._dictionary.Count;
                }
            }
            /// <summary>
            /// Gets the <see cref="T:System.Collections.Generic.IEnumerable`1" /> sequence of <typeparamref name="TValue" />s
            /// associated with the given <typeparamref name="TKey" />. 
            /// </summary>
            /// <param name="key">The <typeparamref name="TKey" /> of the desired sequence.</param>
            /// <value>the <see cref="T:System.Collections.Generic.IEnumerable`1" /> sequence of <typeparamref name="TValue" />s
            /// associated with the given <typeparamref name="TKey" />.</value>
            /// <remarks>Attempting to index on a <typeparamref name="TKey" /> that is not present in the
            /// <see cref="T:System.Linq.ILookup`2" /> will return an empty <see cref="T:System.Collections.Generic.IEnumerable`1" />
            /// rather than throw an exception.</remarks>
            public IEnumerable<TValue> this[TKey key]
            {
                get
                {
                    MultiValueDictionary<TKey, TValue>.InnerCollectionView result;
                    if (this._multiValueDictionary._dictionary.TryGetValue(key, out result))
                    {
                        return result;
                    }
                    return Enumerable.Empty<TValue>();
                }
            }
            internal MultiLookup(MultiValueDictionary<TKey, TValue> multiValueDictionary)
            {
                this._multiValueDictionary = multiValueDictionary;
            }
            public bool Contains(TKey key)
            {
                Requires.NotNullAllowStructs<TKey>(key, "key");
                return this._multiValueDictionary._dictionary.ContainsKey(key);
            }
            public IEnumerator<IGrouping<TKey, TValue>> GetEnumerator()
            {
                return new MultiValueDictionary<TKey, TValue>.MultiLookup.Enumerator(this._multiValueDictionary);
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return new MultiValueDictionary<TKey, TValue>.MultiLookup.Enumerator(this._multiValueDictionary);
            }
        }
        /// <summary>
        /// The private dictionary that this class effectively wraps around
        /// </summary>
        private readonly Dictionary<TKey, MultiValueDictionary<TKey, TValue>.InnerCollectionView> _dictionary;
        /// <summary>
        /// The function to construct a new <see cref="T:System.Collections.Generic.ICollection`1" />
        /// </summary>
        /// <returns></returns>
        private Func<ICollection<TValue>> _newCollectionFactory = () => new List<TValue>();
        /// <summary>
        /// The current version of this MultiValueDictionary used to determine MultiValueDictionary modification
        /// during enumeration
        /// </summary>
        private int _version;
        /// <summary>
        /// Gets each <typeparamref name="TKey" /> in this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> that
        /// has one or more associated <typeparamref name="TValue" />.
        /// </summary>
        /// <value>
        /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> containing each <typeparamref name="TKey" /> 
        /// in this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> that has one or more associated 
        /// <typeparamref name="TValue" />.
        /// </value>
        public IEnumerable<TKey> Keys
        {
            get
            {
                return this._dictionary.Keys;
            }
        }
        /// <summary>
        /// Gets an enumerable of <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" /> from this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />,
        /// where each <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" /> is the collection of every <typeparamref name="TValue" /> associated
        /// with a <typeparamref name="TKey" /> present in the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />. 
        /// </summary>
        /// <value>An IEnumerable of each <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" /> in this 
        /// <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /></value>
        public IEnumerable<IReadOnlyCollection<TValue>> Values
        {
            get
            {
                return this._dictionary.Values;
            }
        }
        /// <summary>
        /// Get every <typeparamref name="TValue" /> associated with the given <typeparamref name="TKey" />. If 
        /// <paramref name="key" /> is not found in this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />, will 
        /// throw a <see cref="T:System.Collections.Generic.KeyNotFoundException" />.
        /// </summary>
        /// <param name="key">The <typeparamref name="TKey" /> of the elements to retrieve.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> must be non-null</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException"><paramref name="key" /> does not have any associated 
        /// <typeparamref name="TValue" />s in this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.</exception>
        /// <value>
        /// An <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" /> containing every <typeparamref name="TValue" />
        /// associated with <paramref name="key" />.
        /// </value>
        /// <remarks>
        /// Note that the <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" /> returned will change alongside any changes 
        /// to the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> 
        /// </remarks>
        public IReadOnlyCollection<TValue> this[TKey key]
        {
            get
            {
                Requires.NotNullAllowStructs<TKey>(key, "key");
                MultiValueDictionary<TKey, TValue>.InnerCollectionView result;
                if (this._dictionary.TryGetValue(key, out result))
                {
                    return result;
                }
                throw new KeyNotFoundException(Sr.GetString(Exceptions.KeyNotFound));
            }
        }
        /// <summary>
        /// Returns the number of <typeparamref name="TKey" />s with one or more associated <typeparamref name="TValue" />
        /// in this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.
        /// </summary>
        /// <value>The number of <typeparamref name="TKey" />s in this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.</value>
        public int Count
        {
            get
            {
                return this._dictionary.Count;
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> 
        /// class that is empty, has the default initial capacity, and uses the default
        /// <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for <typeparamref name="TKey" />.
        /// </summary>
        public MultiValueDictionary()
        {
            this._dictionary = new Dictionary<TKey, MultiValueDictionary<TKey, TValue>.InnerCollectionView>();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> class that is 
        /// empty, has the specified initial capacity, and uses the default <see cref="T:System.Collections.Generic.IEqualityComparer`1" />
        /// for <typeparamref name="TKey" />.
        /// </summary>
        /// <param name="capacity">Initial number of keys that the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> will allocate space for</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">capacity must be &gt;= 0</exception>
        public MultiValueDictionary(int capacity)
        {
            Requires.Range(capacity >= 0, "capacity", null);
            this._dictionary = new Dictionary<TKey, MultiValueDictionary<TKey, TValue>.InnerCollectionView>(capacity);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> class 
        /// that is empty, has the default initial capacity, and uses the 
        /// specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.
        /// </summary>
        /// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
        /// <remarks>If <paramref name="comparer" /> is set to null, then the default <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.</remarks>
        public MultiValueDictionary(IEqualityComparer<TKey> comparer)
        {
            this._dictionary = new Dictionary<TKey, MultiValueDictionary<TKey, TValue>.InnerCollectionView>(comparer);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> class 
        /// that is empty, has the specified initial capacity, and uses the 
        /// specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.
        /// </summary>
        /// <param name="capacity">Initial number of keys that the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> will allocate space for</param>
        /// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Capacity must be &gt;= 0</exception>
        /// <remarks>If <paramref name="comparer" /> is set to null, then the default <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.</remarks>
        public MultiValueDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            Requires.Range(capacity >= 0, "capacity", null);
            this._dictionary = new Dictionary<TKey, MultiValueDictionary<TKey, TValue>.InnerCollectionView>(capacity, comparer);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> class that contains 
        /// elements copied from the specified IEnumerable&lt;KeyValuePair&lt;TKey, IReadOnlyCollection&lt;TValue&gt;&gt;&gt; and uses the 
        /// default <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the <typeparamref name="TKey" /> type.
        /// </summary>
        /// <param name="enumerable">IEnumerable to copy elements into this from</param>
        /// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
        public MultiValueDictionary(IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> enumerable)
            : this(enumerable, null)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> class that contains 
        /// elements copied from the specified IEnumerable&lt;KeyValuePair&lt;TKey, IReadOnlyCollection&lt;TValue&gt;&gt;&gt; and uses the 
        /// specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.
        /// </summary>
        /// <param name="enumerable">IEnumerable to copy elements into this from</param>
        /// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
        /// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
        /// <remarks>If <paramref name="comparer" /> is set to null, then the default <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.</remarks>
        public MultiValueDictionary(IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> enumerable, IEqualityComparer<TKey> comparer)
        {
            Requires.NotNullAllowStructs<IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>>>(enumerable, "enumerable");
            this._dictionary = new Dictionary<TKey, MultiValueDictionary<TKey, TValue>.InnerCollectionView>(comparer);
            foreach (var current in enumerable)
            {
                this.AddRange(current.Key, current.Value);
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> class that contains 
        /// elements copied from the specified IEnumerable&lt;IGrouping&lt;TKey, TValue&gt;&gt; and uses the 
        /// default <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the <typeparamref name="TKey" /> type.
        /// </summary>
        /// <param name="enumerable">IEnumerable to copy elements into this from</param>
        /// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
        public MultiValueDictionary(IEnumerable<IGrouping<TKey, TValue>> enumerable)
            : this(enumerable, null)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> class that contains 
        /// elements copied from the specified IEnumerable&lt;IGrouping&lt;TKey, TValue&gt;&gt; and uses the 
        /// specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.
        /// </summary>
        /// <param name="enumerable">IEnumerable to copy elements into this from</param>
        /// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
        /// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
        /// <remarks>If <paramref name="comparer" /> is set to null, then the default <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.</remarks>
        public MultiValueDictionary(IEnumerable<IGrouping<TKey, TValue>> enumerable, IEqualityComparer<TKey> comparer)
        {
            Requires.NotNullAllowStructs<IEnumerable<IGrouping<TKey, TValue>>>(enumerable, "enumerable");
            this._dictionary = new Dictionary<TKey, MultiValueDictionary<TKey, TValue>.InnerCollectionView>(comparer);
            foreach (var current in enumerable)
            {
                this.AddRange(current.Key, current);
            }
        }
        /// <summary>
        /// Creates a new new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> 
        /// class that is empty, has the default initial capacity, and uses the default
        /// <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for <typeparamref name="TKey" />. The 
        /// internal dictionary will use instances of the <typeparamref name="TValueCollection" />
        /// class as its collection type.
        /// </summary>
        /// <typeparam name="TValueCollection">
        /// The collection type that this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />
        /// will contain in its internal dictionary.
        /// </typeparam>
        /// <returns>A new <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> with the specified
        /// parameters.</returns>
        /// <exception cref="T:System.InvalidOperationException"><typeparamref name="TValueCollection" /> must not have
        /// IsReadOnly set to true by default.</exception>
        /// <remarks>
        /// Note that <typeparamref name="TValueCollection" /> must implement <see cref="T:System.Collections.Generic.ICollection`1" />
        /// in addition to being constructable through new(). The collection returned from the constructor
        /// must also not have IsReadOnly set to True by default.
        /// </remarks>
        public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>() where TValueCollection : ICollection<TValue>, new()
        {
            var tValueCollection = (default(TValueCollection) == null) ? Activator.CreateInstance<TValueCollection>() : default(TValueCollection);
            if (tValueCollection.IsReadOnly)
            {
                throw new InvalidOperationException(Sr.GetString(Exceptions.CreateTValueCollectionReadOnly));
            }
            return new MultiValueDictionary<TKey, TValue>
            {
                _newCollectionFactory = () => (default(TValueCollection) == null) ? Activator.CreateInstance<TValueCollection>() : default(TValueCollection)
            };
        }
        /// <summary>
        /// Creates a new new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> 
        /// class that is empty, has the specified initial capacity, and uses the default
        /// <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for <typeparamref name="TKey" />. The 
        /// internal dictionary will use instances of the <typeparamref name="TValueCollection" />
        /// class as its collection type.
        /// </summary>
        /// <typeparam name="TValueCollection">
        /// The collection type that this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />
        /// will contain in its internal dictionary.
        /// </typeparam>
        /// <param name="capacity">Initial number of keys that the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> will allocate space for</param>
        /// <returns>A new <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> with the specified
        /// parameters.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Capacity must be &gt;= 0</exception>
        /// <exception cref="T:System.InvalidOperationException"><typeparamref name="TValueCollection" /> must not have
        /// IsReadOnly set to true by default.</exception>
        /// <remarks>
        /// Note that <typeparamref name="TValueCollection" /> must implement <see cref="T:System.Collections.Generic.ICollection`1" />
        /// in addition to being constructable through new(). The collection returned from the constructor
        /// must also not have IsReadOnly set to True by default.
        /// </remarks>
        public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(int capacity) where TValueCollection : ICollection<TValue>, new()
        {
            Requires.Range(capacity >= 0, "capacity", null);
            var tValueCollection = (default(TValueCollection) == null) ? Activator.CreateInstance<TValueCollection>() : default(TValueCollection);
            if (tValueCollection.IsReadOnly)
            {
                throw new InvalidOperationException(Sr.GetString(Exceptions.CreateTValueCollectionReadOnly));
            }
            return new MultiValueDictionary<TKey, TValue>(capacity)
            {
                _newCollectionFactory = () => (default(TValueCollection) == null) ? Activator.CreateInstance<TValueCollection>() : default(TValueCollection)
            };
        }
        /// <summary>
        /// Creates a new new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> 
        /// class that is empty, has the default initial capacity, and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for <typeparamref name="TKey" />. The 
        /// internal dictionary will use instances of the <typeparamref name="TValueCollection" />
        /// class as its collection type.
        /// </summary>
        /// <typeparam name="TValueCollection">
        /// The collection type that this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />
        /// will contain in its internal dictionary.
        /// </typeparam>
        /// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
        /// <exception cref="T:System.InvalidOperationException"><typeparamref name="TValueCollection" /> must not have
        /// IsReadOnly set to true by default.</exception>
        /// <returns>A new <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> with the specified
        /// parameters.</returns>
        /// <remarks>If <paramref name="comparer" /> is set to null, then the default <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.</remarks>
        /// <remarks>
        /// Note that <typeparamref name="TValueCollection" /> must implement <see cref="T:System.Collections.Generic.ICollection`1" />
        /// in addition to being constructable through new(). The collection returned from the constructor
        /// must also not have IsReadOnly set to True by default.
        /// </remarks>
        public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(IEqualityComparer<TKey> comparer) where TValueCollection : ICollection<TValue>, new()
        {
            var tValueCollection = (default(TValueCollection) == null) ? Activator.CreateInstance<TValueCollection>() : default(TValueCollection);
            if (tValueCollection.IsReadOnly)
            {
                throw new InvalidOperationException(Sr.GetString(Exceptions.CreateTValueCollectionReadOnly));
            }
            return new MultiValueDictionary<TKey, TValue>(comparer)
            {
                _newCollectionFactory = () => (default(TValueCollection) == null) ? Activator.CreateInstance<TValueCollection>() : default(TValueCollection)
            };
        }
        /// <summary>
        /// Creates a new new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> 
        /// class that is empty, has the specified initial capacity, and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for <typeparamref name="TKey" />. The 
        /// internal dictionary will use instances of the <typeparamref name="TValueCollection" />
        /// class as its collection type.
        /// </summary>
        /// <typeparam name="TValueCollection">
        /// The collection type that this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />
        /// will contain in its internal dictionary.
        /// </typeparam>
        /// <param name="capacity">Initial number of keys that the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> will allocate space for</param>
        /// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
        /// <returns>A new <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> with the specified
        /// parameters.</returns>
        /// <exception cref="T:System.InvalidOperationException"><typeparamref name="TValueCollection" /> must not have
        /// IsReadOnly set to true by default.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Capacity must be &gt;= 0</exception>
        /// <remarks>If <paramref name="comparer" /> is set to null, then the default <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.</remarks>
        /// <remarks>
        /// Note that <typeparamref name="TValueCollection" /> must implement <see cref="T:System.Collections.Generic.ICollection`1" />
        /// in addition to being constructable through new(). The collection returned from the constructor
        /// must also not have IsReadOnly set to True by default.
        /// </remarks>
        public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(int capacity, IEqualityComparer<TKey> comparer) where TValueCollection : ICollection<TValue>, new()
        {
            Requires.Range(capacity >= 0, "capacity", null);
            var tValueCollection = (default(TValueCollection) == null) ? Activator.CreateInstance<TValueCollection>() : default(TValueCollection);
            if (tValueCollection.IsReadOnly)
            {
                throw new InvalidOperationException(Sr.GetString(Exceptions.CreateTValueCollectionReadOnly));
            }
            return new MultiValueDictionary<TKey, TValue>(capacity, comparer)
            {
                _newCollectionFactory = () => (default(TValueCollection) == null) ? Activator.CreateInstance<TValueCollection>() : default(TValueCollection)
            };
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> class that contains 
        /// elements copied from the specified IEnumerable&lt;KeyValuePair&lt;TKey, IReadOnlyCollection&lt;TValue&gt;&gt;&gt;
        /// and uses the default <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the <typeparamref name="TKey" /> type.
        /// The internal dictionary will use instances of the <typeparamref name="TValueCollection" />
        /// class as its collection type.
        /// </summary>
        /// <typeparam name="TValueCollection">
        /// The collection type that this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />
        /// will contain in its internal dictionary.
        /// </typeparam>
        /// <param name="enumerable">IEnumerable to copy elements into this from</param>
        /// <returns>A new <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> with the specified
        /// parameters.</returns>
        /// <exception cref="T:System.InvalidOperationException"><typeparamref name="TValueCollection" /> must not have
        /// IsReadOnly set to true by default.</exception>
        /// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
        /// <remarks>
        /// Note that <typeparamref name="TValueCollection" /> must implement <see cref="T:System.Collections.Generic.ICollection`1" />
        /// in addition to being constructable through new(). The collection returned from the constructor
        /// must also not have IsReadOnly set to True by default.
        /// </remarks>
        public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> enumerable) where TValueCollection : ICollection<TValue>, new()
        {
            Requires.NotNullAllowStructs<IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>>>(enumerable, "enumerable");
            var tValueCollection = (default(TValueCollection) == null) ? Activator.CreateInstance<TValueCollection>() : default(TValueCollection);
            if (tValueCollection.IsReadOnly)
            {
                throw new InvalidOperationException(Sr.GetString(Exceptions.CreateTValueCollectionReadOnly));
            }
            var multiValueDictionary = new MultiValueDictionary<TKey, TValue>();
            multiValueDictionary._newCollectionFactory = (() => (default(TValueCollection) == null) ? Activator.CreateInstance<TValueCollection>() : default(TValueCollection));
            foreach (var current in enumerable)
            {
                multiValueDictionary.AddRange(current.Key, current.Value);
            }
            return multiValueDictionary;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> class that contains 
        /// elements copied from the specified IEnumerable&lt;KeyValuePair&lt;TKey, IReadOnlyCollection&lt;TValue&gt;&gt;&gt;
        /// and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the <typeparamref name="TKey" /> type.
        /// The internal dictionary will use instances of the <typeparamref name="TValueCollection" />
        /// class as its collection type.
        /// </summary>
        /// <typeparam name="TValueCollection">
        /// The collection type that this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />
        /// will contain in its internal dictionary.
        /// </typeparam>
        /// <param name="enumerable">IEnumerable to copy elements into this from</param>
        /// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
        /// <returns>A new <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> with the specified
        /// parameters.</returns>
        /// <exception cref="T:System.InvalidOperationException"><typeparamref name="TValueCollection" /> must not have
        /// IsReadOnly set to true by default.</exception>
        /// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
        /// <remarks>If <paramref name="comparer" /> is set to null, then the default <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.</remarks>
        /// <remarks>
        /// Note that <typeparamref name="TValueCollection" /> must implement <see cref="T:System.Collections.Generic.ICollection`1" />
        /// in addition to being constructable through new(). The collection returned from the constructor
        /// must also not have IsReadOnly set to True by default.
        /// </remarks>
        public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> enumerable, IEqualityComparer<TKey> comparer) where TValueCollection : ICollection<TValue>, new()
        {
            Requires.NotNullAllowStructs<IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>>>(enumerable, "enumerable");
            var tValueCollection = (default(TValueCollection) == null) ? Activator.CreateInstance<TValueCollection>() : default(TValueCollection);
            if (tValueCollection.IsReadOnly)
            {
                throw new InvalidOperationException(Sr.GetString(Exceptions.CreateTValueCollectionReadOnly));
            }
            var multiValueDictionary = new MultiValueDictionary<TKey, TValue>(comparer);
            multiValueDictionary._newCollectionFactory = (() => (default(TValueCollection) == null) ? Activator.CreateInstance<TValueCollection>() : default(TValueCollection));
            foreach (var current in enumerable)
            {
                multiValueDictionary.AddRange(current.Key, current.Value);
            }
            return multiValueDictionary;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> class that contains 
        /// elements copied from the specified IEnumerable&lt;&lt;TKey, TValue&gt;&gt;
        /// and uses the default <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the <typeparamref name="TKey" /> type.
        /// The internal dictionary will use instances of the <typeparamref name="TValueCollection" />
        /// class as its collection type.
        /// </summary>
        /// <typeparam name="TValueCollection">
        /// The collection type that this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />
        /// will contain in its internal dictionary.
        /// </typeparam>
        /// <param name="enumerable">IEnumerable to copy elements into this from</param>
        /// <returns>A new <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> with the specified
        /// parameters.</returns>
        /// <exception cref="T:System.InvalidOperationException"><typeparamref name="TValueCollection" /> must not have
        /// IsReadOnly set to true by default.</exception>
        /// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
        /// <remarks>
        /// Note that <typeparamref name="TValueCollection" /> must implement <see cref="T:System.Collections.Generic.ICollection`1" />
        /// in addition to being constructable through new(). The collection returned from the constructor
        /// must also not have IsReadOnly set to True by default.
        /// </remarks>
        public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(IEnumerable<IGrouping<TKey, TValue>> enumerable) where TValueCollection : ICollection<TValue>, new()
        {
            Requires.NotNullAllowStructs<IEnumerable<IGrouping<TKey, TValue>>>(enumerable, "enumerable");
            var tValueCollection = (default(TValueCollection) == null) ? Activator.CreateInstance<TValueCollection>() : default(TValueCollection);
            if (tValueCollection.IsReadOnly)
            {
                throw new InvalidOperationException(Sr.GetString(Exceptions.CreateTValueCollectionReadOnly));
            }
            var multiValueDictionary = new MultiValueDictionary<TKey, TValue>();
            multiValueDictionary._newCollectionFactory = (() => (default(TValueCollection) == null) ? Activator.CreateInstance<TValueCollection>() : default(TValueCollection));
            foreach (var current in enumerable)
            {
                multiValueDictionary.AddRange(current.Key, current);
            }
            return multiValueDictionary;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> class that contains 
        /// elements copied from the specified IEnumerable&lt;IGrouping&lt;TKey, TValue&gt;&gt;
        /// and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the <typeparamref name="TKey" /> type.
        /// The internal dictionary will use instances of the <typeparamref name="TValueCollection" />
        /// class as its collection type.
        /// </summary>
        /// <typeparam name="TValueCollection">
        /// The collection type that this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />
        /// will contain in its internal dictionary.
        /// </typeparam>
        /// <param name="enumerable">IEnumerable to copy elements into this from</param>
        /// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
        /// <returns>A new <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> with the specified
        /// parameters.</returns>
        /// <exception cref="T:System.InvalidOperationException"><typeparamref name="TValueCollection" /> must not have
        /// IsReadOnly set to true by default.</exception>
        /// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
        /// <remarks>If <paramref name="comparer" /> is set to null, then the default <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.</remarks>
        /// <remarks>
        /// Note that <typeparamref name="TValueCollection" /> must implement <see cref="T:System.Collections.Generic.ICollection`1" />
        /// in addition to being constructable through new(). The collection returned from the constructor
        /// must also not have IsReadOnly set to True by default.
        /// </remarks>
        public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(IEnumerable<IGrouping<TKey, TValue>> enumerable, IEqualityComparer<TKey> comparer) where TValueCollection : ICollection<TValue>, new()
        {
            Requires.NotNullAllowStructs<IEnumerable<IGrouping<TKey, TValue>>>(enumerable, "enumerable");
            var tValueCollection = (default(TValueCollection) == null) ? Activator.CreateInstance<TValueCollection>() : default(TValueCollection);
            if (tValueCollection.IsReadOnly)
            {
                throw new InvalidOperationException(Sr.GetString(Exceptions.CreateTValueCollectionReadOnly));
            }
            var multiValueDictionary = new MultiValueDictionary<TKey, TValue>(comparer);
            multiValueDictionary._newCollectionFactory = (() => (default(TValueCollection) == null) ? Activator.CreateInstance<TValueCollection>() : default(TValueCollection));
            foreach (var current in enumerable)
            {
                multiValueDictionary.AddRange(current.Key, current);
            }
            return multiValueDictionary;
        }
        /// <summary>
        /// Creates a new new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> 
        /// class that is empty, has the default initial capacity, and uses the default
        /// <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for <typeparamref name="TKey" />. The 
        /// internal dictionary will use instances of the <typeparamref name="TValueCollection" />
        /// class as its collection type.
        /// </summary>
        /// <typeparam name="TValueCollection">
        /// The collection type that this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />
        /// will contain in its internal dictionary.
        /// </typeparam>
        /// <param name="collectionFactory">A function to create a new <see cref="T:System.Collections.Generic.ICollection`1" /> to use
        /// in the internal dictionary store of this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.</param>
        /// <returns>A new <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> with the specified
        /// parameters.</returns>
        /// <exception cref="T:System.InvalidOperationException"><paramref name="collectionFactory" /> must create collections with
        /// IsReadOnly set to true by default.</exception>
        /// <remarks>
        /// Note that <typeparamref name="TValueCollection" /> must implement <see cref="T:System.Collections.Generic.ICollection`1" />
        /// in addition to being constructable through new(). The collection returned from the constructor
        /// must also not have IsReadOnly set to True by default.
        /// </remarks>
        public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(Func<TValueCollection> collectionFactory) where TValueCollection : ICollection<TValue>
        {
            var tValueCollection = collectionFactory();
            if (tValueCollection.IsReadOnly)
            {
                throw new InvalidOperationException(Sr.GetString(Exceptions.CreateTValueCollectionReadOnly));
            }
            return new MultiValueDictionary<TKey, TValue>
            {
                _newCollectionFactory = () => (ICollection<TValue>)collectionFactory()
            };
        }
        /// <summary>
        /// Creates a new new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> 
        /// class that is empty, has the specified initial capacity, and uses the default
        /// <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for <typeparamref name="TKey" />. The 
        /// internal dictionary will use instances of the <typeparamref name="TValueCollection" />
        /// class as its collection type.
        /// </summary>
        /// <typeparam name="TValueCollection">
        /// The collection type that this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />
        /// will contain in its internal dictionary.
        /// </typeparam>
        /// <param name="capacity">Initial number of keys that the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> will allocate space for</param>
        /// <param name="collectionFactory">A function to create a new <see cref="T:System.Collections.Generic.ICollection`1" /> to use
        /// in the internal dictionary store of this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.</param> 
        /// <returns>A new <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> with the specified
        /// parameters.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Capacity must be &gt;= 0</exception>
        /// <exception cref="T:System.InvalidOperationException"><paramref name="collectionFactory" /> must create collections with
        /// IsReadOnly set to true by default.</exception>
        /// <remarks>
        /// Note that <typeparamref name="TValueCollection" /> must implement <see cref="T:System.Collections.Generic.ICollection`1" />
        /// in addition to being constructable through new(). The collection returned from the constructor
        /// must also not have IsReadOnly set to True by default.
        /// </remarks>
        public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(int capacity, Func<TValueCollection> collectionFactory) where TValueCollection : ICollection<TValue>
        {
            Requires.Range(capacity >= 0, "capacity", null);
            var tValueCollection = collectionFactory();
            if (tValueCollection.IsReadOnly)
            {
                throw new InvalidOperationException(Sr.GetString(Exceptions.CreateTValueCollectionReadOnly));
            }
            return new MultiValueDictionary<TKey, TValue>(capacity)
            {
                _newCollectionFactory = () => (ICollection<TValue>)collectionFactory()
            };
        }
        /// <summary>
        /// Creates a new new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> 
        /// class that is empty, has the default initial capacity, and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for <typeparamref name="TKey" />. The 
        /// internal dictionary will use instances of the <typeparamref name="TValueCollection" />
        /// class as its collection type.
        /// </summary>
        /// <typeparam name="TValueCollection">
        /// The collection type that this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />
        /// will contain in its internal dictionary.
        /// </typeparam>
        /// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
        /// <param name="collectionFactory">A function to create a new <see cref="T:System.Collections.Generic.ICollection`1" /> to use
        /// in the internal dictionary store of this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.</param> 
        /// <exception cref="T:System.InvalidOperationException"><paramref name="collectionFactory" /> must create collections with
        /// IsReadOnly set to true by default.</exception>
        /// <returns>A new <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> with the specified
        /// parameters.</returns>
        /// <remarks>If <paramref name="comparer" /> is set to null, then the default <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.</remarks>
        /// <remarks>
        /// Note that <typeparamref name="TValueCollection" /> must implement <see cref="T:System.Collections.Generic.ICollection`1" />
        /// in addition to being constructable through new(). The collection returned from the constructor
        /// must also not have IsReadOnly set to True by default.
        /// </remarks>
        public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(IEqualityComparer<TKey> comparer, Func<TValueCollection> collectionFactory) where TValueCollection : ICollection<TValue>
        {
            var tValueCollection = collectionFactory();
            if (tValueCollection.IsReadOnly)
            {
                throw new InvalidOperationException(Sr.GetString(Exceptions.CreateTValueCollectionReadOnly));
            }
            return new MultiValueDictionary<TKey, TValue>(comparer)
            {
                _newCollectionFactory = () => (ICollection<TValue>)collectionFactory()
            };
        }
        /// <summary>
        /// Creates a new new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> 
        /// class that is empty, has the specified initial capacity, and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for <typeparamref name="TKey" />. The 
        /// internal dictionary will use instances of the <typeparamref name="TValueCollection" />
        /// class as its collection type.
        /// </summary>
        /// <typeparam name="TValueCollection">
        /// The collection type that this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />
        /// will contain in its internal dictionary.
        /// </typeparam>
        /// <param name="capacity">Initial number of keys that the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> will allocate space for</param>
        /// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
        /// <param name="collectionFactory">A function to create a new <see cref="T:System.Collections.Generic.ICollection`1" /> to use
        /// in the internal dictionary store of this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.</param> 
        /// <returns>A new <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> with the specified
        /// parameters.</returns>
        /// <exception cref="T:System.InvalidOperationException"><paramref name="collectionFactory" /> must create collections with
        /// IsReadOnly set to true by default.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Capacity must be &gt;= 0</exception>
        /// <remarks>If <paramref name="comparer" /> is set to null, then the default <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.</remarks>
        /// <remarks>
        /// Note that <typeparamref name="TValueCollection" /> must implement <see cref="T:System.Collections.Generic.ICollection`1" />
        /// in addition to being constructable through new(). The collection returned from the constructor
        /// must also not have IsReadOnly set to True by default.
        /// </remarks>
        public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(int capacity, IEqualityComparer<TKey> comparer, Func<TValueCollection> collectionFactory) where TValueCollection : ICollection<TValue>
        {
            Requires.Range(capacity >= 0, "capacity", null);
            var tValueCollection = collectionFactory();
            if (tValueCollection.IsReadOnly)
            {
                throw new InvalidOperationException(Sr.GetString(Exceptions.CreateTValueCollectionReadOnly));
            }
            return new MultiValueDictionary<TKey, TValue>(capacity, comparer)
            {
                _newCollectionFactory = () => (ICollection<TValue>)collectionFactory()
            };
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> class that contains 
        /// elements copied from the specified IEnumerable&lt;KeyValuePair&lt;TKey, IReadOnlyCollection&lt;TValue&gt;&gt;&gt;
        /// and uses the default <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the <typeparamref name="TKey" /> type.
        /// The internal dictionary will use instances of the <typeparamref name="TValueCollection" />
        /// class as its collection type.
        /// </summary>
        /// <typeparam name="TValueCollection">
        /// The collection type that this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />
        /// will contain in its internal dictionary.
        /// </typeparam>
        /// <param name="enumerable">IEnumerable to copy elements into this from</param>
        /// <param name="collectionFactory">A function to create a new <see cref="T:System.Collections.Generic.ICollection`1" /> to use
        /// in the internal dictionary store of this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.</param> 
        /// <returns>A new <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> with the specified
        /// parameters.</returns>
        /// <exception cref="T:System.InvalidOperationException"><paramref name="collectionFactory" /> must create collections with
        /// IsReadOnly set to true by default.</exception>
        /// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
        /// <remarks>
        /// Note that <typeparamref name="TValueCollection" /> must implement <see cref="T:System.Collections.Generic.ICollection`1" />
        /// in addition to being constructable through new(). The collection returned from the constructor
        /// must also not have IsReadOnly set to True by default.
        /// </remarks>
        public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> enumerable, Func<TValueCollection> collectionFactory) where TValueCollection : ICollection<TValue>
        {
            Requires.NotNullAllowStructs<IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>>>(enumerable, "enumerable");
            var tValueCollection = collectionFactory();
            if (tValueCollection.IsReadOnly)
            {
                throw new InvalidOperationException(Sr.GetString(Exceptions.CreateTValueCollectionReadOnly));
            }
            var multiValueDictionary = new MultiValueDictionary<TKey, TValue>();
            multiValueDictionary._newCollectionFactory = () => (ICollection<TValue>)collectionFactory();
            foreach (var current in enumerable)
            {
                multiValueDictionary.AddRange(current.Key, current.Value);
            }
            return multiValueDictionary;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> class that contains 
        /// elements copied from the specified IEnumerable&lt;KeyValuePair&lt;TKey, IReadOnlyCollection&lt;TValue&gt;&gt;&gt;
        /// and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the <typeparamref name="TKey" /> type.
        /// The internal dictionary will use instances of the <typeparamref name="TValueCollection" />
        /// class as its collection type.
        /// </summary>
        /// <typeparam name="TValueCollection">
        /// The collection type that this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />
        /// will contain in its internal dictionary.
        /// </typeparam>
        /// <param name="enumerable">IEnumerable to copy elements into this from</param>
        /// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
        /// <param name="collectionFactory">A function to create a new <see cref="T:System.Collections.Generic.ICollection`1" /> to use
        /// in the internal dictionary store of this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.</param> 
        /// <returns>A new <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> with the specified
        /// parameters.</returns>
        /// <exception cref="T:System.InvalidOperationException"><paramref name="collectionFactory" /> must create collections with
        /// IsReadOnly set to true by default.</exception>
        /// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
        /// <remarks>If <paramref name="comparer" /> is set to null, then the default <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.</remarks>
        /// <remarks>
        /// Note that <typeparamref name="TValueCollection" /> must implement <see cref="T:System.Collections.Generic.ICollection`1" />
        /// in addition to being constructable through new(). The collection returned from the constructor
        /// must also not have IsReadOnly set to True by default.
        /// </remarks>
        public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> enumerable, IEqualityComparer<TKey> comparer, Func<TValueCollection> collectionFactory) where TValueCollection : ICollection<TValue>
        {
            Requires.NotNullAllowStructs<IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>>>(enumerable, "enumerable");
            var tValueCollection = collectionFactory();
            if (tValueCollection.IsReadOnly)
            {
                throw new InvalidOperationException(Sr.GetString(Exceptions.CreateTValueCollectionReadOnly));
            }
            var multiValueDictionary = new MultiValueDictionary<TKey, TValue>(comparer);
            multiValueDictionary._newCollectionFactory = () => (ICollection<TValue>)collectionFactory();
            foreach (var current in enumerable)
            {
                multiValueDictionary.AddRange(current.Key, current.Value);
            }
            return multiValueDictionary;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> class that contains 
        /// elements copied from the specified IEnumerable&lt;&lt;TKey, TValue&gt;&gt;
        /// and uses the default <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the <typeparamref name="TKey" /> type.
        /// The internal dictionary will use instances of the <typeparamref name="TValueCollection" />
        /// class as its collection type.
        /// </summary>
        /// <typeparam name="TValueCollection">
        /// The collection type that this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />
        /// will contain in its internal dictionary.
        /// </typeparam>
        /// <param name="enumerable">IEnumerable to copy elements into this from</param>
        /// <param name="collectionFactory">A function to create a new <see cref="T:System.Collections.Generic.ICollection`1" /> to use
        /// in the internal dictionary store of this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.</param> 
        /// <returns>A new <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> with the specified
        /// parameters.</returns>
        /// <exception cref="T:System.InvalidOperationException"><paramref name="collectionFactory" /> must create collections with
        /// IsReadOnly set to true by default.</exception>
        /// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
        /// <remarks>
        /// Note that <typeparamref name="TValueCollection" /> must implement <see cref="T:System.Collections.Generic.ICollection`1" />
        /// in addition to being constructable through new(). The collection returned from the constructor
        /// must also not have IsReadOnly set to True by default.
        /// </remarks>
        public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(IEnumerable<IGrouping<TKey, TValue>> enumerable, Func<TValueCollection> collectionFactory) where TValueCollection : ICollection<TValue>
        {
            Requires.NotNullAllowStructs<IEnumerable<IGrouping<TKey, TValue>>>(enumerable, "enumerable");
            var tValueCollection = collectionFactory();
            if (tValueCollection.IsReadOnly)
            {
                throw new InvalidOperationException(Sr.GetString(Exceptions.CreateTValueCollectionReadOnly));
            }
            var multiValueDictionary = new MultiValueDictionary<TKey, TValue>();
            multiValueDictionary._newCollectionFactory = () => (ICollection<TValue>)collectionFactory();
            foreach (var current in enumerable)
            {
                multiValueDictionary.AddRange(current.Key, current);
            }
            return multiValueDictionary;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> class that contains 
        /// elements copied from the specified IEnumerable&lt;IGrouping&lt;TKey, TValue&gt;&gt;
        /// and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the <typeparamref name="TKey" /> type.
        /// The internal dictionary will use instances of the <typeparamref name="TValueCollection" />
        /// class as its collection type.
        /// </summary>
        /// <typeparam name="TValueCollection">
        /// The collection type that this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />
        /// will contain in its internal dictionary.
        /// </typeparam>
        /// <param name="enumerable">IEnumerable to copy elements into this from</param>
        /// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
        /// <param name="collectionFactory">A function to create a new <see cref="T:System.Collections.Generic.ICollection`1" /> to use
        /// in the internal dictionary store of this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.</param> 
        /// <returns>A new <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> with the specified
        /// parameters.</returns>
        /// <exception cref="T:System.InvalidOperationException"><paramref name="collectionFactory" /> must create collections with
        /// IsReadOnly set to true by default.</exception>
        /// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
        /// <remarks>If <paramref name="comparer" /> is set to null, then the default <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.</remarks>
        /// <remarks>
        /// Note that <typeparamref name="TValueCollection" /> must implement <see cref="T:System.Collections.Generic.ICollection`1" />
        /// in addition to being constructable through new(). The collection returned from the constructor
        /// must also not have IsReadOnly set to True by default.
        /// </remarks>
        public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(IEnumerable<IGrouping<TKey, TValue>> enumerable, IEqualityComparer<TKey> comparer, Func<TValueCollection> collectionFactory) where TValueCollection : ICollection<TValue>
        {
            Requires.NotNullAllowStructs<IEnumerable<IGrouping<TKey, TValue>>>(enumerable, "enumerable");
            var tValueCollection = collectionFactory();
            if (tValueCollection.IsReadOnly)
            {
                throw new InvalidOperationException(Sr.GetString(Exceptions.CreateTValueCollectionReadOnly));
            }
            var multiValueDictionary = new MultiValueDictionary<TKey, TValue>(comparer);
            multiValueDictionary._newCollectionFactory = () => (ICollection<TValue>)collectionFactory();
            foreach (var current in enumerable)
            {
                multiValueDictionary.AddRange(current.Key, current);
            }
            return multiValueDictionary;
        }
        /// <summary>
        /// Adds the specified <typeparamref name="TKey" /> and <typeparamref name="TValue" /> to the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.
        /// </summary>
        /// <param name="key">The <typeparamref name="TKey" /> of the element to add.</param>
        /// <param name="value">The <typeparamref name="TValue" /> of the element to add.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is <c>null</c>.</exception>
        /// <remarks>
        /// Unlike the Add for <see cref="T:System.Collections.IDictionary" />, the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> Add will not
        /// throw any exceptions. If the given <typeparamref name="TKey" /> is already in the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />,
        /// then <typeparamref name="TValue" /> will be added to <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" /> associated with <paramref name="key" />
        /// </remarks>
        /// <remarks>
        /// A call to this Add method will always invalidate any currently running enumeration regardless
        /// of whether the Add method actually modified the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.
        /// </remarks>
        public void Add(TKey key, TValue value)
        {
            Requires.NotNullAllowStructs<TKey>(key, "key");
            MultiValueDictionary<TKey, TValue>.InnerCollectionView innerCollectionView;
            if (!this._dictionary.TryGetValue(key, out innerCollectionView))
            {
                innerCollectionView = new MultiValueDictionary<TKey, TValue>.InnerCollectionView(key, this._newCollectionFactory());
                this._dictionary.Add(key, innerCollectionView);
            }
            innerCollectionView.AddValue(value);
            this._version++;
        }
        /// <summary>
        /// Adds a number of key-value pairs to this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />, where
        /// the key for each value is <paramref name="key" />, and the value for a pair
        /// is an element from <paramref name="values" />
        /// </summary>
        /// <param name="key">The <typeparamref name="TKey" /> of all entries to add</param>
        /// <param name="values">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of values to add</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> and <paramref name="values" /> must be non-null</exception>
        /// <remarks>
        /// A call to this AddRange method will always invalidate any currently running enumeration regardless
        /// of whether the AddRange method actually modified the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.
        /// </remarks>
        public void AddRange(TKey key, IEnumerable<TValue> values)
        {
            Requires.NotNullAllowStructs<TKey>(key, "key");
            Requires.NotNullAllowStructs<IEnumerable<TValue>>(values, "values");
            MultiValueDictionary<TKey, TValue>.InnerCollectionView innerCollectionView;
            if (!this._dictionary.TryGetValue(key, out innerCollectionView))
            {
                innerCollectionView = new MultiValueDictionary<TKey, TValue>.InnerCollectionView(key, this._newCollectionFactory());
                this._dictionary.Add(key, innerCollectionView);
            }
            foreach (var current in values)
            {
                innerCollectionView.AddValue(current);
            }
            this._version++;
        }
        /// <summary>
        /// Removes every <typeparamref name="TValue" /> associated with the given <typeparamref name="TKey" />
        /// from the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.
        /// </summary>
        /// <param name="key">The <typeparamref name="TKey" /> of the elements to remove</param>
        /// <returns><c>true</c> if the removal was successful; otherwise <c>false</c></returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is <c>null</c>.</exception>
        public bool Remove(TKey key)
        {
            Requires.NotNullAllowStructs<TKey>(key, "key");
            MultiValueDictionary<TKey, TValue>.InnerCollectionView innerCollectionView;
            if (this._dictionary.TryGetValue(key, out innerCollectionView) && this._dictionary.Remove(key))
            {
                this._version++;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Removes the first instance (if any) of the given <typeparamref name="TKey" />-<typeparamref name="TValue" /> 
        /// pair from this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />. 
        /// </summary>
        /// <param name="key">The <typeparamref name="TKey" /> of the element to remove</param>
        /// <param name="value">The <typeparamref name="TValue" /> of the element to remove</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> must be non-null</exception>
        /// <returns><c>true</c> if the removal was successful; otherwise <c>false</c></returns>
        /// <remarks>
        /// If the <typeparamref name="TValue" /> being removed is the last one associated with its <typeparamref name="TKey" />, then that 
        /// <typeparamref name="TKey" /> will be removed from the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> and its 
        /// associated <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" /> will be freed as if a call to <see cref="M:System.Collections.Generic.MultiValueDictionary`2.Remove(`0)" />
        /// had been made.
        /// </remarks>
        public bool Remove(TKey key, TValue value)
        {
            Requires.NotNullAllowStructs<TKey>(key, "key");
            MultiValueDictionary<TKey, TValue>.InnerCollectionView innerCollectionView;
            if (this._dictionary.TryGetValue(key, out innerCollectionView) && innerCollectionView.RemoveValue(value))
            {
                if (innerCollectionView.Count == 0)
                {
                    this._dictionary.Remove(key);
                }
                this._version++;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Determines if the given <typeparamref name="TKey" />-<typeparamref name="TValue" /> 
        /// pair exists within this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.
        /// </summary>
        /// <param name="key">The <typeparamref name="TKey" /> of the element.</param>
        /// <param name="value">The <typeparamref name="TValue" /> of the element.</param>
        /// <returns><c>true</c> if found; otherwise <c>false</c></returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> must be non-null</exception>
        public bool Contains(TKey key, TValue value)
        {
            Requires.NotNullAllowStructs<TKey>(key, "key");
            MultiValueDictionary<TKey, TValue>.InnerCollectionView innerCollectionView;
            return this._dictionary.TryGetValue(key, out innerCollectionView) && innerCollectionView.Contains(value);
        }
        /// <summary>
        /// Determines if the given <typeparamref name="TValue" /> exists within this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.
        /// </summary>
        /// <param name="value">A <typeparamref name="TValue" /> to search the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> for</param>
        /// <returns><c>true</c> if the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> contains the <paramref name="value" />; otherwise <c>false</c></returns>      
        public bool ContainsValue(TValue value)
        {
            foreach (var current in this._dictionary.Values)
            {
                if (current.Contains(value))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Gets a read-only <see cref="T:System.Linq.ILookup`2" /> view of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />
        /// that changes as the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> changes.
        /// </summary>
        /// <value>a read-only <see cref="T:System.Linq.ILookup`2" /> view of the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /></value>
        public ILookup<TKey, TValue> AsLookup()
        {
            return new MultiValueDictionary<TKey, TValue>.MultiLookup(this);
        }
        /// <summary>
        /// Removes every <typeparamref name="TKey" /> and <typeparamref name="TValue" /> from this 
        /// <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.
        /// </summary>
        public void Clear()
        {
            this._dictionary.Clear();
            this._version++;
        }
        /// <summary>
        /// Determines if the given <typeparamref name="TKey" /> exists within this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> and has
        /// at least one <typeparamref name="TValue" /> associated with it.
        /// </summary>
        /// <param name="key">The <typeparamref name="TKey" /> to search the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> for</param>
        /// <returns><c>true</c> if the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> contains the requested <typeparamref name="TKey" />;
        /// otherwise <c>false</c>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> must be non-null</exception>
        public bool ContainsKey(TKey key)
        {
            Requires.NotNullAllowStructs<TKey>(key, "key");
            return this._dictionary.ContainsKey(key);
        }
        /// <summary>
        /// Attempts to get the <typeparamref name="TValue" /> associated with the given
        /// <typeparamref name="TKey" /> and place it into <paramref name="value" />.
        /// </summary>
        /// <param name="key">The <typeparamref name="TKey" /> of the element to retrieve</param>
        /// <param name="value">
        /// When this method returns, contains the <typeparamref name="TValue" /> associated with the specified
        /// <typeparamref name="TKey" /> if it is found; otherwise contains the default value of <typeparamref name="TValue" />.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <see cref="T:System.Collections.Generic.MultiValueDictionary`2" /> contains an element with the specified 
        /// <typeparamref name="TKey" />; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> must be non-null</exception>
        public bool TryGetValue(TKey key, out IReadOnlyCollection<TValue> value)
        {
            MultiValueDictionary<TKey, TValue>.InnerCollectionView innerCollectionView;
            var result = this._dictionary.TryGetValue(key, out innerCollectionView);
            value = innerCollectionView;
            return result;
        }
        /// <summary>
        /// Get an Enumerator over the <typeparamref name="TKey" />-<see cref="T:System.Collections.Generic.IReadOnlyCollection`1" />
        /// pairs in this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.
        /// </summary>
        /// <returns>an Enumerator over the <typeparamref name="TKey" />-<see cref="T:System.Collections.Generic.IReadOnlyCollection`1" />
        /// pairs in this <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />.</returns>
        public IEnumerator<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> GetEnumerator()
        {
            return new MultiValueDictionary<TKey, TValue>.Enumerator(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new MultiValueDictionary<TKey, TValue>.Enumerator(this);
        }
    }
}
