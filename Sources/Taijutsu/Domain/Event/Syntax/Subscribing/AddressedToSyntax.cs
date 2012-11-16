// Copyright 2009-2012 Taijutsu.
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
using System.Linq;
using Taijutsu.Domain.Event.Internal;

namespace Taijutsu.Domain.Event.Syntax.Subscribing
{
    public static class AddressedToSyntax
    {
        // ReSharper disable InconsistentNaming

        #region Nested type: All

        public interface All : IHiddenObjectMembers
        {
            Or Or { get; }
            SubscriptionSyntax.All<IEventDueToFact<TFact>> DueTo<TFact>() where TFact : IFact;
        }

        public interface All<out TEntity> : IHiddenObjectMembers where TEntity : IEntity
        {
            Or Or { get; }
            Full<TEntity, TFact> DueTo<TFact>() where TFact : IFact;
        }

        #endregion

        #region Nested type: AllImpl

        internal class AllImpl : All
        {
            private readonly Func<IInternalEventHandler, Action> addHadlerAction;
            private readonly List<Type> addressTypes = new List<Type>();

            public AllImpl(Func<IInternalEventHandler, Action> addHadlerAction, IEnumerable<Type> addressTypes)
            {
                this.addHadlerAction = addHadlerAction;
                this.addressTypes.AddRange(addressTypes);
            }

            #region All Members

            Or All.Or
            {
                get { return new OrImpl(addHadlerAction, new List<Type>(addressTypes)); }
            }

            SubscriptionSyntax.All<IEventDueToFact<TFact>> All.DueTo<TFact>()
            {
                return new SubscriptionSyntax.AllImpl<IEventDueToFact<TFact>>(addHadlerAction, addressTypes
                                                                              .Select<Type, Func<IEventDueToFact<TFact>, bool>>(t => e => t.IsAssignableFrom(e.GetType())));
            }

            #endregion
        }

        #endregion

        #region Nested type: Full

        public interface Full<out TEntity, out TFact> : SubscriptionSyntax.All<IExternalEvent<TEntity, TFact>>
            where TEntity : IEntity where TFact : IFact
        {
            Action Subscribe(Func<TEntity, Action<TFact>> subscriber, int priority = 0);
            Action Subscribe(Func<TEntity, DateTime, Action<TFact, DateTime>> subscriber, int priority = 0);
            Action Subscribe(Func<TEntity, DateTime, DateTime, Action<TFact, DateTime, DateTime>> subscriber, int priority = 0);
        }

        #endregion

        #region Nested type: FullImpl

        internal class FullImpl<TEntity, TFact> : SubscriptionSyntax.AllImpl<IExternalEvent<TEntity, TFact>>,
                                                  Full<TEntity, TFact>
            where TEntity : IEntity where TFact : IFact
        {
            internal FullImpl(Func<IInternalEventHandler, Action> addHadlerAction,
                              IEnumerable<Func<IExternalEvent<TEntity, TFact>, bool>> eventFilters = null)
                : base(addHadlerAction, eventFilters)
            {
            }

            #region Full<TEntity,TFact> Members

            public Action Subscribe(Func<TEntity, Action<TFact>> subscriber, int priority = 0)
            {
                Action<IExternalEvent<TEntity, TFact>> modSubscriber = e => subscriber(e.AddressedTo)(e.Fact);
                return
                    AddHadlerAction(new InternalEventHandler<IExternalEvent<TEntity, TFact>>(modSubscriber, e => !EventFilters.Any(f => !f(e)), priority));
            }

            public Action Subscribe(Func<TEntity, DateTime, Action<TFact, DateTime>> subscriber, int priority = 0)
            {
                Action<IExternalEvent<TEntity, TFact>> modSubscriber =
                    e => subscriber(e.AddressedTo, e.DateOfOccurrence)(e.Fact, e.DateOfOccurrence);
                return
                    AddHadlerAction(new InternalEventHandler<IExternalEvent<TEntity, TFact>>(modSubscriber, e =>!EventFilters.Any(f => !f(e)),priority));
            }

            public Action Subscribe(Func<TEntity, DateTime, DateTime, Action<TFact, DateTime, DateTime>> subscriber,
                                    int priority = 0)
            {
                Action<IExternalEvent<TEntity, TFact>> modSubscriber =
                    e =>
                    subscriber(e.AddressedTo, e.DateOfOccurrence, e.DateOfNotice)(e.Fact, e.DateOfOccurrence,
                                                                                  e.DateOfNotice);

                return
                    AddHadlerAction(new InternalEventHandler<IExternalEvent<TEntity, TFact>>(modSubscriber, e => !EventFilters.Any(f => !f(e)), priority));
            }

            #endregion
        }

