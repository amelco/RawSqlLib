# RawSqlLib - Raw SQL Library

This library is a simple wrapper to make the use of raw sql queries simpler.

I believe its better to learn actual SQL than some DTO  abstractions. 
When things get complicated, you use SQL to solve it.

It is NOT ready for production yet.

## Advantages
- There is no use of DTO's of any kind
- Use of raw sql queries (learn SQL instead of 

## Usage

For now, you can use it by clonning this git repository (main branch) and add it as a new project in your solution.

After that, you can call the methods of RawSqlLib. You will need a delegate function to map the result of the sql query. See example below:

```csharp
public class ExampleRepository
    {
        private readonly RawSql _sqlExecutor;

        public ExampleRepository()
        {
            _sqlExecutor = new RawSql(EnvironmentVariables.Database.Core.ConnectionString);
        }

        public async Task<Example?> GetById(int _id, CancellationToken cancellationToken)
        {
            var sql = $"select * from example where id = @id";
            _sqlExecutor.AddParam("@id", System.Data.SqlDbType.Int, _id);

            var retorno = await _sqlExecutor.QueryAsync(sql, ExampleMapping, cancellationToken);
            return retorno;
        }

        private Example ExampleMapping(SqlDataReader reader)
        {
            return new Example
            {
                Id = reader.ObterValor<int>(0),
                Name = reader.ObterValor<string>(1) ?? "",
                Email = reader.ObterValor<string>(2) ?? "",
            };
        }
    }
```

In the example code, there is an Example entity that represents a table in the database with the fields id, name and email, in that order.
The mapping of the table elements to the Example class is done in the ExampleMapping() function. This function is passed as a argument (delegate) to the QueryAsync() method.
The mapping of the delegate function uses a implemented extension of the SqlDataReader class that gets the value of the database field.
This extension needs the type of the database field.

## TODO
- [] Translate all method names to english
