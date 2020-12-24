﻿using System;
using System.Reflection;
using Unity.BuiltIn;

namespace Unity.Extension
{
    /// <summary>
    /// This extension supplies the default behavior of the UnityContainer API
    /// by handling the context events and setting policies.
    /// </summary>
    public class UnityDefaultBehaviorExtension
    {
        /// <summary>
        /// Install the default container behavior into the container.
        /// </summary>
        public static void Initialize(ExtensionContext context)
        {
            // Array Target type selector
            context.Policies.Set<Array, UnitySelector<Type, Type>>(ArrayTypeSelector.Selector);

            // Set Constructor selector
            context.Policies.Set<ConstructorInfo, UnitySelector<ConstructorInfo[], ConstructorInfo?>>(ConstructorSelector.Selector);

            // Set Member Selectors: GetConstructors(), GetFields(), etc.
            context.Policies.Set<ConstructorInfo, MembersSelector<ConstructorInfo>>(MembersSelector.GetConstructors);
            context.Policies.Set<PropertyInfo,    MembersSelector<PropertyInfo>>(MembersSelector.GetProperties);
            context.Policies.Set<MethodInfo,      MembersSelector<MethodInfo>>(MembersSelector.GetMethods);
            context.Policies.Set<FieldInfo,       MembersSelector<FieldInfo>>(MembersSelector.GetFields);

            #region Factories

            BuiltIn.PipelineFactory.Setup(context);
            BuiltIn.LazyFactory.Setup(context);
            BuiltIn.FuncFactory.Setup(context);

            #endregion
        }
    }
}
