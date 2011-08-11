// Copyright 2009-2011 Taijutsu.
//   
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
//  
//      http://www.apache.org/licenses/LICENSE-2.0 
//  
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using Taijutsu.Domain.Event.Syntax;

namespace Taijutsu.Domain.Event.Internal
{
    internal class EventAggregator : IEventAggregator, IObservableSyntax, IEventStreamSyntax
    {
        private readonly object sync = new object();
        private IDictionary<Type, IList<IEventHandler>> handlersDictionary = new Dictionary<Type, IList<IEventHandler>>();

        #region IEventStreamSyntax Members

        DueToSyntax.Init<TFact> IEventStreamSyntax.DueTo<TFact>()
        {
            return new DueToSyntax.InitImpl<TFact>(Subscribe);
        }

        InitiatedBySyntax.Init<TEntity> IEventStreamSyntax.InitiatedBy<TEntity>()
        {
            return new InitiatedBySyntax.InitImpl<TEntity>(Subscribe);
        }

        AddressedToSyntax.Init<TEntity> IEventStreamSyntax.AddressedTo<TEntity>()
        {
            return new AddressedToSyntax.InitImpl<TEntity>(Subscribe);
        }

        #endregion

        #region IObservable Members

        public virtual IObservableSyntax OnStream
        {
            get { return this; }
        }

        public virtual void Raise<TEvent>(TEvent ev) where TEvent : IEvent
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IObservableSyntax Members

        IEventStreamSyntax IObservableSyntax.OfEvents
        {
            get { return this; }
        }

        SubscriptionSyntax.All<TEvent> IObservableSyntax.Of<TEvent>()
        {
            return new SubscriptionSyntax.AllImpl<TEvent>(Subscribe);
        }

        #endregion

        protected virtual Action Subscribe(IEventHandler handler)
        {
            lock (sync)
            {
                IList<IEventHandler> handlers;
                if (!handlersDictionary.TryGetValue(handler.EventType, out handlers))
                {
                    var newHandlersDictionary = new Dictionary<Type, IList<IEventHandler>>(handlersDictionary) { { handler.EventType, new List<IEventHandler> { handler } } };
                    handlersDictionary = newHandlersDictionary;
                }
                else
                {
                    var newHandlers = new List<IEventHandler>(handlers) { handler };
                    handlersDictionary[handler.EventType] = newHandlers;
                }
            }
            
            return GenerateUnsubscriptionAction(handler);
        }


        protected virtual Action GenerateUnsubscriptionAction(IEventHandler handler)
        {
            return delegate
            {
                lock (sync)
                {
                    IList<IEventHandler> handlers;
                    if (handlersDictionary.TryGetValue(handler.EventType, out handlers))
                    {
                        var newHandlers = new List<IEventHandler>(handlers);
                        newHandlers.Remove(handler);
                        handlersDictionary[handler.EventType] = newHandlers;
                    }
                }
            };
        }
    }
}