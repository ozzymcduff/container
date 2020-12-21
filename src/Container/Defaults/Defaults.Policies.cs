﻿using System;
using System.Diagnostics;
using Unity.Extension;
using Unity.Storage;

namespace Unity.Container
{
    public partial class Defaults
    {
        #region Policy Change Handler

        public delegate void PolicyChangeHandler(Type? target, Type type, object? policy);

        #endregion


        #region Allocate

        /// <summary>
        /// Allocates placeholder
        /// </summary>
        /// <param name="type"><see cref="Type"/> of policy</param>
        /// <returns>Position of the element</returns>
        private int Allocate(Type type)
        {
            var hash = (uint)(37 ^ type.GetHashCode());

            lock (_syncRoot)
            {
                ref var bucket = ref Meta[hash % Meta.Length];
                var position = bucket.Position;

                while (position > 0)
                {
                    ref var candidate = ref Data[position];
                    if (candidate.Target is null && ReferenceEquals(candidate.Type, type))
                        throw new InvalidOperationException($"{type.Name} already allocated");

                    position = Meta[position].Location;
                }

                if (++Count >= Data.Length)
                {
                    Expand();
                    bucket = ref Meta[hash % Meta.Length];
                }

                // Add new
                Data[Count] = new Policy(hash, type, null);
                Meta[Count].Location = bucket.Position;
                bucket.Position = Count;
                return Count;
            }
        }

        /// <summary>
        /// Allocates placeholder
        /// </summary>
        /// <param name="target"><see cref="Type"/> of target</param>
        /// <param name="type"><see cref="Type"/> of policy</param>
        /// <returns></returns>
        private int Allocate(Type? target, Type type)
        {
            var hash = (uint)(((target?.GetHashCode() ?? 0) + 37) ^ type.GetHashCode());

            lock (_syncRoot)
            {
                ref var bucket = ref Meta[hash % Meta.Length];
                var position = bucket.Position;

                while (position > 0)
                {
                    ref var candidate = ref Data[position];
                    if (ReferenceEquals(candidate.Target, target) &&
                        ReferenceEquals(candidate.Type, type))
                        throw new InvalidOperationException($"Combination {target?.Name} - {type.Name} already allocated");

                    position = Meta[position].Location;
                }

                if (++Count >= Data.Length)
                {
                    Expand();
                    bucket = ref Meta[hash % Meta.Length];
                }

                // Add new registration
                Data[Count] = new Policy(hash, target, type, null);
                Meta[Count].Location = bucket.Position;
                bucket.Position = Count;
                return Count;
            }
        }

        #endregion


        #region Get Or Add

        public TPolicy GetOrAdd<TPolicy>(TPolicy value, PolicyChangeHandler subscriber)
            where TPolicy : class
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            var hash = (uint)(37 ^ typeof(TPolicy).GetHashCode());

            lock (_syncRoot)
            {
                ref var bucket = ref Meta[hash % Meta.Length];
                var position = bucket.Position;

                while (position > 0)
                {
                    ref var candidate = ref Data[position];
                    if (candidate.Target is null && ReferenceEquals(candidate.Type, typeof(TPolicy)))
                    {
                        if (candidate.Value is null) candidate.Value = value;

                        candidate.PolicyChanged += subscriber;
                        return (TPolicy)candidate.Value;
                    }

                    position = Meta[position].Location;
                }

                if (++Count >= Data.Length)
                {
                    Expand();
                    bucket = ref Meta[hash % Meta.Length];
                }

                // Add new
                ref var entry = ref Data[Count];
                entry = new Policy(hash, typeof(TPolicy), value);
                entry.PolicyChanged += subscriber;

                Meta[Count].Location = bucket.Position;
                bucket.Position = Count;

                return value;
            }
        }

        /// <summary>
        /// Adds pipeline, if does not exist already, or returns existing
        /// </summary>
        /// <param name="type">Target <see cref="Type"/></param>
        /// <param name="pipeline">Pipeline to add</param>
        /// <returns>Existing or added pipeline</returns>
        public ResolveDelegate<PipelineContext> GetOrAdd(Type? type, ResolveDelegate<PipelineContext> pipeline)
        {
            var hash = (uint)(((type?.GetHashCode() ?? 0) + 37) ^ _resolverHash);

            lock (_syncRoot)
            {
                ref var bucket = ref Meta[hash % Meta.Length];
                var position = bucket.Position;

                while (position > 0)
                {
                    ref var candidate = ref Data[position];
                    if (ReferenceEquals(candidate.Target, type) &&
                        ReferenceEquals(candidate.Type, typeof(ResolveDelegate<PipelineContext>)))
                    {
                        // Found existing
                        if (candidate.Value is null) candidate.Value = pipeline;
                        return (ResolveDelegate<PipelineContext>)candidate.Value;
                    }

                    position = Meta[position].Location;
                }

                if (++Count >= Data.Length)
                {
                    Expand();
                    bucket = ref Meta[hash % Meta.Length];
                }

                // Add new registration
                Data[Count] = new Policy(hash, type, typeof(ResolveDelegate<PipelineContext>), pipeline);
                Meta[Count].Location = bucket.Position;
                bucket.Position = Count;

                return pipeline;
            }
        }

