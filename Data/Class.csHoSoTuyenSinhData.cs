using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace NEUManagement.Data
{
	public class HoSoTuyenSinhData
	{
		private readonly string _connectionString;

		public HoSoTuyenSinhData(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("DefaultConnection")
				?? throw new InvalidOperationException(
					"Chưa cấu hình chuỗi kết nối DefaultConnection.");
		}

		public async Task<DataTable> GetAllAsync()
		{
			var table = new DataTable();

			const string sql = @"
                SELECT
                    MaHoSo,
                    HoTen,
                    NgayDangKy,
                    NganhDangKy,
                    TrangThai
                FROM dbo.HoSoTuyenSinh
                ORDER BY MaHoSo ASC;";

			await using var connection = new SqlConnection(_connectionString);
			await using var command = new SqlCommand(sql, connection);

			await connection.OpenAsync();

			await using var reader = await command.ExecuteReaderAsync();
			table.Load(reader);

			return table;
		}

		public async Task<DataTable> GetAdmissionStatisticsAsync()
		{
			var table = new DataTable();

			const string sql = @"
        SELECT
            COUNT(*) AS TongHoSo,
            SUM(CASE
                WHEN TrangThai = N'Chờ xét duyệt' THEN 1
                ELSE 0
            END) AS ChoXetDuyet,
            SUM(CASE
                WHEN TrangThai = N'Đã duyệt' THEN 1
                ELSE 0
            END) AS DaDuyet,
            SUM(CASE
                WHEN TrangThai = N'Từ chối' THEN 1
                ELSE 0
            END) AS TuChoi
        FROM dbo.HoSoTuyenSinh;";

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


		public async Task<DataTable> SearchAsync(
			string? keyword,
			string? status,
			string? major)
		{
			var table = new DataTable();

			const string sql = @"
        SELECT
            MaHoSo,
            HoTen,
            NgayDangKy,
            NganhDangKy,
            TrangThai
        FROM dbo.HoSoTuyenSinh
        WHERE
            (
                @Keyword = N''
                OR HoTen LIKE N'%' + @Keyword + N'%'
                OR CONVERT(NVARCHAR(20), MaHoSo) LIKE N'%' + @Keyword + N'%'
            )
            AND (@Status = N'' OR TrangThai = @Status)
            AND (@Major = N'' OR NganhDangKy = @Major)
        ORDER BY MaHoSo ASC;";

			string safeKeyword = keyword?.Trim() ?? "";
			string safeStatus = status?.Trim() ?? "";
			string safeMajor = major?.Trim() ?? "";

			await using var connection = new SqlConnection(_connectionString);
			await using var command = new SqlCommand(sql, connection);

			command.Parameters.Add("@Keyword", SqlDbType.NVarChar, 100)
				.Value = safeKeyword;

			command.Parameters.Add("@Status", SqlDbType.NVarChar, 30)
				.Value = safeStatus;

			command.Parameters.Add("@Major", SqlDbType.NVarChar, 100)
				.Value = safeMajor;

			await connection.OpenAsync();

			await using var reader = await command.ExecuteReaderAsync();
			table.Load(reader);

			return table;
		}


		public async Task AddAsync(NEUManagement.Models.HoSoTuyenSinh hoSo)
		{
			const string sql = @"
        INSERT INTO dbo.HoSoTuyenSinh
            (HoTen, NgaySinh, SoDienThoai, Email, NganhDangKy)
        VALUES
            (@HoTen, @NgaySinh, @SoDienThoai, @Email, @NganhDangKy);";

			await using var connection = new SqlConnection(_connectionString);
			await using var command = new SqlCommand(sql, connection);

			command.Parameters.Add("@HoTen", SqlDbType.NVarChar, 100)
				.Value = hoSo.HoTen;

			command.Parameters.Add("@NgaySinh", SqlDbType.Date)
				.Value = hoSo.NgaySinh.HasValue
					? hoSo.NgaySinh.Value.Date
					: DBNull.Value;

			command.Parameters.Add("@SoDienThoai", SqlDbType.VarChar, 15)
				.Value = (object?)hoSo.SoDienThoai ?? DBNull.Value;

			command.Parameters.Add("@Email", SqlDbType.NVarChar, 150)
				.Value = (object?)hoSo.Email ?? DBNull.Value;

			command.Parameters.Add("@NganhDangKy", SqlDbType.NVarChar, 100)
				.Value = hoSo.NganhDangKy;

			await connection.OpenAsync();
			await command.ExecuteNonQueryAsync();
		}

		public async Task<bool> UpdateStatusAsync(int maHoSo, string trangThai)
		{
			if (trangThai != "Chờ xét duyệt"
				&& trangThai != "Đã duyệt"
				&& trangThai != "Từ chối")
			{
				return false;
			}

			const string sql = @"
        UPDATE dbo.HoSoTuyenSinh
        SET TrangThai = @TrangThai
        WHERE MaHoSo = @MaHoSo;";

			await using var connection = new SqlConnection(_connectionString);
			await using var command = new SqlCommand(sql, connection);

			command.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 30)
				.Value = trangThai;

			command.Parameters.Add("@MaHoSo", SqlDbType.Int)
				.Value = maHoSo;

			await connection.OpenAsync();

			int rowsAffected = await command.ExecuteNonQueryAsync();

			return rowsAffected > 0;
		}


		public async Task<DataTable> GetDanhMucTuyenSinhAsync(string keyword = "")
		{
			var table = new DataTable();

			const string sql = @"
		
		SELECT
			MaDanhMuc,
			NamTuyenSinh,
			MaNganh,
			TenNganh,
			MaChuongTrinh,
			ChiTieu,
			NhomChuongTrinh,
			MoTa,
			NguonChinhThuc

		FROM dbo.DanhMucTuyenSinh
		WHERE DangHienThi = 1
		  AND (
			  @Keyword = N''
			  OR MaNganh LIKE @SearchKeyword
			  OR TenNganh LIKE @SearchKeyword
		  )
		ORDER BY TenNganh;";

			await using var connection = new SqlConnection(_connectionString);
			await using var command = new SqlCommand(sql, connection);

			string searchKeyword = "%" + keyword.Trim() + "%";

			command.Parameters.Add("@Keyword", SqlDbType.NVarChar, 200)
				.Value = keyword.Trim();

			command.Parameters.Add("@SearchKeyword", SqlDbType.NVarChar, 202)
				.Value = searchKeyword;

			await connection.OpenAsync();

			await using var reader = await command.ExecuteReaderAsync();
			table.Load(reader);

			return table;
		}
		public async Task<DataTable> GetDanhSachNganhAsync()
		{
			using var connection = new SqlConnection(_connectionString);

			const string sql = @"
        SELECT MaNganh, TenNganh
        FROM dbo.DanhMucTuyenSinh
        WHERE NamTuyenSinh = 2025
          AND DangHienThi = 1
        ORDER BY MaNganh;";

			using var command = new SqlCommand(sql, connection);
			using var adapter = new SqlDataAdapter(command);

			var table = new DataTable();

			await connection.OpenAsync();
			adapter.Fill(table);

			return table;
		}

		public async Task<DataTable> GetMyAdmissionAsync(int maTaiKhoan)
		{
			var result = new DataTable();

			await using var connection = 
			      new SqlConnection(_connectionString);

			const string sql = @"
        SELECT
            hs.MaHoSo,
            hs.HoTen,
            hs.NgaySinh,
            hs.SoDienThoai,
            hs.Email,
            hs.NganhDangKy,
            hs.NgayDangKy,
            hs.TrangThai
        FROM dbo.TaiKhoan AS tk
        INNER JOIN dbo.HoSoTuyenSinh AS hs
            ON tk.MaHoSo = hs.MaHoSo
        WHERE tk.MaTaiKhoan = @MaTaiKhoan
          AND tk.VaiTro = N'Student'
          AND tk.DangHoatDong = 1;";

			await using var command =
				new SqlCommand(sql, connection);

			command.Parameters.Add(
				"@MaTaiKhoan",
				SqlDbType.Int
			).Value = maTaiKhoan;

			await connection.OpenAsync();

			await using var reader =
				await command.ExecuteReaderAsync();

			result.Load(reader);

			return result;
		}


	}

}
