using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ObjectMetaDataTaggingLibrary.Services
{
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

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public double LoadFactor
        {
            get { return (double)_count / _buckets.Length; }
        }

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

        public uint DJB2HashFunction(TKey key, int capacity)
        {
            uint hash = 5381;

            if (key != null)
            {
                string keyString = key.ToString();
                foreach (char c in keyString)
                {
                    hash = ((hash << 5) + hash) ^ c;
                }
            }

            return hash % (uint)capacity;
        }


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
                        //Console.Write($"[Tag:{i}] ");
                        currentNode = currentNode.Next;
                    }
                    Console.WriteLine();
                }

                Console.WriteLine($"\nTotal collisions: {CollisionCount}");
            }
        }




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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeIfNecessary()
        {
            if (LoadFactor >= LOADFACTOR)
            {
                var oldBuckets = _buckets;
                 
                int newCapacity = PowerOf2(_buckets.Length * 2);
                Console.WriteLine($"NEW CAPACITY: {newCapacity}");
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int PowerOf2(int capacity)
        {
            return (int)BitOperations.RoundUpToPowerOf2((uint)capacity);
        }

    }

    public class Node<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public Node<TKey, TValue> Next { get; set; }
       
    }

}
