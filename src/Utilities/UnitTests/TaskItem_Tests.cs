﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using System.Collections;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Build.Shared;
using System.Collections.Generic;

namespace Microsoft.Build.UnitTests
{
    [TestClass]
    public class TaskItemTests
    {
        // Make sure a TaskItem can be constructed using an ITaskItem
        [TestMethod]
        public void ConstructWithITaskItem()
        {
            TaskItem from = new TaskItem();
            from.ItemSpec = "Monkey.txt";
            from.SetMetadata("Dog", "Bingo");
            from.SetMetadata("Cat", "Morris");

            TaskItem to = new TaskItem((ITaskItem)from);
            Assert.AreEqual("Monkey.txt", to.ItemSpec);
            Assert.AreEqual("Monkey.txt", (string)to);
            Assert.AreEqual("Bingo", to.GetMetadata("Dog"));
            Assert.AreEqual("Morris", to.GetMetadata("Cat"));

            // Test that item metadata are case-insensitive.
            to.SetMetadata("CaT", "");
            Assert.AreEqual("", to.GetMetadata("Cat"));

            // manipulate the item-spec a bit
            Assert.AreEqual("Monkey", to.GetMetadata(FileUtilities.ItemSpecModifiers.Filename));
            Assert.AreEqual(".txt", to.GetMetadata(FileUtilities.ItemSpecModifiers.Extension));
            Assert.AreEqual(String.Empty, to.GetMetadata(FileUtilities.ItemSpecModifiers.RelativeDir));
        }

