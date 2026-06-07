namespace MyApp.Dtos;

public class BinaryTreeNodeDto
{
  public int    Id           { get; set; }
  public string Username     { get; set; } = string.Empty;
  public string FullName     { get; set; } = string.Empty;
  public bool   IsActivated  { get; set; }
  public string? RankCode    { get; set; }
  public bool   HasMoreBelow { get; set; }
  public BinaryTreeNodeDto? Left  { get; set; }
  public BinaryTreeNodeDto? Right { get; set; }
}

public class MemberSearchResult
{
  public int    Id       { get; set; }
  public string Username { get; set; } = string.Empty;
  public string FullName { get; set; } = string.Empty;
}

public class MemberBinarySlot
{
  public int    MemberId { get; set; }
  public string Username { get; set; } = string.Empty;
  public string FullName { get; set; } = string.Empty;
  public string Position { get; set; } = string.Empty;
  public bool   HasLeft  { get; set; }
  public bool   HasRight { get; set; }
}
