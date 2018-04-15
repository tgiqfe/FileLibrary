using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
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
                string targetDir = args[0];
                
                //  DB接続開始
                ConnectDB cdb = new ConnectDB(GlobalItem.MongoDB, GlobalItem.DBName);
                cdb.Connect_fllist(GlobalItem.CollectionName_fllist);
                cdb.Connect_aclist(GlobalItem.CollectionName_aclist);
                List<ActivityLog> acList = new List<ActivityLog>();
                acList.Add(new ActivityLog(1, "global", "開始"));
                Console.WriteLine("開始");
                string endMessage = "終了";

                //  DBからFileLibraryリストを取得
                FilterDefinition<FileLibrary> filter =
                    Builders<FileLibrary>.Filter.Eq("TopDir", Path.GetFileName(targetDir).ToUpper());
                List<FileLibrary> dbFLList = cdb.Collection_fllist.Find(filter).ToList();

                if (Directory.Exists(targetDir))
                {
                    List<FileLibrary>  myFLList = FileLibrary.Search(targetDir);
                    
                    //  DBとローカルファイルの照合
                    for (int i = myFLList.Count - 1; i >= 0; i--)
                    {
                        if (dbFLList.Contains(myFLList[i]))
                        {
                            dbFLList.Remove(myFLList[i]);
                            myFLList.RemoveAt(i);
                        }
                    }

                    //  DBから不一致分を削除
                    FilterDefinitionBuilder<FileLibrary> builder = Builders<FileLibrary>.Filter;
                    foreach (FileLibrary tempFL in dbFLList)
                    {
                        cdb.Collection_fllist.DeleteOne(builder.Eq("_id", tempFL._id));
                        string message = "削除：" + tempFL.TopDir + "/" + tempFL.RelativePath;
                        acList.Add(new ActivityLog(1, "global", message));
                        Console.WriteLine(message);
                    }

                    //  DBに追加
                    if (myFLList.Count > 0)
                    {
                        cdb.Collection_fllist.InsertMany(myFLList);
                        foreach (FileLibrary tempFL in myFLList)
                        {
                            string message = "追加/更新：" + tempFL.TopDir + "/" + tempFL.RelativePath;
                            acList.Add(new ActivityLog(1, "global", message));
                            Console.WriteLine(message);
                        }
                    }

                    endMessage += "  追加/更新：" + myFLList.Count.ToString() + ", 削除：" + dbFLList.Count.ToString();
                }
                else
                {
                    //  DBから不一致分を削除
                    FilterDefinitionBuilder<FileLibrary> builder = Builders<FileLibrary>.Filter;
                    foreach (FileLibrary tempFL in dbFLList)
                    {
                        cdb.Collection_fllist.DeleteOne(builder.Eq("_id", tempFL._id));
                        string message = "削除：" + tempFL.TopDir + "/" + tempFL.RelativePath;
                        acList.Add(new ActivityLog(1, "global", message));
                        Console.WriteLine(message);
                    }
                    
                    acList.Add(new ActivityLog(1, "global", "対象フォルダー無し：" + targetDir));
                    Console.WriteLine("対象フォルダー無し：" + targetDir);
                    endMessage += "  削除：" + dbFLList.Count.ToString();
                }

                //  終了
                acList.Add(new ActivityLog(1, "global", endMessage));
                Console.WriteLine(endMessage);
                cdb.Collection_aclist.InsertMany(acList);
            }
        }
    }
}
