﻿// Copyright 2009-2011 Taijutsu.
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

using Taijutsu.Domain.Query;

namespace Taijutsu.Domain
{
    public interface IUnitOfWork
    {
        object MarkAsCreated<TEntity>(TEntity entity) where TEntity : IAggregateRoot;

        void MarkAsRemoved<TEntity>(TEntity entity) where TEntity : IRemovableEntity;

        IQueryOfEntities<TEntity> AllOf<TEntity>() where TEntity : class, IQueryableEntity;

        IQueryOfEntityByKey<TEntity> UniqueOf<TEntity>(object key) where TEntity : class, IQueryableEntity;

        IMarkingStep<TEntity> Mark<TEntity>(TEntity entity) where TEntity : IRemovableEntity, IAggregateRoot;

    }

    public interface IMarkingStep<out TEntity> where TEntity : IRemovableEntity, IAggregateRoot
    {
        object AsCreated();
        void AsRemoved();
    }


    public static class UnitOfWorkEntityEx
    {
        public static object AsCreatedIn(this IAggregateRoot entity, IUnitOfWork uow)
        {
            return uow.MarkAsCreated(entity);
        }

        public static void AsRemovedIn(this IRemovableEntity entity, IUnitOfWork uow)
        {
            uow.MarkAsRemoved(entity);
        }
    }
}