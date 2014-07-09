using System;
using System.Data;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework.Providers;
using Microsoft.ApplicationBlocks.Data;

namespace Nevoweb.DNN.NBrightBuy.Components.SqlDataProvider
{

	/// -----------------------------------------------------------------------------
	/// <summary>
	/// SQL Server implementation of the abstract DataProvider class
	/// </summary>
	/// -----------------------------------------------------------------------------
	public class SqlDataProvider : DataProvider
	{

		#region Private Members

		private const string ProviderType = "data";
		private const string ModuleQualifier = "NBrightBuy_";

		private readonly ProviderConfiguration _providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
		private readonly string _connectionString;
		private readonly string _providerPath;
		private readonly string _objectQualifier;
		private readonly string _databaseOwner;

		#endregion

		#region Constructors

		public SqlDataProvider()
		{

			// Read the configuration specific information for this provider
			Provider objProvider = (Provider)(_providerConfiguration.Providers[_providerConfiguration.DefaultProvider]);

			// Read the attributes for this provider

			//Get Connection string from web.config
			_connectionString = Config.GetConnectionString();

			if (string.IsNullOrEmpty(_connectionString))
			{
				// Use connection string specified in provider
				_connectionString = objProvider.Attributes["connectionString"];
			}

			_providerPath = objProvider.Attributes["providerPath"];

			_objectQualifier = objProvider.Attributes["objectQualifier"];
			if (!string.IsNullOrEmpty(_objectQualifier) && _objectQualifier.EndsWith("_", StringComparison.Ordinal) == false)
			{
				_objectQualifier += "_";
			}

			_databaseOwner = objProvider.Attributes["databaseOwner"];
			if (!string.IsNullOrEmpty(_databaseOwner) && _databaseOwner.EndsWith(".", StringComparison.Ordinal) == false)
			{
				_databaseOwner += ".";
			}

		}

		#endregion

		#region Properties

		public string ConnectionString
		{
			get
			{
				return _connectionString;
			}
		}

		public string ProviderPath
		{
			get
			{
				return _providerPath;
			}
		}

		public string ObjectQualifier
		{
			get
			{
				return _objectQualifier;
			}
		}

		public string DatabaseOwner
		{
			get
			{
				return _databaseOwner;
			}
		}

		private string NamePrefix
		{
			get { return DatabaseOwner + ObjectQualifier + ModuleQualifier; }
		}

		#endregion

		#region Private Methods

		private static object GetNull(object Field)
		{
			return DotNetNuke.Common.Utilities.Null.GetNull(Field, DBNull.Value);
		}

		#endregion
        
        #region Public Methods

        public override IDataReader GetList(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string sqlOrderBy = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0, string typeCodeLang = "", string lang = "")
        {
            return SqlHelper.ExecuteReader(ConnectionString, DatabaseOwner + ObjectQualifier + "NBrightBuy_GetList", portalId, moduleId, typeCode, sqlSearchFilter, sqlOrderBy, returnLimit, pageNumber, pageSize, recordCount, typeCodeLang, lang);
        }

        public override int GetListCount(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string typeCodeLang = "", string lang = "")
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(ConnectionString, DatabaseOwner + ObjectQualifier + "NBrightBuy_GetListCount", portalId, moduleId, typeCode, sqlSearchFilter, typeCodeLang, lang));
        }

        public override IDataReader Get(int itemId, string typeCodeLang = "", string lang = "")
        {
            return SqlHelper.ExecuteReader(ConnectionString, DatabaseOwner + ObjectQualifier + "NBrightBuy_Get", itemId, typeCodeLang, lang);
        }

        public override int Update(int ItemId, int PortalId, int ModuleId, String TypeCode, String XMLData, String GUIDKey, DateTime ModifiedDate, String TextData, int XrefItemId, int ParentItemId, int UserId, string Lang)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(ConnectionString, DatabaseOwner + ObjectQualifier + "NBrightBuy_Update", ItemId, PortalId, ModuleId, TypeCode, XMLData, GUIDKey, ModifiedDate, TextData, XrefItemId, ParentItemId, UserId, Lang));
        }

        public override void Delete(int ItemID)
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, DatabaseOwner + ObjectQualifier + "NBrightBuy_Delete", ItemID);
        }

        public override void CleanData()
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, DatabaseOwner + ObjectQualifier + "NBrightBuy_CleanData");
        }

        public override String GetSqlxml(string commandText)
        {
            return Convert.ToString(SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text ,commandText));
        }

        public override IDataReader GetDnnUsers(int portalId, string sqlSearchFilter = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0)
        {
            return SqlHelper.ExecuteReader(ConnectionString, DatabaseOwner + ObjectQualifier + "NBrightBuy_GetDNNUsers", portalId, sqlSearchFilter, returnLimit, pageNumber, pageSize, recordCount);
        }

        public override int GetDnnUsersCount(int portalId, string sqlSearchFilter = "")
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(ConnectionString, DatabaseOwner + ObjectQualifier + "NBrightBuy_GetDNNUsersCount", portalId, sqlSearchFilter));
        }


        #endregion




	}

}