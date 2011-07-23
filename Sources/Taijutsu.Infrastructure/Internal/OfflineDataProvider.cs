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
using System.Data;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;


namespace Taijutsu.Infrastructure.Internal
{
    public class OfflineDataProvider : DataProvider
    {
        public override object NativeProvider
        {
            get { return new object(); }
        }

        public override void Close()
        {
        }

        public override void BeginTransaction(IsolationLevel level)
        {
        }

        public override void Commit()
        {
        }

        public override void Rollback()
        {
        }

        protected virtual Exception GenerateException()
        {
            return new Exception("It's impossible to use offline data provider.");
        }

        public override object MarkAsCreated<TEntity>(TEntity entity)
        {
            throw GenerateException();
        }

        public override void MarkAsRemoved<TEntity>(TEntity entity)
        {
            throw GenerateException();
        }

        public override IQueryOfEntities<TEntity> AllOf<TEntity>()
        {
            throw GenerateException();
        }

        public override IQueryOfEntityByKey<TEntity> UniqueOf<TEntity>(object key)
        {
            throw GenerateException();
        }


        public override IQueryOverBuilder<TEntity> QueryOver<TEntity>()
        {
            throw GenerateException();
        }


        public override IMarkingStep<TEntity> Mark<TEntity>(TEntity entity)
        {
            throw GenerateException();
        }
    }
}