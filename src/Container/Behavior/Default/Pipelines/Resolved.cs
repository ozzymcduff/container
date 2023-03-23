﻿using System.Linq;
using Unity.Builder;
using Unity.Resolution;
using Unity.Storage;
using Unity.Strategies;

namespace Unity.Container
{
    internal static partial class Pipelines<TContext>
    {
        #region Fields

        private static ResolveDelegate<TContext>? Analyse;

        #endregion


        public static ResolveDelegate<TContext> PipelineResolved(ref TContext context)
        {
            var policies = (Policies<TContext>)context.Policies;
            var chain = policies.TypeChain;

            var factory = Analyse ??= chain.AnalyzePipeline<TContext>();

            var analytics = factory(ref context);

            var builder = new PipelineBuilder<TContext>(ref context);

            return builder.BuildPipeline((object?[])analytics!);
        }


        public static ResolveDelegate<TContext> ResolvedBuildUpPipelineFactory(IStagedStrategyChain<BuilderStrategy, UnityBuildStage> chain)
        {
            var processors = chain.Values.ToArray();

            return (ref TContext context) =>
            {
                var i = -1;

                while (!context.IsFaulted && ++i < processors.Length)
                    processors[i].PreBuildUp(ref context);

                while (!context.IsFaulted && --i >= 0)
                    processors[i].PostBuildUp(ref context);

                return context.Existing;
            };
        }
    }
}
