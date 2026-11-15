
using System.ComponentModel.DataAnnotations;

namespace NEUManagement.Models
{
	public class DotTuyenSinh
	{
		public int MaDot { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập tên đợt tuyển sinh.")]
		[StringLength(150)]
		public string TenDot { get; set; } = string.Empty;

		[Range(2000, 2100,
			ErrorMessage = "Năm tuyển sinh không hợp lệ.")]
		public int NamTuyenSinh { get; set; }

		[Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
		[DataType(DataType.Date)]
		public DateTime NgayBatDau { get; set; }
			= DateTime.Today;

		[Required(ErrorMessage = "Vui lòng chọn hạn đăng ký.")]
		[DataType(DataType.Date)]
		public DateTime HanDangKy { get; set; }
			= DateTime.Today.AddDays(30);

		[Required]
		public string TrangThai { get; set; } = "Chưa mở";

		[StringLength(500)]
		public string? MoTa { get; set; }
	}
}

