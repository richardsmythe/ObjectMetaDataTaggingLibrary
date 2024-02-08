using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ObjectMetaDataTaggingLibrary.Services
{
    public sealed class CustomHashTable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {

        private const int INITIALCAPACITY = 4;       // Default capacity.
        private const double LOADFACTOR = 0.35;      // Threshold for when resizing will happen. When the number of entries is 75% or more of the current array size
        private int _count;                          // Number of entries in the hash table
        private Node<TKey, TValue>[] _buckets;       // Internal array to store key-value pairs
        private readonly object _lock = new object();

        public CustomHashTable()
        {
            _buckets = new Node<TKey, TValue>[INITIALCAPACITY];
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
                //int index = MultiplicativeHashFunction(key);
                //uint index = FNV1aHashFunction(key);
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
                //int index = MultiplicativeHashFunction(key);
                //uint index = FNV1aHashFunction(key);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint DJB2HashFunction(TKey key, int capacity)
        {
            // hash function needs to know the current capacity 
            uint hash = 2166136261;
            foreach (char c in key.ToString())
            {
                hash = (hash << 5) + hash + c;
            }
            return (uint)(hash % capacity);
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey key, TValue value)
        {
            lock (_lock)
            {
                ResizeIfNecessary();

                uint index = DJB2HashFunction(key, _buckets.Length);

                // Get current node at the index
                Node<TKey, TValue> currentNode = _buckets[index];

                if (currentNode != null)
                {
                    // If collision detected, perform linear probing
                    int probeCount = 0;
                    Console.WriteLine($"Collision occurred at index {index} for key {key}");
                    while (currentNode != null)
                    {
                        probeCount++;
                        index = (index + 1) % (uint)_buckets.Length; // move to the next slot

                        // if current slot is available, insert the new node
                        if (_buckets[index] == null)
                        {
                            _buckets[index] = new Node<TKey, TValue> { Key = key, Value = value };
                            _count++;
                            return;
                        }

                        if (probeCount == _buckets.Length)
                        {
                            // If we have probed all slots, resize and start again
                            ResizeIfNecessary();
                            index = DJB2HashFunction(key, _buckets.Length);
                            probeCount = 0;
                        }

                        currentNode = _buckets[index];
                    }
                }
                else
                {
                    // if key doesn't exist
                    _buckets[index] = new Node<TKey, TValue> { Key = key, Value = value };
                }
                _count++;
            }
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue Get(TKey key)
        {
            lock (_lock)
            {
                //int index = MultiplicativeHashFunction(key);
                //uint index = FNV1aHashFunction(key);
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
                // Resize the internal array when the load factor exceeds 0.75.
                // Ensures that the resulting newCapacity is the smallest power of two that
                // is greater than or equal to the doubled current size.
                int newCapacity = GetNextPowerOfTwo(_buckets.Length * 2);
                Node<TKey, TValue>[] newBuckets = new Node<TKey, TValue>[newCapacity];

                // Rehash existing entries into the new array
                foreach (var bucket in _buckets)
                {
                    Node<TKey, TValue> currentNode = bucket;

                    while (currentNode != null)
                    {
                        uint newIndex = DJB2HashFunction(currentNode.Key, newCapacity);

                        Node<TKey, TValue> nextNode = currentNode.Next;

                        currentNode.Next = newBuckets[newIndex];
                        newBuckets[newIndex] = currentNode;

                        currentNode = nextNode;
                    }
                }

                _buckets = newBuckets;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetNextPowerOfTwo(int x)
        {
            // E.g., the next power of two after 5 is 8 so return 8.
            int power = 1;
            while (power < x)
            {
                power *= 2;
            }
            return power;
        }
    }

    public class Node<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public Node<TKey, TValue> Next { get; set; }
    }
}
