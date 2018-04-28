﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

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
    class Program
    {
        static void Main(string[] args)
        {
            using (Mutex mutex = new Mutex(false, GlobalItem.MutexName))
            {
                //  多重禁止
                if (!mutex.WaitOne(0, false)) { return; }

                //  カレントディレクトリ
                Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                //  対象のフォルダーを検索してFileLibraryリストを取得
                if (args.Length == 0) { return; }

                UpdateLibrary ul = new UpdateLibrary();
                ul.Search(args[0]);
            }
        }
    }
}
