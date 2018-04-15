using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class GlobalItem
    {
        //  プロセス実行時のミューテックス名
        public const string MutexName = "FileLibrary";

        //  DB接続情報
        public const string MongoDB = "mongodb://localhost";
        public const string DBName = "fileLibrary";
        public const string CollectionName_fllist = "flList";
        public const string CollectionName_aclist = "acList";
        
        //  ハッシュモード
        public enum EHashMode { MD5, SHA256, CRC32 }
        public static EHashMode HashMode = EHashMode.SHA256;
    }
}
