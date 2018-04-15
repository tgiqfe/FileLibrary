using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IO;

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
    class UpdateLibrary
    {
        //  DB接続用クラス
        private ConnectDB cdb = null;

        //  コンストラクタ
        public UpdateLibrary()
        {
            this.cdb = new ConnectDB(GlobalItem.MongoDB, GlobalItem.DBName);
            cdb.Connect_fllist(GlobalItem.CollectionName_fllist);
        }

        //  
        public void Search(string targetDir)
        {
            FilterDefinition<FileLibrary> filter =
                Builders<FileLibrary>.Filter.Eq("TopDir", Path.GetFileName(targetDir).ToUpper());
            List<FileLibrary> dbFLList = cdb.Collection_fllist.Find(filter).ToList();

            if (Directory.Exists(targetDir))
            {
                //  対象フォルダーのFileLibrary取得
                List<FileLibrary> myFLList = FileLibrary.Search(targetDir);

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
                }

                //  DBに追加
                if (myFLList.Count > 0)
                {
                    cdb.Collection_fllist.InsertMany(myFLList);
                }
            }
            else
            {
                //  対象フォルダーが存在しない場合の処理
                FilterDefinitionBuilder<FileLibrary> builder = Builders<FileLibrary>.Filter;
                foreach (FileLibrary tempFL in dbFLList)
                {
                    cdb.Collection_fllist.DeleteOne(builder.Eq("_id", tempFL._id));
                }
            }
        }
    }
}
