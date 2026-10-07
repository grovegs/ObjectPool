using NUnit.Framework;
using UnityEngine;

namespace GroveGames.ObjectPool.Unity.Tests
{
    [TestFixture]
    public sealed class KeyedGameObjectPoolTests
    {
        private GameObject _firstPrefab;
        private GameObject _secondPrefab;

        [SetUp]
        public void SetUp()
        {
            _firstPrefab = new GameObject("FirstPrefab");
            _secondPrefab = new GameObject("SecondPrefab");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_firstPrefab);
            Object.DestroyImmediate(_secondPrefab);
        }

        [Test]
        public void Rent_StringKey_InstantiatesProvidedPrefab()
        {
            using var pool = new KeyedGameObjectPool<string>(key => key == "second" ? _secondPrefab : _firstPrefab, null, 0, 10);

            var instance = pool.Rent("second");

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.name, Does.StartWith(_secondPrefab.name));
            Assert.That(instance.activeSelf, Is.True);

            Object.DestroyImmediate(instance);
        }

        [Test]
        public void Return_RentedInstance_DeactivatesAndStoresIt()
        {
            using var pool = new KeyedGameObjectPool<string>(_ => _firstPrefab, null, 0, 10);
            var instance = pool.Rent("first");

            pool.Return("first", instance);

            Assert.That(instance.activeSelf, Is.False);
            Assert.That(pool.Count("first"), Is.EqualTo(1));
        }

        [Test]
        public void Rent_AfterReturn_ReusesInstance()
        {
            using var pool = new KeyedGameObjectPool<string>(_ => _firstPrefab, null, 0, 10);
            var instance = pool.Rent("first");
            pool.Return("first", instance);

            var reused = pool.Rent("first");

            Assert.That(reused, Is.SameAs(instance));
        }

        [Test]
        public void Warm_Key_PreAllocatesInstances()
        {
            using var pool = new KeyedGameObjectPool<string>(_ => _firstPrefab, null, 4, 10);

            pool.Warm("first");

            Assert.That(pool.Count("first"), Is.EqualTo(4));
            Assert.That(pool.Count("second"), Is.EqualTo(0));
        }

        [Test]
        public void Return_NullItem_LogsError()
        {
            using var pool = new KeyedGameObjectPool<string>(_ => _firstPrefab, null, 0, 10);

            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "Returned item cannot be null");
            pool.Return("first", null!);

            Assert.That(pool.Count("first"), Is.EqualTo(0));
        }
    }
}
