
using Microsoft.Data.SqlClient;
using System.Data;

namespace NEUManagement.Data
{
	public class DangKyTuyenSinhData
	{
		private readonly string _connectionString;

		public DangKyTuyenSinhData(IConfiguration configuration)
		{
			_connectionString =
				configuration.GetConnectionString("DefaultConnection")
				?? throw new InvalidOperationException(
					"Chưa cấu hình chuỗi kết nối DefaultConnection.");
		}

		// 1. Lấy các đợt tuyển sinh đang mở
		public async Task<DataTable> GetDanhSachDotDangMoAsync()
		{
			var table = new DataTable();

			const string sql = @"
                SELECT
                    MaDot,
                    TenDot,
                    NamTuyenSinh,
                    NgayBatDau,
                    HanDangKy,
                    MoTa
                FROM dbo.DotTuyenSinh
                WHERE TrangThai = N'Đang mở'
                  AND NgayBatDau <= CAST(GETDATE() AS DATE)
                  AND HanDangKy >= CAST(GETDATE() AS DATE)
                ORDER BY NgayBatDau DESC;";

			await using var connection =
				new SqlConnection(_connectionString);

			await using var command =
				new SqlCommand(sql, connection);

			await connection.OpenAsync();

			await using var reader =
				await command.ExecuteReaderAsync();

			table.Load(reader);

			return table;
		}

		// 2. Lấy ngành thuộc năm tuyển sinh của đợt được chọn
		public async Task<DataTable> GetDanhSachNganhAsync(int maDot)
		{
			var table = new DataTable();

			const string sql = @"
                SELECT
                    dm.MaDanhMuc,
                    dm.MaNganh,
                    dm.TenNganh,
                    dm.MaChuongTrinh,
                    dm.ChiTieu
                FROM dbo.DanhMucTuyenSinh dm
                INNER JOIN dbo.DotTuyenSinh d
                    ON dm.NamTuyenSinh = d.NamTuyenSinh
                WHERE d.MaDot = @MaDot
                  AND d.TrangThai = N'Đang mở'
                  AND d.NgayBatDau <= CAST(GETDATE() AS DATE)
                  AND d.HanDangKy >= CAST(GETDATE() AS DATE)
                  AND dm.DangHienThi = 1
                ORDER BY dm.TenNganh;";

			await using var connection =
				new SqlConnection(_connectionString);

			await using var command =
				new SqlCommand(sql, connection);

			command.Parameters.Add("@MaDot", SqlDbType.Int)
				.Value = maDot;

			await connection.OpenAsync();

			await using var reader =
				await command.ExecuteReaderAsync();

			table.Load(reader);

			return table;
		}

		// 3. Sinh viên gửi đơn đăng ký
		// MaHoSo được lấy tự động từ tài khoản đăng nhập
		public async Task<int> AddAsync(
			int maTaiKhoan,
			int maDot,
			int maDanhMuc)
		{
			const string sql = @"
                INSERT INTO dbo.DangKyTuyenSinh
                    (MaHoSo, MaDot, MaDanhMuc)
                OUTPUT INSERTED.MaDangKy
                SELECT
                    tk.MaHoSo,
                    d.MaDot,
                    dm.MaDanhMuc
                FROM dbo.TaiKhoan tk
                INNER JOIN dbo.DotTuyenSinh d
                    ON d.MaDot = @MaDot
                INNER JOIN dbo.DanhMucTuyenSinh dm
                    ON dm.MaDanhMuc = @MaDanhMuc
                   AND dm.NamTuyenSinh = d.NamTuyenSinh
                WHERE tk.MaTaiKhoan = @MaTaiKhoan
                  AND tk.VaiTro = N'Student'
                  AND tk.DangHoatDong = 1
                  AND tk.MaHoSo IS NOT NULL
                  AND d.TrangThai = N'Đang mở'
                  AND d.NgayBatDau <= CAST(GETDATE() AS DATE)
                  AND d.HanDangKy >= CAST(GETDATE() AS DATE)
                  AND dm.DangHienThi = 1;";

			await using var connection =
				new SqlConnection(_connectionString);

			await using var command =
				new SqlCommand(sql, connection);

			command.Parameters.Add("@MaTaiKhoan", SqlDbType.Int)
				.Value = maTaiKhoan;

			command.Parameters.Add("@MaDot", SqlDbType.Int)
				.Value = maDot;

			command.Parameters.Add("@MaDanhMuc", SqlDbType.Int)
				.Value = maDanhMuc;

			await connection.OpenAsync();

			var result = await command.ExecuteScalarAsync();

			// Không có bản ghi phù hợp hoặc đợt không còn mở
			if (result == null || result == DBNull.Value)
			{
				return 0;
			}

			return Convert.ToInt32(result);
		}

		// 4. Lấy danh sách đơn đăng ký của sinh viên
		public async Task<DataTable> GetDonDangKyCuaSinhVienAsync(
			int maTaiKhoan)
		{
			var table = new DataTable();

			const string sql = @"
                SELECT
                    dk.MaDangKy,
                    d.TenDot,
                    dm.MaNganh,
                    dm.TenNganh,
                    dk.NgayDangKy,
                    dk.TrangThai,
                    dk.GhiChu
                FROM dbo.DangKyTuyenSinh dk
                INNER JOIN dbo.TaiKhoan tk
                    ON dk.MaHoSo = tk.MaHoSo
                INNER JOIN dbo.DotTuyenSinh d
                    ON dk.MaDot = d.MaDot
                INNER JOIN dbo.DanhMucTuyenSinh dm
                    ON dk.MaDanhMuc = dm.MaDanhMuc
                WHERE tk.MaTaiKhoan = @MaTaiKhoan
                  AND tk.VaiTro = N'Student'
                ORDER BY dk.NgayDangKy DESC;";

			await using var connection =
				new SqlConnection(_connectionString);

			await using var command =
				new SqlCommand(sql, connection);

			command.Parameters.Add("@MaTaiKhoan", SqlDbType.Int)
				.Value = maTaiKhoan;

			await connection.OpenAsync();

			await using var reader =
				await command.ExecuteReaderAsync();

			table.Load(reader);

			return table;
		}
	}
}
