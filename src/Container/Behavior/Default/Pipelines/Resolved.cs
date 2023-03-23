﻿using Unity.Resolution;
using Unity.Strategies;

namespace Unity.Container
{
    internal static partial class Pipelines
    {
        #region Fields

        private static ResolveDelegate<BuilderContext>? Analyse;

        #endregion


        public static ResolveDelegate<BuilderContext> PipelineResolved(ref BuilderContext context)
        {
            var policies = (Policies<BuilderContext>)context.Policies;
            var chain    = policies.TypeChain;
            
            var factory  = Analyse ??= chain.AnalyzePipeline<BuilderContext>();

            var analytics = factory(ref context);

            var builder = new PipelineBuilder<BuilderContext>(ref context);

            return builder.BuildPipeline((object?[])analytics!);
        }
    }
}
