﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Extension;
using Unity.Pipeline;
using Unity.Policy;
using Unity.Storage;

namespace Unity
{
    // Extension Management
    public partial class UnityContainer : IEnumerable<Type>
    {
        #region Fields

        private PrivateExtensionContext? _context;
        private event RegistrationEvent? _registering;
        private event ChildCreatedEvent? _childContainerCreated;
        private List<IUnityContainerExtensionConfigurator>? _extensions;

        #endregion


        #region Events

        protected event RegistrationEvent Registering
        {
            add 
            { 
                if (null == _registering && null != Parent)
                    Parent.Registering += OnParentRegistering;

                _registering += value; 
            }

            remove 
            { 
                _registering -= value;

                if (null == _registering && null != Parent)
                    Parent.Registering -= OnParentRegistering;
            }
        }

        private void OnParentRegistering(ref RegistrationData registration) 
            => _registering?.Invoke(ref registration);

        protected event ChildCreatedEvent ChildContainerCreated
        {
            add => _childContainerCreated += value;
            remove => _childContainerCreated -= value;
        }

        #endregion


        #region IEnumerable

        /// <summary>
        /// This method returns <see cref="IEnumerator{Type}"/> with types of available 
        /// managed extensions
        /// </summary>
        /// <remarks>
        /// Extensions, after executing method <see cref="UnityContainerExtension.Initialize"/>,
        /// either discarded or added to the container's storage based on implementation of 
        /// <see cref="IUnityContainerExtensionConfigurator"/> interface:
        /// <para>If extension implements <see cref="IUnityContainerExtensionConfigurator"/> interface,
        /// it is stored in the container and kept alive until the container goes out of scope.</para>
        /// <para>If extension does not implement the interface, its reference is released 
        /// immediately after initialization</para>
        /// </remarks>
        /// <returns><see cref="Type"/> enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Type>)this).GetEnumerator();

        /// <summary>
        /// This method returns <see cref="IEnumerator{Type}"/> with types of available 
        /// managed extensions
        /// </summary>
        /// <remarks>
        /// Extensions, after executing method <see cref="UnityContainerExtension.Initialize"/>,
        /// either discarded or added to the container's storage based on implementation of 
        /// <see cref="IUnityContainerExtensionConfigurator"/> interface:
        /// <para>If extension implements <see cref="IUnityContainerExtensionConfigurator"/> interface,
        /// it is stored in the container and kept alive until the container goes out of scope.</para>
        /// <para>If extension does not implement the interface, its reference is released 
        /// immediately after initialization</para>
        /// </remarks>
        /// <returns><see cref="Type"/> enumerator</returns>
        IEnumerator<Type> IEnumerable<Type>.GetEnumerator()
        {
            if (null == _extensions) yield break;

            foreach (var extension in _extensions)
                yield return extension.GetType();
        }

        #endregion


        #region Extension Context implementation

        /// <summary>
        /// Implementation of the ExtensionContext that is used extension management.
        /// </summary>
        /// <remarks>
        /// This is a nested class so that it can access state in the container that 
        /// would otherwise be inaccessible.
        /// </remarks>
        [DebuggerTypeProxy(typeof(ExtensionContext))]
        private class PrivateExtensionContext : ExtensionContext
        {
            #region Constructors

            public PrivateExtensionContext(UnityContainer container) => Container = container;

            #endregion


            #region Container

            /// <inheritdoc />
            public override UnityContainer Container { get; }

            /// <inheritdoc />
            public override IPolicyList Policies => Container._policies;

            /// <inheritdoc />
            public override ICollection<IDisposable> Lifetime => Container._scope;

            #endregion


            #region Pipelines

            /// <inheritdoc />
            public override IStagedStrategyChain<PipelineProcessor, BuilderStage> FactoryPipeline 
                => Container._policies.FactoryPipeline;

            /// <inheritdoc />
            public override IStagedStrategyChain<PipelineProcessor, BuilderStage> InstancePipeline 
                => Container._policies.InstancePipeline;

            /// <inheritdoc />
            public override IStagedStrategyChain<PipelineProcessor, BuilderStage> TypePipeline 
                => Container._policies.TypePipeline;

            #endregion


            #region Events

            /// <inheritdoc />
            public override event RegistrationEvent Registering
            {
                add    => Container.Registering += value;
                remove => Container.Registering -= value;
            }

            /// <inheritdoc />
            public override event ChildCreatedEvent ChildContainerCreated
            {
                add    => Container.ChildContainerCreated += value;
                remove => Container.ChildContainerCreated -= value;
            }

            #endregion
        }

        #endregion
    }
}
