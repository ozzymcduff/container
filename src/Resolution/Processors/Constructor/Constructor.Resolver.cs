﻿using System.Reflection;
using Unity.Builder;
using Unity.Storage;

namespace Unity.Processors
{
    public partial class ConstructorProcessor
    {
        public override void BuildResolver<TContext>(ref TContext context)
        {
            var members = GetDeclaredMembers(context.TargetType);

            // Error if no constructors
            if (0 == members.Length)
            {
                context.Target = 
                    (ref BuilderContext context) => context.Error($"No accessible constructors on type {context.Type}");   
                
                return;
            }

            var info = SelectConstructor(ref context, members);
            
            if (context.IsFaulted) return;

            ResolverBuild(ref context, ref info);

            if (context.IsFaulted) return;

            if (context.Target is not null)
            {
                context.Target = info.DataValue.Type switch
                {
                    DataType.None     => ResolverBuild<TContext>(info.MemberInfo, EmptyParametersArray,  context.Target),
                    DataType.Value    => ResolverBuild<TContext>(info.MemberInfo, info.DataValue.Value!, context.Target),
                    DataType.Pipeline => ResolverBuild<TContext>(info.MemberInfo, (ResolverPipeline)info.DataValue.Value!, context.Target),
                    _ => throw new NotImplementedException(),
                };
            }
            else
            {
                context.Target = info.DataValue.Type switch
                {
                    DataType.None     => ResolverBuild<TContext>(info.MemberInfo, EmptyParametersArray),
                    DataType.Value    => ResolverBuild<TContext>(info.MemberInfo, info.DataValue.Value!),
                    DataType.Pipeline => ResolverBuild<TContext>(info.MemberInfo, (ResolverPipeline)info.DataValue.Value!),
                    _ => throw new NotImplementedException(),
                };
            }
        }

        private BuilderStrategyPipeline ResolverBuild<TContext>(ConstructorInfo constructor, object parameters)
        {
            return (ref BuilderContext context) =>
            {
                if (context.IsFaulted || context.Target is not null) return;

                try
                {
                    context.Existing = constructor.Invoke((object?[])parameters);
                }
                catch (Exception ex) when (ex is ArgumentException ||
                                           ex is MemberAccessException)
                {
                    context.Error(ex.Message);
                }
                catch (Exception exception)
                {
                    context.Capture(exception);
                }
            };        
        }

        private BuilderStrategyPipeline ResolverBuild<TContext>(ConstructorInfo constructor, ResolverPipeline parameters)
        {
            return (ref BuilderContext context) =>
            {
                if (context.IsFaulted || context.Target is not null) return;

                try
                {
                    context.Existing = constructor.Invoke((object?[])parameters(ref context)!);
                }
                catch (Exception ex) when (ex is ArgumentException ||
                                           ex is MemberAccessException)
                {
                    context.Error(ex.Message);
                }
                catch (Exception exception)
                {
                    context.Capture(exception);
                }
            };
        }

        private BuilderStrategyPipeline ResolverBuild<TContext>(ConstructorInfo constructor, object parameters, BuilderStrategyPipeline pipeline)
        {
            return (ref BuilderContext context) =>
            {
                pipeline(ref context);

                if (context.IsFaulted || context.Target is not null) return;

                try
                {
                    context.Existing = constructor.Invoke((object?[])parameters);
                }
                catch (Exception ex) when (ex is ArgumentException ||
                                           ex is MemberAccessException)
                {
                    context.Error(ex.Message);
                }
                catch (Exception exception)
                {
                    context.Capture(exception);
                }
            };
        }

        private BuilderStrategyPipeline ResolverBuild<TContext>(ConstructorInfo constructor, ResolverPipeline parameters, BuilderStrategyPipeline pipeline)
        {
            return (ref BuilderContext context) =>
            {
                pipeline(ref context);

                if (context.IsFaulted || context.Target is not null) return;

                try
                {
                    context.Existing = constructor.Invoke((object?[])parameters(ref context)!);
                }
                catch (Exception ex) when (ex is ArgumentException ||
                                           ex is MemberAccessException)
                {
                    context.Error(ex.Message);
                }
                catch (Exception exception)
                {
                    context.Capture(exception);
                }
            };
        }

    }
}
