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
using System.Data;
using Taijutsu.Infrastructure.Internal;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Infrastructure
{
    public class UnitOfWork : IUnitOfWork, IAdvancedUnitOfWork, INativeUnitOf, IDisposable
    {
        private readonly IDataContext dataContext;
        private bool? completed;

        public UnitOfWork(string source = "", IsolationLevel isolation = IsolationLevel.Unspecified,
                           Require require = Require.None) : this(new UnitOfWorkConfig(source, isolation, require))
        {
        }

        public UnitOfWork(IsolationLevel isolation = IsolationLevel.Unspecified)
            : this(new UnitOfWorkConfig("", isolation, Require.None))
        {
        }

        public UnitOfWork(Require require)
            : this(new UnitOfWorkConfig("", IsolationLevel.Unspecified, require))
        {
        }

        public UnitOfWork(string source)
            : this(new UnitOfWorkConfig(source, IsolationLevel.Unspecified, Require.None))
        {
        }


        public UnitOfWork() : this(new UnitOfWorkConfig())
        {
        }

        public UnitOfWork(UnitOfWorkConfig unitOfWorkConfig)
        {
            dataContext = SupervisorContext.DataContextSupervisor.RegisterUnitOfWorkBasedOn(unitOfWorkConfig);
        }

        #region IAdvancedUnitOfWork Members

        IDictionary<string, IDisposable> IAdvancedUnitOfWork.Extension
        {
            get { return dataContext.Extension; }
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            try
            {
                if (!completed.HasValue)
                {
                    dataContext.Rollback();
                }
            }
            finally
            {
                dataContext.Close();
            }
        }

        #endregion

        #region INativeUnitOf Members

        object INativeUnitOf.Native
        {
            get { return dataContext.Provider.NativeProvider; }
        }

        #endregion

        #region IUnitOfWork Members

        public object MarkAsCreated<TEntity>(TEntity entity) where TEntity : IAggregateRoot
        {
            return dataContext.Provider.MarkAsCreated(entity);
        }

        public void MarkAsRemoved<TEntity>(TEntity entity) where TEntity : IRemovableEntity
        {
            dataContext.Provider.MarkAsRemoved(entity);
        }

        public IMarkingStep<TEntity> Mark<TEntity>(TEntity entity) where TEntity : IRemovableEntity, IAggregateRoot
        {
            return new MarkingStep<TEntity>(() => MarkAsCreated(entity), () => MarkAsRemoved(entity));
        }

        public virtual IQueryOfEntities<TEntity> AllOf<TEntity>() where TEntity : class, IQueryableEntity
        {
            return dataContext.Provider.AllOf<TEntity>();
        }

        public virtual IQueryOfEntityByKey<TEntity> UniqueOf<TEntity>(object key)
            where TEntity : class, IQueryableEntity
        {
            return dataContext.Provider.UniqueOf<TEntity>(key);
        }

        public virtual IQueryOverBuilder<TEntity> Over<TEntity>() where TEntity : class, IQueryableEntity
        {
            return dataContext.Provider.QueryOver<TEntity>();
        }

        #endregion

        public virtual void Complete()
        {
            if (!dataContext.Ready)
            {
                throw new Exception(
                    "Unit of work can not be successfully completed, because of not all subordinate units of work are successfully completed.");
            }

            if (completed.HasValue)
            {
                throw new Exception(string.Format("Unit of work has been already completed(with success - {0}).", completed));                
            }

            try
            {
                dataContext.Commit();
            }
            catch
            {
                completed = false;
                throw;
            }

            completed = true;
        }

        public virtual T Complete<T>(Func<UnitOfWork, T> forReturn)
        {
            var result = forReturn(this);
            Complete();
            return result;
        }

        public virtual T Complete<T>(Func<T> forReturn)
        {
            var result = forReturn();
            Complete();
            return result;
        }

        public virtual T Complete<T>(T forReturn)
        {
            Complete();
            return forReturn;
        }
    }
}