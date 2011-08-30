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
using System.Diagnostics;

namespace Taijutsu.Data.Internal
{
    public class DataContext : IDataContext
    {
        private readonly DataContextSupervisor supervisor;
        private readonly UnitOfWorkConfig unitOfWorkConfig;
        private bool closed;
        private bool commited;
        private DataProvider dataProvider;
        private IDictionary<string, IDisposable> extension;
        private bool rolledback;
        private int slaveCount;

        public DataContext(UnitOfWorkConfig unitOfWorkConfig, DataContextSupervisor supervisor)
        {
            this.unitOfWorkConfig = unitOfWorkConfig;
            this.supervisor = supervisor;
            dataProvider = supervisor.CreateDataProvider(unitOfWorkConfig);
            dataProvider.BeginTransaction(unitOfWorkConfig.IsolationLevel);
        }

        protected internal virtual DataProvider DataProvider
        {
            get { return dataProvider; }
        }

        public virtual bool Commited
        {
            get { return commited; }
        }

        public virtual bool Rolledback
        {
            get { return rolledback; }
        }

        #region IDataContext Members

        public virtual UnitOfWorkConfig UnitOfWorkConfig
        {
            get { return unitOfWorkConfig; }
        }


        public virtual IDataProvider Provider
        {
            get { return DataProvider; }
        }


        IReadOnlyDataProvider IReadOnlyDataContext.ReadOnlyProvider
        {
            get { return Provider; }
        }

        public virtual bool Ready
        {
            get { return slaveCount == 0; }
        }

        public virtual bool IsRoot
        {
            get { return true; }
        }

        public virtual bool Closed
        {
            get { return closed; }
        }

        public virtual void Commit()
        {
            if (commited || closed || rolledback)
            {
                throw new Exception(
                    string.Format(
                        "Data context can't commit data provider. State map: commited '{0}', rolledback '{1}', closed '{2}'.",
                        commited, rolledback, closed));
            }
            commited = true;
            DataProvider.CommitTransaction();
        }

        public virtual void Rollback()
        {
            if (commited || closed || rolledback)
            {
                throw new Exception(
                    string.Format(
                        "Data context can't rollback data provider. State map: commited '{0}', rolledback '{1}', closed '{2}'.",
                        commited, rolledback, closed));
            }
            rolledback = true;
            DataProvider.RollbackTransaction();
        }

        public virtual void Close()
        {
            if (closed)
            {
                throw new Exception(
                    string.Format(
                        "Data context can't close data provider, because it has been already closed."));
            }
            closed = true;
            Exception lastExException = null;
            if (extension != null)
            {
                foreach (var disposable in extension.Values)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception exception)
                    {
                        lastExException = exception;
                        Trace.TraceWarning(exception.ToString());
                    }
                }
            }

            extension = null;
            dataProvider = supervisor.RegisterForTerminate(this);
            
            if (lastExException != null)
            {
                throw lastExException;
            }

        }

        void IDisposable.Dispose()
        {
            if (!Closed)
            {
                Close();
            }
        }

        public virtual IDictionary<string, IDisposable> Extension
        {
            get { return extension ?? (extension = new Dictionary<string, IDisposable>()); }
        }

        #endregion

        public virtual void RegisterCompletedSlave()
        {
            slaveCount++;
        }

        public virtual void RegisterUncompletedSlave()
        {
            slaveCount--;
        }
    }

    public class ChildDataContext : IDataContext
    {
        private readonly DataContext dataContext;
        private bool? completed;
        private IDictionary<string, IDisposable> extension;

        public ChildDataContext(DataContext dataContext)
        {
            this.dataContext = dataContext;
            dataContext.RegisterUncompletedSlave();
        }

        #region IDataContext Members

        public virtual bool IsRoot
        {
            get { return false; }
        }

        public virtual bool Closed
        {
            get { return dataContext.Closed; }
        }

        IReadOnlyDataProvider IReadOnlyDataContext.ReadOnlyProvider
        {
            get { return ((IReadOnlyDataContext) dataContext).ReadOnlyProvider; }
        }


        public virtual UnitOfWorkConfig UnitOfWorkConfig
        {
            get { return dataContext.UnitOfWorkConfig; }
        }

        public virtual IDataProvider Provider
        {
            get { return dataContext.Provider; }
        }

        public virtual bool Ready
        {
            get { return true; }
        }

        public virtual void Commit()
        {
            if (!completed.HasValue)
            {
                dataContext.RegisterCompletedSlave();
                completed = true;
            }
        }

        public virtual void Rollback()
        {
            if (!completed.HasValue || !completed.Value)
            {
                completed = false;
            }
        }

        void IReadOnlyDataContext.Close()
        {
            Exception lastExException = null;
            if (extension != null)
            {
                foreach (var disposable in extension.Values)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception exception)
                    {
                        lastExException = exception;
                        Trace.TraceWarning(exception.ToString());
                    }
                }
            }

            extension = null;

            if (lastExException != null)
            {
                throw lastExException;
            }
        }

        void IDisposable.Dispose()
        {
            if (!Closed)
            {
                ((IReadOnlyDataContext) this).Close();
            }
        }

        
        public virtual IDictionary<string, IDisposable> Extension
        {
            get { return extension ?? (extension = new Dictionary<string, IDisposable>()); }
        }

        #endregion
    }
}