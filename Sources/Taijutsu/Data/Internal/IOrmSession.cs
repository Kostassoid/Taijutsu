#region License

// Copyright 2009-2012 Taijutsu.
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
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Data.Internal
{
    public interface IOrmSession : ICompletableScope, IHasNativeObject
    {
        T As<T>(string name = null) where T : class;

        object MarkAsCreated<TEntity>(TEntity entity, dynamic options = null) where TEntity : IAggregateRoot;

        object MarkAsCreated<TEntity>(Func<TEntity> entityFactory, dynamic options = null) where TEntity : IAggregateRoot;

        void MarkAsDeleted<TEntity>(TEntity entity, dynamic options = null) where TEntity : IDeletableEntity;

        IQueryOfEntities<TEntity> AllOf<TEntity>(dynamic options) where TEntity : class, IEntity;

        IQueryOfEntityByKey<TEntity> UniqueOf<TEntity>(object key, dynamic options) where TEntity : class, IEntity;

        IQueryOverContinuation<TEntity> QueryOver<TEntity>() where TEntity : class, IEntity;
    }
}