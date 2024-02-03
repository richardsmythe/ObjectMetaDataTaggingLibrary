
namespace ObjectMetaDataTaggingLibrary.Services
{
    public sealed class CustomHashTable<TKey, TValue>
    {
        private const int INITIALCAPACITY = 4;       // Initial capacity for the internal array
        private const double LOADFACTOR = 0.75;      // Threshold for when resizing will happen. When the number of entries is 75% or more of the current array size
        private int _count;                          // Number of entries in the hashtable
        private List<Node<TKey, TValue>>[] _buckets; // Internal array to store key-value pairs
        private readonly int _prime;
        private readonly object _lock = new object();
        private static Random random = new Random();

        public CustomHashTable()
        {
            _prime = GenerateRandomPrime();
            _buckets = new List<Node<TKey, TValue>>[INITIALCAPACITY];
        }

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

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_lock)
            {
                int index = PolynomialHashFunction(key);
                List<Node<TKey, TValue>> bucket = _buckets[index];

                if (bucket != null)
                {
                    Node<TKey, TValue> node = bucket.Find(n => n.Key.Equals(key));

                    if (node != null)
                    {
                        value = node.Value;
                        return true;
                    }
                }

                value = default;
                return false;
            }
        }


        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (_lock)
            {
                foreach (var bucket in _buckets)
                {
                    if (bucket != null)
                    {
                        foreach (var node in bucket)
                        {
                            yield return new KeyValuePair<TKey, TValue>(node.Key, node.Value);
                        }
                    }
                }
            }
        }

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

        public bool TryRemove(TKey key, out TValue removedValue)
        {
            lock (_lock)
            {
                int index = PolynomialHashFunction(key);
                List<Node<TKey, TValue>> bucket = _buckets[index];

                if (bucket != null)
                {
                    Node<TKey, TValue> node = bucket.Find(n => n.Key.Equals(key));

                    if (node != null)
                    {
                        removedValue = node.Value;
                        bucket.Remove(node);
                        return true;
                    }
                }

                removedValue = default;
                return false;
            }
        }


        public void Print()
        {
            for (int i = 0; i < _buckets.Length; i++)
            {
                List<Node<TKey, TValue>> bucket = _buckets[i];

                if (bucket == null || bucket.Count == 0)
                {
                    Console.WriteLine($"[{i}]: null");
                }
                else
                {
                    Console.Write($"[{i}]: ");

                    for (int j = 0; j < bucket.Count; j++)
                    {
                        var node = bucket[j];

                        Console.Write($"({node.Key}, {node.Value})");

                        if (node.Next != null)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(" --> Collision --> ");
                            Console.ResetColor();
                        }

                        if (j < bucket.Count - 1)
                        {
                            Console.Write(" | ");
                        }
                    }

                    Console.WriteLine();
                }
            }
        }

        private int PolynomialHashFunction(TKey key)
        {
            // Might be better to use the built in getHashCode() but this
            // provides more control which may be useful.

            // The distribution of hash codes across buckets is influenced
            // by the combination of the input key and the polynomial hash function,
            // aiming to spread different keys evenly across the available buckets.
            // The goal is to achieve a balanced distribution of entries.

            string keyStr = key.ToString();
            int hashCode = 0;
            for (int i = 0; i < keyStr.Length; i++)
            {
                hashCode = (hashCode * _prime + keyStr[i]) % _buckets.Length;
            }

            return hashCode;
        }

        public void Add(TKey key, TValue value)
        {
            lock (_lock)
            {
                ResizeIfNecessary();

                int index = PolynomialHashFunction(key);

                if (_buckets[index] == null)
                {
                    _buckets[index] = new List<Node<TKey, TValue>>();
                }

                List<Node<TKey, TValue>> bucket = _buckets[index];

                // Check if the key already exists in the bucket
                Node<TKey, TValue> existingNode = bucket.Find(node => node.Key.Equals(key));

                if (existingNode != null)
                {
                    // Key already exists, update the value
                    existingNode.Value = value;
                }
                else
                {
                    // Key doesn't exist, add a new node to the bucket
                    Node<TKey, TValue> newNode = new Node<TKey, TValue> { Key = key, Value = value };
                    bucket.Add(newNode);

                    // Connect nodes within the same bucket
                    if (bucket.Count > 1)
                    {
                        Node<TKey, TValue> previousNode = bucket[bucket.Count - 2];
                        previousNode.Next = newNode;
                    }

                    _count++;
                }
            }
        }

        public TValue Get(TKey key)
        {
            lock (_lock)
            {
                int index = PolynomialHashFunction(key);
                List<Node<TKey, TValue>> bucket = _buckets[index];

                if (bucket != null)
                {
                    Node<TKey, TValue> node = bucket.Find(n => n.Key.Equals(key));

                    if (node != null)
                    {
                        return node.Value;
                    }
                }

                throw new KeyNotFoundException("The key was not found");
            }
        }

        private void ResizeIfNecessary()
        {
            if (LoadFactor >= LOADFACTOR)
            {
                // Resize the internal array when the load factor exceeds 0.75.
                // Ensures that the resulting newCapacity is the smallest power of two that
                // is greater than or equal to the doubled current size.
                int newCapacity = GetNextPowerOfTwo(_buckets.Length * 2);

                List<Node<TKey, TValue>>[] newBuckets = new List<Node<TKey, TValue>>[newCapacity];

                // Rehash existing entries into the new array
                foreach (var bucket in _buckets)
                {
                    if (bucket != null)
                    {
                        foreach (var node in bucket)
                        {
                            int newIndex = PolynomialHashFunction(node.Key);
                            if (newBuckets[newIndex] == null)
                            {
                                newBuckets[newIndex] = new List<Node<TKey, TValue>>();
                            }
                            newBuckets[newIndex].Add(node);
                        }
                    }
                }

                _buckets = newBuckets;
            }
        }

        private int GetNextPowerOfTwo(int x)
        {
            // E.g the next power of two after 5 is 8 so return 8.
            int power = 1;
            while (power < x)
            {
                power *= 2;
            }
            return power;
        }

        private int GenerateRandomPrime()
        {
            int min = 9999;
            int max = 19999;
            int prime = 0;
            bool isPrime = false;

            while (!isPrime)
            {
                isPrime = true;
                prime = random.Next(min, max + 1);

                if (prime % 2 == 0 && prime > 2)
                {
                    isPrime = false;  // Skip even numbers greater than 2
                    continue;
                }

                for (int i = 3; i <= Math.Sqrt(prime); i += 2)
                {
                    if (prime % i == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }
            }

            return prime;
        }
    }

    public class Node<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public Node<TKey, TValue> Next { get; set; }
        public Node<TKey, TValue> Previous { get; set; }
    }
}
