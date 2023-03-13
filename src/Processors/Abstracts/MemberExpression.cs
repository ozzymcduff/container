﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Unity.Injection;

namespace Unity.Processors
{
    public abstract partial class MemberProcessor<TMemberInfo, TData> where TMemberInfo : MemberInfo
    {
        #region Selection Processing

        protected virtual IEnumerable<Expression> ExpressionsFromSelection(Type type, IEnumerable<object> members)
        {
            HashSet<TMemberInfo> memberSet = new HashSet<TMemberInfo>();

            foreach (var member in members)
            {
                switch (member)
                {
                    // TMemberInfo
                    case TMemberInfo info:
                        if (!memberSet.Add(info)) continue;
                        object value = DependencyAttribute.Instance; 
                        foreach (var node in AttributeFactories)
                        {
                            var attribute = GetCustomAttribute(info, node.Type);
                            if (null == attribute) continue;

                            value = null == node.Factory ? (object)attribute : node.Factory(attribute, info, null);
                            break;
                        }

                        yield return GetResolverExpression(info, value);
                        break;
                    
                        // Injection Member
                    case InjectionMember<TMemberInfo, TData> injectionMember:
                        var selection = injectionMember.MemberInfo(type);
                        if (!memberSet.Add(selection)) continue;
                        yield return GetResolverExpression(selection, injectionMember.Data);
                        break;

                    // Unknown
                    default:
                        throw new InvalidOperationException($"Unknown MemberInfo<{typeof(TMemberInfo)}> type");
                }
            }
        }

        #endregion


        #region Implementation

        protected abstract Expression GetResolverExpression(TMemberInfo info, object? resolver);

        #endregion
    }
}
