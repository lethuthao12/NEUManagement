using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NEUManagement.Data;
using NEUManagement.Models;

namespace NEUManagement.Controllers
{
	public class HomeController : Controller
	{

		[Authorize(Roles = "Student")]
		[HttpGet]
		public async Task<IActionResult> MyAdmission()
		{
			var maTaiKhoanClaim =
				User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (!int.TryParse(maTaiKhoanClaim, out int maTaiKhoan))
			{
				return Challenge();
			}

			var data =
				await _hoSoTuyenSinhData.GetMyAdmissionAsync(maTaiKhoan);

			if (data.Rows.Count == 0)
			{
				ViewBag.Message = "Tài khoản chưa được liên kết với hồ sơ tuyển sinh.";
			}

			return View(data);
		}

		private readonly HoSoTuyenSinhData _hoSoTuyenSinhData;

		public HomeController(HoSoTuyenSinhData hoSoTuyenSinhData)
		{
			_hoSoTuyenSinhData = hoSoTuyenSinhData;
		}
		

		[Authorize]
		public async Task<IActionResult> Index()
		{
			try
			{
				var statistics =
					await _hoSoTuyenSinhData.GetAdmissionStatisticsAsync();

				ViewBag.TongHoSo = Convert.ToInt32(
					statistics.Rows[0]["TongHoSo"]);

				ViewBag.ChoXetDuyet = Convert.ToInt32(
					statistics.Rows[0]["ChoXetDuyet"]);

				ViewBag.DaDuyet = Convert.ToInt32(
					statistics.Rows[0]["DaDuyet"]);

				ViewBag.TuChoi = Convert.ToInt32(
					statistics.Rows[0]["TuChoi"]);

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





		public async Task<IActionResult> Admissions(
			string? keyword,
			string? status,
			string? major)
		{
			try
			{
				var data = await _hoSoTuyenSinhData.SearchAsync(
					keyword, status, major);

				// Lấy toàn bộ hồ sơ để thống kê chính xác,
				// không phụ thuộc điều kiện tìm kiếm hiện tại.
				var allData = await _hoSoTuyenSinhData.GetAllAsync();

				int pendingCount = 0;
				int approvedCount = 0;
				int rejectedCount = 0;

				foreach (System.Data.DataRow row in allData.Rows)
				{
					string trangThai =
						Convert.ToString(row["TrangThai"]) ?? "";

					if (trangThai == "Chờ xét duyệt")
						pendingCount++;

					if (trangThai == "Đã duyệt")
						approvedCount++;

					if (trangThai == "Từ chối")
						rejectedCount++;
				}

				ViewBag.TotalCount = allData.Rows.Count;
				ViewBag.PendingCount = pendingCount;
				ViewBag.ApprovedCount = approvedCount;
				ViewBag.RejectedCount = rejectedCount;


				ViewBag.Keyword = keyword ?? "";
				ViewBag.Status = status ?? "";
				ViewBag.Major = major ?? "";

				ViewBag.DanhSachNganh =
					await _hoSoTuyenSinhData.GetDanhSachNganhAsync();

				return View(data);

			}
			catch (Microsoft.Data.SqlClient.SqlException)
			{
				return View("Error", new ErrorViewModel
				{
					RequestId = HttpContext.TraceIdentifier
				});
			}
		}



		
		[Authorize(Roles = "Staff")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateAdmissionStatus(
			int maHoSo,
			string trangThai)

		{
			try
			{
				bool updated = await _hoSoTuyenSinhData.UpdateStatusAsync(
					maHoSo,
					trangThai);

				if (!updated)
				{
					TempData["ErrorMessage"] =
						"Không tìm thấy hồ sơ hoặc trạng thái không hợp lệ.";
				}
				else
				{
					TempData["SuccessMessage"] =
						"Cập nhật trạng thái hồ sơ thành công!";
				}
			}
			catch (Microsoft.Data.SqlClient.SqlException)
			{
				TempData["ErrorMessage"] =
					"Không thể cập nhật hồ sơ lúc này. Vui lòng thử lại sau.";
			}

			return RedirectToAction(nameof(Admissions));
		}



		[HttpGet]
		[Authorize(Roles = "Staff")]
		public async Task<IActionResult> CreateAdmission()
		{
			try
			{
				ViewBag.DanhSachNganh =
					await _hoSoTuyenSinhData.GetDanhSachNganhAsync();

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


		[HttpPost]
		[Authorize(Roles = "Staff")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateAdmission(
			NEUManagement.Models.HoSoTuyenSinh hoSo)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.DanhSachNganh =
					await _hoSoTuyenSinhData.GetDanhSachNganhAsync();

				return View(hoSo);
			}

			try
			{
				await _hoSoTuyenSinhData.AddAsync(hoSo);

				TempData["SuccessMessage"] =
					"Thêm hồ sơ tuyển sinh thành công!";

				return RedirectToAction(nameof(Admissions));
			}
			catch (Microsoft.Data.SqlClient.SqlException)
			{
				ModelState.AddModelError(
					string.Empty,
					"Không thể lưu hồ sơ lúc này. Vui lòng thử lại sau.");

				ViewBag.DanhSachNganh =
					await _hoSoTuyenSinhData.GetDanhSachNganhAsync();

				return View(hoSo);
			}
		}


		public async Task<IActionResult> DanhMucTuyenSinh(string keyword = "")
		{
			try
			{
				var data = await _hoSoTuyenSinhData
					.GetDanhMucTuyenSinhAsync(keyword);

				ViewBag.Keyword = keyword;

				return View(data);
			}
			catch (Microsoft.Data.SqlClient.SqlException)
			{
				TempData["ErrorMessage"] =
					"Không thể tải danh mục tuyển sinh lúc này. Vui lòng thử lại sau.";

				ViewBag.Keyword = keyword;

				return View("DanhMucTuyenSinh", new System.Data.DataTable());
			}
		}



		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel
			{
				RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
			});
		}
	}
}
