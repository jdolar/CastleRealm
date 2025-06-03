namespace DataBase.SQL;
public static class Merge
{
    /*
        0 - TableName
        1 - Field 1 Name
    */

    public static string ByValue { get; private set; } = @"
        MERGE INTO {0} AS target
        USING (VALUES (@{1})) AS source ({1})
            ON target.{1} = source.{1}
        WHEN MATCHED THEN
	        UPDATE SET target.{1} = source.{1}
        WHEN NOT MATCHED THEN
            INSERT ({1}) VALUES (source.{1})
        OUTPUT 
            inserted.Id;";
    public static string FormatSql(string tableName, string fieldName) => string.Format(ByValue, tableName, fieldName);
}
