using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.Constants;
using MyApp.Helper;
using MyApp.Helper.DB;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Areas.Admin.Pages.Members;

public class ManageModel : AdminPageModel
{
  private readonly MemberDbHelper     _memberDbHelper;
  private readonly TranslationService _translation;

  [BindProperty] public string  txtNewUsername    { get; set; } = string.Empty;
  [BindProperty] public int?    NewSponsorId      { get; set; }
  [BindProperty] public int?    NewBinaryParentId { get; set; }
  [BindProperty] public string? ddlNewBinaryPos   { get; set; }
  [BindProperty] public string? ddlNewRankCode    { get; set; }
  [BindProperty] public string  ddlNewStatus      { get; set; } = string.Empty;

  public int      MemberId    { get; set; }
  public string   Username    { get; set; } = string.Empty;
  public string   FullName    { get; set; } = string.Empty;
  public string   Status      { get; set; } = string.Empty;
  public bool     IsActivated { get; set; }
  public string?  CurrentRankCode { get; set; }
  public string?  SponsorUsername { get; set; }
  public string?  BinaryParentUsername { get; set; }
  public string?  BinaryPosition { get; set; }

  public List<MemberChangeLog> ChangeLogs  { get; set; } = new();
  public List<SelectListItem>  ddlRanks    { get; set; } = new();
  public List<SelectListItem>  ddlStatuses { get; set; } = new();

  public ManageModel(MemberDbHelper memberDbHelper, TranslationService translation)
  {
    _memberDbHelper = memberDbHelper;
    _translation    = translation;
  }

  public async Task<IActionResult> OnGetAsync(int id)
  {
    AlertMessageType = "";
    var member = await _memberDbHelper.GetByIdAsync(id);
    if (member == null)
    {
      AlertMessageType    = MessageType.Error;
      AlertMessageTitle   = MessageTitle.Error;
      AlertMessageContent = await _translation.GetAsync(MessageConstants.NotFound);
      return RedirectToPage(Routes.AdminMembers);
    }

    await PopulateAsync(member);
    return Page();
  }

  public async Task<IActionResult> OnPostChangeUsernameAsync(int id)
  {
    try
    {
      if (!await _memberDbHelper.IsUsernameUniqueAsync(txtNewUsername.Trim(), id))
      {
        var errMsg = await _translation.GetAsync("Members.Error.UsernameTaken");
        return new JsonResult(new { success = false, message = errMsg });
      }
      await _memberDbHelper.ChangeUsernameAsync(id, txtNewUsername.Trim(), CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.UpdateSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.SaveError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  public async Task<IActionResult> OnPostChangeSponsorAsync(int id)
  {
    try
    {
      await _memberDbHelper.ChangeSponsorAsync(id, NewSponsorId, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.UpdateSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.SaveError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  public async Task<IActionResult> OnPostChangeBinaryAsync(int id)
  {
    try
    {
      await _memberDbHelper.ChangeBinaryParentAsync(id, NewBinaryParentId, ddlNewBinaryPos, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.UpdateSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.SaveError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  public async Task<IActionResult> OnPostChangeRankAsync(int id)
  {
    try
    {
      await _memberDbHelper.ChangeRankAsync(id, ddlNewRankCode, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.UpdateSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.SaveError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  public async Task<IActionResult> OnPostChangeStatusAsync(int id)
  {
    try
    {
      await _memberDbHelper.ChangeStatusAsync(id, ddlNewStatus, CurrentUsername);
      var msg = await _translation.GetAsync(MessageConstants.UpdateSuccess);
      return new JsonResult(new { success = true, message = msg });
    }
    catch
    {
      var msg = await _translation.GetAsync(MessageConstants.SaveError);
      return new JsonResult(new { success = false, message = msg });
    }
  }

  private async Task PopulateAsync(Member member)
  {
    MemberId            = member.Id;
    Username            = member.Username;
    FullName            = member.FullName;
    Status              = member.Status;
    IsActivated         = member.IsActivated;
    CurrentRankCode     = member.CurrentRankCode;
    SponsorUsername     = member.Sponsor?.Username;
    BinaryParentUsername = member.BinaryParent?.Username;
    BinaryPosition      = member.BinaryPosition;

    ChangeLogs = await _memberDbHelper.GetChangeLogsAsync(member.Id);

    var ranks = await _memberDbHelper.GetAllRanksAsync();
    ddlRanks = new List<SelectListItem>
    {
      new() { Value = string.Empty, Text = "-- Select Rank --" }
    };
    ddlRanks.AddRange(ranks.Select(r => new SelectListItem { Value = r.RankCode, Text = r.RankName }));

    ddlStatuses = new List<SelectListItem>
    {
      new() { Value = StatusConstants.Active,   Text = await _translation.GetAsync("status.active") },
      new() { Value = StatusConstants.Inactive, Text = await _translation.GetAsync("status.inactive") }
    };
  }
}
