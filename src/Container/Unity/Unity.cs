﻿using System;
using System.Collections.Generic;
using Unity.BuiltIn;
using Unity.Container;

namespace Unity
{
    public partial class UnityContainer : IDisposable
    {
        #region Fields

        private readonly int BUILT_IN_CONTRACT_COUNT;
        private readonly int _depth;

        internal Scope _scope;
        internal readonly Defaults _policies;

        #endregion

        #region Constructors

        /// <summary>
        /// Create <see cref="UnityContainer"/> container
        /// </summary>
        /// <param name="name">Name of the container</param>
        /// <param name="capacity">Preallocated capacity</param>
        public UnityContainer(string name, int capacity)
        {
            _depth = 0;
            Root = this;
            Name = name;

            _policies = new Defaults();
            _context  = new PrivateExtensionContext(this);

            // Registration Scope
            _scope = new ContainerScope(capacity);

            var manager = new ContainerLifetimeManager(this);
            _scope.Add(typeof(IUnityContainer),      manager, true);
            _scope.Add(typeof(IUnityContainerAsync), manager, true);
            _scope.Add(typeof(IServiceProvider),     manager, true);
            BUILT_IN_CONTRACT_COUNT = _scope.Count;

            // Generic Factories
            _policies.Set<PipelineFactory<Type>>(typeof(IEnumerable<>), ResolveEnumerable);
            
            // Setup Built-In Components
            BuiltInComponents.Setup(_context);
        }

        /// <summary>
        /// Child container constructor
        /// </summary>
        /// <param name="parent">Parent <see cref="UnityContainer"/></param>
        /// <param name="name">Name of this container</param>
        protected UnityContainer(UnityContainer parent, string? name, int capacity)
        {
            _depth = parent._depth + 1;
            Name   = name;
            Root   = parent.Root;
            Parent = parent;

            _policies = parent.Root._policies;

            // Registration Scope
            _scope = parent._scope.CreateChildScope(capacity);

            var manager = new ContainerLifetimeManager(this);
            _scope.Add(typeof(IUnityContainer),      manager, true);
            _scope.Add(typeof(IUnityContainerAsync), manager, true);
            _scope.Add(typeof(IServiceProvider),     manager, true);
            BUILT_IN_CONTRACT_COUNT = _scope.Count;
        }

        #endregion


        #region IDisposable

        public void Dispose()
        {
            // Child container dispose
            if (null != Parent) Parent.Registering -= OnParentRegistering;

            _registering = null;
            _childContainerCreated = null;

            _scope.Dispose();
        }

        #endregion
    }
}
