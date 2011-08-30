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

using System.Linq;

namespace Taijutsu.Domain
{
    public static class EntityEx
    {
        public static T As<T>(this IEntity entity) where T : class
        {
            var conversion = EntityConversionRegistry.Custom.Where(ec => ec.IsApplicableFor(entity)).FirstOrDefault();

            return conversion != null ? conversion.SafeCast<T>(entity) : EntityConversionRegistry.Native.SafeCast<T>(entity);
        }

        public static bool Is<T>(this IEntity entity) where T : class
        {
            return As<T>(entity) != null;
        }
    }
}