        // Make sure metadata can be cloned from an existing ITaskItem
        [TestMethod]
        public void CopyMetadataFromITaskItem()
        {
            TaskItem from = new TaskItem();
            from.ItemSpec = "Monkey.txt";
            from.SetMetadata("Dog", "Bingo");
            from.SetMetadata("Cat", "Morris");
            from.SetMetadata("Bird", "Big");

            TaskItem to = new TaskItem();
            to.ItemSpec = "Bonobo.txt";
            to.SetMetadata("Sponge", "Bob");
            to.SetMetadata("Dog", "Harriet");
            to.SetMetadata("Cat", "Mike");
            from.CopyMetadataTo(to);

            Assert.AreEqual("Bonobo.txt", to.ItemSpec);          // ItemSpec is never overwritten
            Assert.AreEqual("Bob", to.GetMetadata("Sponge"));   // Metadata not in source are preserved.
            Assert.AreEqual("Harriet", to.GetMetadata("Dog"));  // Metadata present on destination are not overwritten.
            Assert.AreEqual("Mike", to.GetMetadata("Cat"));
            Assert.AreEqual("Big", to.GetMetadata("Bird"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullITaskItem()
        {
            ITaskItem item = null;
            TaskItem taskItem = new TaskItem(item);

            // no NullReferenceException
        }

        /// <summary>
        /// Even without any custom metadata metadatanames should
        /// return the built in metadata
        /// </summary>
        [TestMethod]
        public void MetadataNamesNoCustomMetadata()
        {
            TaskItem taskItem = new TaskItem("x");

            Assert.AreEqual(FileUtilities.ItemSpecModifiers.All.Length, taskItem.MetadataNames.Count);
            Assert.AreEqual(FileUtilities.ItemSpecModifiers.All.Length, taskItem.MetadataCount);

            // Now add one
            taskItem.SetMetadata("m", "m1");

            Assert.AreEqual(FileUtilities.ItemSpecModifiers.All.Length + 1, taskItem.MetadataNames.Count);
            Assert.AreEqual(FileUtilities.ItemSpecModifiers.All.Length + 1, taskItem.MetadataCount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullITaskItemCast()
        {
            TaskItem item = null;
            string result = (string)item;

            // no NullReferenceException
        }

        [TestMethod]
        public void ConstructFromDictionary()
        {
            Hashtable h = new Hashtable();
            h[FileUtilities.ItemSpecModifiers.Filename] = "foo";
            h[FileUtilities.ItemSpecModifiers.Extension] = "bar";
            h["custom"] = "hello";

            TaskItem t = new TaskItem("bamboo.baz", h);

            // item-spec modifiers were not overridden by dictionary passed to constructor
            Assert.AreEqual("bamboo", t.GetMetadata(FileUtilities.ItemSpecModifiers.Filename));
            Assert.AreEqual(".baz", t.GetMetadata(FileUtilities.ItemSpecModifiers.Extension));
            Assert.AreEqual("hello", t.GetMetadata("CUSTOM"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotChangeModifiers()
        {
            TaskItem t = new TaskItem("foo");

            try
            {
                t.SetMetadata(FileUtilities.ItemSpecModifiers.FullPath, "bazbaz");
            }
            catch (Exception e)
            {
                // so I can see the exception message in NUnit's "Standard Out" window
                Console.WriteLine(e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotRemoveModifiers()
        {
            TaskItem t = new TaskItem("foor");

            try
            {
                t.RemoveMetadata(FileUtilities.ItemSpecModifiers.RootDir);
            }
            catch (Exception e)
            {
                // so I can see the exception message in NUnit's "Standard Out" window
                Console.WriteLine(e.Message);
                throw;
            }
        }

        [TestMethod]
        public void CheckMetadataCount()
        {
            TaskItem t = new TaskItem("foo");

            Assert.AreEqual(FileUtilities.ItemSpecModifiers.All.Length, t.MetadataCount);

            t.SetMetadata("grog", "RUM");

            Assert.AreEqual(FileUtilities.ItemSpecModifiers.All.Length + 1, t.MetadataCount);
        }


        [TestMethod]
        public void NonexistentRequestFullPath()
        {
            TaskItem from = new TaskItem();
            from.ItemSpec = "Monkey.txt";
            Assert.AreEqual
            (
                Path.Combine
                (
                    Directory.GetCurrentDirectory(),
                    "Monkey.txt"
                ),
                from.GetMetadata(FileUtilities.ItemSpecModifiers.FullPath)
            );
        }

        [TestMethod]
        public void NonexistentRequestRootDir()
        {
            TaskItem from = new TaskItem();
            from.ItemSpec = "Monkey.txt";
            Assert.AreEqual
            (
                Path.GetPathRoot
                (
                    from.GetMetadata(FileUtilities.ItemSpecModifiers.FullPath)
                ),
                from.GetMetadata(FileUtilities.ItemSpecModifiers.RootDir)
            );
        }

        [TestMethod]
        public void NonexistentRequestFilename()
        {
            TaskItem from = new TaskItem();
            from.ItemSpec = "Monkey.txt";
            Assert.AreEqual
            (
                "Monkey",
                from.GetMetadata(FileUtilities.ItemSpecModifiers.Filename)
            );
        }

        [TestMethod]
        public void NonexistentRequestExtension()
        {
            TaskItem from = new TaskItem();
            from.ItemSpec = "Monkey.txt";
            Assert.AreEqual
            (
                ".txt",
                from.GetMetadata(FileUtilities.ItemSpecModifiers.Extension)
            );
        }

        [TestMethod]
        public void NonexistentRequestRelativeDir()
        {
            TaskItem from = new TaskItem();
            from.ItemSpec = "Monkey.txt";
            Assert.IsTrue
            (
                from.GetMetadata(FileUtilities.ItemSpecModifiers.RelativeDir).Length == 0
            );
        }

        [TestMethod]
        public void NonexistentRequestDirectory()
        {
            TaskItem from = new TaskItem();
            from.ItemSpec = @"c:\subdir\Monkey.txt";
            Assert.AreEqual
            (
                @"subdir\",
                from.GetMetadata(FileUtilities.ItemSpecModifiers.Directory)
            );
        }

        [TestMethod]
        public void NonexistentRequestDirectoryUNC()
        {
            TaskItem from = new TaskItem();
            from.ItemSpec = @"\\local\share\subdir\Monkey.txt";
            Assert.AreEqual
            (
                @"subdir\",
                from.GetMetadata(FileUtilities.ItemSpecModifiers.Directory)
            );
        }

        [TestMethod]
        public void NonexistentRequestRecursiveDir()
        {
            TaskItem from = new TaskItem();
            from.ItemSpec = "Monkey.txt";

            Assert.IsTrue
            (
                from.GetMetadata(FileUtilities.ItemSpecModifiers.RecursiveDir).Length == 0
            );
        }

        [TestMethod]
        public void NonexistentRequestIdentity()
        {
            TaskItem from = new TaskItem();
            from.ItemSpec = "Monkey.txt";
            Assert.AreEqual
            (
                "Monkey.txt",
                from.GetMetadata(FileUtilities.ItemSpecModifiers.Identity)
            );
        }

        [TestMethod]
        public void RequestTimeStamps()
        {
            TaskItem from = new TaskItem();
            from.ItemSpec = FileUtilities.GetTemporaryFile();

            Assert.IsTrue
            (
                from.GetMetadata(FileUtilities.ItemSpecModifiers.ModifiedTime).Length > 0
            );

            Assert.IsTrue
            (
                from.GetMetadata(FileUtilities.ItemSpecModifiers.CreatedTime).Length > 0
            );

            Assert.IsTrue
            (
                from.GetMetadata(FileUtilities.ItemSpecModifiers.AccessedTime).Length > 0
            );

            File.Delete(from.ItemSpec);

            Assert.IsTrue
            (
                from.GetMetadata(FileUtilities.ItemSpecModifiers.ModifiedTime).Length == 0
            );

            Assert.IsTrue
            (
                from.GetMetadata(FileUtilities.ItemSpecModifiers.CreatedTime).Length == 0
            );

            Assert.IsTrue
            (
                from.GetMetadata(FileUtilities.ItemSpecModifiers.AccessedTime).Length == 0
            );
        }

        /// <summary>
        /// Verify metadata cannot be created with null name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNullNamedMetadata()
        {
            TaskItem item = new TaskItem("foo");
            item.SetMetadata(null, "x");
        }

        /// <summary>
        /// Verify metadata cannot be created with empty name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateEmptyNamedMetadata()
        {
            TaskItem item = new TaskItem("foo");
            item.SetMetadata("", "x");
        }

        /// <summary>
        /// Create a TaskItem with a null metadata value -- this is allowed, but 
        /// internally converted to the empty string. 
        /// </summary>
        [TestMethod]
        public void CreateTaskItemWithNullMetadata()
        {
            IDictionary<string, string> metadata = new Dictionary<string, string>();
            metadata.Add("m", null);

            TaskItem item = new TaskItem("bar", (IDictionary)metadata);
            Assert.AreEqual(String.Empty, item.GetMetadata("m"));
        }

        /// <summary>
        /// Set metadata value to null value -- this is allowed, but 
        /// internally converted to the empty string. 
        /// </summary>
        [TestMethod]
        public void SetNullMetadataValue()
        {
            TaskItem item = new TaskItem("bar");
            item.SetMetadata("m", null);
            Assert.AreEqual(String.Empty, item.GetMetadata("m"));
        }

        /// <summary>
        /// Test that task items can be successfully constructed based on a task item from another appdomain.  
        /// </summary>
        [TestMethod]
        public void RemoteTaskItem()
        {
            AppDomain appDomain = null;
            try
            {
                appDomain = AppDomain.CreateDomain
                            (
                                "generateResourceAppDomain",
                                null,
                                AppDomain.CurrentDomain.SetupInformation
                            );

                object obj = appDomain.CreateInstanceFromAndUnwrap
                   (
                       typeof(TaskItemCreator).Module.FullyQualifiedName,
                       typeof(TaskItemCreator).FullName
                   );

                TaskItemCreator creator = (TaskItemCreator)obj;

                IDictionary<string, string> metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                metadata.Add("c", "C");
                metadata.Add("d", "D");

                creator.Run(new string[] { "a", "b" }, metadata);

                ITaskItem[] itemsInThisAppDomain = new ITaskItem[creator.CreatedTaskItems.Length];

                for (int i = 0; i < creator.CreatedTaskItems.Length; i++)
                {
                    itemsInThisAppDomain[i] = new TaskItem(creator.CreatedTaskItems[i]);

                    Assert.AreEqual(creator.CreatedTaskItems[i].ItemSpec, itemsInThisAppDomain[i].ItemSpec);
                    Assert.AreEqual(creator.CreatedTaskItems[i].MetadataCount + 1, itemsInThisAppDomain[i].MetadataCount);

                    foreach (string metadatum in creator.CreatedTaskItems[i].MetadataNames)
                    {
                        if (!String.Equals("OriginalItemSpec", metadatum))
                        {
                            Assert.AreEqual(creator.CreatedTaskItems[i].GetMetadata(metadatum), itemsInThisAppDomain[i].GetMetadata(metadatum));
                        }
                    }
                }
            }
            finally
            {
                if (appDomain != null)
                {
                    AppDomain.Unload(appDomain);
                }
            }
        }

        /// <summary>
        /// Miniature class to be remoted to another appdomain that just creates some TaskItems and makes them available for returning. 
        /// </summary>
        private sealed class TaskItemCreator : MarshalByRefObject
        {
            /// <summary>
            /// Task items that will be consumed by the other appdomain
            /// </summary>
            public ITaskItem[] CreatedTaskItems
            {
                get;
                private set;
            }

            /// <summary>
            /// Creates task items 
            /// </summary>
            public void Run(string[] includes, IDictionary<string, string> metadataToAdd)
            {
                ErrorUtilities.VerifyThrowArgumentNull(includes, "includes");

                CreatedTaskItems = new TaskItem[includes.Length];

                for (int i = 0; i < includes.Length; i++)
                {
                    CreatedTaskItems[i] = new TaskItem(includes[i], (IDictionary)metadataToAdd);
                }
            }
        }
    }
}
