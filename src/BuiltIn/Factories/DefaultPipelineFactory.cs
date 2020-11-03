﻿using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Container;
using Unity.Extension;
using Unity.Resolution;

namespace Unity.BuiltIn
{
    public static class DefaultPipelineFactory
    {
        private static Defaults? _policies;

        public static void Setup(ExtensionContext context)
        {
            _policies = (Defaults)context.Policies;
            _policies.Set<PipelineFactory>(typeof(Defaults.TypeCategory),     BuildTypePipeline);
            _policies.Set<PipelineFactory>(typeof(Defaults.InstanceCategory), BuildInstancePipeline);
            _policies.Set<PipelineFactory>(typeof(Defaults.FactoryCategory),  BuildFactoryPipeline);
        }

        private static ResolveDelegate<PipelineContext> BuildFactoryPipeline(Type type)
        {
            var factoryProcessors = ((IEnumerable<PipelineProcessor>)_policies!.FactoryChain).ToArray();
            if (factoryProcessors is null || 0 == factoryProcessors.Length) throw new InvalidOperationException("List of visitors is empty");
            return (ref PipelineContext context) =>
            {
                var i = -1;

                while (!context.IsFaulted && ++i < factoryProcessors.Length)
                    factoryProcessors[i].PreBuild(ref context);

                while (!context.IsFaulted && --i >= 0)
                    factoryProcessors[i].PostBuild(ref context);

                return context.Target;
            };
        }

        private static ResolveDelegate<PipelineContext> BuildInstancePipeline(Type type)
        {
            var instanceProcessors = ((IEnumerable<PipelineProcessor>)_policies!.InstanceChain).ToArray();
            if (instanceProcessors is null || 0 == instanceProcessors.Length) throw new InvalidOperationException("List of visitors is empty");

            return (ref PipelineContext context) =>
            {
                var i = -1;

                while (!context.IsFaulted && ++i < instanceProcessors.Length)
                    instanceProcessors[i].PreBuild(ref context);

                while (!context.IsFaulted && --i >= 0)
                    instanceProcessors[i].PostBuild(ref context);

                return context.Target;
            };
        }

        private static ResolveDelegate<PipelineContext> BuildTypePipeline(Type type)
        {
            var typeProcessors = ((IEnumerable<PipelineProcessor>)_policies!.TypeChain).ToArray();
            if (typeProcessors is null || 0 == typeProcessors.Length) throw new InvalidOperationException("List of visitors is empty");

            return (ref PipelineContext context) =>
            {
                var i = -1;

                while (!context.IsFaulted && ++i < typeProcessors.Length)
                    typeProcessors[i].PreBuild(ref context);

                while (!context.IsFaulted && --i >= 0)
                    typeProcessors[i].PostBuild(ref context);

                return context.Target;
            };
        }
    }
}
