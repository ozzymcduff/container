﻿using System;
using System.Diagnostics;

namespace Unity.Storage
{
    /// <summary>
    /// Represents a chain of builder strategies partitioned by stages.
    /// </summary>
    /// <typeparam name="UnityBuildStage">The stage enumeration to partition the strategies.</typeparam>
    /// <typeparam name="BuilderStrategy"><see cref="Type"/> of strategy</typeparam>
    public partial class StagedStrategyChain<TStrategyType, TStageEnum> : IStagedStrategyChain<TStrategyType, TStageEnum>
        where TStrategyType : class
        where TStageEnum : Enum
    {
        #region Constants

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const string ERROR_MESSAGE = "An element with the same key already exists";

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly TStageEnum[] _values = (TStageEnum[])Enum.GetValues(typeof(TStageEnum));

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsReadOnly => false;

        #endregion


        #region Fields

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private readonly int     _size;
        private readonly Entry[] _stages;
        private static readonly EventArgs _args = new EventArgs();

        #endregion


        #region Constructors

        public StagedStrategyChain()
        {
            _size   = _values.Length;
            _stages = new Entry[_size];

            for (var i = 0; i < _size; i++) _stages[i].Stage = _values[i];
        }

        #endregion


        #region Properties

        public int Count { get; protected set; }

        public int Version { get; protected set; }

        #endregion


        #region IStagedStrategyChain

        /// <inheritdoc/>
        public void Add(TStrategyType strategy, TStageEnum stage)
        {
            ref var entry = ref _stages[Convert.ToInt32(stage)];

            if (entry.Strategy is not null) throw new ArgumentException(ERROR_MESSAGE);

            entry.Strategy = strategy;

            Count += 1;
            Version += 1;
            Invalidated?.Invoke(this, _args);
        }

        /// <inheritdoc/>
        public void Add(params (TStageEnum, TStrategyType)[] stages)
        {
            for (var i = 0; i < stages.Length; i++)
            {
                ref var pair = ref stages[i];
                ref var entry = ref _stages[Convert.ToInt32(pair.Item1)];

                if (entry.Strategy is not null) throw new ArgumentException(ERROR_MESSAGE);

                Count += 1;
                entry.Strategy = pair.Item2;
            }

            Version += 1;
            Invalidated?.Invoke(this, _args);
        }

        public TStrategyType[] MakeStrategyChain()
        {
            var array = new TStrategyType[Count];
            int i = 0, index = -1;

            while (++index < _size && i < Count)
            {
                ref var entry = ref _stages[index];
                if (entry.Strategy is not null)
                    array[i++] = entry.Strategy;
            }

            return array;
        }

        public event EventHandler? Invalidated;

        #endregion
    }
}
