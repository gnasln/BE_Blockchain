using Base_BE.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceStack;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Domain.Constants;
using Base_BE.Domain.Entities;
using Base_BE.Dtos;
using Base_BE.Helper.Services;
using Base_BE.Services;
using NetHelper.Common.Models;
using Microsoft.EntityFrameworkCore;
using Base_BE.Helper.key;

namespace Base_BE.Endpoints
{
    public class User : EndpointGroupBase
    {
        public override void Map(WebApplication app)
        {
            app.MapGroup(this)
                .MapPost(RegisterUser, "/register")
                .MapPost(ForgotPassword, "/forgot-password")
                ;


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
                .MapGet(GetUserById, "/get-user/{id}")
                ;

            app.MapGroup(this)
                .RequireAuthorization("admin")
                .MapGet(GetAllUsers, "/get-all-users")
                // .MapPost(DisableAccount, "/disable-account/{id}")
                //.MapGet(GetUserById, "/get-user/{id}")
                .MapGet(SelectCandidates, "/select-candidates")
                ;
        }

        public async Task<IResult> RegisterUser([FromBody] RegisterForm newUser,
            UserManager<ApplicationUser> _userManager, [FromServices] IEmailSender _emailSender,
            [FromServices] IBackgroundTaskQueue taskQueue)
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

            // Tạo cặp khóa 
            var privateKey = RandomPrivateKeyGenerator.GetRandomPrivateKey();
            var keyPair = RandomPrivateKeyGenerator.GenerateKeyPair(privateKey);


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
                PublicKey = keyPair["publicKey"]
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
                taskQueue.QueueBackgroundWorkItem(async ct =>
                {
                    await _emailSender.SendEmailRegisterAsync(newUser.Email, newUser.FullName, newUser.UserName,
                        passwordSeed, keyPair["privateKey"]);
                });

                return Results.Ok("200|User created successfully");
            }
            else
            {
                // Trả về lỗi nếu quá trình tạo tài khoản thất bại
                return Results.BadRequest($"500|{string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        public async Task<IResult> ForgotPassword([FromBody] ForgotPassword forgotPassword,
            UserManager<ApplicationUser> _userManager, [FromServices] IEmailSender _emailSender,
            [FromServices] IBackgroundTaskQueue taskQueue)
        {
            try
            {
                if (forgotPassword == null)
                {
                    return Results.BadRequest("400|Missing email address");
                }

                var user = await _userManager.FindByNameAsync(forgotPassword.UserName);
                if (user == null)
                {
                    return Results.BadRequest("400|User not found");
                }

                var newPassword = GenerateSecurePassword();
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (result.Succeeded)
                {
                    // await _emailSender.SendEmailAsync(forgotPassword.Email, user.UserName,
                    //     $"Mật khẩu mới của bạn là: {newPassword}");
                    taskQueue.QueueBackgroundWorkItem(async ct =>
                    {
                        await _emailSender.SendEmailAsync(forgotPassword.Email, user.UserName,
                            $"Mật khẩu mới của bạn là: {newPassword}");
                    });
                    return Results.Ok("200|Password reset successfully");
                }

                return Results.BadRequest("400|Password reset failed");
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while resetting the password.", statusCode: 500);
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
                currentUser.Gender = updateUser.Gender;
                if (!(updateUser.Birthday == null)) currentUser.Birthday = updateUser.Birthday;
                if (!updateUser.ImageUrl.IsNullOrEmpty()) currentUser.ImageUrl = updateUser.ImageUrl;
                if (!updateUser.IdentityCardImage.IsNullOrEmpty())
                    currentUser.IdentityCardImage = updateUser.IdentityCardImage;
                if (!updateUser.IdentityCardNumber.IsNullOrEmpty())
                    currentUser.IdentityCardNumber = updateUser.IdentityCardNumber;
                if (!(updateUser.IdentityCardDate == null)) currentUser.IdentityCardDate = updateUser.IdentityCardDate;
                if (!updateUser.IdentityCardPlace.IsNullOrEmpty())
                    currentUser.IdentityCardPlace = updateUser.IdentityCardPlace;


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
            [FromServices] IEmailSender _emailSender, [FromBody] SendOTPRequest request, [FromServices] IUser _user)
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

            if (!currentUser.Email.IsNullOrEmpty() && currentUser.NewEmail.IsNullOrEmpty())
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
            [FromServices] UserManager<ApplicationUser> _userManager,
            string? fullName,
            string? email,
            string? cellPhone,
            string? status,
            string? role,
            int page,
            int pageSize)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            // Tìm kiếm theo Fullname
            if (!string.IsNullOrEmpty(fullName))
            {
                usersQuery = usersQuery.Where(u => u.FullName.Contains(fullName));
            }

            // Tìm kiếm theo Email
            if (!string.IsNullOrEmpty(email))
            {
                usersQuery = usersQuery.Where(u => u.Email.Contains(email));
            }

            // Tìm kiếm theo CellPhone
            if (!string.IsNullOrEmpty(cellPhone))
            {
                usersQuery = usersQuery.Where(u => u.CellPhone.Contains(cellPhone));
            }

            // Tìm kiếm theo Status
            if (!string.IsNullOrEmpty(status))
            {
                usersQuery = usersQuery.Where(u => u.Status.Contains(status));
            }

            // Lấy danh sách người dùng và lấy roles ngoài truy vấn để tránh vấn đề với GetRolesAsync
            var usersList = await usersQuery.ToListAsync();

            var usersDtoList = new List<UserDto>();

            foreach (var u in usersList)
            {
                // Lấy roles cho người dùng
                var roles = await _userManager.GetRolesAsync(u);

                var userDto = new UserDto
                {
                    Id = u.Id,
                    Fullname = u.FullName,
                    UserName = u.UserName,
                    Email = u.Email,
                    CellPhone = u.CellPhone,
                    status = u.Status,
                    Birthday = u.Birthday,
                    Address = u.Address,
                    CreatedAt = u.CreateDate,
                    Role = roles.FirstOrDefault(),
                    ImageUrl = u.ImageUrl,
                    IdentityCardImage = u.IdentityCardImage
                };

                usersDtoList.Add(userDto);
            }

            // Lọc theo Role
            if (!string.IsNullOrEmpty(role))
            {
                usersDtoList = usersDtoList.Where(u => u.Role == role).ToList();
            }

            // Sắp xếp theo CreatedAt
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
                Role = (await _userManager.GetRolesAsync(currentUser)).FirstOrDefault(),
                ImageUrl = currentUser.ImageUrl,
                IdentityCardImage = currentUser.IdentityCardImage
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
                    CellPhone = user.CellPhone,
                    status = user.Status,
                    Birthday = user.Birthday,
                    Address = user.Address,
                    CreatedAt = user.CreateDate,
                    Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault(),
                    ImageUrl = user.ImageUrl,
                    IdentityCardImage = user.IdentityCardImage
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
                    CellPhone = user.CellPhone,
                    status = user.Status,
                    Birthday = user.Birthday,
                    Address = user.Address,
                    CreatedAt = user.CreateDate,
                    Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault(),
                    ImageUrl = user.ImageUrl,
                    IdentityCardImage = user.IdentityCardImage
                };

                usersDtoList.Add(userDto);
            }

            return Results.Ok(usersDtoList);
        }
    }
}