#region License

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

using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Data.Internal
{
    public interface IOrmSession : ICompletableScope, IHasNativeObject, IUnitOfWork
    {
        T As<T>(object options = null) where T : class;

        TQuery QueryWith<TEntity, TQuery>(string name = null) where TEntity : class, IEntity where TQuery : IQuery<TEntity>;

        TRepository QueryFrom<TEntity, TRepository>(string name = null) where TEntity : class, IEntity where TRepository : IRepository<TEntity>;
    }
}