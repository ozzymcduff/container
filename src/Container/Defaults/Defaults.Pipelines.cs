﻿using Unity.Lifetime;
using Unity.Pipeline;
using Unity.Resolution;
using Unity.Storage;

namespace Unity.Container
{
    public partial class Defaults
    {
        #region Chains

        public StagedChain<BuilderStage, PipelineProcessor> TypeChain { get; }

        public StagedChain<BuilderStage, PipelineProcessor> FactoryChain { get; }

        public StagedChain<BuilderStage, PipelineProcessor> InstanceChain { get; }

        public StagedChain<BuilderStage, PipelineProcessor> UnregisteredChain { get; }

        #endregion


        #region Activators

        /// <summary>
        /// Resolve object with <see cref="ResolutionStyle.OnceInLifetime"/> lifetime and
        /// <see cref="RegistrationCategory.Type"/> registration
        /// </summary>
        public ResolveDelegate<ResolveContext> TypePipeline
            => (ResolveDelegate<ResolveContext>)_data[PIPELINE_TYPE].Value!;

        /// <summary>
        /// Resolve object with <see cref="ResolutionStyle.OnceInLifetime"/> lifetime and
        /// <see cref="RegistrationCategory.Instance"/> registration
        /// </summary>
        public ResolveDelegate<ResolveContext> InstancePipeline
            => (ResolveDelegate<ResolveContext>)_data[PIPELINE_INSTANCE].Value!;

        /// <summary>
        /// Resolve object with <see cref="ResolutionStyle.OnceInLifetime"/> lifetime and
        /// <see cref="RegistrationCategory.Factory"/> registration
        /// </summary>
        public ResolveDelegate<ResolveContext> FactoryPipeline
            => (ResolveDelegate<ResolveContext>)_data[PIPELINE_FACTORY].Value!;

        #endregion


        #region Factories

        /// <summary>
        /// Create resolution pipeline for <see cref="ResolutionStyle.OnceInLifetime"/> lifetime
        /// </summary>
        public ResolveDelegateFactory UnregisteredPipelineFactory
            => (ResolveDelegateFactory)_data[FACTORY_UNREGISTERED].Value!;
        
        /// <summary>
        /// Create resolution pipeline for <see cref="ResolutionStyle.OnceInWhile"/> lifetime
        /// </summary>
        public BalancedFactoryDelegate BalancedPipelineFactory
            => (BalancedFactoryDelegate)_data[FACTORY_BALANCED].Value!;

        /// <summary>
        /// Create resolution pipeline for <see cref="ResolutionStyle.EveryTime"/> lifetime
        /// </summary>
        public OptimizedFactoryDelegate OptimizedPipelineFactory
            => (OptimizedFactoryDelegate)_data[FACTORY_OPTIMIZED].Value!;

        #endregion
    }
}
