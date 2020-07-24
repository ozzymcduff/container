﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Unity;
using Unity.Lifetime;

namespace Container.Scope
{
    public partial class ScopeTests
    {
        #region Add(LifetimeManager, Type[])

        [TestMethod]
        public void AddJustManagerTest()
        {
            // Act
            Scope.Add(Manager);

            // Validate
            Assert.AreEqual(0, Scope.Names);
            Assert.AreEqual(0, Scope.Version);
            Assert.AreEqual(0, Scope.Contracts);
            Assert.AreEqual(0, Scope.ToArray().Length);
        }

        [TestMethod]
        public void AddManagerTest()
        {
            // Act
            Scope.Add(Manager, Manager.GetType());

            // Validate
            Assert.AreEqual(0, Scope.Names);
            Assert.AreEqual(1, Scope.Version);
            Assert.AreEqual(1, Scope.Contracts);
            Assert.AreEqual(1, Scope.ToArray().Length);
        }

        [TestMethod]
        public void AddAliasedManagerTest()
        {
            // Act
            Scope.Add(Manager, Manager.GetType(), typeof(LifetimeManager));

            // Validate
            Assert.AreEqual(0, Scope.Names);
            Assert.AreEqual(2, Scope.Version);
            Assert.AreEqual(2, Scope.Contracts);
            Assert.AreEqual(2, Scope.ToArray().Length);
        }

        [TestMethod]
        public void AddManagerIgnoresNullTest()
        {
            // Act
            Scope.Add(Manager, Manager.GetType(), null, null);

            // Validate
            Assert.AreEqual(0, Scope.Names);
            Assert.AreEqual(1, Scope.Version);
            Assert.AreEqual(1, Scope.Contracts);
            Assert.AreEqual(1, Scope.ToArray().Length);
        }

        [TestMethod]
        public void AddManagerAsSameTypeTest()
        {
            // Act
            Scope.Add(Manager, Manager.GetType());
            Scope.Add(Manager, Manager.GetType());

            // Validate
            Assert.AreEqual(0, Scope.Names);
            Assert.AreEqual(2, Scope.Version);
            Assert.AreEqual(1, Scope.Contracts);
            Assert.AreEqual(1, Scope.ToArray().Length);
        }

        [TestMethod]
        public void AddManagerExpandsTest()
        {
            // Act
            Scope.Add(Manager, TestTypes);

            // Validate
            Assert.AreEqual(0, Scope.Names);
            Assert.AreEqual(TestTypes.Length, Scope.Version);
            Assert.AreEqual(TestTypes.Length, Scope.Contracts);
            Assert.AreEqual(TestTypes.Length, Scope.ToArray().Length);
        }

        #endregion


        #region Add(ReadOnlySpan)

        [TestMethod]
        public void AddUninitializedSpanTest()
        {
            ReadOnlySpan<RegistrationDescriptor> span = new ReadOnlySpan<RegistrationDescriptor>();

            // Act
            Scope.Add(in span);

            // Validate
            Assert.AreEqual(0, Scope.Names);
            Assert.AreEqual(0, Scope.Version);
            Assert.AreEqual(0, Scope.Contracts);
            Assert.AreEqual(0, Scope.ToArray().Length);
        }

        [TestMethod]
        public void AddArrayWithJustManagerTest()
        {
            // Arrange
            var array = new[] { new RegistrationDescriptor(Manager) };

            // Act
            Scope.Add(array);

            // Validate
            Assert.AreEqual(0, Scope.Names);
            Assert.AreEqual(0, Scope.Version);
            Assert.AreEqual(0, Scope.Contracts);
            Assert.AreEqual(0, Scope.ToArray().Length);
        }

        [TestMethod]
        public void AddRegistrationsTest()
        {
            // Act
            Scope.Add(Registrations);

            // Validate
            Assert.AreEqual(100, Scope.Names);
            Assert.AreEqual(5995, Scope.Version);
            Assert.AreEqual(5995, Scope.Contracts);
            Assert.AreEqual(5995, Scope.ToArray().Length);
        }

        [TestMethod]
        public void AddAllTest()
        {
            // Act
            Scope.Add(Registrations);

            // Validate
            Assert.AreEqual(100, Scope.Names);
            Assert.AreEqual(5995, Scope.Version);
            Assert.AreEqual(5995, Scope.Contracts);
            Assert.AreEqual(5995, Scope.ToArray().Length);
        }

        [TestMethod]
        public void AddEdgeCasesTest()
        {
            var registrations = new[]
            {
                new RegistrationDescriptor( Manager, typeof(ScopeTests) ),
                new RegistrationDescriptor( Manager, typeof(ScopeTests), null, Manager.GetType() ),
                new RegistrationDescriptor( Manager, typeof(ScopeTests), null, typeof(string), null )
            };

            // Act
            Scope.Add(registrations);

            // Validate
            Assert.AreEqual(0, Scope.Names);
            Assert.AreEqual(5, Scope.Version);
            Assert.AreEqual(3, Scope.Contracts);
            Assert.AreEqual(3, Scope.ToArray().Length);
        }

        [TestMethod]
        public void AddNamedEdgeCasesTest()
        {
            var registrations = new[]
            {
                new RegistrationDescriptor( Name, Manager, typeof(ScopeTests) ),
                new RegistrationDescriptor( Name, Manager, typeof(ScopeTests), null, Manager.GetType() ),
                new RegistrationDescriptor( Name, Manager, typeof(ScopeTests), null, typeof(string), null )
            };

            // Act
            Scope.Add(registrations);

            // Validate
            Assert.AreEqual(1, Scope.Names);
            Assert.AreEqual(5, Scope.Version);
            Assert.AreEqual(3, Scope.Contracts);
            Assert.AreEqual(3, Scope.ToArray().Length);
        }

        #endregion
    }
}
