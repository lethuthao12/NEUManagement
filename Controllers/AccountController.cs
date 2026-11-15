
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using System.Data;

namespace NEUManagement.Controllers
{
	public class AccountController : Controller
	{
		private readonly IConfiguration _configuration;
		private readonly PasswordHasher<object> _passwordHasher =
			new PasswordHasher<object>();

		public AccountController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		// Mở trang đăng nhập
		[HttpGet]
		public IActionResult Login()
		{
			return View();
		}

		// Xử lý khi người dùng bấm Đăng nhập
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(
			string TenDangNhap,
			string MatKhau,
			string VaiTro,
			bool GhiNhoDangNhap = false)
		{
			if (string.IsNullOrWhiteSpace(TenDangNhap) ||
				string.IsNullOrWhiteSpace(MatKhau) ||
				string.IsNullOrWhiteSpace(VaiTro))
			{
				ViewBag.ErrorMessage =
					"Vui lòng nhập đầy đủ tài khoản, mật khẩu và vai trò.";
				return View();
			}

			if (VaiTro != "Student" && VaiTro != "Staff")
			{
				ViewBag.ErrorMessage = "Vai trò đăng nhập không hợp lệ.";
				return View();
			}

			var connectionString =
				_configuration.GetConnectionString("DefaultConnection");

			if (string.IsNullOrWhiteSpace(connectionString))
			{
				ViewBag.ErrorMessage =
					"Hệ thống chưa được cấu hình kết nối. Vui lòng thử lại sau.";
				return View();
			}

			try
			{
				string? hoTen = null;
				string? passwordHash = null;
				string? vaiTroTrongDb = null;
				int maTaiKhoan = 0;

				await using (var connection =
					new SqlConnection(connectionString))
				{
					await connection.OpenAsync();

					const string sql = @"
                        SELECT MaTaiKhoan, TenDangNhap,
                               MatKhauHash, HoTen, VaiTro
                        FROM dbo.TaiKhoan
                        WHERE TenDangNhap = @TenDangNhap
                          AND VaiTro = @VaiTro
                          AND DangHoatDong = 1;";

					await using var command =
						new SqlCommand(sql, connection);

					command.Parameters.Add(
						"@TenDangNhap", SqlDbType.NVarChar, 100
					).Value = TenDangNhap.Trim();

					command.Parameters.Add(
						"@VaiTro", SqlDbType.NVarChar, 20
					).Value = VaiTro;

					await using var reader =
						await command.ExecuteReaderAsync();

					if (await reader.ReadAsync())
					{
						maTaiKhoan = reader.GetInt32(
							reader.GetOrdinal("MaTaiKhoan"));

						passwordHash = reader.GetString(
							reader.GetOrdinal("MatKhauHash"));

						hoTen = reader.GetString(
							reader.GetOrdinal("HoTen"));

						vaiTroTrongDb = reader.GetString(
							reader.GetOrdinal("VaiTro"));
					}
				}

				// Không tiết lộ tài khoản có tồn tại hay không
				if (passwordHash == null ||
					hoTen == null ||
					vaiTroTrongDb == null)
				{
					ViewBag.ErrorMessage =
						"Tài khoản, mật khẩu hoặc vai trò không chính xác.";
					return View();
				}

				var verification = _passwordHasher.VerifyHashedPassword(
					new object(),
					passwordHash,
					MatKhau);

				if (verification == PasswordVerificationResult.Failed)
				{
					ViewBag.ErrorMessage =
						"Tài khoản, mật khẩu hoặc vai trò không chính xác.";
					return View();
				}

				var claims = new List<Claim>
				{
					new Claim(
						ClaimTypes.NameIdentifier,
						maTaiKhoan.ToString()),

					new Claim(ClaimTypes.Name, TenDangNhap.Trim()),

					new Claim("HoTen", hoTen),

					new Claim(ClaimTypes.Role, vaiTroTrongDb)
				};

				var identity = new ClaimsIdentity(
					claims,
					CookieAuthenticationDefaults.AuthenticationScheme);

				var principal = new ClaimsPrincipal(identity);

				await HttpContext.SignInAsync(
					CookieAuthenticationDefaults.AuthenticationScheme,
					principal,
					new AuthenticationProperties
					{
						IsPersistent = GhiNhoDangNhap,
						AllowRefresh = true
					});

				return RedirectToAction("Index", "Home");
			}
			catch (SqlException ex)
			{
				// Chỉ ghi chi tiết kỹ thuật vào log phía máy chủ
				HttpContext.RequestServices
					.GetRequiredService<ILogger<AccountController>>()
					.LogError(ex, "Lỗi SQL khi xử lý đăng nhập.");

				ViewBag.ErrorMessage =
					"Hệ thống đang gặp sự cố. Vui lòng thử lại sau.";
				return View();
			}
			catch (Exception ex)
			{
				HttpContext.RequestServices
					.GetRequiredService<ILogger<AccountController>>()
					.LogError(ex, "Lỗi không xác định khi đăng nhập.");

				ViewBag.ErrorMessage =
					"Không thể xử lý đăng nhập lúc này. Vui lòng thử lại sau.";
				return View();
			}
		}


		[HttpGet]
		public IActionResult AccessDenied()
		{
			return View();
		}


		// Đăng xuất
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(
				CookieAuthenticationDefaults.AuthenticationScheme);

			return RedirectToAction("Login", "Account");
		}
	}
}


