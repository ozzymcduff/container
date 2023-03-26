﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Unity.Builder;
using Unity.Extension;
using Unity.Storage;
using Unity.Strategies;

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
                // TODO: Registration propagation?
                //if (null != Parent && _registering is null)
                //    Parent.Registering += OnParentRegistering;

                _registering += value; 
            }

            remove 
            { 
                _registering -= value;

                //if (_registering is null && null != Parent)
                //    Parent.Registering -= OnParentRegistering;
            }
        }

        // TODO: Find better place 
        private void OnParentRegistering(object container, in ReadOnlySpan<RegistrationDescriptor> registrations) 
            => _registering?.Invoke(container, in registrations);

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
            if (_extensions is null) yield break;

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
            public override IPolicies Policies => Container.Policies;

            /// <inheritdoc />
            public override ICollection<IDisposable> Lifetime => Container.Scope;

            #endregion


            #region Pipelines

            /// <inheritdoc />
            public override IActivateChain ActivateStrategies
                => Container.Policies.ActivateChain;

            /// <inheritdoc />
            public override IFactoryChain FactoryStrategies 
                => Container.Policies.FactoryChain;

            /// <inheritdoc />
            public override IInstanceChain InstanceStrategies 
                => Container.Policies.InstanceChain;

            /// <inheritdoc />
            public override IMappingChain MappingStrategies 
                => Container.Policies.MappingChain;

            public override IBuildPlanChain BuildPlanStrategies
                => Container.Policies.BuildPlanChain;

            #endregion


            #region Declarations

            /// <inheritdoc />
            public override Func<Type, ConstructorInfo[]>? GetConstructors
            {
                get => Policies.Get<Func<Type, ConstructorInfo[]>>();
                set => Policies.Set(value ?? throw new ArgumentNullException(nameof(Func<Type, ConstructorInfo[]>)));
            }

            /// <inheritdoc />
            public override Func<Type, FieldInfo[]>? GetFields
            {
                get => Policies.Get<Func<Type, FieldInfo[]>>();
                set => Policies.Set(value ?? throw new ArgumentNullException(nameof(Func<Type, FieldInfo[]>)));
            }

            /// <inheritdoc />
            public override Func<Type, PropertyInfo[]>? GetProperties
            {
                get => Policies.Get<Func<Type, PropertyInfo[]>>();
                set => Policies.Set(value ?? throw new ArgumentNullException(nameof(Func<Type, PropertyInfo[]>)));
            }

            /// <inheritdoc />
            public override Func<Type, MethodInfo[]>? GetMethods
            {
                get => Policies.Get<Func<Type, MethodInfo[]>>();
                set => Policies.Set(value ?? throw new ArgumentNullException(nameof(Func<Type, MethodInfo[]>)));
            }

            #endregion


            #region Selection

            /// <inheritdoc />
            public override ConstructorSelector? ConstructorSelector 
            { 
                get => Policies.Get<ConstructorSelector>(); 
                set => Policies.Set(value ?? throw new ArgumentNullException(nameof(ConstructorSelector))); 
            }

            /// <inheritdoc />
            public override FieldsSelector? FieldsSelector
            {
                get => Policies.Get<FieldsSelector>();
                set => Policies.Set(value ?? throw new ArgumentNullException(nameof(FieldsSelector)));
            }

            /// <inheritdoc />
            public override PropertiesSelector? PropertiesSelector
            {
                get => Policies.Get<PropertiesSelector>();
                set => Policies.Set(value ?? throw new ArgumentNullException(nameof(PropertiesSelector)));
            }

            /// <inheritdoc />
            public override MethodsSelector? MethodsSelector
            {
                get => Policies.Get<MethodsSelector>();
                set => Policies.Set(value ?? throw new ArgumentNullException(nameof(MethodsSelector)));
            }

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
