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
    class ConnectDB
    {
        //  プライベートクラスパラメータ
        private IMongoClient CLient { get; set; }
        private IMongoDatabase Database_fl { get; set; }
        public IMongoCollection<FileLibrary> Collection_fllist { get; set; }
        public IMongoCollection<ActivityLog> Collection_aclist { get; set; }

        //  コンストラクタ
        public ConnectDB() { }
        public ConnectDB(string mongoDB, string dbName)
        {
            this.CLient = new MongoClient(mongoDB);
            this.Database_fl = CLient.GetDatabase(dbName);
        }

        //  FileLibrary用のコレクションに接続
        public void Connect_fllist(string collectionName)
        {
            this.Collection_fllist = Database_fl.GetCollection<FileLibrary>(collectionName);
        }

        //  ActivityLog用のコレクションに接続
        public void Connect_aclist(string collectionName)
        {
            this.Collection_aclist = Database_fl.GetCollection<ActivityLog>(collectionName);
        }
    }
}
