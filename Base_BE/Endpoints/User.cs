using Base_BE.Identity;
using Base_BE.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceStack;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Base_BE.Endpoints
{
	public class User : EndpointGroupBase
	{
		public override void Map(WebApplication app)
		{
			app.MapGroup(this)
				.MapPost(RegisterUser, "/register");

		}

		public async Task<IResult> RegisterUser([FromBody] RegisterForm newUser, UserManager<ApplicationUser> _userManager)
		{
			if (string.IsNullOrEmpty(newUser.UserName))
			{
				return Results.BadRequest("400|Username is required");
			}

			// Kiểm tra username chỉ chứa chữ cái và chữ số
			if (!newUser.UserName.All(char.IsLetterOrDigit))
			{
				return Results.BadRequest("400|Username can only contain letters or digits.");
			}

			var user = await _userManager.FindByNameAsync(newUser.UserName);
			if (user != null)
			{
				return Results.BadRequest("501|User existed");
			}


			// Tạo mới đối tượng ApplicationUser
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
				IdentityCardImage = newUser.UrlIdentityCardImage
			};

			// Tạo tài khoản người dùng mới
			var result = await _userManager.CreateAsync(newUserEntity, PasswordGenerator.GenerateRandomPassword(12));
			if (result.Succeeded)
			{
				return Results.Ok("200|User created");
			}
			else
			{
				// Ghi nhận lỗi nếu quá trình tạo người dùng thất bại
				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				return Results.BadRequest($"500|{errors}");
			}
		}


		public static class PasswordGenerator
		{
			public static string GenerateRandomPassword(int length)
			{
				// Điều kiện tối thiểu cho mật khẩu
				const string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
				const string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
				const string numberChars = "1234567890";
				const string specialChars = "!@#$%^&*()";

				// Kiểm tra độ dài mật khẩu tối thiểu
				if (length < 8)
					throw new ArgumentException("Độ dài mật khẩu phải ít nhất 8 ký tự.");

				// Chọn ít nhất 1 ký tự từ mỗi nhóm để đảm bảo yêu cầu
				Random random = new Random();
				StringBuilder result = new StringBuilder(length);

				result.Append(lowercaseChars[random.Next(lowercaseChars.Length)]);
				result.Append(uppercaseChars[random.Next(uppercaseChars.Length)]);
				result.Append(numberChars[random.Next(numberChars.Length)]);
				result.Append(specialChars[random.Next(specialChars.Length)]);

				// Phần còn lại sẽ ngẫu nhiên từ tất cả các ký tự hợp lệ
				const string allValidChars = lowercaseChars + uppercaseChars + numberChars + specialChars;
				byte[] randomBytes = new byte[length - 4];

				using (var rng = RandomNumberGenerator.Create())
				{
					rng.GetBytes(randomBytes);
				}

				foreach (byte b in randomBytes)
				{
					result.Append(allValidChars[b % allValidChars.Length]);
				}

				// Đảo ngẫu nhiên các ký tự để mật khẩu không có mẫu dễ đoán
				return new string(result.ToString().OrderBy(_ => random.Next()).ToArray());
			}
		}

	}
}
