namespace MyApp.Dtos;

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
