﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rant.Engine.Compiler
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class TokenParserAttribute : Attribute
    {
        public string Name { get; set; }
        public R? TokenType => tokenType;

        private readonly R? tokenType;

        public TokenParserAttribute()
        {
        }

        public TokenParserAttribute(R tokenType)
        {
            this.tokenType = tokenType;
        }
    }
}
