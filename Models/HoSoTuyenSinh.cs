
using System.ComponentModel.DataAnnotations;

namespace NEUManagement.Models
{
	public class HoSoTuyenSinh
	{
		public int MaHoSo { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập họ tên")]
		[StringLength(100)]
		public string HoTen { get; set; } = string.Empty;

		[DataType(DataType.Date)]
		public DateTime? NgaySinh { get; set; }

		[Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
		[StringLength(15)]
		public string? SoDienThoai { get; set; }

		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		[StringLength(150)]
		public string? Email { get; set; }

		[Required(ErrorMessage = "Vui lòng chọn ngành đăng ký")]
		[StringLength(100)]
		public string NganhDangKy { get; set; } = string.Empty;

		public DateTime NgayDangKy { get; set; }

		public string TrangThai { get; set; } = "Chờ xét duyệt";
	}
}

