﻿using System;
using System.Reflection;
using Unity.Extension;

namespace Unity.Container
{
    internal static partial class Factories<TContext>
    {
        #region Fields

        private static MethodInfo? LazyPipelineMethodInfo;

        #endregion


        #region Factory


        public static ResolveDelegate<TContext> LazyFactory(ref TContext context)
        {
            var target = context.Type.GenericTypeArguments[0];
            
            return (LazyPipelineMethodInfo ??= typeof(Factories<TContext>)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(LazyPipeline))!)
                .CreatePipeline<TContext>(target);
        }

        #endregion


        #region Implementation

        private static object? LazyPipeline<TElement>(ref PipelineContext context)
        {
            var name  = context.Name;
            var scope = context.Container;

            context.Target = new Lazy<TElement>(ResolverMethod);

            return context.Target;

            // Func<TElement>
            TElement ResolverMethod() => (TElement)scope.Resolve(typeof(TElement), name)!;
        }
        
        #endregion
    }
}
