﻿#region License

//  Copyright 2009-2013 Nikita Govorov
//    
//  Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
//  this file except in compliance with the License. You may obtain a copy of the 
//  License at 
//   
//  http://www.apache.org/licenses/LICENSE-2.0 
//   
//  Unless required by applicable law or agreed to in writing, software distributed 
//  under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
//  CONDITIONS OF ANY KIND, either express or implied. See the License for the 
//  specific language governing permissions and limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace Taijutsu
{
    public static class LogicContext
    {
        private static ILogicContext context = new DefaultLogicContext();
        private static bool initialized;
        private static readonly object sync = new object();

        // ReSharper disable ParameterHidesMember
        public static void Customize(ILogicContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            lock (sync)
            {
                if (initialized)
                {
                    throw new Exception("Context has been already initialized. Any method call causes to initialization of the context. Context can be customized only once before initialization.");
                }
            
                initialized = true;

                LogicContext.context = context;
            }
        }

        // ReSharper restore ParameterHidesMember

        private static void CheckInitialization()
        {
            if (!initialized)
            {
                lock (sync)
                {
                    if (!initialized)
                    {
                        initialized = true;
                    }
                }
            }
        }

        public static object FindData(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            CheckInitialization();

            return context.FindData(name);
        }

        public static void SetData(string name, object value)
        {
            if (name == null) throw new ArgumentNullException("name");

            CheckInitialization();

            context.SetData(name, value);
        }

        public static void ReleaseData(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            CheckInitialization();

            context.ReleaseData(name);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static void Reset()
        {
            initialized = false;
            context= new DefaultLogicContext();
        }
    }

    public class DefaultLogicContext : ILogicContext
    {
        object ILogicContext.FindData(string name)
        {
            return CallContext.GetData(name);
        }

        void ILogicContext.SetData(string name, object value)
        {
            CallContext.SetData(name, value);
        }

        void ILogicContext.ReleaseData(string name)
        {
            CallContext.FreeNamedDataSlot(name);
        }

        bool ILogicContext.IsApplicable()
        {
            return true;
        }
    }

    public class HybridLogicContext : ILogicContext
    {
        readonly IEnumerable<ILogicContext> contexts;

        public HybridLogicContext(IEnumerable<ILogicContext> contexts)
        {
            if (contexts == null) throw new ArgumentNullException("contexts");

            this.contexts = contexts.ToList();
        }

        protected virtual ILogicContext DetermineContext()
        {
            var context =  contexts.FirstOrDefault(c => c.IsApplicable());
            
            if (context == null)
            {
                throw new Exception("Unable to determine context implementation in the current runtime environment.");    
            }

            return context;
        }

        object ILogicContext.FindData(string name)
        {
            return DetermineContext().FindData(name);
        }

        void ILogicContext.SetData(string name, object value)
        {
            DetermineContext().SetData(name, value);
        }

        void ILogicContext.ReleaseData(string name)
        {
            DetermineContext().ReleaseData(name);
        }

        bool ILogicContext.IsApplicable()
        {
            return true;
        }
    }
}