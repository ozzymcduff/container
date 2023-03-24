﻿using Unity.Resolution;

namespace Unity.Extension
{
    public delegate void PipelineDelegate<TContext>(ref TContext context)
        where TContext : IBuilderContext;
}

