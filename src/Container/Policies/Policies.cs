﻿using System.Diagnostics;
using Unity.Builder;
using Unity.Resolution;
using Unity.Storage;
using Unity.Strategies;

namespace Unity.Container
{
    public partial class Policies<TContext> where TContext : IBuilderContext
    {
        #region Fields

        protected int Count;
        
        [CLSCompliant(false)] 
        protected Policy[] Data;

        [DebuggerBrowsable(DebuggerBrowsableState.Never), CLSCompliant(false)] 
        protected Metadata[] Meta;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
        protected readonly object SyncRoot = new object();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
        protected int Prime = 3;

        private IActivateChain?  _activateChain;
        private IBuildPlanChain? _buildPlanChain;
        private IInstanceChain?  _instanceChain;
        private IFactoryChain?   _factoryChain;
        private IMappingChain?   _mappingChain;

        #endregion


        #region Constructors

        internal Policies()
        {
            // Storage
            Data = new Policy[Storage.Prime.Numbers[Prime]];
            Meta = new Metadata[Storage.Prime.Numbers[++Prime]];

            // Build Chains
            _activateChain = (IActivateChain)new StagedStrategyChain<BuilderStrategyDelegate<TContext>, UnityActivateStage>();

            // Setup build on change for the chains
            ActivateChain.Invalidated += OnTypeChainChanged;

            // Resolve Unregistered Type
            Allocate<ResolveDelegate<TContext>>(OnResolveUnregisteredChanged);

            // Resolve Registered Type
            Allocate<ResolveDelegate<TContext>>(typeof(ContainerRegistration), OnResolveRegisteredChanged);

            // Resolve Array
            Allocate<ResolveDelegate<TContext>>(typeof(Array), OnResolveArrayChanged);
        }

        #endregion
    }
}
