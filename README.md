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

Call Stored Procedure without parameters
=====================
 	string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;	
	 IQueryAsync storedProcedure = new StoredProcedure(connString);	
	IEnumerable<dynamic> result =await storedProcedure.ExecuteAsync("proc_test2");

Call Stored Procedure with parameters
=====================
the names of properties of the object are the same with the parameters of stored procedure
 
	string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;	
	 IQueryAsync storedProcedure = new StoredProcedure(connString);	
	IEnumerable<dynamic> result =await storedProcedure.ExecuteAsync("proc_Login", new {UserName="BillGates",Password="WinRT"});
	
Projection
=====================
the names of properties of the object are the same with the parameters of stored procedure
 
	string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
 	IQueryAsync storedProcedure = new StoredProcedure(connString);
	IEnumerable<dynamic> result =await storedProcedure.ExecuteAsync("proc_Login", new {UserName="BillGates",Password="WinRT"});
	
	IEnumerable<User> users=	result.Select(x=>new User{
													FirstName=x.First_Name,
													LastName=x.Last_Name
												});
