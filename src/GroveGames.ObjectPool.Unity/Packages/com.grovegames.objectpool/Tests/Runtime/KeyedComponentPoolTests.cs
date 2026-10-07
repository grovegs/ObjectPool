using System;
using NUnit.Framework;
using UnityEngine;

namespace GroveGames.ObjectPool.Unity.Tests
{
    [TestFixture]
    public sealed class KeyedComponentPoolTests
    {
        private BoxCollider _firstPrefab;
        private BoxCollider _secondPrefab;

        [SetUp]
        public void SetUp()
        {
            _firstPrefab = new GameObject("FirstPrefab").AddComponent<BoxCollider>();
            _secondPrefab = new GameObject("SecondPrefab").AddComponent<BoxCollider>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_firstPrefab.gameObject);
            UnityEngine.Object.DestroyImmediate(_secondPrefab.gameObject);
        }

        [Test]
        public void Rent_PrefabKey_InstantiatesThatPrefab()
        {
            using var pool = new KeyedComponentPool<BoxCollider, BoxCollider>(static prefab => prefab, null, 0, 10);

            var instance = pool.Rent(_secondPrefab);

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.gameObject.name, Does.StartWith(_secondPrefab.gameObject.name));
            Assert.That(instance.gameObject.activeSelf, Is.True);

            UnityEngine.Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void Return_RentedInstance_DeactivatesAndStoresIt()
        {
            using var pool = new KeyedComponentPool<BoxCollider, BoxCollider>(static prefab => prefab, null, 0, 10);
            var instance = pool.Rent(_firstPrefab);

            pool.Return(_firstPrefab, instance);

            Assert.That(instance.gameObject.activeSelf, Is.False);
            Assert.That(pool.Count(_firstPrefab), Is.EqualTo(1));
            Assert.That(pool.Count(_secondPrefab), Is.EqualTo(0));
        }

        [Test]
        public void Rent_AfterReturn_ReusesInstance()
        {
            using var pool = new KeyedComponentPool<BoxCollider, BoxCollider>(static prefab => prefab, null, 0, 10);
            var instance = pool.Rent(_firstPrefab);
            pool.Return(_firstPrefab, instance);

            var reused = pool.Rent(_firstPrefab);

            Assert.That(reused, Is.SameAs(instance));
            Assert.That(reused.gameObject.activeSelf, Is.True);
        }

        [Test]
        public void Rent_StringKey_UsesProvidedPrefab()
        {
            using var pool = new KeyedComponentPool<string, BoxCollider>(key => key == "first" ? _firstPrefab : _secondPrefab, null, 0, 10);

            var instance = pool.Rent("first");

            Assert.That(instance.gameObject.name, Does.StartWith(_firstPrefab.gameObject.name));

            UnityEngine.Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void Rent_WithParent_InstantiatesUnderParent()
        {
            var parent = new GameObject("Parent").transform;
            using var pool = new KeyedComponentPool<BoxCollider, BoxCollider>(static prefab => prefab, parent, 0, 10);

            var instance = pool.Rent(_firstPrefab);

            Assert.That(instance.transform.parent, Is.EqualTo(parent));

            UnityEngine.Object.DestroyImmediate(parent.gameObject);
        }

        [Test]
        public void Warm_Key_PreAllocatesInstances()
        {
            using var pool = new KeyedComponentPool<BoxCollider, BoxCollider>(static prefab => prefab, null, 3, 10);

            pool.Warm(_firstPrefab);

            Assert.That(pool.Count(_firstPrefab), Is.EqualTo(3));
            Assert.That(pool.Count(_secondPrefab), Is.EqualTo(0));
        }

        [Test]
        public void Clear_Key_EmptiesOnlyThatPool()
        {
            using var pool = new KeyedComponentPool<BoxCollider, BoxCollider>(static prefab => prefab, null, 2, 10);
            pool.Warm(_firstPrefab);
            pool.Warm(_secondPrefab);

            pool.Clear(_firstPrefab);

            Assert.That(pool.Count(_firstPrefab), Is.EqualTo(0));
            Assert.That(pool.Count(_secondPrefab), Is.EqualTo(2));
        }

        [Test]
        public void Rent_AfterDispose_ReturnsNull()
        {
            var pool = new KeyedComponentPool<BoxCollider, BoxCollider>(static prefab => prefab, null, 0, 10);
            pool.Dispose();

            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("is disposed"));
            var instance = pool.Rent(_firstPrefab);

            Assert.That(instance, Is.Null);
        }

        [Test]
        public void Constructor_NullPrefabProvider_LogsError()
        {
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "Prefab provider cannot be null");

            using var pool = new KeyedComponentPool<BoxCollider, BoxCollider>(null!, null, 0, 10);
        }

        [Test]
        public void Dispose_CalledMultipleTimes_DoesNotThrow()
        {
            var pool = new KeyedComponentPool<BoxCollider, BoxCollider>(static prefab => prefab, null, 0, 10);

            Assert.DoesNotThrow(() =>
            {
                pool.Dispose();
                pool.Dispose();
            });
        }
    }
}
