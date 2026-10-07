using NUnit.Framework;
using UnityEngine;

namespace GroveGames.ObjectPool.Unity.Tests
{
    [TestFixture]
    public sealed class IndexedComponentPoolTests
    {
        private enum Effect
        {
            First,
            Second
        }

        private BoxCollider[] _prefabs;

        [SetUp]
        public void SetUp()
        {
            _prefabs = new[]
            {
                new GameObject("FirstPrefab").AddComponent<BoxCollider>(),
                new GameObject("SecondPrefab").AddComponent<BoxCollider>()
            };
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var prefab in _prefabs)
            {
                Object.DestroyImmediate(prefab.gameObject);
            }
        }

        [Test]
        public void Rent_EnumIndex_InstantiatesPrefabAtThatIndex()
        {
            using var pool = new IndexedComponentPool<BoxCollider>(index => _prefabs[index], null, 0, 10);

            var instance = pool.Rent((int)Effect.Second);

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.gameObject.name, Does.StartWith(_prefabs[1].gameObject.name));
            Assert.That(instance.gameObject.activeSelf, Is.True);

            Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void Return_RentedInstance_DeactivatesAndStoresIt()
        {
            using var pool = new IndexedComponentPool<BoxCollider>(index => _prefabs[index], null, 0, 10);
            var instance = pool.Rent(0);

            pool.Return(0, instance);

            Assert.That(instance.gameObject.activeSelf, Is.False);
            Assert.That(pool.Count(0), Is.EqualTo(1));
            Assert.That(pool.Count(1), Is.EqualTo(0));
        }

        [Test]
        public void Rent_AfterReturn_ReusesInstance()
        {
            using var pool = new IndexedComponentPool<BoxCollider>(index => _prefabs[index], null, 0, 10);
            var instance = pool.Rent(0);
            pool.Return(0, instance);

            var reused = pool.Rent(0);

            Assert.That(reused, Is.SameAs(instance));
        }

        [Test]
        public void Warm_Index_PreAllocatesInstances()
        {
            using var pool = new IndexedComponentPool<BoxCollider>(index => _prefabs[index], null, 3, 10);

            pool.Warm(1);

            Assert.That(pool.Count(1), Is.EqualTo(3));
            Assert.That(pool.Count(0), Is.EqualTo(0));
        }

        [Test]
        public void Rent_NegativeIndex_LogsErrorAndReturnsNull()
        {
            using var pool = new IndexedComponentPool<BoxCollider>(index => _prefabs[index], null, 0, 10);

            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "Index cannot be negative: -1");
            var instance = pool.Rent(-1);

            Assert.That(instance, Is.Null);
        }

        [Test]
        public void Dispose_CalledMultipleTimes_DoesNotThrow()
        {
            var pool = new IndexedComponentPool<BoxCollider>(index => _prefabs[index], null, 0, 10);

            Assert.DoesNotThrow(() =>
            {
                pool.Dispose();
                pool.Dispose();
            });
        }
    }
}
