using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace App.Areas.Identity.Models.UserViewModels
{
  public class AddUserRoleModel
  {
    public AppUser user { get; set; }
    
    [DisplayName("Thiết lập vai trò gán cho tài khoản")]
    [Required(ErrorMessage = "Phải chọn vai trò cho tài khoản")]
    public string[] RoleNames { get; set; }
  }
}