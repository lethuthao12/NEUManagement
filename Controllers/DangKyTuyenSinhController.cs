
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NEUManagement.Data;
using NEUManagement.Models;

namespace NEUManagement.Controllers
{
	[Authorize]
	public class DangKyTuyenSinhController : Controller
	{
		private readonly DangKyTuyenSinhData _data;

		public DangKyTuyenSinhController(
			DangKyTuyenSinhData data)
		{
			_data = data;
		}

		// Trang đăng ký và danh sách đơn của sinh viên
		[Authorize(Roles = "Student")]
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			try
			{
				int maTaiKhoan = int.Parse(
					User.FindFirstValue(ClaimTypes.NameIdentifier)!
				);

				ViewBag.DanhSachDot =
					await _data.GetDanhSachDotDangMoAsync();

				ViewBag.DanhSachDon =
					await _data.GetDonDangKyCuaSinhVienAsync(
						maTaiKhoan);

				return View();
			}
			catch (Microsoft.Data.SqlClient.SqlException)
			{
				return View("Error", new ErrorViewModel
				{
					RequestId = HttpContext.TraceIdentifier
				});
			}
		}

		// Lấy danh sách ngành theo đợt được chọn
		[Authorize(Roles = "Student")]
		[HttpGet]
		public async Task<IActionResult> GetNganh(int maDot)
		{
			try
			{
				var danhSach =
					await _data.GetDanhSachNganhAsync(maDot);

				return Json(danhSach);
			}
			catch (Microsoft.Data.SqlClient.SqlException)
			{
				return StatusCode(500,
					new { message = "Không thể tải danh sách ngành." });
			}
		}

		// Sinh viên gửi đơn đăng ký
		[Authorize(Roles = "Student")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(
			int maDot,
			int maDanhMuc)
		{
			if (maDot <= 0 || maDanhMuc <= 0)
			{
				TempData["ErrorMessage"] =
					"Vui lòng chọn đợt và ngành tuyển sinh.";

				return RedirectToAction(nameof(Index));
			}

			try
			{
				int maTaiKhoan = int.Parse(
					User.FindFirstValue(ClaimTypes.NameIdentifier)!
				);

				int maDangKy = await _data.AddAsync(
					maTaiKhoan,
					maDot,
					maDanhMuc
				);

				if (maDangKy == 0)
				{
					TempData["ErrorMessage"] =
						"Không thể đăng ký. Đợt tuyển sinh có thể đã đóng hoặc thông tin không hợp lệ.";
				}
				else
				{
					TempData["SuccessMessage"] =
						"Đăng ký tuyển sinh thành công!";
				}

				return RedirectToAction(nameof(Index));
			}
			catch (Microsoft.Data.SqlClient.SqlException ex)
				when (ex.Number == 2601 || ex.Number == 2627)
			{
				TempData["ErrorMessage"] =
					"Bạn đã đăng ký ngành này trong đợt tuyển sinh đã chọn.";

				return RedirectToAction(nameof(Index));
			}
			catch (Microsoft.Data.SqlClient.SqlException)
			{
				return View("Error", new ErrorViewModel
				{
					RequestId = HttpContext.TraceIdentifier
				});
			}
		}
	}
}

