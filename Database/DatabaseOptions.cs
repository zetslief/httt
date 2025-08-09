using System.ComponentModel.DataAnnotations;

namespace gen;

public class DatabaseOptions
{
    public const string Database = "Database";

    [Required(AllowEmptyStrings = false)]
    public required string DatabasePath { get; init; }
}