# GroveGames.ObjectPool

A high-performance object pooling library for .NET, Unity, and Godot with Native AOT support. Built for game development scenarios where allocation-free code is critical.

[![Build Status](https://github.com/grovegs/ObjectPool/actions/workflows/release.yml/badge.svg)](https://github.com/grovegs/ObjectPool/actions/workflows/release.yml)
[![Tests](https://github.com/grovegs/ObjectPool/actions/workflows/tests.yml/badge.svg)](https://github.com/grovegs/ObjectPool/actions/workflows/tests.yml)
[![Latest Release](https://img.shields.io/github/v/release/grovegs/ObjectPool)](https://github.com/grovegs/ObjectPool/releases/latest)
[![NuGet](https://img.shields.io/nuget/v/GroveGames.ObjectPool)](https://www.nuget.org/packages/GroveGames.ObjectPool)

---

## Features

- **Generic Object Pooling**: Pool any reference type with customizable factory, rent, and return callbacks
- **Collection Pools**: Specialized pools for List, Dictionary, Queue, Stack, HashSet, LinkedList, and Array
- **Thread-Safe Variants**: Concurrent versions of all pools for multi-threaded scenarios
- **Multi-Type Pooling**: Pool multiple derived types with a single pool using frozen dictionaries
- **Rental Pattern**: Ref struct-based rentals for automatic return via `using` statements
- **Native AOT Compatible**: Fully supports ahead-of-time compilation for maximum performance
- **Unity Integration**: GameObjectPool and ComponentPool with automatic activation management
- **Godot Integration**: Available as a Godot addon

## .NET

Install via NuGet:

```bash
dotnet add package GroveGames.ObjectPool
```

### Basic Object Pooling

```csharp
using GroveGames.ObjectPool;

var pool = new ObjectPool<MyObject>(
    factory: () => new MyObject(),
    onRent: obj => obj.Initialize(),
    onReturn: obj => obj.Reset(),
    initialSize: 10,
    maxSize: 100
);

var obj = pool.Rent();
obj.DoWork();
pool.Return(obj);

pool.Dispose();
```

### Using the Rental Pattern

The rental pattern provides automatic return via `using` statements with zero-allocation ref structs:

```csharp
using GroveGames.ObjectPool;

var pool = new ObjectPool<MyObject>(
    () => new MyObject(),
    null,
    obj => obj.Reset(),
    0,
    100
);

using (pool.Rent(out var obj))
{
    obj.DoWork();
}
```

### Collection Pools

Pool collections to avoid repeated allocations:

```csharp
using GroveGames.ObjectPool;

var listPool = new ListPool<int>(initialSize: 5, maxSize: 50);

var list = listPool.Rent();
list.Add(1);
list.Add(2);
list.Add(3);
listPool.Return(list);

using (listPool.Rent(out var items))
{
    items.Add(42);
    ProcessItems(items);
}
```

Available collection pools:
- `ListPool<T>` - Pools `List<T>` instances
- `DictionaryPool<TKey, TValue>` - Pools `Dictionary<TKey, TValue>` instances
- `QueuePool<T>` - Pools `Queue<T>` instances
- `StackPool<T>` - Pools `Stack<T>` instances
- `HashSetPool<T>` - Pools `HashSet<T>` instances
- `LinkedListPool<T>` - Pools `LinkedList<T>` instances
- `ArrayPool<T>` - Pools array instances

### Thread-Safe Pools

Use concurrent pools for multi-threaded scenarios:

```csharp
using GroveGames.ObjectPool.Concurrent;

var concurrentPool = new ConcurrentObjectPool<MyObject>(
    () => new MyObject(),
    null,
    obj => obj.Reset(),
    10,
    100
);

var concurrentListPool = new ConcurrentListPool<int>(5, 50);
var concurrentDictPool = new ConcurrentDictionaryPool<string, int>(5, 50);
```

### Multi-Type Object Pool

Pool multiple derived types with a single pool interface:

```csharp
using GroveGames.ObjectPool;

public abstract class Enemy { }
public class Zombie : Enemy { }
public class Skeleton : Enemy { }
public class Ghost : Enemy { }

var pools = new MultiTypeObjectPoolBuilder<Enemy>()
    .AddPool(() => new Zombie(), null, null, 5, 20)
    .AddPool(() => new Skeleton(), null, null, 5, 20)
    .AddPool(() => new Ghost(), null, null, 3, 10)
    .Build();

var multiPool = new MultiTypeObjectPool<Enemy>(pools);

var zombie = multiPool.Rent<Zombie>();
var skeleton = multiPool.Rent<Skeleton>();

multiPool.Return(zombie);
multiPool.Return(skeleton);
```

### Core Components

- **`IObjectPool<T>`**: Core pooling interface with Rent, Return, Clear, and Dispose
- **`ObjectPool<T>`**: Main implementation with customizable callbacks
- **`ObjectRental<T>`**: Ref struct for automatic return via using statements
- **`ListPool<T>`, `DictionaryPool<TKey, TValue>`, etc.**: Specialized collection pools
- **`MultiTypeObjectPool<T>`**: Polymorphic pooling with frozen dictionary lookup
- **`MultiTypeObjectPoolBuilder<T>`**: Fluent builder for multi-type pools

### Concurrent Components

- **`ConcurrentObjectPool<T>`**: Thread-safe object pool
- **`ConcurrentListPool<T>`, `ConcurrentDictionaryPool<TKey, TValue>`, etc.**: Thread-safe collection pools
- **`ConcurrentMultiTypeObjectPool<T>`**: Thread-safe polymorphic pooling

## Unity

There are two installation steps required to use it in Unity.

1. Install `GroveGames.ObjectPool` from NuGet using [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity). Open Window from NuGet → Manage NuGet Packages, search "GroveGames.ObjectPool" and press Install.

2. Install the `GroveGames.ObjectPool.Unity` package by referencing the git URL:

```
https://github.com/grovegs/ObjectPool.git?path=src/GroveGames.ObjectPool.Unity/Packages/com.grovegames.objectpool
```

With the Unity package, `GameObjectPool` and `ComponentPool` become available for pooling Unity objects with automatic activation/deactivation management.

### GameObjectPool

```csharp
using GroveGames.ObjectPool.Unity;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private int _initialSize = 10;
    [SerializeField] private int _maxSize = 100;

    private GameObjectPool _pool;

    private void Awake()
    {
        _pool = new GameObjectPool(_bulletPrefab, transform, _initialSize, _maxSize);
    }

    public GameObject SpawnBullet()
    {
        return _pool.Rent();
    }

    public void DespawnBullet(GameObject bullet)
    {
        _pool.Return(bullet);
    }

    private void OnDestroy()
    {
        _pool.Dispose();
    }
}
```

### GameObjectPool with Rental Pattern

```csharp
using GroveGames.ObjectPool.Unity;
using UnityEngine;

public class EffectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _effectPrefab;
    private GameObjectPool _pool;

    private void Awake()
    {
        _pool = new GameObjectPool(_effectPrefab, transform, 5, 20);
    }

    public void SpawnEffect(Vector3 position)
    {
        using (_pool.Rent(out var effect))
        {
            effect.transform.position = position;
        }
    }
}
```

### ComponentPool

Pool specific components for type-safe access:

```csharp
using GroveGames.ObjectPool.Unity;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private int _maxEnemies = 50;

    private ComponentPool<Enemy> _pool;

    private void Awake()
    {
        _pool = new ComponentPool<Enemy>(_enemyPrefab, transform, 0, _maxEnemies);
    }

    public Enemy SpawnEnemy(Vector3 position)
    {
        var enemy = _pool.Rent();
        enemy.transform.position = position;
        enemy.Initialize();
        return enemy;
    }

    public void DespawnEnemy(Enemy enemy)
    {
        _pool.Return(enemy);
    }

    private void OnDestroy()
    {
        _pool.Dispose();
    }
}
```

### ComponentPool with Rental Pattern

```csharp
using GroveGames.ObjectPool.Unity;
using UnityEngine;

public class ProjectileSystem : MonoBehaviour
{
    [SerializeField] private Rigidbody _projectilePrefab;
    private ComponentPool<Rigidbody> _pool;

    private void Awake()
    {
        _pool = new ComponentPool<Rigidbody>(_projectilePrefab, transform, 10, 100);
    }

    public void FireProjectile(Vector3 position, Vector3 velocity)
    {
        using (_pool.Rent(out var projectile))
        {
            projectile.transform.position = position;
            projectile.linearVelocity = velocity;
        }
    }
}
```

### Unity Components

- **`IGameObjectPool`**: Interface for GameObject pooling
- **`GameObjectPool`**: Pools GameObjects with automatic activation/deactivation
- **`GameObjectRental`**: Ref struct for automatic GameObject return
- **`IComponentPool<T>`**: Interface for Component pooling
- **`ComponentPool<T>`**: Pools Components with automatic activation/deactivation
- **`ComponentRental<T>`**: Ref struct for automatic Component return

## Godot

Install via NuGet:

```bash
dotnet add package GroveGames.ObjectPool.Godot
```

Download the Godot addon from the [latest release](https://github.com/grovegs/ObjectPool/releases/latest) and extract it to your project's `addons` folder. Enable the addon in Project Settings → Plugins.

```text
res://
├── addons/
│   └── GroveGames.ObjectPool/
│       ├── plugin.cfg
│       └── ...
└── ...
```

## Architecture

### Performance Optimizations

1. **Ref Struct Rentals**: Zero-allocation automatic return pattern
2. **Frozen Dictionaries**: O(1) type lookup for multi-type pools
3. **Queue-Based Storage**: Efficient FIFO pooling behavior
4. **Native AOT**: Full compatibility with ahead-of-time compilation
5. **No Boxing**: Generic constraints ensure no boxing allocations

---

## Testing

Run tests:

```bash
dotnet test
```

---

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Write tests for new functionality
4. Submit a pull request

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