        #endregion

        #region Nested type: Init

        public interface Init<out TEntity> : All<TEntity>,
                                             SubscriptionSyntax.All<IExternalEvent<TEntity>>
            where TEntity : IEntity
        {
        }

        #endregion

        #region Nested type: InitImpl

        internal class InitImpl<TEntity> : Init<TEntity> where TEntity : IEntity
        {
            private readonly Func<IInternalEventHandler, Action> addHadlerAction;

            public InitImpl(Func<IInternalEventHandler, Action> addHadlerAction)
            {
                this.addHadlerAction = addHadlerAction;
            }

            #region Init<TEntity> Members

            Or All<TEntity>.Or
            {
                get { return new OrImpl(addHadlerAction, new[] {typeof (IExternalEvent<TEntity>)}); }
            }

            Full<TEntity, TFact> All<TEntity>.DueTo<TFact>()
            {
                return new FullImpl<TEntity, TFact>(addHadlerAction);
            }

            SubscriptionSyntax.All<IExternalEvent<TEntity>> SubscriptionSyntax.All<IExternalEvent<TEntity>>.Where(
                Func<IExternalEvent<TEntity>, bool> filter)
            {
                return new SubscriptionSyntax.AllImpl<IExternalEvent<TEntity>>(addHadlerAction, new[] {filter});
            }

            SubscriptionSyntax.Projection<IExternalEvent<TEntity>, TProjection>
                SubscriptionSyntax.All<IExternalEvent<TEntity>>.Select<TProjection>(
                Func<IExternalEvent<TEntity>, TProjection> projection)
            {
                return
                    new SubscriptionSyntax.ProjectionImpl<IExternalEvent<TEntity>, TProjection>(
                        new SubscriptionSyntax.AllImpl<IExternalEvent<TEntity>>(addHadlerAction), projection);
            }

            Action SubscriptionSyntax.All<IExternalEvent<TEntity>>.Subscribe(Action<IExternalEvent<TEntity>> subscriber,
                                                                             int priority)
            {
                Predicate<IExternalEvent<TEntity>> filter = e => true;
                return addHadlerAction(new InternalEventHandler<IExternalEvent<TEntity>>(subscriber, filter, priority));
            }

            Action SubscriptionSyntax.All<IExternalEvent<TEntity>>.Subscribe(IHandlerOf<IExternalEvent<TEntity>> subscriber, int priority)
            {
                return (this as SubscriptionSyntax.All<IExternalEvent<TEntity>>).Subscribe(subscriber.Handle, priority);
            }

            #endregion
        }

        #endregion

        #region Nested type: Or

        public interface Or : IHiddenObjectMembers
        {
            All AddressedTo<TEntity>() where TEntity : IEntity;
        }

        #endregion

        #region Nested type: OrImpl

        internal class OrImpl : Or
        {
            private readonly Func<IInternalEventHandler, Action> addHadlerAction;
            private readonly List<Type> addressTypes = new List<Type>();

            public OrImpl(Func<IInternalEventHandler, Action> addHadlerAction, IEnumerable<Type> addressTypes)
            {
                this.addHadlerAction = addHadlerAction;
                this.addressTypes.AddRange(addressTypes);
            }

            #region Or Members

            All Or.AddressedTo<TEntity>()
            {
                return new AllImpl(addHadlerAction, new List<Type>(addressTypes) {typeof (IExternalEvent<TEntity>)});
            }

            #endregion
        }

        #endregion

        // ReSharper restore InconsistentNaming       
    }
}