        #endregion


        #region Subscribe

        public TPolicy? Subscribe<TTarget, TPolicy>(PolicyChangeHandler subscriber)
        {
            var hash = (uint)((typeof(TTarget).GetHashCode() + 37) ^ typeof(TPolicy).GetHashCode());

            lock (_syncRoot)
            {
                ref var bucket = ref Meta[hash % Meta.Length];
                var position = bucket.Position;

                while (position > 0)
                {
                    ref var candidate = ref Data[position];
                    if (ReferenceEquals(candidate.Target, typeof(TTarget)) &&
                        ReferenceEquals(candidate.Type, typeof(TPolicy)))
                    {
                        // Found existing
                        candidate.PolicyChanged += subscriber;
                        return (TPolicy)candidate.Value;
                    }

                    position = Meta[position].Location;
                }

                if (++Count >= Data.Length)
                {
                    Expand();
                    bucket = ref Meta[hash % Meta.Length];
                }

                // Allocate placeholder 
                ref var entry = ref Data[Count];
                entry = new Policy(hash, typeof(TTarget), typeof(TPolicy), default);
                entry.PolicyChanged += subscriber;
                Meta[Count].Location = bucket.Position;
                bucket.Position = Count;
                return default;
            }
        }

        public TPolicy? Subscribe<TPolicy>(PolicyChangeHandler subscriber)
        {
            var hash = (uint)(37 ^ typeof(TPolicy).GetHashCode());

            lock (_syncRoot)
            {
                ref var bucket = ref Meta[hash % Meta.Length];
                var position = bucket.Position;

                while (position > 0)
                {
                    ref var candidate = ref Data[position];
                    if (candidate.Target is null && ReferenceEquals(candidate.Type, typeof(TPolicy)))
                    {
                        // Found existing
                        candidate.PolicyChanged += subscriber;
                        return (TPolicy)candidate.Value;
                    }

                    position = Meta[position].Location;
                }

                if (++Count >= Data.Length)
                {
                    Expand();
                    bucket = ref Meta[hash % Meta.Length];
                }

                // Allocate placeholder 
                ref var entry = ref Data[Count];
                entry = new Policy(hash, null, typeof(TPolicy), default);
                entry.PolicyChanged += subscriber;
                Meta[Count].Location = bucket.Position;
                bucket.Position = Count;
                return default;
            }
        }

        #endregion


        #region Span

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal ReadOnlySpan<Policy> Span => new ReadOnlySpan<Policy>(Data, 1, Count);

        #endregion


        #region Implementation

        protected virtual void Expand()
        {
            Array.Resize(ref Data, Storage.Prime.Numbers[Prime++]);
            Meta = new Metadata[Storage.Prime.Numbers[Prime]];

            for (var current = 1; current < Count; current++)
            {
                var bucket = Data[current].Hash % Meta.Length;
                Meta[current].Location = Meta[bucket].Position;
                Meta[bucket].Position = current;
            }
        }

        #endregion


        #region Policy structure

        [DebuggerDisplay("Policy = { Type?.Name }", Name = "{ Target?.Name }")]
        [CLSCompliant(false)]
        public struct Policy
        {
            private object? _value;
            public readonly uint Hash;
            public readonly Type? Target;
            public readonly Type  Type;


            public Policy(uint hash, Type type, object? value)
            {
                Hash = hash;
                Target = null;
                Type = type;
                _value = value;
                PolicyChanged = default;
            }

            public Policy(uint hash, Type? target, Type type, object? value)
            {
                Hash = hash;
                Target = target;
                Type = type;
                _value = value;
                PolicyChanged = default;
            }


            #region Property

            public object? Value
            {
                get => _value;
                set
                {
                    _value = value;
                    PolicyChanged?.Invoke(Target, Type, value!);
                }
            }

            #endregion


            #region Notification

            public event PolicyChangeHandler? PolicyChanged;

            #endregion
        }

        #endregion
    }
}
