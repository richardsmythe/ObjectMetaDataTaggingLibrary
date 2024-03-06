using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ObjectMetaDataTaggingLibrary.Services
{
    /// <summary>
    /// Represents a custom hash table implementation tailored for storing key value pairs for the tagging library.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the hash table.</typeparam>
    /// <typeparam name="TValue">The type of values in the hash table.</typeparam>
    public sealed class CustomHashTable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private int _collisionCount;
        private const int INITIALCAPACITY = 2;       // if set higher performance will be higher at cost of memory
        public const double LOADFACTOR = 0.75;       // When the number of entries is 75% or more of the current array size it will then trigger a resize.
        private int _count;                          // Number of entries in the hash table
        private Node<TKey, TValue>[] _buckets;       // Internal array to store key-value pairs
        private readonly object _lock = new object();
        public int CollisionCount => _collisionCount;
        public CustomHashTable()
        {
            _buckets = new Node<TKey, TValue>[INITIALCAPACITY];
        }
 
        public CustomHashTable(int initialCapacity)
        {
            if (initialCapacity <= 0)
            {
                throw new ArgumentException("Initial capacity must be greater than zero.", nameof(initialCapacity));
            }

            _buckets = new Node<TKey, TValue>[initialCapacity];
        } 

        /// <summary>
        /// Gets the load factor of the hash table.
        /// </summary>
        /// <remarks>
        /// The load factor is the ratio of the number of entries in the hash table to the capacity of the hash table.
        /// </remarks>
        public double LoadFactor
        {
            get { return (double)_count / _buckets.Length; }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key does not exist in the hash table.</exception>
        public TValue this[TKey key]
        {
            get
            {
                lock (_lock)
                {
                    return Get(key);
                }
            }
            set
            {
                lock (_lock)
                {
                    Add(key, value);
                }
            }
        }

        /// <summary>
        /// Determines whether the hash table contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the hash table.</param>
        /// <returns>
        /// true if the hash table contains an element with the specified key; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key)
        {
            try
            {
                TValue value = Get(key);
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to get the value associated with the specified key from the hash table.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if the hash table contains an element with the specified key; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            lock (_lock)
            {
                uint index = DJB2HashFunction(key, _buckets.Length);

                Node<TKey, TValue> currentNode = _buckets[index];

                while (currentNode != null)
                {
                    if (currentNode.Key.Equals(key))
                    {
                        value = currentNode.Value;
                        return true;
                    }

                    currentNode = currentNode.Next;
                }

                value = default;
                return false;
            }
        }

        /// <summary>
        /// Tries to remove the key-value pair with the specified key from the hash table.
        /// </summary>
        /// <param name="key">The key of the key-value pair to remove.</param>
        /// <param name="removedValue">When this method returns, contains the value associated with the specified key if the key was found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if the key-value pair was successfully found and removed; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(TKey key, out TValue removedValue)
        {
            lock (_lock)
            {
                uint index = DJB2HashFunction(key, _buckets.Length);

                Node<TKey, TValue> currentNode = _buckets[index];
                Node<TKey, TValue> previousNode = null;

                while (currentNode != null)
                {
                    if (currentNode.Key.Equals(key))
                    {
                        removedValue = currentNode.Value;

                        if (previousNode != null)
                        {
                            previousNode.Next = currentNode.Next;
                        }
                        else
                        {
                            _buckets[index] = currentNode.Next;
                        }

                        _count--;
                        return true;
                    }

                    previousNode = currentNode;
                    currentNode = currentNode.Next;
                }

                removedValue = default;
                return false;
            }
        }

        /// <summary>
        /// Gets the value associated with the specified key if the key exists in the hash table; otherwise, adds the key-value pair to the hash table and returns the specified value.
        /// </summary>
        /// <param name="key">The key to search for or add.</param>
        /// <param name="value">The value to add if the key does not exist.</param>
        /// <returns>
        /// If the key is found, the value associated with the key; otherwise, the specified value.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetOrAdd(TKey key, TValue value)
        {
            lock (_lock)
            {
                try
                {
                    return Get(key);
                }
                catch (KeyNotFoundException)
                {
                    Add(key, value);
                    return value;
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the hash table.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the hash table.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (_lock)
            {
                foreach (var bucket in _buckets)
                {
                    Node<TKey, TValue> currentNode = bucket;

                    while (currentNode != null)
                    {
                        yield return new KeyValuePair<TKey, TValue>(currentNode.Key, currentNode.Value);
                        currentNode = currentNode.Next;
                    }
                }
            }
        }

        /// <summary>
        /// Hashes the specified key using the DJB2 algorithm and returns the index within the hash table.
        /// </summary>
        /// <param name="key">The key to hash.</param>
        /// <param name="capacity">The capacity of the hash table.</param>
        /// <returns>The index within the hash table where the key-value pair should be stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint DJB2HashFunction(TKey key, int capacity)
        {
            uint hash = 5381;

            if (key != null)
            {
                string keyString = key.ToString();
                foreach (char c in keyString)
                {
                    hash = ((hash << 4) + hash) ^ c;
                }
            }

            return hash % (uint)capacity;
        }

        /// <summary>
        /// Adds the specified key-value pair to the hash table. Uses chaining to handle collisions.
        /// </summary>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">The value of the entry to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey key, TValue value)
        {
            lock (_lock)
            {
                ResizeIfNecessary();
                uint index = DJB2HashFunction(key, _buckets.Length);

                // Add to bucket
                if (_buckets[index] == null)
                {
                    _buckets[index] = new Node<TKey, TValue> { Key = key, Value = value };
                    _count++;
                    return;
                }

                // Collision detected, add to the chain
                Node<TKey, TValue> currentNode = _buckets[index];
                while (currentNode.Next != null)
                {
                    // If the key already exists in the chain, update the value
                    if (currentNode.Key.Equals(key))
                    {
                        currentNode.Value = value;
                        return;
                    }
                    currentNode = currentNode.Next;
                    _collisionCount++;
                }

                // Here currentNode points to the last node in the chain
                currentNode.Next = new Node<TKey, TValue> { Key = key, Value = value };
                _count++;

            }
        }

        /// <summary>
        /// Prints the contents of the hash table to the console, including each key-value pair and the total number of collisions.
        /// </summary>
        public void Print()
        {
            lock (_lock)
            {
                for (int i = 0; i < _buckets.Length; i++)
                {
                    Node<TKey, TValue> currentNode = _buckets[i];
                    Console.Write($"[{i}]: ");
                    while (currentNode != null)
                    {
                        Console.Write($"[{currentNode.Key} - {currentNode.Value}] ");
                        currentNode = currentNode.Next;
                    }
                    Console.WriteLine();
                }

                Console.WriteLine($"\nTotal collisions: {CollisionCount}");
            }
        }

        /// <summary>
        /// Retrieves the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to retrieve.</param>
        /// <returns>The value associated with the specified key.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the key is not found in the hash table.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue Get(TKey key)
        {
            lock (_lock)
            {
                uint index = DJB2HashFunction(key, _buckets.Length);

                Node<TKey, TValue> currentNode = _buckets[index];

                while (currentNode != null)
                {
                    if (currentNode.Key.Equals(key))
                    {
                        return currentNode.Value;
                    }

                    currentNode = currentNode.Next;
                }

                throw new KeyNotFoundException("The key was not found");
            }
        }


        /// <summary>
        /// Resizes the hash table if necessary to maintain the load factor.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeIfNecessary()
        {
            if (LoadFactor >= LOADFACTOR)
            {
                var oldBuckets = _buckets;
                 
                int newCapacity = PowerOf2(_buckets.Length * 2);              
                _buckets = new Node<TKey, TValue>[newCapacity];
          
                _count = 0;
                _collisionCount = 0;

                foreach (var bucket in oldBuckets)
                {
                    Node<TKey, TValue> currentNode = bucket;

                    while (currentNode != null)
                    {
                        Add(currentNode.Key, currentNode.Value);
                        currentNode = currentNode.Next;
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the next power of 2 greater than or equal to the specified capacity.
        /// </summary>
        /// <param name="capacity">The capacity for which to calculate the next power of 2.</param>
        /// <returns>The next power of 2 greater than or equal to the specified capacity.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int PowerOf2(int capacity)
        {
            return (int)BitOperations.RoundUpToPowerOf2((uint)capacity);
        }

    }


    /// <summary>
    /// Represents a node in the custom hash table.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class Node<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public Node<TKey, TValue> Next { get; set; }
       
    }

}
