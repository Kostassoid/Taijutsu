﻿#region License

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
using System.ComponentModel;

namespace Taijutsu
{
    public static class SystemTime
    {
        private static ITimeController controller = new InternalTimeController();

        public static DateTime Now
        {
            get { return controller.Now; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ITimeController TimeController
        {
            get { return controller; }
            set { controller = value; }
        }
    }

    internal class InternalTimeController : ITimeController
    {
        private Func<DateTime> nowFunc = () => DateTime.Now;

        public DateTime Now
        {
            get { return nowFunc(); }
        }

        public void Customize(Func<DateTime> func)
        {
            nowFunc = func;
        }

        public void SetDate(DateTime date)
        {
            var whnStd = DateTime.Now;
            Func<DateTime> func = () => date + (DateTime.Now - whnStd);
            nowFunc = func;
        }

        public void SetFrozenDate(DateTime date)
        {
            nowFunc = () => date;
        }

        public void Reset()
        {
            nowFunc = () => DateTime.Now;
        }
    }

    public interface ITimeController
    {
        DateTime Now { get; }
        void Customize(Func<DateTime> dateFunc);
        void SetDate(DateTime date);
        void SetFrozenDate(DateTime date);
        void Reset();
    }
}