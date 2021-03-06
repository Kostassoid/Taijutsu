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

namespace Taijutsu.Domain
{
    [Serializable]
    public abstract class ValueObject<TValueObject> : IdentifiableObject<object>, IValueObject, IEquatable<TValueObject>
        where TValueObject : ValueObject<TValueObject>
    {
        public virtual bool Equals(TValueObject other)
        {
            if (ReferenceEquals(other, null)) return false;
            return ReferenceEquals(other, this) || Equals(other.BuildIdentity(), BuildIdentity());
        }


        public override bool Equals(object obj)
        {
            return Equals(obj as TValueObject);
        }

        public override int GetHashCode()
        {
            return BuildIdentity().GetHashCode();
        }

        public static bool operator ==(ValueObject<TValueObject> left, ValueObject<TValueObject> right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals((TValueObject) right);
        }

        public static bool operator !=(ValueObject<TValueObject> left, ValueObject<TValueObject> right)
        {
            return !(left == right);
        }
    }
}