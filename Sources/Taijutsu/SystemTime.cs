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
using System.ComponentModel;

namespace Taijutsu
{
    public static class SystemTime
    {
        private static ITimeController controller = new UtcTimeController();

        public static DateTime Now
        {
            get { return controller.Now; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Reset()
        {
            controller = new UtcTimeController();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ITimeController TimeController
        {
            get { return controller; }
            set { controller = value ?? controller; }
        }
    }

    internal class UtcTimeController : ITimeController
    {
        private Func<DateTime> nowFunc = () => DateTime.UtcNow;

        public DateTime Now
        {
            get { return nowFunc(); }
        }

        public void Customize(Func<DateTime> func)
        {
            if (func == null) throw new ArgumentNullException("func");

            nowFunc = func;
        }

        public void SetDate(DateTime date)
        {
            var whnStd = DateTime.UtcNow;
            Func<DateTime> func = () => date + (DateTime.UtcNow - whnStd);
            nowFunc = func;
        }

        public void SetFrozenDate(DateTime date)
        {
            nowFunc = () => date;
        }

        public void Reset()
        {
            nowFunc = () => DateTime.UtcNow;
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