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

namespace Taijutsu
{
    public static class ActionExtensions
    {
        public static IDisposable AsDisposable(this Action self)
        {
            if (self == null) throw new ArgumentNullException("self");

            return new DisposableAction(self);
        }

        private class DisposableAction : IDisposable
        {
            private Action action;

            public DisposableAction(Action action)
            {
                this.action = action;
            }

            public void Dispose()
            {
                if (action == null)
                    return;

                var preserved = action;

                action = null;

                preserved();
            }
        }
    }
}