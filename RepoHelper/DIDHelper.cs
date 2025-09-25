using Dapper;
using KBN.Models.DBModels;
using KBN.Models.DIDModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Data;
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

        public DIDViewModel GetData(RangeViewModel? filter = null, int? pageNumber = 0, int? pageSize = 10)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);


                var parameters = new DynamicParameters();
                parameters.Add("@DID", filter?.DID);
                parameters.Add("@City", filter?.City);
                parameters.Add("@Country", filter?.Country);
                parameters.Add("@PageNumber", pageNumber ?? 0);
                parameters.Add("@PageSize", pageSize ?? 10);
                parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

                var data = connection.Query<DID_Assigning>("dbo.GetDIDsData", parameters, commandType: CommandType.StoredProcedure).ToList();
                int totalCount = parameters.Get<int>("@TotalCount");
                // Return view model
                return new DIDViewModel
                {
                    DIDList = data,
                    TotalCount = totalCount
                };

                }catch (Exception ex)
            {
                _logger.LogError("Error while fetching DID data: " + ex);
                throw new Exception("Exception while fetching the DID Data", ex);
            }
        }

        public (bool Success, Dictionary<long, string> Errors) AddData(string RecordBy , List<RangeViewModel> data)
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
                                errors[d.DID.Value] = "DID must be 11 digits";
                                continue;
                               
                            }
                            if (existingDIDs.Contains(d.DID.Value))
                            {
                                errors[d.DID.Value] = "DID already exists";
                                continue;
                            }
                            d.RecordedBy = RecordBy;
                            validData.Add(d);
                        }

                        if (validData.Any())
                        {
                            string sql = @"
                        INSERT INTO DID_Assigning (DID, City, Country ,RecordAt ,RecoredBy)
                        VALUES (@DID, @City, @Country ,GetDate(),@RecordedBy);";
                          
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

                //string deleteSql = @"DELETE FROM DID_Assigning WHERE DID = @DID";
                string deleteSql = @" UPDATE DID_Assigning SET IsVoid = 1, DeletedOn = GetDate() WHERE DID = @DID";

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
                if (!data.DID.HasValue)
                {
                    return new ResultModel { Success = false, Message = "please insert some value in this" };
                }
                if (data.DID.HasValue && data.DID.ToString().Length != 11)
                {
                    Console.WriteLine("the length of DID in Update id ", data.DID.ToString().Length!);
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
                            return new ResultModel { Success = false, Message = "DID already present !" };
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
