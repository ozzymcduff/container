﻿using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Container;
using Unity.Resolution;

namespace Unity.Injection
{
    public abstract class InjectionMemberInfo<TMemberInfo> : InjectionMember<TMemberInfo, object>,
                                                             IImportInfoProvider<TMemberInfo>
                                         where TMemberInfo : MemberInfo
    {
        #region Fields

        private readonly Type? _type;
        private readonly string? _name;
        private readonly bool _optional;

        #endregion


        #region Constructors

        protected InjectionMemberInfo(string member, object data)
            : base(member, data)
        {
        }

        protected InjectionMemberInfo(string member, bool optional)
            : base(member, RegistrationManager.NoValue)
        {
            _optional = optional;
        }

        protected InjectionMemberInfo(string member, Type contractType, bool optional)
            : base(member, RegistrationManager.NoValue)
        {
            _type = contractType;
            _optional = optional;
        }


        protected InjectionMemberInfo(string member, Type contractType, string? contractName, bool optional)
            : base(member, RegistrationManager.NoValue)
        {
            _type = contractType;
            _name = contractName;
            _optional = optional;
        }

        #endregion


        #region Implementation

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Type MemberType(TMemberInfo info);

        public ImportInfo<TMemberInfo> GetImportInfo(TMemberInfo member)
        {
            if (Data is IImportInfoProvider<TMemberInfo> provider)
                return provider.GetImportInfo(member);

            var type = MemberType(member);

            return _type switch
            {
                null when Data is Type target && typeof(Type) != type
                        => new ImportInfo<TMemberInfo>(member, target, _optional),

                null when !ReferenceEquals(RegistrationManager.NoValue, Data)
                        => Data switch
                        {
                            RegistrationManager.InvalidValue _        => new ImportInfo<TMemberInfo>(member, type, _optional),
                            IResolve iResolve                         => new ImportInfo<TMemberInfo>(member, type, _optional, (ResolveDelegate<PipelineContext>)iResolve.Resolve, ImportType.Pipeline),
                            ResolveDelegate<PipelineContext> resolver => new ImportInfo<TMemberInfo>(member, type, _optional, Data,                                               ImportType.Pipeline),
                            IResolverFactory<TMemberInfo> infoFactory => new ImportInfo<TMemberInfo>(member, type, _optional, infoFactory.GetResolver<PipelineContext>(member),   ImportType.Pipeline),
                            IResolverFactory<Type> typeFactory        => new ImportInfo<TMemberInfo>(member, type, _optional, typeFactory.GetResolver<PipelineContext>(type),     ImportType.Pipeline),
                            _                                         => new ImportInfo<TMemberInfo>(member, type, _optional, Data,                                               ImportType.Value),
                        },

                _ => new ImportInfo<TMemberInfo>(member, _type ?? type, _name, _optional),
            };
        }

        #endregion
    }
}
