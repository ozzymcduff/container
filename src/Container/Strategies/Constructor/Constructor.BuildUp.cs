﻿using System.Reflection;

namespace Unity.Container
{
    public partial class ConstructorStrategy<TContext>
    {
        public override void PreBuildUp(ref TContext context)
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
                var constructors = context.Registration?.Constructors;
                if (constructors is not null && 0 < constructors.Length)
                {
                    int index;
                    Span<int> set = stackalloc int[members.Length];
                    var ctor = constructors[0];

                    if (-1 == (index = SelectMember(ctor, members, ref set)))
                    {
                        context.Error($"Injected constructor '{ctor}' doesn't match any accessible constructors on type {type}");
                        return;
                    }

                    var descriptor = new MemberDescriptor<ConstructorInfo>(members[index]);
                    ctor.ProvideInfo(ref descriptor);

                    BuildUp(ref context, ref descriptor);

                    context.PerResolve = descriptor.MemberInfo.Invoke((object[]?)descriptor.ValueData.Value);
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
                    var descriptor = new MemberDescriptor<ConstructorInfo>(member);

                    ImportProvider.ProvideInfo(ref descriptor);

                    if (!descriptor.IsImport) continue;

                    BuildUp(ref context, ref descriptor);

                    context.PerResolve = member.Invoke((object[]?)descriptor.ValueData.Value);
                    return;
                }


                ///////////////////////////////////////////////////////////////////
                // Select using algorithm
                ConstructorInfo? info = SelectAlgorithmically(ref context, members)?.FirstOrDefault();
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


        private void BuildUp(ref TContext context, ConstructorInfo info)
        {
            var parameters = info.GetParameters();
            var arguments  = 0 == parameters.Length
                ? EmptyParametersArray
                : BuildUp(ref context, parameters);
            
            context.PerResolve = info.Invoke(arguments);
        }

    }
}
