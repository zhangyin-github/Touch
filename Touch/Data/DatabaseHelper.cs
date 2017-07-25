﻿using System;
using System.Diagnostics;
using Microsoft.Data.Sqlite.Internal;

namespace Touch.Data
{
    /// <summary>
    ///     所有数据库集合
    /// </summary>
    public class DatabaseHelper
    {
        /// <summary>
        ///     数据库文件名
        /// </summary>
        private const string DbFileName = "TouchSQLite.db";

        private static DatabaseHelper _uniqueInstance;
        private static readonly object Locker = new object();

        /// <summary>
        ///     文件夹 数据库
        /// </summary>
        public readonly FolderDatabase FolderDatabase;

        /// <summary>
        /// 图片 数据库
        /// </summary>
        public readonly ImageDatabase ImageDatabase;

        private DatabaseHelper()
        {
            // 初始化数据库
            try
            {
                // Configuring library to use SDK version of SQLite
                SqliteEngine.UseWinSqlite3();
            }
            catch (InvalidOperationException exception)
            {
                Debug.WriteLine(exception);
            }
            FolderDatabase = new FolderDatabase(DbFileName);
            FolderDatabase.Create();
            ImageDatabase = new ImageDatabase(DbFileName);
            ImageDatabase.Create();
        }

        /// <summary>
        ///     获得一个内存数据库的实例（单例）
        /// </summary>
        /// <returns></returns>
        public static DatabaseHelper GetInstance()
        {
            if (_uniqueInstance != null)
                return _uniqueInstance;
            lock (Locker)
            {
                _uniqueInstance = new DatabaseHelper();
            }
            return _uniqueInstance;
        }
    }
}