﻿using Unity.Lifetime;

namespace Unity.Container
{
    /// <summary>
    /// This is a custom lifetime manager that acts like <see cref="TransientLifetimeManager"/>,
    /// but also provides a signal to the default build plan, marking the type so that
    /// instances are reused across the build up object graph.
    /// </summary>
    internal class PerResolveLifetimeManager : Lifetime.PerResolveLifetimeManager
    {
        #region Fields

        protected readonly object? _value = UnityContainer.NoValue;

        #endregion


        /// <summary>
        /// Construct a new <see cref="Lifetime.PerResolveLifetimeManager"/> object that stores the
        /// give value. This value will be returned by <see cref="LifetimeManager.GetValue"/>
        /// but is not stored in the lifetime manager, nor is the value disposed.
        /// This WithLifetime manager is intended only for internal use, which is why the
        /// normal <see cref="LifetimeManager.SetValue"/> method is not used here.
        /// </summary>
        /// <param name="value">InjectionParameterValue to store.</param>
        public PerResolveLifetimeManager(object? value)
        {
            _value = value;
        }

        /// <inheritdoc/>
        public override object? GetValue(ILifetimeContainer scope)
        {
            return _value;
        }
    }
}
