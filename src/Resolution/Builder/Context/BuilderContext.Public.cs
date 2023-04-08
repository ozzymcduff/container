using System.Runtime.CompilerServices;
using Unity.Policy;
using Unity.Resolution;
using Unity.Storage;

namespace Unity.Builder
{
    public partial struct BuilderContext
    {
        #region Public Properties

        public Type Type => _type ??= _manager?.Type ?? Contract.Type;

        public Type TargetType => _type ??= _manager?.Type ?? Contract.Type;

        public string? Name
        {
            get
            {
                unsafe
                {
                    return Unsafe.AsRef<Contract>(_contract.ToPointer()).Name;
                }
            }
        }

        /// <inheritdoc/>
        public IPolicies Policies => Container.Policies;

        #endregion


        #region Value

        public object? Existing 
        { 
            get => _target;
            set
            {
                _target = value;

                unsafe
                {
                    if (_perResolve)
                    {
                        ref var contract = ref Unsafe.AsRef<Contract>(_registration.ToPointer());
                        Request.PerResolve = new PerResolveOverride(in contract, value);
                    }
                }
            }
        }

        public object? Target   
        { 
            get => _target; 
            set => _target = value; 
        }

        #endregion


        #region Indirection

        public RegistrationManager? Registration
        {
            get => _manager;
            set
            {
                if (!_perResolve) _perResolve = value is Lifetime.PerResolveLifetimeManager;
                _manager = value;
            }
        }

        public bool IsFaulted
        {
            get
            {
                unsafe
                {
                    return Unsafe.AsRef<ErrorDescriptor>(_error.ToPointer()).IsFaulted;
                }
            }
        }

        public readonly ref Contract Contract
        {
            get
            {
                unsafe
                {
                    return ref Unsafe.AsRef<Contract>(_contract.ToPointer());
                }
            }
        }

        public readonly ref ErrorDescriptor ErrorInfo
        {
            get
            {
                unsafe
                {
                    return ref Unsafe.AsRef<ErrorDescriptor>(_error.ToPointer());
                }
            }
        }

        private readonly ref RequestInfo Request
        {
            get
            {
                unsafe
                {
                    return ref Unsafe.AsRef<RequestInfo>(_request.ToPointer());
                }
            }
        }

        public readonly ref BuilderContext Parent
        {
            get
            {
                unsafe
                {
                    return ref Unsafe.AsRef<BuilderContext>(_parent.ToPointer());
                }
            }
        }

        public readonly ResolverOverride[] Overrides
        {
            get
            {
                unsafe
                {
                    return Unsafe.AsRef<RequestInfo>(_request.ToPointer()).Overrides;
                }
            }
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Create new <see cref="RequestInfo"/> request structure
        /// </summary>
        /// <param name="overrides">Array of <see cref="ResolverOverride"/> members</param>
        /// <returns><see cref="RequestInfo"/> structure</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RequestInfo NewRequest(ResolverOverride[]? overrides = null)
            => new RequestInfo(overrides);


        public void Error(string error)
        {
            unsafe
            {
                Unsafe.AsRef<ErrorDescriptor>(_error.ToPointer())
                      .Error(error);
            }
        }

        public object Throw(Exception exception)
        {
            unsafe
            {
                return Unsafe.AsRef<ErrorDescriptor>(_error.ToPointer())
                             .Throw(exception);
            }
        }

        public object Capture(Exception exception)
        {
            unsafe
            {
                return Unsafe.AsRef<ErrorDescriptor>(_error.ToPointer())
                             .Capture(exception);
            }
        }

        public object? GetValueRecursively<TInfo>(TInfo info, object? value)
        {
            return value switch
            {
                ResolverPipeline resolver           => GetValueRecursively(info, resolver(ref this)),

                IResolve iResolve                   => GetValueRecursively(info, iResolve.Resolve(ref this)),

                IResolverFactory<TInfo> infoFactory => GetValueRecursively(info, infoFactory.GetResolver<BuilderContext>(info)
                                                                                 .Invoke(ref this)),
                IResolverFactory<Type> typeFactory  => GetValueRecursively(info, typeFactory.GetResolver<BuilderContext>(Type)
                                                                                       .Invoke(ref this)),
                _ => value,
            };
        }

        #endregion
    }
}
