using BenchmarkDotNet.Attributes;

using GroveGames.ObjectPool;
using GroveGames.ObjectPool.Concurrent;

namespace DotnetBenchmark;

[MemoryDiagnoser]
[ShortRunJob]
public class Benchmark
{
    private const int PoolCount = 8;

    private IndexedObjectPool<Item> _indexed = null!;
    private KeyedObjectPool<int, Item> _keyed = null!;
    private KeyedObjectPool<ItemKind, Item> _enumKeyed = null!;
    private ConcurrentIndexedObjectPool<Item> _concurrentIndexed = null!;
    private ConcurrentKeyedObjectPool<int, Item> _concurrentKeyed = null!;

    [GlobalSetup]
    public void Setup()
    {
        _indexed = new IndexedObjectPool<Item>(static _ => new ObjectPool<Item>(static () => new Item(), null, null, 1, 4));
        _keyed = new KeyedObjectPool<int, Item>(static _ => new ObjectPool<Item>(static () => new Item(), null, null, 1, 4));
        _enumKeyed = new KeyedObjectPool<ItemKind, Item>(static _ => new ObjectPool<Item>(static () => new Item(), null, null, 1, 4));
        _concurrentIndexed = new ConcurrentIndexedObjectPool<Item>(static _ => new ConcurrentObjectPool<Item>(static () => new Item(), null, null, 1, 4));
        _concurrentKeyed = new ConcurrentKeyedObjectPool<int, Item>(static _ => new ConcurrentObjectPool<Item>(static () => new Item(), null, null, 1, 4));

        for (var i = 0; i < PoolCount; i++)
        {
            _indexed.Warm(i);
            _keyed.Warm(i);
            _enumKeyed.Warm((ItemKind)i);
            _concurrentIndexed.Warm(i);
            _concurrentKeyed.Warm(i);
        }
    }

    [Benchmark(Baseline = true)]
    public void IndexedRentReturn()
    {
        for (var i = 0; i < PoolCount; i++)
        {
            _indexed.Return(i, _indexed.Rent(i));
        }
    }

    [Benchmark]
    public void KeyedIntRentReturn()
    {
        for (var i = 0; i < PoolCount; i++)
        {
            _keyed.Return(i, _keyed.Rent(i));
        }
    }

    [Benchmark]
    public void KeyedEnumRentReturn()
    {
        for (var i = 0; i < PoolCount; i++)
        {
            var kind = (ItemKind)i;
            _enumKeyed.Return(kind, _enumKeyed.Rent(kind));
        }
    }

    [Benchmark]
    public void ConcurrentIndexedRentReturn()
    {
        for (var i = 0; i < PoolCount; i++)
        {
            _concurrentIndexed.Return(i, _concurrentIndexed.Rent(i));
        }
    }

    [Benchmark]
    public void ConcurrentKeyedIntRentReturn()
    {
        for (var i = 0; i < PoolCount; i++)
        {
            _concurrentKeyed.Return(i, _concurrentKeyed.Rent(i));
        }
    }

    public sealed class Item
    {
    }

    public enum ItemKind
    {
        Zero,
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven
    }
}
