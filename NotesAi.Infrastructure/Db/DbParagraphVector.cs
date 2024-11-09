namespace NotesAi.Infrastructure.Db;

public class DbParagraphVector
{
    public int RowId { get; init; }
    public required byte[] Embedding { get; init; }
}
