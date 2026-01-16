using System;
using NUnit.Framework;
using UnityEngine;

namespace GroveGames.ObjectPool.Unity.Tests
{
    [TestFixture]
    public sealed class ComponentPoolTests
    {
        private BoxCollider _prefab;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("TestPrefab");
            _prefab = go.AddComponent<BoxCollider>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_prefab != null)
            {
                UnityEngine.Object.DestroyImmediate(_prefab.gameObject);
            }
        }

        [Test]
        public void Constructor_WithValidParameters_CreatesPool()
        {
            using var pool = new ComponentPool<BoxCollider>(_prefab, null, 0, 10);

            Assert.That(pool.MaxSize, Is.EqualTo(10));
            Assert.That(pool.Count, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_WithNullPrefab_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ComponentPool<BoxCollider>(null, null, 0, 10));
        }

        [Test]
        public void Constructor_WithNegativeInitialSize_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ComponentPool<BoxCollider>(_prefab, null, -1, 10));
        }

        [Test]
        public void Constructor_WithInitialSizeGreaterThanMaxSize_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ComponentPool<BoxCollider>(_prefab, null, 15, 10));
        }

        [Test]
        public void Constructor_WithZeroMaxSize_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ComponentPool<BoxCollider>(_prefab, null, 0, 0));
        }

        [Test]
        public void Constructor_WithNegativeMaxSize_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ComponentPool<BoxCollider>(_prefab, null, 0, -1));
        }

        [Test]
        public void Rent_FromEmptyPool_CreatesNewComponent()
        {
            using var pool = new ComponentPool<BoxCollider>(_prefab, null, 0, 10);

            var instance = pool.Rent();

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.gameObject.activeSelf, Is.True);

            UnityEngine.Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void Rent_FromPrewarmedPool_ReturnsExistingComponent()
        {
            using var pool = new ComponentPool<BoxCollider>(_prefab, null, 1, 10);

            var instance = pool.Rent();

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.gameObject.activeSelf, Is.True);
            Assert.That(pool.Count, Is.EqualTo(0));

            UnityEngine.Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void Return_DeactivatesGameObject()
        {
            using var pool = new ComponentPool<BoxCollider>(_prefab, null, 0, 10);
            var instance = pool.Rent();

            pool.Return(instance);

            Assert.That(instance.gameObject.activeSelf, Is.False);
            Assert.That(pool.Count, Is.EqualTo(1));

            UnityEngine.Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void RentAndReturn_Cycle_ReusesComponent()
        {
            using var pool = new ComponentPool<BoxCollider>(_prefab, null, 0, 10);
            var first = pool.Rent();
            pool.Return(first);

            var second = pool.Rent();

            Assert.That(second, Is.SameAs(first));

            UnityEngine.Object.DestroyImmediate(second.gameObject);
        }

        [Test]
        public void Clear_RemovesAllPooledObjects()
        {
            using var pool = new ComponentPool<BoxCollider>(_prefab, null, 5, 10);

            pool.Clear();

            Assert.That(pool.Count, Is.EqualTo(0));
        }

        [Test]
        public void Dispose_PreventsRent()
        {
            var pool = new ComponentPool<BoxCollider>(_prefab, null, 0, 10);
            pool.Dispose();

            Assert.Throws<ObjectDisposedException>(() => pool.Rent());
        }

        [Test]
        public void Dispose_PreventsReturn()
        {
            var pool = new ComponentPool<BoxCollider>(_prefab, null, 0, 10);
            var instance = pool.Rent();
            pool.Dispose();

            Assert.Throws<ObjectDisposedException>(() => pool.Return(instance));

            UnityEngine.Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void Dispose_PreventsClear()
        {
            var pool = new ComponentPool<BoxCollider>(_prefab, null, 0, 10);
            pool.Dispose();

            Assert.Throws<ObjectDisposedException>(() => pool.Clear());
        }

        [Test]
        public void Dispose_PreventsCountAccess()
        {
            var pool = new ComponentPool<BoxCollider>(_prefab, null, 0, 10);
            pool.Dispose();

            Assert.Throws<ObjectDisposedException>(() => _ = pool.Count);
        }

        [Test]
        public void Dispose_PreventsMaxSizeAccess()
        {
            var pool = new ComponentPool<BoxCollider>(_prefab, null, 0, 10);
            pool.Dispose();

            Assert.Throws<ObjectDisposedException>(() => _ = pool.MaxSize);
        }

        [Test]
        public void Dispose_CalledMultipleTimes_DoesNotThrow()
        {
            var pool = new ComponentPool<BoxCollider>(_prefab, null, 0, 10);

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

            using var pool = new ComponentPool<BoxCollider>(_prefab, parent, 1, 10);
            var instance = pool.Rent();

            Assert.That(instance.transform.parent, Is.EqualTo(parent));

            UnityEngine.Object.DestroyImmediate(instance.gameObject);
            UnityEngine.Object.DestroyImmediate(parent.gameObject);
        }

        [Test]
        public void Rent_ReturnsCorrectComponentType()
        {
            using var pool = new ComponentPool<BoxCollider>(_prefab, null, 0, 10);

            var instance = pool.Rent();

            Assert.That(instance, Is.TypeOf<BoxCollider>());

            UnityEngine.Object.DestroyImmediate(instance.gameObject);
        }
    }
}
