﻿using System;
using Unity.Lifetime;


namespace Unity.Container
{
    internal static partial class Algorithms<TContext>
    {
        /// <summary>
        /// Default algorithm for resolution of registered types
        /// </summary>
        public static object? RegisteredAlgorithm(ref TContext context)
        {
            var manager = context.Registration!;
            var policies = (Policies<TContext>)context.Policies;

            // Double lock check and create pipeline
            var pipeline = manager.GetPipeline<TContext>(context.Container.Scope);
            if (pipeline is null)
            {
                lock (manager)
                { 
                    if ((pipeline = manager.GetPipeline<TContext>(context.Container.Scope)) is null)
                    {
                        switch (manager.Category)
                        {
                            case RegistrationCategory.Factory:
                                pipeline = policies.FactoryPipeline;
                                break;

                            case RegistrationCategory.Instance:
                                pipeline = policies.InstancePipeline;
                                break;

                            case RegistrationCategory.Type:
                                pipeline = !manager.RequireBuild && context.Contract.Type != manager.Type
                                    ? policies.MappingPipeline
                                    : policies.PipelineFactory(ref context);
                                break;
                            
                            default:
                                throw new NotSupportedException();
                        }

                        manager.SetPipeline(context.Container.Scope, pipeline);
                    }
                }
            }

            // Resolve
            pipeline(ref context);

            // Handle errors, if any
            if (context.IsFaulted)
            {
                (manager as SynchronizedLifetimeManager)?.Recover();
                return UnityContainer.NoValue;
            }

            // Save resolved value
            manager.SetValue(context.Existing, context.Container.Scope);

            return context.Existing;
        }
    }
}
