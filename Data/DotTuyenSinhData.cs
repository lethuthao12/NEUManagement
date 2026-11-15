
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using NEUManagement.Models;

namespace NEUManagement.Data
{
	public class DotTuyenSinhData
	{
		private readonly string _connectionString;

		public DotTuyenSinhData(IConfiguration configuration)
		{
			_connectionString =
				configuration.GetConnectionString("DefaultConnection")
				?? throw new InvalidOperationException(
					"Chưa cấu hình DefaultConnection.");
		}

		public async Task<DataTable> GetAllAsync()
		{
			var table = new DataTable();

			const string sql = @"
                SELECT MaDot, TenDot, NamTuyenSinh,
                       NgayBatDau, HanDangKy, TrangThai, MoTa
                FROM dbo.DotTuyenSinh
                ORDER BY MaDot DESC;";

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

		public async Task AddAsync(DotTuyenSinh dot)
		{
			const string sql = @"
                INSERT INTO dbo.DotTuyenSinh
                    (TenDot, NamTuyenSinh, NgayBatDau,
                     HanDangKy, TrangThai, MoTa)
                VALUES
                    (@TenDot, @NamTuyenSinh, @NgayBatDau,
                     @HanDangKy, @TrangThai, @MoTa);";

			await using var connection =
				new SqlConnection(_connectionString);

			await using var command =
				new SqlCommand(sql, connection);

			command.Parameters.Add("@TenDot", SqlDbType.NVarChar, 150)
				.Value = dot.TenDot.Trim();

			command.Parameters.Add("@NamTuyenSinh", SqlDbType.Int)
				.Value = dot.NamTuyenSinh;

			command.Parameters.Add("@NgayBatDau", SqlDbType.Date)
				.Value = dot.NgayBatDau.Date;

			command.Parameters.Add("@HanDangKy", SqlDbType.Date)
				.Value = dot.HanDangKy.Date;

			command.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 30)
				.Value = dot.TrangThai;

			command.Parameters.Add("@MoTa", SqlDbType.NVarChar, 500)
				.Value = (object?)dot.MoTa ?? DBNull.Value;

			await connection.OpenAsync();
			await command.ExecuteNonQueryAsync();
		}

		public async Task<DotTuyenSinh?> GetByIdAsync(int maDot)
		{
			const string sql = @"
        SELECT MaDot, TenDot, NamTuyenSinh,
               NgayBatDau, HanDangKy, TrangThai, MoTa
        FROM dbo.DotTuyenSinh
        WHERE MaDot = @MaDot;";

			await using var connection =
				new SqlConnection(_connectionString);
			await using var command = new SqlCommand(sql, connection);

			command.Parameters.Add("@MaDot", SqlDbType.Int).Value = maDot;

			await connection.OpenAsync();

			await using var reader = await command.ExecuteReaderAsync();

			if (!await reader.ReadAsync())
				return null;

			return new DotTuyenSinh
			{
				MaDot = reader.GetInt32(reader.GetOrdinal("MaDot")),
				TenDot = reader.GetString(reader.GetOrdinal("TenDot")),
				NamTuyenSinh = reader.GetInt32(
					reader.GetOrdinal("NamTuyenSinh")),
				NgayBatDau = reader.GetDateTime(
					reader.GetOrdinal("NgayBatDau")),
				HanDangKy = reader.GetDateTime(
					reader.GetOrdinal("HanDangKy")),
				TrangThai = reader.GetString(
					reader.GetOrdinal("TrangThai")),
				MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa"))
					? null
					: reader.GetString(reader.GetOrdinal("MoTa"))
			};
		}

		public async Task<bool> UpdateAsync(DotTuyenSinh dot)
		{
			const string sql = @"
        UPDATE dbo.DotTuyenSinh
        SET TenDot = @TenDot,
            NamTuyenSinh = @NamTuyenSinh,
            NgayBatDau = @NgayBatDau,
            HanDangKy = @HanDangKy,
            TrangThai = @TrangThai,
            MoTa = @MoTa
        WHERE MaDot = @MaDot;";

			await using var connection =
				new SqlConnection(_connectionString);
			await using var command = new SqlCommand(sql, connection);

			command.Parameters.Add("@MaDot", SqlDbType.Int)
				.Value = dot.MaDot;
			command.Parameters.Add("@TenDot", SqlDbType.NVarChar, 150)
				.Value = dot.TenDot.Trim();
			command.Parameters.Add("@NamTuyenSinh", SqlDbType.Int)
				.Value = dot.NamTuyenSinh;
			command.Parameters.Add("@NgayBatDau", SqlDbType.Date)
				.Value = dot.NgayBatDau.Date;
			command.Parameters.Add("@HanDangKy", SqlDbType.Date)
				.Value = dot.HanDangKy.Date;
			command.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 30)
				.Value = dot.TrangThai;
			command.Parameters.Add("@MoTa", SqlDbType.NVarChar, 500)
				.Value = (object?)dot.MoTa ?? DBNull.Value;

			await connection.OpenAsync();

			return await command.ExecuteNonQueryAsync() > 0;
		}

	}
}
	

	
