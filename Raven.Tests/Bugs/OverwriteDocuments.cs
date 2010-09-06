﻿using System;
using System.IO;
using System.Linq;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Database.Exceptions;
using Xunit;

namespace Raven.Tests.Bugs
{
    public class OverwriteDocuments : IDisposable
    {
        private DocumentStore documentStore;

        private void CreateFreshDocumentStore() {
            if (documentStore != null)
                documentStore.Dispose();

            if (Directory.Exists("HiLoData")) Directory.Delete("HiLoData", true);
            documentStore = new DocumentStore
            {
            	Configuration =
            		{
            			DataDirectory = "HiLoData"
            		}
            };
        	documentStore.Initialize();

            documentStore.DatabaseCommands.PutIndex("Foo/Something", new IndexDefinition<Foo> {
                                                                                                  Map = docs => from doc in docs select new { doc.Something }
                                                                                              });
        }

        [Fact]
        public void Will_throw_if_asked_to_store_new_document_which_exists_when_optimistic_concurrency_is_on()
        {
            CreateFreshDocumentStore();

            using (var session = documentStore.OpenSession()) {
                var foo = new Foo() { Id = "foos/1", Something = "something1" };
                session.Store(foo);
                session.SaveChanges();
            }

            using (var session = documentStore.OpenSession())
            {
            	session.UseOptimisticConcurrency = true;
				var foo = new Foo() { Id = "foos/1", Something = "something1" };
                session.Store(foo);
            	Assert.Throws<ConcurrencyException>(() => session.SaveChanges());
            }
        }

		[Fact]
		public void Will_overwrite_doc_if_asked_to_store_new_document_which_exists_when_optimistic_concurrency_is_off()
		{
			CreateFreshDocumentStore();

			using (var session = documentStore.OpenSession())
			{
				var foo = new Foo() { Id = "foos/1", Something = "something1" };
				session.Store(foo);
				session.SaveChanges();
			}

			using (var session = documentStore.OpenSession())
			{
				var foo = new Foo() { Id = "foos/1", Something = "something1" };
				session.Store(foo);
				session.SaveChanges();
			}
		}


        public class Foo
        {
            public string Id { get; set; }
            public string Something { get; set; }
        }

        public void Dispose()
        {
            if (documentStore != null)
                documentStore.Dispose();
            if (Directory.Exists("HiLoData")) Directory.Delete("HiLoData", true);
        }
    }
}