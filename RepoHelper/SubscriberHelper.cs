using BCrypt.Net;
using Dapper;
using Humanizer;
using KBN.Models.DIDModel;
using KBN.Models.SubscriberModels;
using KBN.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;                    
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace KBN.RepoHelper
{
    public class SubscriberHelper
    {
        private readonly string _connectionString;
        private readonly ILogger<DIDHelper> _logger;
        public SubscriberHelper(IConfiguration configuration, ILogger<DIDHelper> logger) 
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public SubscriberViewModel GetData(Subscriber? filter = null, int? pageNumber = 0, int? pageSize = 5)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@Username", filter?.username);
                parameters.Add("@CustomerName", filter?.customer_name);
                parameters.Add("@PageNumber", pageNumber ?? 0);
                parameters.Add("@PageSize", pageSize ?? 5);
                parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

                var data = connection.Query<Subscriber>(
                    "dbo.GetSubscriberData",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                int totalCount = parameters.Get<int>("@TotalCount");

                return new SubscriberViewModel
                {
                    subscriberList = data,
                    count = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting subscriber data: " + ex);
                throw new Exception("Exception while getting subscriber data", ex);
            }
        }

        public bool isExist_Username(string username ,int? id = 0)
        {
            var connection = new SqlConnection(_connectionString);
            string namechecker = @"SELECT COUNT(*) FROM Subscriber WHERE username = @name AND is_void = 0
                                      AND (@Id IS NULL OR id != @Id);";
            int count = connection.ExecuteScalar<int>(namechecker, new { name = username , Id = (id == 0 ? (int?)null :id) });
            return count > 0;
        }

        public bool isExist_Customername(string? customername)
        {
            try
            {
                if (string.IsNullOrEmpty(customername))
                {
                    return false;
                }
                var connection = new SqlConnection(_connectionString);
                string customerchecker = @"SELECT  CASE  WHEN COUNT(*) > 0 THEN CAST(1 AS BIT)  ELSE CAST(0 AS BIT) END AS IsValid
                                                FROM Customer_Data WHERE name = @name AND sub1 IS NOT NULL AND sub2 IS NOT NULL";

                 return connection.ExecuteScalar<bool>(customerchecker, new { name = customername });
            }
            catch(Exception ex)
            {
                _logger.LogError("Error while checking the customer name : " + ex);
                throw new Exception("Exception while checking the customer name", ex);
            }
        }

        public int Add_Customername(string customername ,int? id)
        {
            try
            {
                var connection = new SqlConnection(_connectionString);
                
                var sql = @" IF EXISTS (SELECT 1 FROM Customer_Data WHERE name = @Name)  BEGIN
                                        UPDATE Customer_Data SET 
                                            sub1 = CASE WHEN sub1 IS NULL THEN @Value ELSE sub1 END,
                                            sub2 = CASE WHEN sub2 IS NULL AND sub1 IS NOT NULL THEN @Value ELSE sub2 END
                                        WHERE name = @Name;
                                         END  
                                    ELSE
                                    BEGIN
                                        INSERT INTO Customer_Data (name, sub1)
                                        VALUES (@Name, @Value);
                                    END";

                  return connection.Execute(sql, new { Name = customername, Value = id });
                
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Adding the customer name : " + ex);
                throw new Exception("Exception while Adding the customer name", ex);
            }
        }

        public ReturnModal Add(Subscriber sb)
        {
            try
            {
                    var errors = new Dictionary<string, string>();
                
                    if (string.IsNullOrWhiteSpace(sb.username))
                        errors["username"] = "Username is required";

                    if (string.IsNullOrWhiteSpace(sb.password))
                        errors["password"] = "Password is required";

                    if (errors.Count > 0)
                    {
                    //return (false, errors);
                    return new ReturnModal
                    {
                        success = false,
                        error = errors
                    };
                    }
                var connection = new SqlConnection(_connectionString);
                  
                        if (isExist_Username(sb.username)) // username check 
                        {
                            errors["username"] = "Username is already taken";
                                return new ReturnModal
                                {
                                    success = false,
                                    error = errors
                                };
                        }

                        sb.password =  Security.Encrypt(sb.password,"munishwar");

                        string InsertSub = @"INSERT INTO Subscriber (username, password)
                                        VALUES (@username, @password) select cast(SCOPE_IDENTITY() as int)";

                         int sub  = connection.QuerySingle<int>(InsertSub ,new { username = sb.username , password = sb.password});

                        if (string.IsNullOrWhiteSpace(sb.customer_name))
                        {
                            return new ReturnModal
                            {
                                success = true,
                                error = errors
                            };
                        }

                        if (isExist_Customername(sb.customer_name)) // cutomer name check 
                        {
                            errors["customer_name"] = "Customer name is Already Occupied";
                            return new ReturnModal
                            {
                                success = false,
                                error = errors
                            };
                        }

                        Add_Customername(sb.customer_name , sub);
                        return new ReturnModal
                        {
                            success = true,
                            error = errors
                        };

                }
            catch (Exception ex)
            {
                _logger.LogError("error while getting the data from helper " + ex);
                throw new Exception("Exception while getting the data", ex);
            }
        }

        public bool Delete(int id) // For Delete the DID
        {
            try
            {
                var connection = new SqlConnection(_connectionString);

                string deleteSql = @"UPDATE Subscriber SET is_void = 1, voided_at = GetDate() WHERE ID = @ID ;";
                int rowAffected = connection.Execute(deleteSql, new { ID = id });
                Delete_Customername(id);
                return rowAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while deleting the data: " + ex);
                throw new Exception("Exception while deleting the DID Data", ex);
            }
        }
        
        public bool Delete_Customername(int? id)
        {
            try
            {
                var connection = new SqlConnection(_connectionString);
                string deletesql = @"UPDATE customer_data SET sub1 = CASE WHEN sub1 = @ID THEN NULL ELSE sub1 END,
                                    sub2 = CASE WHEN sub2 = @ID THEN NULL ELSE sub2 END WHERE sub1 = @ID OR sub2 = @ID;
                                     UPDATE customer_data SET sub1 = sub2, sub2 = NULL WHERE sub1 IS NULL AND sub2 IS NOT NULL;";


                connection.Execute(deletesql, new { ID = id });
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while deleting the Customername: " + ex);
                throw new Exception("Exception while deleting the Customername ", ex);
            }
        }
        public Subscriber getUpdateList(int id)
        {
            try
            {
                var connection = new SqlConnection(_connectionString);

                string getUpdate = @"SELECT s.id, s.username,s.password, c.name as customer_name
                                        FROM Subscriber s
                                        LEFT JOIN Customer_Data c
                                        ON c.sub1 = s.id OR c.sub2 = s.id where s.id = @Id AND is_void = 0";

                var data = connection.QueryFirstOrDefault<Subscriber>(getUpdate, new { ID = id });
                 data.password = Security.Decrypt(data.password, "munishwar");
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while inserting the data: " + ex);
                throw new Exception("Exception while inserting the DID Data", ex);
            }
        }
        public ReturnModal UpdateData(Subscriber sb)
        {
            try
            {
                        var errors = new Dictionary<string, string>();
                
                        if (string.IsNullOrWhiteSpace(sb.username))
                            errors["username"] = "Username is required";

                        if (string.IsNullOrWhiteSpace(sb.password))
                            errors["password"] = "Password is required";

                        if (errors.Count > 0)
                        {
                            return new ReturnModal
                            {
                                success = false,
                                error = errors
                            };
                         }
                        var connection = new SqlConnection(_connectionString);

                        if (isExist_Username(sb.username ,sb.id)) // username check 
                        {
                            errors["username"] = "Username is already taken";
                            return new ReturnModal
                            {
                                success = false,
                                error = errors
                            };
                        }

                            sb.password =  Security.Encrypt(sb.password,"munishwar");

                            string InsertSub = @"UPDATE Subscriber SET username = @username,  password = @password WHERE id = @Id";

                            connection.Execute(InsertSub ,new { username = sb.username , password = sb.password ,Id = sb.id});

                       
                        Delete_Customername(sb.id);
                       
                        if (isExist_Customername(sb.customer_name)) // cutomer name check 
                        {
                            errors["customer_name"] = "Customer name is Already Occupied";
                            return new ReturnModal
                            {
                                success = false,
                                error = errors
                            };
                        }
                        if (!string.IsNullOrEmpty(sb.customer_name))
                        {
                        Add_Customername(sb.customer_name, sb.id);
                        }
                        return new ReturnModal
                        {
                            success = true,
                            error = errors
                        };
            }
            catch (Exception ex)
            {
                _logger.LogError("error while getting the data from helper " + ex);
                throw new Exception("Exception while getting the data", ex);
            }
        }
    }
}
