using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

//  License
//  This software includes the work that is distributed in the Apache License 2.0

/*
   Copyright [2018] [name of copyright owner]

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific languag
*/

namespace FileLibrary
{
    class ActivityLog
    {
        //  クラスパラメータ
        public ObjectId _id { get; set; }
        public long LogTime { get; set; }
        public int LogLevel { get; set; }
        public string LogType { get; set; }
        public string Message { get; set; }

        //  コンストラクタ
        public ActivityLog() { }
        public ActivityLog(int logLevel, string logType, string message)
        {
            this.LogTime = DateTime.Now.ToFileTime();
            this.LogLevel = logLevel;
            this.LogType = logType;
            this.Message = message;
        }
    }
}
