using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using NEUManagement.Data;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(
	CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = "/Account/Login";
		options.AccessDeniedPath = "/Account/AccessDenied";
		options.Cookie.Name = "NEUManagement.Auth";
		options.Cookie.HttpOnly = true;
		options.Cookie.SecurePolicy =
			CookieSecurePolicy.Always;
		options.Cookie.SameSite = SameSiteMode.Lax;
		options.ExpireTimeSpan = TimeSpan.FromHours(2);
		options.SlidingExpiration = true;
	});

builder.Services.AddAuthorization();

builder.Services.AddScoped<NEUManagement.Data.HoSoTuyenSinhData>();
builder.Services.AddScoped<DotTuyenSinhData>();
builder.Services.AddScoped<DangKyTuyenSinhData>();

var app = builder.Build();




// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Account}/{action=Login}/{id?}");

if (app.Environment.IsDevelopment())
{
	var connectionString =
		app.Configuration.GetConnectionString("DefaultConnection");

	if (!string.IsNullOrWhiteSpace(connectionString))
	{
		using var connection =
			new SqlConnection(connectionString);

		connection.Open();

		var passwordHasher = new PasswordHasher<object>();
		var user = new object();

		var accounts = new[]
		{
			new
			{
				Username = "SV001",
				Password = "SinhVien@123",
				FullName = "Sinh viên thử nghiệm",
				Role = "Student"
			},
			new
			{
				Username = "CB001",
				Password = "CanBo@123",
				FullName = "Cán bộ thử nghiệm",
				Role = "Staff"
			}
		};

		foreach (var account in accounts)
		{
			var checkSql = @"
                SELECT COUNT(1)
                FROM dbo.TaiKhoan
                WHERE TenDangNhap = @Username;";

			using var checkCommand =
				new SqlCommand(checkSql, connection);

			checkCommand.Parameters.AddWithValue(
				"@Username", account.Username);

			var exists = (int)checkCommand.ExecuteScalar()!;

			if (exists == 0)
			{
				var passwordHash =
					passwordHasher.HashPassword(
						user, account.Password);

				var insertSql = @"
                    INSERT INTO dbo.TaiKhoan
                    (
                        TenDangNhap,
                        MatKhauHash,
                        HoTen,
                        VaiTro,
                        DangHoatDong
                    )
                    VALUES
                    (
                        @Username,
                        @PasswordHash,
                        @FullName,
                        @Role,
                        1
                    );";

				using var insertCommand =
					new SqlCommand(insertSql, connection);

				insertCommand.Parameters.AddWithValue(
					"@Username", account.Username);

				insertCommand.Parameters.AddWithValue(
					"@PasswordHash", passwordHash);

				insertCommand.Parameters.AddWithValue(
					"@FullName", account.FullName);

				insertCommand.Parameters.AddWithValue(
					"@Role", account.Role);

				insertCommand.ExecuteNonQuery();
			}
		}
	}
}

app.Run();




