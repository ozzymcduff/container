﻿using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Builder;
using Unity.Storage;

namespace Unity.Injection
{
    [DebuggerDisplay("Member: {MemberInfo} Type: {ContractType?.Name}, Name: {ContractName ?? \"null\"} Value: {DataValue.Type}")]
    public struct InjectionInfoStruct<TMember> : IInjectionInfo<TMember>
    {
        #region Fields

        private IntPtr _pointer;
        private ContractHost _contract;

        public ValueData InjectedValue;
        public ValueData DefaultValue;

        #endregion


        #region Constructors

        public InjectionInfoStruct(TMember info, Type type)
        {
            MemberInfo = info;
            MemberType = type;

            _contract.Type = type;
            _contract.HashCode = Contract.GetHashCode(_contract.Type);
        }

        private InjectionInfoStruct(ref InjectionInfoStruct<TMember> parent, Type type, object? data)
        {
            MemberInfo = parent.MemberInfo;
            MemberType = parent.MemberType;

            _contract.Type = type;
            _contract.HashCode = Contract.GetHashCode(_contract.Type);

            Data = data;
        }

        #endregion


        #region Member Info

        /// <inheritdoc />
        public TMember MemberInfo { get; }

        /// <inheritdoc />
        public Type MemberType { get; }

        #endregion


        #region Metadata

        /// <inheritdoc />
        public bool IsImport { get; set; }

        #endregion


        #region Contract

        public Type ContractType
        {
            get => _contract.Type;
            set
            {
                _contract.Type = value;
                _contract.HashCode = Contract.GetHashCode(_contract.Type, _contract.Name);
            }
        }

        public string? ContractName
        {
            get => _contract.Name;
            set
            {
                _contract.Name = value;
                _contract.HashCode = Contract.GetHashCode(_contract.Type, _contract.Name);
            }
        }

        public ref Contract Contract
        {
            get
            {
                unsafe
                {
                    if (IntPtr.Zero == _pointer)
                        _pointer = new IntPtr(Unsafe.AsPointer(ref _contract));

                    return ref Unsafe.AsRef<Contract>(_pointer.ToPointer());
                }
            }
        }


        #endregion


        #region Parameters

        public object?[] Arguments
        {
            set => InjectedValue[DataType.Array] = value ?? throw new ArgumentNullException(nameof(Arguments));
        }

        #endregion


        #region Default Value

        /// <inheritdoc />
        public bool AllowDefault 
        { 
            get => DataType.None != DefaultValue.Type;
            set
            {
                if (!value) 
                { 
                    DefaultValue = default;
                    return;
                } 

                if (DataType.None == DefaultValue.Type) 
                {
                    DefaultValue[DataType.Pipeline] = (ResolverPipeline)GetDefaultValue;
                    return;
                }
            }
        }

        /// <inheritdoc />
        public object? Default
        {
            set
            {
                AllowDefault = true;
                DefaultValue[DataType.Value] = value;
            }
        }

        #endregion


        #region Value

        /// <inheritdoc />
        public object? Data
        {
            set => InjectedValue[DataType.Unknown] = value;
        }

        #endregion


        #region Scope

        public InjectionInfoStruct<TMember> With(Type type, object? value)
            => new InjectionInfoStruct<TMember>(ref this, type, value);

        #endregion
        
        
        #region Implementation

        private static object? GetDefaultValue(ref BuilderContext context)
            => (context.TargetType.IsValueType && Nullable.GetUnderlyingType(context.TargetType) == null)
                ? Activator.CreateInstance(context.TargetType) : null;

        #endregion
    }
}
