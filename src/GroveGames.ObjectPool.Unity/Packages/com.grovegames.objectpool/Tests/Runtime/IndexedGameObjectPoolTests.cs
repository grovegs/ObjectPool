using NUnit.Framework;
using UnityEngine;

namespace GroveGames.ObjectPool.Unity.Tests
{
    [TestFixture]
    public sealed class IndexedGameObjectPoolTests
    {
        private GameObject[] _prefabs;

        [SetUp]
        public void SetUp()
        {
            _prefabs = new[] { new GameObject("FirstPrefab"), new GameObject("SecondPrefab") };
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var prefab in _prefabs)
            {
                Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Rent_Index_InstantiatesPrefabAtThatIndex()
        {
            using var pool = new IndexedGameObjectPool(index => _prefabs[index], null, 0, 10);

            var instance = pool.Rent(1);

            Assert.That(instance.name, Does.StartWith(_prefabs[1].name));
            Assert.That(instance.activeSelf, Is.True);

            Object.DestroyImmediate(instance);
        }

        [Test]
        public void Return_RentedInstance_DeactivatesAndStoresIt()
        {
            using var pool = new IndexedGameObjectPool(index => _prefabs[index], null, 0, 10);
            var instance = pool.Rent(0);

            pool.Return(0, instance);

            Assert.That(instance.activeSelf, Is.False);
            Assert.That(pool.Count(0), Is.EqualTo(1));
        }

        [Test]
        public void Clear_Index_EmptiesOnlyThatPool()
        {
            using var pool = new IndexedGameObjectPool(index => _prefabs[index], null, 2, 10);
            pool.Warm(0);
            pool.Warm(1);

            pool.Clear(0);

            Assert.That(pool.Count(0), Is.EqualTo(0));
            Assert.That(pool.Count(1), Is.EqualTo(2));
        }

        [Test]
        public void Warm_NegativeIndex_LogsError()
        {
            using var pool = new IndexedGameObjectPool(index => _prefabs[index], null, 2, 10);

            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "Index cannot be negative: -2");
            pool.Warm(-2);
        }
    }
}
