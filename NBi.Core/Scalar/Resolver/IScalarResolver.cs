﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBi.Core.Scalar.Resolver
{
    public interface IScalarResolver
    {
        object Execute();
    }

    public interface IScalarResolver<T> : IScalarResolver
    {
        new T Execute();
    }
}
