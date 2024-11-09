using Base_BE.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceStack;
using System.Security.Cryptography;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Domain.Constants;
using Base_BE.Domain.Entities;
using Base_BE.Dtos;
using Base_BE.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using NetHelper.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Base_BE.Endpoints
{
    public class User : EndpointGroupBase
    {
        public override void Map(WebApplication app)
        {
            app.MapGroup(this)
                .MapPost(RegisterUser, "/register");


            app.MapGroup(this)
                .RequireAuthorization()
                .MapPut(ChangePassword, "/change-password")
                .MapPatch(UpdateUser, "/update-user")
                .MapPost(SendOTP, "/send-otp")
                .MapPost(VerifyOTP, "/verify-otp")
                .MapPut(ChangeEmail, "/change-email")
                .MapGet(GetCurrentUser, "/UserInfo")
                .MapGet(CheckPasswordFirstTime, "/check-password-first-time")
                .MapPost(CheckPasswork, "/check-password")
            ;

            app.MapGroup(this)
                .RequireAuthorization("admin")
                .MapGet(GetAllUsers, "/get-all-users")
                // .MapPost(DisableAccount, "/disable-account/{id}")
                .MapGet(GetUserById, "/get-user/{id}")
                .MapGet(SelectCandidates, "/select-candidates")
                ;
        }

        public async Task<IResult> RegisterUser([FromBody] RegisterForm newUser,
            UserManager<ApplicationUser> _userManager, [FromServices] EmailSender _emailSender)
        {
            // Kiểm tra Username
            if (string.IsNullOrEmpty(newUser.UserName))
            {
                return Results.BadRequest("400|Username is required");
            }

            // Kiểm tra Username chỉ chứa chữ cái và chữ số
            if (!newUser.UserName.All(char.IsLetterOrDigit))
            {
                return Results.BadRequest("400|Username can only contain letters or digits.");
            }

            // Kiểm tra xem User đã tồn tại chưa
            var user = await _userManager.FindByNameAsync(newUser.UserName);
            if (user != null)
            {
                return Results.BadRequest("501|User already exists");
            }

            // Tạo cặp khóa RSA
            using var rsa = new RSACryptoServiceProvider(1024);
            var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
            var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

            // Tạo đối tượng ApplicationUser
            var newUserEntity = new ApplicationUser
            {
                UserName = newUser.UserName,
                Email = newUser.Email,
                FullName = newUser.FullName,
                Birthday = newUser.Birthday,
                Address = newUser.Address,
                Gender = newUser.Gender,
                CellPhone = newUser.CellPhone,
                IdentityCardNumber = newUser.IdentityCardNumber,
                IdentityCardDate = newUser.IdentityCardDate,
                IdentityCardPlace = newUser.IdentityCardPlace,
                ImageUrl = newUser.ImageUrl,
                IdentityCardImage = newUser.UrlIdentityCardImage,
                PublicKey = publicKey
            };

            // Tạo mật khẩu mặc định dựa trên ngày sinh hoặc giá trị mặc định nếu không có ngày sinh
            var passwordSeed = GenerateSecurePassword();
            var result = await _userManager.CreateAsync(newUserEntity, passwordSeed);

            if (result.Succeeded)
            {
                // Gán vai trò cho người dùng mới tạo
                var roleName = newUser.IsAdmin == true ? Roles.Administrator : Roles.User;
                var roleResult = await _userManager.AddToRoleAsync(newUserEntity, roleName);

                if (!roleResult.Succeeded)
                {
                    return Results.BadRequest("500|Failed to assign role to user");
                }

                // Gửi email chứa thông tin tài khoản và khóa riêng (private key)
                await _emailSender.SendEmailRegisterAsync(newUser.Email, newUser.FullName, newUser.UserName,
                    passwordSeed, privateKey);

                return Results.Ok("200|User created successfully");
            }
            else
            {
                // Trả về lỗi nếu quá trình tạo tài khoản thất bại
                return Results.BadRequest($"500|{string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        private string GenerateSecurePassword()
        {
            const int length = 12;
            const string upperCase = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
            const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specialChars = "!@#$%^&*?_-";
            const string allChars = upperCase + lowerCase + digits + specialChars;

            var random = new Random();
            var password = new char[length];

            // Ensure the password has at least one character from each category
            password[0] = upperCase[random.Next(upperCase.Length)];
            password[1] = lowerCase[random.Next(lowerCase.Length)];
            password[2] = digits[random.Next(digits.Length)];
            password[3] = specialChars[random.Next(specialChars.Length)];

            // Fill the rest of the password with random characters from all categories
            for (int i = 4; i < length; i++)
            {
                password[i] = allChars[random.Next(allChars.Length)];
            }

            // Shuffle the characters to ensure randomness
            return new string(password.OrderBy(x => random.Next()).ToArray());
        }


        //change password
        public async Task<IResult> ChangePassword(UserManager<ApplicationUser> _userManager,
            [FromBody] ChangePassword changePassword, IUser _user)
        {
            try
            {
                if (changePassword == null || string.IsNullOrEmpty(_user.Id))
                {
                    return Results.BadRequest("400| Missing or invalid user ID or change password data.");
                }

                var currentUser = await _userManager.FindByIdAsync(_user.Id);
                if (currentUser == null)
                {
                    return Results.BadRequest("400| Invalid UserId provided.");
                }

                var isOldPasswordCorrect =
                    await _userManager.CheckPasswordAsync(currentUser, changePassword.oldPassword);
                if (!isOldPasswordCorrect)
                {
                    return Results.BadRequest("400| The old password is incorrect.");
                }

                if (!changePassword.newPassword.Equals(changePassword.comfirmedPassword))
                {
                    return Results.BadRequest("400| The new password and confirmation do not match.");
                }

                var result = await _userManager.ChangePasswordAsync(currentUser, changePassword.oldPassword,
                    changePassword.newPassword);
                if (result.Succeeded)
                {
                    currentUser.ChangePasswordFirstTime = true;
                    _userManager.UpdateAsync(currentUser).Wait();
                    return Results.Ok("200| Password changed successfully.");
                }
                else
                {
                    var errorDescriptions = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Results.BadRequest($"500| {errorDescriptions}");
                }
            }
            catch (Exception ex)
            {
                // Log the full stack trace here if possible for more in-depth debugging.
                Console.WriteLine($"Error occurred while changing password: {ex}");
                return Results.Problem("An error occurred while changing the password.", statusCode: 500);
            }
        }

/*
C****: Update User
*/
        public async Task<IResult> UpdateUser(UserManager<ApplicationUser> _userManager,
            [FromBody] UpdateUser updateUser,
            [FromServices] IUser _user)
        {
            try
            {
                var userId = _user.Id;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.BadRequest("400|UserId is empty");
                }

                var currentUser = await _userManager.FindByIdAsync(userId);
                if (currentUser == null)
                {
                    return Results.BadRequest("400|UserId không hợp lệ !");
                }

                if (!updateUser.CellPhone.IsNullOrEmpty()) currentUser.CellPhone = updateUser.CellPhone;
                if (!updateUser.Address.IsNullOrEmpty()) currentUser.Address = updateUser.Address;
                if (!updateUser.FullName.IsNullOrEmpty()) currentUser.FullName = updateUser.FullName;
                currentUser.UpdateDate = DateTime.Now;

                var result = await _userManager.UpdateAsync(currentUser);
                if (result.Succeeded)
                {
                    return Results.Ok("200|User updated successfully");
                }
                else
                {
                    return Results.BadRequest($"500|{string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error", ex.Message);
                return Results.Problem("An error occurred while updating the user", statusCode: 500);
            }
        }

        //change email
        public async Task<IResult> ChangeEmail([FromBody] ChangeEmail changeEmail,
            [FromServices] UserManager<ApplicationUser> _userManager, IUser _user)
        {
            try
            {
                if (changeEmail == null || string.IsNullOrEmpty(_user.Id))
                {
                    return Results.BadRequest("400| Missing or invalid user ID or change password data.");
                }

                var currentUser = await _userManager.FindByIdAsync(_user.Id);
                if (currentUser == null)
                {
                    return Results.BadRequest("400| Invalid UserId provided.");
                }

                currentUser.NewEmail = changeEmail.NewEmail;
                var res = await _userManager.UpdateAsync(currentUser);
                if (res.Succeeded)
                {
                    return Results.Ok("200| Email changed successfully.");
                }

                return Results.BadRequest("400| Change email failed.");
            }
            catch (Exception ex)
            {
                // Log the full stack trace here if possible for more in-depth debugging.
                Console.WriteLine($"Error occurred while changing password: {ex}");
                return Results.Problem("An error occurred while changing the password.", statusCode: 500);
            }
        }

        public async Task<IResult> SendOTP([FromServices] OTPService _otpService,
            [FromServices] IEmailSender _emailSender, [FromBody] SendOTPRequest request, [FromServices]IUser _user)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.Email))
                {
                    return Results.BadRequest("400|Email is required");
                }
                var otp = _otpService.GenerateOTP();

                _otpService.SaveOTP(request.Email, otp);

                await _emailSender.SendEmailAsync(request.Email, _user.UserName!
                    , $"Mã xác minh của bạn là: {otp}");
            
                return Results.Ok("200|OTP sent successfully");
            }
            catch (Exception e)
            {
                return Results.BadRequest("500|Error while sending OTP: " + e.Message);
            }
        }

        public async Task<IResult> VerifyOTP([FromBody] VerifyOTPRequest request, [FromServices] OTPService _otpService,
            [FromServices] UserManager<ApplicationUser> _userManager, [FromServices] IUser _user)
        {
            var currentUser = await _userManager.FindByIdAsync(_user.Id);
            if (currentUser == null)
            {
                return Results.NotFound("khong tim thay nguoi dung.");
            }

            var isValid = false;
            
            if(!currentUser.Email.IsNullOrEmpty() && currentUser.NewEmail.IsNullOrEmpty())
            {
                isValid = currentUser.Email != null && _otpService.VerifyOTP(currentUser.Email, request.OTP);
            }
            else
            {
                isValid = currentUser.NewEmail != null && _otpService.VerifyOTP(currentUser.NewEmail, request.OTP);
            }
            

            if (isValid)
            {
                currentUser.EmailConfirmed = true;
                await _userManager.UpdateAsync(currentUser);
                return Results.Ok("200|Xác minh thành công.");
            }

            return Results.BadRequest("Mã xác minh không hợp lệ hoặc đã hết hạn.");
        }

        public async Task<ResultCustomPaginate<IEnumerable<UserDto>>> GetAllUsers(
            [FromServices] UserManager<ApplicationUser> _userManager, int page, int pageSize)
        {
            var usersQuery = _userManager.Users;

            var usersList = await usersQuery.ToListAsync();

            var usersDtoList = new List<UserDto>();

            foreach (var u in usersList)
            {
                // Tạo đối tượng UserDto
                var userDto = new UserDto
                {
                    Id = u.Id,
                    Fullname = u.FullName,
                    UserName = u.UserName,
                    Email = u.Email,
                    CellPhone = u.PhoneNumber,
                    status = u.Status,
                    Birthday = u.Birthday,
                    Address = u.Address,
                    CreatedAt = u.CreateDate,
                };

                usersDtoList.Add(userDto);
            }

            // Sort by ActivationDate
            var sortedUsersDtoList = usersDtoList.OrderByDescending(u => u.CreatedAt).ToList();

            // Áp dụng phân trang
            var paginatedUsers = sortedUsersDtoList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var result = new ResultCustomPaginate<IEnumerable<UserDto>>
            {
                Data = paginatedUsers,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = sortedUsersDtoList.Count,
                TotalPages = (int)Math.Ceiling((double)sortedUsersDtoList.Count / pageSize)
            };

            return result;
        }

        public async Task<IResult> GetCurrentUser([FromServices] UserManager<ApplicationUser> _userManager, IUser _user)
        {
            var currentUser = await _userManager.FindByIdAsync(_user.Id);
            if (currentUser == null)
            {
                return Results.BadRequest("400|User not found");
            }

            var userDto = new UserDto
            {
                Id = currentUser.Id,
                Fullname = currentUser.FullName,
                UserName = currentUser.UserName,
                Email = currentUser.Email,
                NewEmail = currentUser.NewEmail,
                IdentityCardNumber = currentUser.IdentityCardNumber,
                IdentityCardDate = currentUser.IdentityCardDate,
                IdentityCardPlace = currentUser.IdentityCardPlace,
                Gender = currentUser.Gender,
                CellPhone = currentUser.CellPhone,
                status = currentUser.Status,
                Birthday = currentUser.Birthday,
                Address = currentUser.Address,
                CreatedAt = currentUser.CreateDate,
                Role = (await _userManager.GetRolesAsync(currentUser)).FirstOrDefault()
                
            };

            return Results.Ok(userDto);
        }

        public async Task<IResult> CheckPasswordFirstTime([FromServices] UserManager<ApplicationUser> _userManager,
            IUser _user)
        {
            var currentUser = await _userManager.FindByIdAsync(_user.Id);
            if (currentUser == null)
            {
                return Results.BadRequest("400|User not found");
            }

            var result = currentUser.ChangePasswordFirstTime;
            if (result == true)
            {
                return Results.Ok("200|Password was changed first time.");
            }

            return Results.BadRequest("400|Password was not changed first time.");
        }

        // public async Task<IResult> DisableAccount([FromRoute] string id,
        //     [FromServices] UserManager<ApplicationUser> _userManager)
        // {
        //     var user = await _userManager.FindByIdAsync(id);
        //
        //     if (user == null)
        //     {
        //         return Results.BadRequest("400|User not found");
        //     }
        //
        //     user.Status = "Disabled";
        //
        //     
        // }

        public async Task<IResult> GetUserById([FromServices] UserManager<ApplicationUser> _userManager,
            [FromRoute] string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Results.BadRequest("400|User not found");
                }

                var result = new UserDto
                {
                    Id = user.Id,
                    Fullname = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email,
                    NewEmail = user.NewEmail,
                    IdentityCardNumber = user.IdentityCardNumber,
                    IdentityCardDate = user.IdentityCardDate,
                    IdentityCardPlace = user.IdentityCardPlace,
                    Gender = user.Gender,
                    CellPhone = user.PhoneNumber,
                    status = user.Status,
                    Birthday = user.Birthday,
                    Address = user.Address,
                    CreatedAt = user.CreateDate,
                    Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
                };
                return Results.Ok(result);
            }
            catch (Exception e)
            {
                return Results.BadRequest(e.Message);
            }
        }

        public async Task<IResult> CheckPasswork([FromServices] UserManager<ApplicationUser> _userManager, IUser _user,
            [FromBody] checkPassword request)
        {
            var currentUser = await _userManager.FindByIdAsync(_user.Id);
            if (currentUser != null)
            {
                var isPasswordCorrect = await _userManager.CheckPasswordAsync(currentUser, request.Password);
                if (isPasswordCorrect)
                {
                    return Results.Ok("200|Password is correct");
                }
            }
            return Results.BadRequest("400|Password is incorrect");
        }

        public async Task<IResult> SelectCandidates(
            [FromServices] UserManager<ApplicationUser> _userManager, int page, int pageSize)
        {
            var usersQuery = _userManager.Users;

            var usersList = await usersQuery.Where(u => u.Status == "Active").ToListAsync();

            var usersDtoList = new List<UserDto>();

            foreach (var user in usersList)
            {
                // Tạo đối tượng UserDto
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Fullname = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email,
                    NewEmail = user.NewEmail,
                    IdentityCardNumber = user.IdentityCardNumber,
                    IdentityCardDate = user.IdentityCardDate,
                    IdentityCardPlace = user.IdentityCardPlace,
                    Gender = user.Gender,
                    CellPhone = user.PhoneNumber,
                    status = user.Status,
                    Birthday = user.Birthday,
                    Address = user.Address,
                    CreatedAt = user.CreateDate,
                    Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
                };

                usersDtoList.Add(userDto);
            }
            return Results.Ok(usersDtoList);
        }
    }
}