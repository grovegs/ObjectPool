using System;
using NUnit.Framework;
using UnityEngine;

namespace GroveGames.ObjectPool.Unity.Tests
{
    [TestFixture]
    public sealed class GameObjectPoolTests
    {
        private GameObject _prefab;

        [SetUp]
        public void SetUp()
        {
            _prefab = new GameObject("TestPrefab");
        }

        [TearDown]
        public void TearDown()
        {
            if (_prefab != null)
            {
                UnityEngine.Object.DestroyImmediate(_prefab);
            }
        }

        [Test]
        public void Constructor_WithValidParameters_CreatesPool()
        {
            using var pool = new GameObjectPool(_prefab, null, 0, 10);

            Assert.That(pool.MaxSize, Is.EqualTo(10));
            Assert.That(pool.Count, Is.EqualTo(0));
        }

        [Test]
        public void Rent_FromEmptyPool_CreatesNewGameObject()
        {
            using var pool = new GameObjectPool(_prefab, null, 0, 10);

            var instance = pool.Rent();

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.activeSelf, Is.True);

            UnityEngine.Object.DestroyImmediate(instance);
        }

        [Test]
        public void Rent_FromPrewarmedPool_ReturnsExistingGameObject()
        {
            using var pool = new GameObjectPool(_prefab, null, 1, 10);

            var instance = pool.Rent();

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.activeSelf, Is.True);
            Assert.That(pool.Count, Is.EqualTo(0));

            UnityEngine.Object.DestroyImmediate(instance);
        }

        [Test]
        public void Return_DeactivatesGameObject()
        {
            using var pool = new GameObjectPool(_prefab, null, 0, 10);
            var instance = pool.Rent();

            pool.Return(instance);

            Assert.That(instance.activeSelf, Is.False);
            Assert.That(pool.Count, Is.EqualTo(1));

            UnityEngine.Object.DestroyImmediate(instance);
        }

        [Test]
        public void RentAndReturn_Cycle_ReusesGameObject()
        {
            using var pool = new GameObjectPool(_prefab, null, 0, 10);
            var first = pool.Rent();
            pool.Return(first);

            var second = pool.Rent();

            Assert.That(second, Is.SameAs(first));

            UnityEngine.Object.DestroyImmediate(second);
        }

        [Test]
        public void Clear_RemovesAllPooledObjects()
        {
            using var pool = new GameObjectPool(_prefab, null, 5, 10);

            pool.Clear();

            Assert.That(pool.Count, Is.EqualTo(0));
        }

        [Test]
        public void Dispose_CalledMultipleTimes_DoesNotThrow()
        {
            var pool = new GameObjectPool(_prefab, null, 0, 10);

            Assert.DoesNotThrow(() =>
            {
                pool.Dispose();
                pool.Dispose();
            });
        }

        [Test]
        public void Constructor_WithParent_SetsParentOnInstantiatedObjects()
        {
            var parent = new GameObject("Parent").transform;

            using var pool = new GameObjectPool(_prefab, parent, 1, 10);
            var instance = pool.Rent();

            Assert.That(instance.transform.parent, Is.EqualTo(parent));

            UnityEngine.Object.DestroyImmediate(instance);
            UnityEngine.Object.DestroyImmediate(parent.gameObject);
        }
    }
}
