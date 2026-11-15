
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NEUManagement.Data;
using NEUManagement.Models;

namespace NEUManagement.Controllers
{
	public class DotTuyenSinhController : Controller
	{
		private readonly DotTuyenSinhData _data;

		public DotTuyenSinhController(DotTuyenSinhData data)
		{
			_data = data;
		}

		// Sinh viên và nhân viên đều được xem danh sách
		[Authorize]
		public async Task<IActionResult> Index()
		{
			try
			{
				var danhSach = await _data.GetAllAsync();
				return View(danhSach);
			}
			catch (SqlException)
			{
				return View("Error", new ErrorViewModel
				{
					RequestId = HttpContext.TraceIdentifier
				});
			}
		}

		// Chỉ nhân viên được mở trang thêm
		[HttpGet]
		[Authorize(Roles = "Staff")]
		public IActionResult Create()
		{
			return View(new DotTuyenSinh
			{
				NamTuyenSinh = DateTime.Today.Year
			});
		}

		// Chỉ nhân viên được thêm đợt
		[HttpPost]
		[Authorize(Roles = "Staff")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(DotTuyenSinh dot)
		{
			if (dot.HanDangKy.Date < dot.NgayBatDau.Date)
			{
				ModelState.AddModelError(
					nameof(dot.HanDangKy),
					"Hạn đăng ký phải bằng hoặc sau ngày bắt đầu.");
			}

			if (dot.TrangThai != "Chưa mở"
				&& dot.TrangThai != "Đang mở"
				&& dot.TrangThai != "Đã đóng")
			{
				ModelState.AddModelError(
					nameof(dot.TrangThai),
					"Trạng thái không hợp lệ.");
			}

			if (!ModelState.IsValid)
			{
				return View(dot);
			}

			try
			{
				await _data.AddAsync(dot);

				TempData["SuccessMessage"] =
					"Thêm đợt tuyển sinh thành công!";

				return RedirectToAction(nameof(Index));
			}
			catch (SqlException)
			{
				ModelState.AddModelError(
					string.Empty,
					"Không thể lưu đợt tuyển sinh. Vui lòng thử lại.");

				return View(dot);
			}
		}

		// Chỉ nhân viên được mở trang sửa
		[HttpGet]
		[Authorize(Roles = "Staff")]
		public async Task<IActionResult> Edit(int id)
		{
			try
			{
				var dot = await _data.GetByIdAsync(id);

				if (dot == null)
				{
					return NotFound();
				}

				return View(dot);
			}
			catch (SqlException)
			{
				return View("Error", new ErrorViewModel
				{
					RequestId = HttpContext.TraceIdentifier
				});
			}
		}

		// Chỉ nhân viên được cập nhật đợt
		[HttpPost]
		[Authorize(Roles = "Staff")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(
			int id, DotTuyenSinh dot)
		{
			if (id != dot.MaDot)
			{
				return BadRequest();
			}

			if (dot.HanDangKy.Date < dot.NgayBatDau.Date)
			{
				ModelState.AddModelError(
					nameof(dot.HanDangKy),
					"Hạn đăng ký phải bằng hoặc sau ngày bắt đầu.");
			}

			if (dot.TrangThai != "Chưa mở"
				&& dot.TrangThai != "Đang mở"
				&& dot.TrangThai != "Đã đóng")
			{
				ModelState.AddModelError(
					nameof(dot.TrangThai),
					"Trạng thái không hợp lệ.");
			}

			if (!ModelState.IsValid)
			{
				return View(dot);
			}

			try
			{
				bool updated = await _data.UpdateAsync(dot);

				if (!updated)
				{
					return NotFound();
				}

				TempData["SuccessMessage"] =
					"Cập nhật đợt tuyển sinh thành công!";

				return RedirectToAction(nameof(Index));
			}
			catch (SqlException)
			{
				ModelState.AddModelError(
					string.Empty,
					"Không thể cập nhật đợt tuyển sinh. Vui lòng thử lại.");

				return View(dot);
			}
		}
	}
}
