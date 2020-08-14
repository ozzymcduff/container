﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity;
using Unity.Lifetime;

namespace Lifetime.Managers
{
    public abstract class LifetimeManagerTests
    {
        #region Initialization

        protected ICollection<IDisposable> LifetimeContainer;
        protected ICollection<IDisposable> OtherContainer;
        protected LifetimeManager TestManager;

        protected object TestObject;

        protected abstract LifetimeManager GetManager();


        [TestInitialize]
        public virtual void SetupTest()
        {
            TestManager = GetManager();
            LifetimeContainer = new FakeLifetimeContainer();
            OtherContainer = new FakeLifetimeContainer();
            TestObject = new object();
        }

        #endregion


        #region   LifetimeManager Members

        [TestMethod]
        public virtual void CloneTest()
        {
            // Act
            var clone = TestManager.Clone();

            // Validate
            Assert.IsInstanceOfType(clone, TestManager.GetType());
        }

        [TestMethod]
        public void ToStringTest()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(TestManager.ToString()));
        }

        #endregion


        #region Get Value

        [TestMethod]
        public virtual void TryGetValueTest()
        {
            // Validate
            Assert.AreSame(LifetimeManager.NoValue, TestManager.TryGetValue(LifetimeContainer));

            // Act
            TestManager.SetValue(TestObject, LifetimeContainer);

            // Validate
            Assert.AreSame(TestObject, TestManager.TryGetValue(LifetimeContainer));
        }

        [TestMethod]
        public virtual void GetValueTest()
        {
            // Validate
            Assert.AreSame(LifetimeManager.NoValue, TestManager.GetValue(LifetimeContainer));

            // Act
            TestManager.SetValue(TestObject, LifetimeContainer);

            // Validate
            Assert.AreSame(TestObject, TestManager.GetValue(LifetimeContainer));
        }

        [TestMethod]
        public virtual void TryGetTest()
        {
            // Validate
            Assert.AreSame(LifetimeManager.NoValue, TestManager.TryGetValue(LifetimeContainer));

            // Act
            TestManager.SetValue(TestObject, LifetimeContainer);

            // Validate
            Assert.AreSame(TestObject, TestManager.TryGetValue(LifetimeContainer));
        }

        [TestMethod]
        public virtual void GetTest()
        {
            // Validate
            Assert.AreSame(LifetimeManager.NoValue, TestManager.GetValue(LifetimeContainer));

            // Act
            TestManager.SetValue(TestObject, LifetimeContainer);

            // Validate
            Assert.AreSame(TestObject, TestManager.GetValue(LifetimeContainer));
        }

        [TestMethod]
        public virtual void TryGetSetOtherContainerTest()
        {
            // Validate
            Assert.AreSame(LifetimeManager.NoValue, TestManager.TryGetValue(LifetimeContainer));
            Assert.AreSame(LifetimeManager.NoValue, TestManager.GetValue(LifetimeContainer));

            // Act
            TestManager.SetValue(TestObject, LifetimeContainer);

            // Validate
            Assert.AreSame(TestObject, TestManager.TryGetValue(LifetimeContainer));
            Assert.AreSame(TestObject, TestManager.GetValue(LifetimeContainer));
        }

        [TestMethod]
        public virtual void TryGetSetNoContainerTest()
        {
            var lifetime = new List<IDisposable>();

            // Validate
            Assert.AreSame(LifetimeManager.NoValue, TestManager.TryGetValue(lifetime));
            Assert.AreSame(LifetimeManager.NoValue, TestManager.GetValue(lifetime));

            // Act
            TestManager.SetValue(TestObject, lifetime);

            // Validate
            Assert.AreSame(TestObject, TestManager.TryGetValue(lifetime));
            Assert.AreSame(TestObject, TestManager.GetValue(lifetime));
        }

        #endregion


        #region Set Value

        [TestMethod]
        public virtual void SetValueTwiceTest()
        {
            Assert.AreSame(LifetimeManager.NoValue, TestManager.TryGetValue(LifetimeContainer));
            Assert.AreSame(LifetimeManager.NoValue, TestManager.GetValue(LifetimeContainer));

            // Act
            TestManager.SetValue(TestObject, LifetimeContainer);
            TestManager.SetValue(TestObject, LifetimeContainer);
        }

        [TestMethod]
        public virtual void SetDifferentValuesTwiceTest()
        {
            Assert.AreSame(LifetimeManager.NoValue, TestManager.TryGetValue(LifetimeContainer));
            Assert.AreSame(LifetimeManager.NoValue, TestManager.TryGetValue(OtherContainer));
            Assert.AreSame(LifetimeManager.NoValue, TestManager.GetValue(LifetimeContainer));
            Assert.AreSame(LifetimeManager.NoValue, TestManager.GetValue(OtherContainer));

            // Act
            TestManager.SetValue(TestObject, LifetimeContainer);
            TestManager.SetValue(TestObject, OtherContainer);
        }

        [TestMethod]
        public virtual void SetTwiceTest()
        {
            Assert.AreSame(LifetimeManager.NoValue, TestManager.TryGetValue(LifetimeContainer));
            Assert.AreSame(LifetimeManager.NoValue, TestManager.GetValue(LifetimeContainer));

            // Act
            TestManager.SetValue(TestObject, LifetimeContainer);
            TestManager.SetValue(TestObject, LifetimeContainer);
        }

        [TestMethod]
        public virtual void SetValuesTwiceTest()
        {
            Assert.AreSame(LifetimeManager.NoValue, TestManager.TryGetValue(LifetimeContainer));
            Assert.AreSame(LifetimeManager.NoValue, TestManager.TryGetValue(OtherContainer));
            Assert.AreSame(LifetimeManager.NoValue, TestManager.GetValue(LifetimeContainer));
            Assert.AreSame(LifetimeManager.NoValue, TestManager.GetValue(OtherContainer));

            // Act
            TestManager.SetValue(TestObject, LifetimeContainer);
            TestManager.SetValue(TestObject, OtherContainer);
        }

        [TestMethod]
        public virtual void SetTest()
        {
            // Act
            TestManager.SetValue(TestObject, LifetimeContainer);

            // Validate
            Assert.AreSame(TestObject, TestManager.TryGetValue(LifetimeContainer));
            Assert.AreSame(TestObject, TestManager.GetValue(LifetimeContainer));
        }

        #endregion


        #region Threading

        [TestMethod]
        public virtual void ValuesFromDifferentThreads()
        {
            TestManager.SetValue(TestObject, LifetimeContainer);

            object value1 = null;
            object value2 = null;
            object value3 = null;
            object value4 = null;

            Thread thread1 = new Thread(delegate ()
            {
                value1 = TestManager.TryGetValue(LifetimeContainer);
                value2 = TestManager.GetValue(LifetimeContainer);

            })
            { Name = "1" };

            Thread thread2 = new Thread(delegate ()
            {
                value3 = TestManager.TryGetValue(LifetimeContainer);
                value4 = TestManager.GetValue(LifetimeContainer);
            })
            { Name = "2" };

            thread1.Start();
            thread2.Start();

            thread2.Join();
            thread1.Join();

            Assert.AreSame(TestObject, TestManager.TryGetValue(LifetimeContainer));
            Assert.AreSame(TestObject, TestManager.GetValue(LifetimeContainer));

            Assert.AreSame(TestObject, value1);
            Assert.AreSame(TestObject, value2);
            Assert.AreSame(TestObject, value3);
            Assert.AreSame(TestObject, value4);
        }

        #endregion


        #region Implementation

        public class FakeLifetimeContainer : List<IDisposable>
        {
            public IUnityContainer Container => throw new System.NotImplementedException();

            public void Dispose()
            {
                throw new System.NotImplementedException();
            }
        }

        #endregion
    }
}
