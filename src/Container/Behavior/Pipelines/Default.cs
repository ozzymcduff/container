﻿using Unity.Builder;
using Unity.Extension;
using Unity.Resolution;
using Unity.Strategies;

namespace Unity.Container
{
    internal static partial class Pipelines<TContext>
        where TContext : IBuilderContext
    {
        public static void Initialize(IPolicies policies)
        {
            // Converter to compile staged chain of strategies into resolver pipeline
            policies.Set<Converter<BuilderStrategyDelegate<TContext>[], ResolveDelegate<TContext>>>(CompiledChainToPipelineFactory);

            // Converter to compile staged chain of strategies into pipeline factory
            policies.Set<Converter<BuilderStrategyDelegate<TContext>[], PipelineFactory<TContext>>>(DefaultCompileProcessorFactory);
        }





        public static ResolveDelegate<TContext> DefaultFactory(ref TContext context)
        {
            //switch (context.Registration?.CreationPolicy)
            //{
            //    case CreationPolicy.Once:
            //        return ((Policies<TContext>)context.Policies).ActivatePipeline;

            //    case CreationPolicy.Always:
            //        return PipelineCompiled(ref context);

            //    case CreationPolicy.OnceInWhile:
            //        return PipelineResolved(ref context);
            //}
            return ((Policies<TContext>)context.Policies).ActivatePipeline;
            //return PipelineCompiled(ref context); 
        }

        public static PipelineFactory<TContext> DefaultCompileProcessorFactory(BuilderStrategyDelegate<TContext>[] chain)
        {
            return DefaultFactory;
        }
    }
}
