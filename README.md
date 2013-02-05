Async ORM
========
A very lightweight dynamic asynchronous data access for .NET, written in C#. 


Supported Platform and Database Server
==========================================
The project only supports .NET 4.5 and Sql Server.

Why do you need this another micro ORM?
=====================
.NET 4.5 and C# 5.0 allow us to utilize asynchronous programming as a core feature of the .NET Framework, and ADO.NET takes full advantage of the standard design patterns.
AsyncORM allows developers  to develop asynchronous data access without dealing with the complexity.

How to use it?
=====================
you have two classes to work with database, StoredProcedure and DynamicQuery, which both implement IQueryAsync interface.
 string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
<code>
 IQueryAsync storedProcedure = new StoredProcedure(connString);
IEnumerable<dynamic> result =await storedProcedure.ExecuteAsync("proc_test2");
</code>