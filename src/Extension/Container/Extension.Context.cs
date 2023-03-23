﻿using System;
using System.Collections.Generic;
using Unity.Builder;
using Unity.Storage;
using Unity.Strategies;

namespace Unity.Extension
{
    /// <summary>
    /// The <see cref="ExtensionContext"/> class provides the means for extension objects
    /// to manipulate the internal state of the <see cref="IUnityContainer"/>.
    /// </summary>
    public abstract partial class ExtensionContext
    {
        #region Container

        /// <summary>
        /// The instance of extended container
        /// </summary>
        /// <value>The instance of <see cref="UnityContainer"/></value>
        public abstract UnityContainer Container { get; }

        /// <summary>
        /// The collection of disposable objects this container is responsible for
        /// </summary>
        /// <value>The <see cref="ICollection{IDisposable}"/> of <see cref="IDisposable"/> objects</value>
        public abstract ICollection<IDisposable> Lifetime { get; }

        #endregion


        #region Strategies

        /// <summary>
        /// Pipeline chain required to process type registrations
        /// </summary>
        public abstract IStagedStrategyChain<BuilderStrategy, UnityBuildStage> TypePipelineChain { get; }

        /// <summary>
        /// Pipeline chain required to process instance registrations
        /// </summary>
        public abstract IStagedStrategyChain<BuilderStrategy, UnityBuildStage> InstancePipelineChain { get; }

        /// <summary>
        /// Pipeline chain required to process factory registrations
        /// </summary>
        public abstract IStagedStrategyChain<BuilderStrategy, UnityBuildStage> FactoryPipelineChain { get; }

        /// <summary>
        /// Pipeline chain required to process mappings
        /// </summary>
        public abstract IStagedStrategyChain<BuilderStrategy, UnityBuildStage> MappingPipelineChain { get; }

        #endregion


        #region Policy Lists

        /// <summary>
        /// The policies this container uses.
        /// </summary>
        /// <remarks>The <see cref="IPolicyList"/> the that container uses to build objects.</remarks>
        public abstract IPolicies Policies { get; }

        #endregion


        #region Events

        /// <summary>
        /// This event is raised on new registration
        /// </summary>
        public abstract event RegistrationEvent Registering;

        /// <summary>
        /// This event is raised when the <see cref="IUnityContainer.CreateChildContainer"/> 
        /// method is called and new child container is created. It allow extensions to 
        /// perform any additional initialization they may require.
        /// </summary>
        public abstract event ChildCreatedEvent ChildContainerCreated;

        #endregion
    }
}
