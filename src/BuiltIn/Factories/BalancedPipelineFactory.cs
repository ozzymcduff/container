﻿using Unity.Container;
using Unity.Resolution;

namespace Unity.BuiltIn
{
    public static class BalancedPipelineFactory
    {
        public static ResolveDelegate<ResolveContext> Factory(in Contract contract, RegistrationManager? manager = null)
        {
            return (ref ResolveContext context) => null;
        }
    }
}
