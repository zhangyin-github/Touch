﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Touch.Data;

namespace Touch.UnitTestProject.Data
{
    [TestClass]
    public class MemoryListDatabaseUnitTest
    {
        /// <summary>
        ///     插入并读出数据
        /// </summary>
        [TestMethod]
        public void InsertAndGetFoldersTest()
        {
            DatabaseHelper.InitDb();
            MemoryListDatabase.Drop();
            MemoryListDatabase.Create();
            MemoryListDatabase.Insert("test_data_1");
            MemoryListDatabase.Insert("test_data_2");
            MemoryListDatabase.Insert("test_data_3");
            var memoryList = MemoryListDatabase.GetMemoryList();
            var count = 1;
            foreach (var memory in memoryList)
                Assert.AreEqual("test_data_" + count++, memory.MemoryName);
        }

        /// <summary>
        ///     删除表
        /// </summary>
        [TestMethod]
        public void DropTest()
        {
            MemoryListDatabase.Drop();
            MemoryListDatabase.Create();
            MemoryListDatabase.Insert("test_data");
            var memoryList = MemoryListDatabase.GetMemoryList();
            foreach (var memory in memoryList)
                Assert.AreEqual("test_data", memory.MemoryName);
        }

        /// <summary>
        ///     通过主键删除并读出数据
        /// </summary>
        [TestMethod]
        public void DeleteKeyAndGetFoldersTest()
        {
            MemoryListDatabase.Drop();
            MemoryListDatabase.Create();
            MemoryListDatabase.Insert("test_data_1");
            MemoryListDatabase.Insert("test_data_2");
            MemoryListDatabase.Insert("test_data_3");
            MemoryListDatabase.Insert("test_data_4");
            MemoryListDatabase.Insert("test_data_5");
            MemoryListDatabase.Delete(1);
            MemoryListDatabase.Delete(3);
            MemoryListDatabase.Delete(5);
            var memoryList = MemoryListDatabase.GetMemoryList();
            var count = 2;
            foreach (var memory in memoryList)
            {
                Assert.AreEqual("test_data_" + count, memory.MemoryName);
                count += 2;
            }
        }
    }
}