
using System;
using System.ComponentModel.DataAnnotations;

namespace NEUManagement.Models
{
	public class DangKyTuyenSinh
	{
		public int MaDangKy { get; set; }

		[Required(ErrorMessage = "Mã hồ sơ không được để trống.")]
		public int MaHoSo { get; set; }

		[Required(ErrorMessage = "Vui lòng chọn đợt tuyển sinh.")]
		public int MaDot { get; set; }

		[Required(ErrorMessage = "Vui lòng chọn ngành tuyển sinh.")]
		public int MaDanhMuc { get; set; }

		public DateTime NgayDangKy { get; set; }

		public string TrangThai { get; set; } = "Chờ xét duyệt";

		[StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
		public string? GhiChu { get; set; }
	}
}

