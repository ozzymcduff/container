﻿using System;
using System.Reflection;
using Unity.Resolution;

namespace Unity.Container
{
    public partial class ConstructorStrategy
    {
        public override void PreBuildUp<TContext>(ref TContext context)
        {
            // Do nothing if building up
            if (null != context.Existing) return;

            Type type = context.Type;
            var members = GetDeclaredMembers(type);

            ///////////////////////////////////////////////////////////////////
            // Error if no constructors
            if (0 == members.Length)
            {
                context.Error($"No accessible constructors on type {type}");
                return;
            }

            try
            {
                ///////////////////////////////////////////////////////////////////
                // Inject the constructor, if available
                if (context.Registration?.Constructors is not null)
                {
                    int index;
                    Span<int> set = stackalloc int[members.Length];
                    var injectedConstr = context.Registration?.Constructors[0]!;

                    if (-1 == (index = SelectMember(injectedConstr, members, ref set)))
                    {
                        context.Error($"Injected constructor '{injectedConstr}' doesn't match any accessible constructors on type {type}");
                        return;
                    }

                    var descriptor = new MemberDescriptor<TContext, ConstructorInfo>(members[index]);
                    injectedConstr.ProvideImport<TContext, MemberDescriptor<TContext, ConstructorInfo>>(ref descriptor);

                    var finalData = BuildUp(ref context, ref descriptor);

                    context.PerResolve = descriptor.MemberInfo.Invoke((object[]?)finalData.Value);
                    return;
                }


                ///////////////////////////////////////////////////////////////////
                // Only one constructor, nothing to select
                if (1 == members.Length)
                {
                    BuildUp(ref context, members[0]);
                    return;
                }


                ///////////////////////////////////////////////////////////////////
                // Check for annotated constructor
                foreach (var member in members)
                {
                    var descriptor = new MemberDescriptor<TContext, ConstructorInfo>(member);

                    ImportProvider.ProvideImport<TContext, MemberDescriptor<TContext, ConstructorInfo>>(ref descriptor);

                    if (!descriptor.IsImport) continue;

                    var finalData = BuildUp(ref context, ref descriptor);
                    context.PerResolve = member.Invoke((object[]?)finalData.Value);
                    return;
                }


                ///////////////////////////////////////////////////////////////////
                // Select using algorithm
                ConstructorInfo? info = SelectAlgorithmically(context.Container, members);
                if (null != info)
                {
                    BuildUp(ref context, info);
                    return;
                }
            }
            catch (Exception ex)    // Catch errors from custom providers
            {
                context.Capture(ex);
            }

            context.Error($"No accessible constructors on type {type}");
        }


        private void BuildUp<TContext>(ref TContext context, ConstructorInfo info)
            where TContext : IBuilderContext
        {
            var parameters = info.GetParameters();
            var arguments  = 0 == parameters.Length
                ? EmptyParametersArray
                : BuildUp(ref context, parameters);
            
            context.PerResolve = info.Invoke(arguments);
        }

    }
}
