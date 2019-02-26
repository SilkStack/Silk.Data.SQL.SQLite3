# Overview

SQLite3 provider for `Silk.Data.SQL.Base`.

# Installing

`Silk.Data.SQL.SQLite3` is available as a NuGet package: https://www.nuget.org/packages/Silk.Data.SQL.SQLite3

You can install it from the NuGet package manager in Visual Studio or from command line with dotnet core:

~~~~
dotnet add package Silk.Data.SQL.SQLite3
~~~~

# Limitations

SQLite3 lacks full support for DECIMAL data types. Decimal data upto 15 digits is supported in testing, anything larger and SQLite3 will return the 15 most significant digits.

# Usage

To execute SQL statements just create an instance of `SQLite3DataProvider`, passing in the name of the file you wish to store your database in.

    var provider = new SQLite3DataProvider("myapp.db");

If you want to create an in-memory database provide `":memory:"`.

## Executing Queries

Non-reader queries:

    provider.ExecuteNonReaderAsync(
        QueryExpression.Insert(
            "Accounts",
            new[] { "DisplayName" },
            new object[] { "John" },
            new object[] { "Jane" }
        )
    );

Queries with results need to be disposed:

    using (var queryResult = provider.ExecuteReader(
        QueryExpression.Select(
            new[] { Expression.Value("Hello World!") }
    )))
    {
        Assert.IsTrue(queryResult.HasRows);
        Assert.IsTrue(queryResult.Read());
        Assert.AreEqual("Hello World!", queryResult.GetString(0));
    }

## Raw SQL

A raw SQL expression is provided on the `SQLite3` helper class.

    var rawSQL = SQLite3.Raw("SELECT random()");

Raw SQL expressions are safe to be used within `TransactionExpression`:

    var transaction = QueryExpression.Transaction(
        SQLite3.Raw("SELECT date()"),
        SQLite3.Raw("SELECT time()")
    );

# License

`Silk.Data.SQL.SQLite3` is made available under the MIT license.