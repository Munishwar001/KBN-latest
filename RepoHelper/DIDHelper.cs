using Dapper;
using KBN.Models.DBModels;
using KBN.Models.DIDModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;

namespace KBN.RepoHelper
{
    [Authorize]
    public class DIDHelper
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly ILogger<DIDHelper> _logger;

        public DIDHelper(UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger<DIDHelper> logger)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _userManager = userManager;
            _logger = logger;
        }

        public List<DID_Assigning> GetData()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var data = connection.GetList<DID_Assigning>("WHERE IsVoid = 0").ToList();
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching the data"+ex);
                throw new Exception("Exception while fetching the DID Data" ,ex);
            }
        }

        public (bool Success, Dictionary<long, string> Errors) AddData(List<RangeViewModel> data)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        // Already existing DIDs
                        string checkDID = @"SELECT DID FROM DID_Assigning";
                        var existingDIDs = connection.Query<long>(
                            checkDID,
                            transaction: transaction
                        ).ToList();

                        var errors = new Dictionary<long, string>();
                        var validData = new List<RangeViewModel>();

                        foreach (var d in data)
                        {
                            if (d.DID.ToString().Length != 11)
                            {
                                errors[d.DID] = "DID must be 11 digits";
                                continue;
                            }
                            if (existingDIDs.Contains(d.DID))
                            {
                                errors[d.DID] = "DID already exists";
                                continue;
                            }

                            validData.Add(d);
                        }

                        if (validData.Any())
                        {
                            string sql = @"
                        INSERT INTO DID_Assigning (DID, City, Country)
                        VALUES (@DID, @City, @Country);";

                            connection.Execute(sql, validData, transaction);
                            transaction.Commit();
                        }

                        return (validData.Any(), errors);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while inserting the data: " + ex);
                throw new Exception("Exception while inserting the DID Data", ex);
            }
        }

        public bool DeleteDID(long did) // For Delete the DID
        {
            try
            {
                var connection = new SqlConnection(_connectionString);

                string deleteSql = @"DELETE FROM DID_Assigning WHERE DID = @DID";

                int rowAffected = connection.Execute(deleteSql, new { DID = did });
                return rowAffected>0;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while inserting the data: " + ex);
                throw new Exception("Exception while inserting the DID Data", ex);
            }
        }
         public RangeViewModel GetUpdateDiD(long did)
        {
            try
            {
                var connection = new SqlConnection(_connectionString);

                string getUpdate = @"select Id ,DID ,City ,Country ,ReqId from DID_Assigning where DID = @DID";

                var  data = connection.QueryFirstOrDefault<RangeViewModel>(getUpdate, new { DID = did });
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while inserting the data: " + ex);
                throw new Exception("Exception while inserting the DID Data", ex);
            }
        }
        public ResultModel updateData(RangeViewModel data)
        {
            try
            {
                if (data.DID.ToString().Length != 11)
                {
                    return new ResultModel { Success = false, Message = "it must be of 11 digits!" };
                }

                using (var connection = new SqlConnection(_connectionString))
                {
                   connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        string query = @"
                                SELECT COUNT(*) 
                                FROM DID_Assigning 
                                WHERE DID = @DID AND Id != @CurrentId";

                        int count = connection.ExecuteScalar<int>(query, new { DID = data.DID, CurrentId = data.Id },transaction);
                        if (count > 0)
                        {
                            return new ResultModel { Success = false, Message = "it must be of 11 digits!" };
                        }
                        string updateDIDSql = @"
                                UPDATE DID_Assigning
                                SET DID = @DID,
                                    City = @City,
                                    Country = @Country
                                WHERE Id = @Id";

                        connection.Execute(updateDIDSql, new{ DID = data.DID, City = data.City,Country = data.Country,Id = data.Id}, transaction);

                        transaction.Commit();
                    }
                }
                return new ResultModel { Success = true, Message = "Updated!" };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while inserting the data: " + ex);
                return new ResultModel { Success = false, Message = $"Error: {ex.Message}" };
            }
        }
    }
}
