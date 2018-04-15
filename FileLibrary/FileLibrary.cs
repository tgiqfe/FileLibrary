using MongoDB.Bson;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;

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

/*  Libraryに格納するファイル/フォルダー情報は、
 *      - ファイル名
 *      - 基準ディレクトリからの相対パス
 *      - 更新日時
 *      - ハッシュ
 *      - ファイルorフォルダーの判定
 *  のみとして、アクセス権、所有者、属性、アクセス日時、作成日時は無視します。
 *  マルチOSを想定しているので、このへんを保存してられない。
 */
namespace FileLibrary
{
    class FileLibrary
    {
        //  クラスパラメータ
        public ObjectId _id { get; set; }           //  MongoDB側で一意に管理されるID。特にC#側で制御はしない
        public string FileName { get; set; }        //  対象のファイル/フォルダー名。なぜFileNameか? だってGetFileNameで取得するから
        public string TopDir { get; set; }          //  サーチする対象フォルダー。MongoDB内を大文字/小文字でFindが難しそうなので、最初から大文字で管理
        public string RelativePath { get; set; }    //  TopDirから見た対象ファイル/フォルダーまでの相対パス。TopDir + RelativePathで一意の値として管理
        public long TimeStamp { get; set; }         //  対象ファイル/フォルダーの更新日時のNTタイムエポック
        public bool IsDirectory { get; set; }       //  フォルダーかどうかのフラグ。
        public string Hash { get; set; }            //  ファイルならばハッシュ値をここに。ハッシュモードはGlobalItemで指定。

        //  コンストラクタ
        public FileLibrary() { }
        public FileLibrary(string filePath, string basePath, bool isDirectory)
        {
            this.FileName = Path.GetFileName(filePath);
            this.TopDir = Path.GetFileName(basePath);
            this.RelativePath =
                new Uri(basePath + Path.DirectorySeparatorChar).
                MakeRelativeUri(new Uri(filePath)).ToString();
            this.TimeStamp = File.GetLastWriteTime(filePath).ToFileTime();
            this.IsDirectory = isDirectory;
            this.Hash = isDirectory ? "" : GetHash(filePath);
        }

        //  対象フォルダーをサーチ
        public static List<FileLibrary> Search(string targetDir)
        {
            List<FileLibrary> resultList = new List<FileLibrary>();
            Action<string, string> searchDir = null;
            searchDir = (cursorDir, baseDir) =>
            {
                foreach (string childFile in Directory.GetFiles(cursorDir))
                {
                    resultList.Add(new FileLibrary(childFile, baseDir, false));
                }
                foreach (string childDir in Directory.GetDirectories(cursorDir))
                {
                    resultList.Add(new FileLibrary(childDir, baseDir, true));
                    searchDir(childDir, baseDir);
                }
            };
            searchDir(targetDir, targetDir.TrimEnd(Path.DirectorySeparatorChar).ToUpper());
            return resultList;
        }

        //  ファイルハッシュ化
        private string GetHash(string targetFile)
        {
            using (FileStream fs = new FileStream(targetFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] tempBytes;
                switch (GlobalItem.HashMode)
                {
                    case GlobalItem.EHashMode.SHA256:
                        SHA256 sha256 = SHA256.Create();
                        tempBytes = sha256.ComputeHash(fs);
                        sha256.Clear();
                        return BitConverter.ToString(tempBytes).Replace("-", "");
                    case GlobalItem.EHashMode.CRC32:
                        int CRC32_TABLELENGTH = 256;
                        int CRC32_BUFLENGTH = 255;
                        uint[] crcTable = new uint[CRC32_TABLELENGTH];
                        for (uint i = 0; i < CRC32_TABLELENGTH; i++)
                        {
                            var x = i;
                            for (int j = 0; j < 8; j++)
                            {
                                x = (uint)((x & 1) == 0 ? x >> 1 : -306674912 ^ x >> 1);
                            }
                            crcTable[i] = x;
                        }
                        byte[] buf = new byte[fs.Length];
                        fs.Read(buf, 0, buf.Length);
                        uint num = uint.MaxValue;
                        for (int i = 0; i < buf.Length; i++)
                        {
                            num = crcTable[(num ^ buf[i]) & CRC32_BUFLENGTH] ^ num >> 8;
                        }
                        tempBytes = BitConverter.GetBytes((uint)(num ^ -1));
                        if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(tempBytes);
                        }
                        return BitConverter.ToString(tempBytes).Replace("-", "");
                    case GlobalItem.EHashMode.MD5:
                    default:
                        MD5 md5 = MD5.Create();
                        tempBytes = md5.ComputeHash(fs);
                        md5.Clear();
                        return BitConverter.ToString(tempBytes).Replace("-", "");
                }
            }
        }

        //  比較用クラス
        public override bool Equals(object obj)
        {
            if (obj != null && this.GetType() == obj.GetType())
            {
                FileLibrary fl = (FileLibrary)obj;
                if (this.IsDirectory)
                {
                    //  ディレクトリの場合の比較
                    if (this.TopDir.Equals(fl.TopDir, StringComparison.OrdinalIgnoreCase) &&
                        this.RelativePath.Equals(fl.RelativePath, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                else
                {
                    //  ファイルの場合の比較
                    if (this.TopDir.Equals(fl.TopDir, StringComparison.OrdinalIgnoreCase) &&
                        this.RelativePath.Equals(fl.RelativePath, StringComparison.OrdinalIgnoreCase) &&
                        this.TimeStamp == fl.TimeStamp &&
                        this.Hash == fl.Hash)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
