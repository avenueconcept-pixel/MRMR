namespace MyApp.Dtos;

public enum MenuAddResult { Created, Restored, DuplicateActive }

public class MenuSortItem
{
  public int  Id        { get; set; }
  public int  SortOrder { get; set; }
  public int? ParentId  { get; set; }
  public int  Level     { get; set; }
}
