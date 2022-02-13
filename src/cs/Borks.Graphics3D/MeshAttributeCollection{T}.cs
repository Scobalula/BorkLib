using System.Collections;
using System.Diagnostics;

namespace Borks.Graphics3D
{
    /// <summary>
    /// A class to hold an expandable collection of single or multi-dimensional vertex attributes such as positions, normals, etc.
    /// This class is provided for fast and easy access and also to allow quick interop with potential Graphics APIs.
    /// </summary>
    /// <typeparam name="T">Attribute type, this type must be unmanaged and not a reference type.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public class MeshAttributeCollection<T> : ICollection<T>, IEnumerable<T> where T : unmanaged
    {
        /// <summary>
        /// Gets the number of elements.
        /// </summary>
        public int ElementCount => _elementCount;

        /// <summary>
        /// Gets or Sets the dimension.
        /// </summary>
        public int Dimension { get; private set; }

        /// <summary>
        /// Gets the total number of entries within the collection independent of 
        /// </summary>
        public int Count => ElementCount * Dimension;

        /// <summary>
        /// Gets a value indicating if this collection is read only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets whether or not this collection is single dimension.
        /// </summary>
        public bool SingleDimension { get; private set; }

        /// <summary>
        /// Gets or Sets the value at the given index.
        /// </summary>
        public T this[int index]
        {
            get
            {
                return _items[index];
            }
            set
            {
                _items[index] = value;
                _version++;
            }
        }

        /// <summary>
        /// Gets or Sets the value at the given index.
        /// </summary>
        public T this[int elementIndex, int itemIndex]
        {
            get
            {
                return this[elementIndex * Dimension + itemIndex];
            }
            set
            {
                this[elementIndex * Dimension + itemIndex] = value;
            }
        }

        /// <summary>
        /// Gets and sets the capacity of this list.  The capacity is the size of
        /// the internal array used to hold items.  When set, the internal
        /// array of the list is reallocated to the given capacity.
        /// </summary>
        public int Capacity
        {
            get => _items.Length;
            set
            {
                if (value != _items.Length)
                {
                    if (value > 0)
                    {
                        var newItems = new T[value * Dimension];
                        var newCounts = new int[value];

                        if (Count > 0)
                        {
                            Array.Copy(_items, newItems, _items.Length);
                            Array.Copy(_countPerVertex, newCounts, _countPerVertex.Length);
                        }

                        _items = newItems;
                        _countPerVertex = newCounts;
                    }
                    else
                    {
                        _items = s_emptyArray;
                        _countPerVertex = s_emptyCountPerVertex;
                    }
                }
            }
        }

        #region Private Properties
        internal T[] _items;

        internal int[] _countPerVertex;

        private int _version;

        private int _elementCount;

#pragma warning disable CA1825
        private static readonly T[] s_emptyArray = new T[0];
        private static readonly int[] s_emptyCountPerVertex = new int[0];
#pragma warning restore CA1825

        private const int DefaultCapacity = 4;

        private const int DefaultDimension = 1;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshAttributeCollection{T}"/> class
        /// </summary>
        public MeshAttributeCollection()
        {
            Dimension = DefaultDimension;
            _elementCount = 0;

            _items = s_emptyArray;
            _countPerVertex = s_emptyCountPerVertex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshAttributeCollection{T}"/> class
        /// </summary>
        public MeshAttributeCollection(MeshAttributeCollection<T>? from)
        {
            if(from != null && from.Count != 0)
            {
                Dimension = from.Dimension;
                _elementCount = from._elementCount;

                _items = new T[from._items.Length];
                _countPerVertex = new int[from._countPerVertex.Length];

                Array.Copy(from._items, _items, _items.Length);
                Array.Copy(from._countPerVertex, _countPerVertex, _countPerVertex.Length);
            }
            else
            {
                Dimension = DefaultDimension;
                _elementCount = 0;

                _items = s_emptyArray;
                _countPerVertex = s_emptyCountPerVertex;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshAttributeCollection{T}"/> class
        /// </summary>
        /// <param name="vertexCapacity">Initial vertex capacity</param>
        public MeshAttributeCollection(int vertexCapacity)
        {
            Dimension = DefaultDimension;
            _elementCount = 0;

            if (vertexCapacity > 0)
            {
                _items          = new T[vertexCapacity * Dimension];
                _countPerVertex = new int[vertexCapacity];
            }
            else
            {
                _items          = s_emptyArray;
                _countPerVertex = s_emptyCountPerVertex;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshAttributeCollection{T}"/> class
        /// </summary>
        /// <param name="vertexCapacity">Initial vertex capacity</param>
        /// <param name="dimension">Initial dimension/values per vertex</param>
        public MeshAttributeCollection(int vertexCapacity, int dimension)
        {
            Dimension = dimension;
            _elementCount = 0;

            if (vertexCapacity > 0)
            {
                _items          = new T[vertexCapacity * Dimension];
                _countPerVertex = new int[vertexCapacity];
            }
            else
            {
                _items          = s_emptyArray;
                _countPerVertex = s_emptyCountPerVertex;
            }
        }

        public void SetCapacity(int vertexCapacity)
        {
            Dimension = DefaultDimension;
            _elementCount = 0;

            if (vertexCapacity > 0)
            {
                _items = new T[vertexCapacity * Dimension];
                _countPerVertex = new int[vertexCapacity];
            }
            else
            {
                _items = s_emptyArray;
                _countPerVertex = s_emptyCountPerVertex;
            }
        }

        public void SetCapacity(int vertexCapacity, int dimension)
        {
            Dimension = dimension;
            _elementCount = 0;

            if (vertexCapacity > 0)
            {
                _items = new T[vertexCapacity * Dimension];
                _countPerVertex = new int[vertexCapacity];
            }
            else
            {
                _items = s_emptyArray;
                _countPerVertex = s_emptyCountPerVertex;
            }
        }

        /// <summary>
        /// Adds an item to the end of the collection.
        /// </summary>
        /// <param name="item">The item to add to the collection.</param>
        public void Add(T item)
        {
            // Handle expansion if our index is at the count
            if (_elementCount == _countPerVertex.Length)
                Grow(_elementCount + 1);

            // Note: must get with stride
            var index = _elementCount * Dimension + _countPerVertex[_elementCount];

            // Now assign our item and count.
            _items[index] = item;
            _countPerVertex[_elementCount]++;

            if (_countPerVertex[_elementCount] >= Dimension)
                _elementCount++;
        }

        public void Add(T item, int vertexIndex)
        {
            // Handle expansion if our index is at the count
            if (vertexIndex >= _countPerVertex.Length)
                Grow(vertexIndex + 1);
            if (_countPerVertex[vertexIndex] == Dimension)
                GrowDimension(_countPerVertex[vertexIndex] + 1);
            if (vertexIndex >= _elementCount)
                _elementCount = vertexIndex + 1;

            // Note: must get with stride
            var index = vertexIndex * Dimension + _countPerVertex[vertexIndex];

            // Now assign our item and count.
            _items[index] = item;
            _countPerVertex[_elementCount]++;
        }

        /// <summary>
        /// If the index being set is outside the bounds of the collection, the collection is resized.
        /// </summary>
        /// <param name="item">Weight being added.</param>
        /// <param name="vertexIndex">Vertex Index</param>
        /// <param name="weightIndex">Weight Index</param>
        public void Add(T item, int vertexIndex, int weightIndex)
        {
            // Handle expansion if our index is at the count
            if (vertexIndex >= _countPerVertex.Length)
                Grow(vertexIndex + 1);
            if (weightIndex >= Dimension)
                GrowDimension(weightIndex + 1);
            if (vertexIndex >= _elementCount)
                _elementCount = vertexIndex + 1;
            if (weightIndex >= _countPerVertex[vertexIndex])
                _countPerVertex[vertexIndex] = weightIndex;

            // Note: must get with stride
            var index = vertexIndex * Dimension + _countPerVertex[vertexIndex];

            // Now assign our item and count.
            _items[index] = item;
        }

        public int GetCountForElement(int elementIndex)
        {
            return _countPerVertex[elementIndex];
        }

        public void Clear()
        {

        }

        /// <summary>
        /// Checks if the provided item exists in the collection
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>True if it exists, otherwise false</returns>
        public bool Contains(T item) => Array.IndexOf(_items, item) != -1;

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }


        public int FindIndex(Predicate<T> match) => FindIndex(0, ElementCount * Dimension, match);

        public int FindIndex(int vertexIndex, Predicate<T> match) => FindIndex(vertexIndex * Dimension, Dimension, match);

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            int endIndex = startIndex + count;

            for (int i = startIndex; i < endIndex; i++)
            {
                if (match(_items[i]))
                    return i;
            }

            return -1;
        }

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public T[] GetArray()
        {
            return _items;
        }

        #region Private Methods
        private void Grow(int capacity)
        {
            Debug.Assert(_items.Length < capacity);

            int nCapacity = _items.Length == 0 ? DefaultCapacity : 2 * _items.Length;

            if (nCapacity < capacity) nCapacity = capacity;

            Capacity = nCapacity;
        }

        private void GrowDimension(int newValuesPerVertex)
        {
            int nValuesPerVertex = _items.Length == 0 ? ElementCount * Dimension * DefaultCapacity : 2 * _items.Length;

            var newItems = new T[newValuesPerVertex * ElementCount];

            for (int v = 0; v < ElementCount; v++)
            {
                var oldOffset = v * Dimension;
                var newOffset = v * newValuesPerVertex;

                for (int w = 0; w < newValuesPerVertex && w < Dimension; w++)
                {
                    newItems[newOffset + w] = _items[oldOffset + w];
                }
            }
        }
        #endregion

        /// <summary>
        /// A struct to hold a <see cref="MeshAttributeCollection{T}"/> enumerator
        /// </summary>
        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            #region Internal/Private
            private readonly MeshAttributeCollection<T> _collection;
            private int _index;
            private readonly int _version;
            private T _current;

            internal Enumerator(MeshAttributeCollection<T> collection)
            {
                _collection = collection;
                _index = 0;
                _version = collection._version;
                _current = default;
            }
            #endregion

            /// <summary>
            /// Disposes of the current enumerator
            /// </summary>
            public void Dispose()
            {
            }

            /// <summary>
            /// Moves to the next item
            /// </summary>
            public bool MoveNext()
            {
                var localList = _collection;

                if (_version == localList._version && ((uint)_index < (uint)localList.Count))
                {
                    _current = localList._items[_index];
                    _index++;
                    return true;
                }
                return MoveNextRare();
            }

            /// <summary>
            /// Moves to the next item
            /// </summary>
            private bool MoveNextRare()
            {
                if (_version != _collection._version)
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");

                _index = _collection.Count + 1;
                _current = default;
                return false;
            }

            /// <summary>
            /// Gets the current item
            /// </summary>
            public T Current => _current;

            /// <summary>
            /// Gets the current item
            /// </summary>
            object IEnumerator.Current
            {
                get
                {
                    if (_version != _collection._version)
                        throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                    return Current;
                }
            }

            /// <summary>
            /// Resets the enumerator
            /// </summary>
            void IEnumerator.Reset()
            {
                if (_version != _collection._version)
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                _index = 0;
                _current = default;
            }
        }
    }
}
