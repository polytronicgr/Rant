﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Rant.Engine.Syntax;
using Rant.Stringes;

namespace Rant.Engine.Compiler.Parselets
{
    internal abstract class Parselet
    {
        // static stuff

        static bool loaded = false;
        static Dictionary<R, Parselet> parseletDict;
        static Parselet defaultParselet;

        static Parselet()
        {
            Load();
        }

        public static void Load(bool forceNewLoad = false)
        {
            if (loaded && !forceNewLoad)
                return;

            // scan the Compiler.Parselets namespace for all parselets, create instances of them store them in a dictionary
            // it's clean, it's super easy to extend. a single statement loads them all from the namespace and sets them up right
            // it's slow and may be a memory hog with many parselets
            // maybe create the instances of the parselets as needed?

            parseletDict = new Dictionary<R, Parselet>();

            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t =>
                t.IsClass &&
                !t.IsNested && // for some reason makes sure some weird type isn't included
                !t.IsAbstract && // makes sure we don't load this base class
                t.Namespace == "Rant.Engine.Compiler.Parselets");

            foreach (var type in types)
            {
                if (type.GetCustomAttribute<DefaultParseletAttribute>() != null)
                {
                    if (defaultParselet != null)
                        throw new RantInternalException($"{defaultParselet.GetType().Name} is already defined as default parselet");

                    defaultParselet = (Parselet)Activator.CreateInstance(type);
                    continue;
                }

                var instance = (Parselet)Activator.CreateInstance(type);

                foreach (var id in instance.Identifiers)
                    parseletDict.Add(id, instance);
            }

            loaded = true;
        }

        // looks kinda silly to pass the compiler in
        public static Parselet GetWithToken(NewRantCompiler compiler, Token<R> token, Action<RantAction> addToOutputDelegate = null)
        {
            Parselet parselet;
            if (parseletDict.TryGetValue(token.ID, out parselet))
            {
                if (addToOutputDelegate == null)
                    parselet.AddToOutput = compiler.AddToOutput;
                else
                    parselet.AddToOutput = addToOutputDelegate;

                parselet.Token = token; // NOTE: this way of passing the appropriate token is kind of ugly but it works
                return parselet;
            }

            return GetDefaultParselet(compiler, token, addToOutputDelegate);
        }

        public static Parselet GetDefaultParselet(NewRantCompiler compiler, Token<R> token, Action<RantAction> addToOutputDelegate = null)
        {
            if (defaultParselet == null)
                throw new RantInternalException("DefaultParselet not set");

            if (addToOutputDelegate == null)
                defaultParselet.AddToOutput = compiler.AddToOutput;
            else
                defaultParselet.AddToOutput = addToOutputDelegate;

            defaultParselet.Token = token;
            return defaultParselet;
        }

        // instance stuff

        public abstract R[] Identifiers { get; }
        protected Token<R> Token { get; private set; }
        public Action<RantAction> AddToOutput { get; set; }

        public Parselet()
        {
        }

        public abstract IEnumerator<Parselet> Parse(NewRantCompiler compiler, TokenReader reader, Token<R> fromToken);
    }
}
