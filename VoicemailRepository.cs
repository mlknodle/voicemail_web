using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using KineticConsole.Dto;
using System.Linq;
using KineticConsole.Extensions;

namespace KineticConsole.Repositories
{
	public class VoicemailRepository
	{
		private readonly String _conStrIvxsvr;


		public VoicemailRepository(String conStrIvxsvr)
		{
			_conStrIvxsvr = conStrIvxsvr;
		}


		public async Task CreateSiteName(FacilityArgs args)
		{
			VmSiteNameDto dto = BuildSiteNameDto(args);

			await CreateSiteName(dto);
		}

		public async Task CreateSiteName(VmSiteNameDto dto)
		{
			await SqlExecute.InvokeProcSaveAsync(_conStrIvxsvr, "uspCreateSiteName", dto);
		}

		public async Task<List<VmSiteNameDto>> LoadAllSiteNames()
		{
			IEnumerable<VmSiteNameDto> result = await SqlParsedExecute.InvokeProcAsync<VmSiteNameDto>(_conStrIvxsvr, "uspLoadAllSiteNames");

			return result.ToList();
		}

		public async Task CreateMailboxSite(VmMailboxSite dto)
		{
			await SqlExecute.InvokeProcSaveAsync(_conStrIvxsvr, "uspCreateMailboxSite", dto);
		}

		public async Task CreateMailboxSite(String siteId)
		{
			VmMailboxSite dto = new VmMailboxSite();
			dto.SiteId = siteId;
			dto.GroupId = siteId;
			dto.IsGroup = true;

			await CreateMailboxSite(dto);
		}

		public async Task ModifyBroadcastGroup(VmBroadcastGroupModify dto)
		{
			await SqlExecute.InvokeProcSaveAsync(_conStrIvxsvr, "uspModifyBroadcastGroup", dto);
		}

		public async Task CreateBroadcastGroup(VmBroadcastGroupCreate dto)
		{
			await SqlExecute.InvokeProcSaveAsync(_conStrIvxsvr, "uspCreateBroadcastGroup", dto);
		}

		public async Task CreateBroadcastGroup(VmBroadcastGroup dto)
		{
			await SqlExecute.InvokeProcSaveAsync(_conStrIvxsvr, "uspCreateBroadcastGroup", dto);
		}

		public async Task CreateDefaultBroadcastGroups(String siteId)
		{
			VmBroadcastGroup group1 = new VmBroadcastGroup();
			group1.GroupId = 1;
			group1.GroupName = "Broadcast GP1";
			group1.SiteId = siteId;

			await CreateBroadcastGroup(group1);

			VmBroadcastGroup group2 = new VmBroadcastGroup();
			group2.GroupId = 2;
			group2.GroupName = "Broadcast GP2";
			group2.SiteId = siteId;

			await CreateBroadcastGroup(group2);

			VmBroadcastGroup group3 = new VmBroadcastGroup();
			group3.GroupId = 3;
			group3.GroupName = "Broadcast GP3";
			group3.SiteId = siteId;

			await CreateBroadcastGroup(group3);
		}

		public async Task<List<VmBroadcastGroup>> LoadBroadcastGroups(String siteId)
		{
			List<SqlParameter> parameters = new List<SqlParameter>();
			parameters.AddSqlParam("@SiteId", siteId);

			IEnumerable<VmBroadcastGroup> result = await SqlParsedExecute.InvokeProcAsync<VmBroadcastGroup>(_conStrIvxsvr, "uspLoadBroadcastGroups", parameters);

			return result.ToList();
		}

		public async Task<List<VmMailboxConfig>> LoadMailboxConfigs(String siteId)
		{
			List<SqlParameter> parameters = new List<SqlParameter>();
			parameters.AddSqlParam("@SiteId", siteId);

			IEnumerable<VmMailboxConfig> result = await SqlParsedExecute.InvokeProcAsync<VmMailboxConfig>(_conStrIvxsvr, "uspLoadMailboxConfigs", parameters);

			return result.ToList();
		}

		public async Task CreateMailboxConfig(VmMailboxConfig dto)
		{
			await SqlExecute.InvokeProcSaveAsync(_conStrIvxsvr, "uspCreateMailboxConfig", dto);
		}

		public async Task CreateDefaultMailboxConfigs(String siteId)
		{
			// Default configuration values are stored as rows in the mbxcfg table with site_id "0000000000"
			List<VmMailboxConfig> defConfigValues = await LoadMailboxConfigs("0000000000");

			foreach (VmMailboxConfig defConfig in defConfigValues)
			{
				defConfig.SiteId = siteId;
				await CreateMailboxConfig(defConfig);
			}
		}

		public async Task EnableFacilityVoicemail(FacilityArgs args)
		{
			VmSiteNameDto dto = BuildSiteNameDto(args);
			await EnableFacilityVoicemail(dto);
		}

		public async Task EnableFacilityVoicemail(VmSiteNameDto dto)
		{
			await SqlExecute.InvokeProcSaveAsync(_conStrIvxsvr, "uspEnableFacilityVoicemail", dto);
		}

		public async Task<List<CreateMailboxDto>> LoadMailboxesToCreate(String siteId)
		{
			List<SqlParameter> parameters = new List<SqlParameter>();
			parameters.AddSqlParam("@SiteId", siteId);

			IEnumerable<CreateMailboxDto> result = await SqlParsedExecute.InvokeProcAsync<CreateMailboxDto>(_conStrIvxsvr, "uspLoadMailboxesToCreate", parameters);

			return result?.ToList();
		}

		public async Task<List<String>> LoadAllMailboxIds()
		{
			IEnumerable<MailboxIdDto> result = await SqlParsedExecute.InvokeProcAsync<MailboxIdDto>(_conStrIvxsvr, "uspLoadAllMailboxIds");

			return result.Select(r => r.MailboxId.Trim()).ToList();
		}

		public async Task<List<CreateMailboxDto>> CreateSiteMailboxes(String siteId)
		{
			List<CreateMailboxDto> mailboxesToCreate = await LoadMailboxesToCreate(siteId);

			if (mailboxesToCreate == null || mailboxesToCreate.Count == 0)
				return new List<CreateMailboxDto>();

			List<String> existingMailboxIds = await LoadAllMailboxIds();

			// Generate random mailbox IDs for each mailbox record being created.
			Random r = new Random();

			for (Int32 i = 0; i < mailboxesToCreate.Count; i++)
			{
				Int32 maxAttempts = 10;
				Int32 curAttempt = 0;

				while (curAttempt < maxAttempts)
				{
					String newMailboxId = r.Next(100000, 999999).ToString();

					if (existingMailboxIds.All(e => e != newMailboxId))
					{
						mailboxesToCreate[i].MailboxId = newMailboxId;
						existingMailboxIds.Add(newMailboxId);
						break;
					}

					curAttempt++;
				}
			}

			return await CreateMailboxes(mailboxesToCreate);
		}

		public async Task<List<CreateMailboxDto>> CreateMailboxes(List<CreateMailboxDto> mailboxes)
		{
			DataTable mailboxesDt = new DataTable();
			mailboxesDt.Columns.Add(new DataColumn("MailboxId", typeof(String)));
			mailboxesDt.Columns.Add(new DataColumn("CosId", typeof(Int32)));
			mailboxesDt.Columns.Add(new DataColumn("SiteId", typeof(String)));
			mailboxesDt.Columns.Add(new DataColumn("UserId", typeof(String)));
			mailboxesDt.Columns.Add(new DataColumn("UserName", typeof(String)));
			mailboxesDt.Columns.Add(new DataColumn("UserPassword", typeof(String)));
			mailboxesDt.Columns.Add(new DataColumn("CreateDateStr", typeof(String)));
			mailboxesDt.Columns.Add(new DataColumn("CreateTimeStr", typeof(String)));
			mailboxesDt.Columns.Add(new DataColumn("BroadcastGroupId", typeof(Int32)));
			mailboxesDt.Columns.Add(new DataColumn("BroadcastGroupName", typeof(String)));

			foreach (CreateMailboxDto dto in mailboxes)
			{
				DataRow row = mailboxesDt.NewRow();

				row["MailboxId"] = dto.MailboxId;
				row["CosId"] = dto.CosId;
				row["SiteId"] = dto.SiteId;
				row["UserId"] = dto.UserId;
				row["UserName"] = dto.UserName;
				row["UserPassword"] = dto.UserPassword;
				row["CreateDateStr"] = dto.CreateDateStr;
				row["CreateTimeStr"] = dto.CreateTimeStr;
				row["BroadcastGroupId"] = dto.BroadcastGroupId;
				row["BroadcastGroupName"] = dto.BroadcastGroupName;

				mailboxesDt.Rows.Add(row);
			}

			List<SqlParameter> parameters = new List<SqlParameter>();
			parameters.AddSqlParamFromDt("@Mailboxes", mailboxesDt);

			IEnumerable<CreateMailboxDto> result = await SqlParsedExecute.InvokeProcAsync<CreateMailboxDto>(_conStrIvxsvr, "uspCreateMailboxes", parameters);

			return result.ToList();
		}

		public async Task<VmMailboxDto> LoadInmateAccountMailbox(String pin, String siteId)
		{
			List<SqlParameter> parameters = new List<SqlParameter>();
			parameters.AddSqlParam("@Pin", pin);
			parameters.AddSqlParam("@SiteId", siteId);

			VmMailboxDto result = await SqlParsedExecute.InvokeProcSingleAsync<VmMailboxDto>(_conStrIvxsvr, "uspLoadInmateAccountMailbox", parameters);

			DateTime createDateTime;
			if (DateTime.TryParseExact($"{result.CreateDate} {result.CreateTime}", "yyyyMMdd HHmmss", null, DateTimeStyles.None, out createDateTime))
				result.CreateDateTime = createDateTime;

			DateTime loginDateTime;
			if (DateTime.TryParseExact($"{result.LoginDate} {result.LoginTime}", "yyyyMMdd HHmmss", null, DateTimeStyles.None, out loginDateTime))
				result.LoginDateTime = loginDateTime;

			return result;
		}

		public async Task ModifyMailbox(VmMailboxModifyDto dto)
		{
			await SqlExecute.InvokeProcSaveAsync(_conStrIvxsvr, "uspModifyMailbox", dto);
		}

		public async Task ModifyMailboxConfig(VmMailboxConfigModifyDto dto)
		{
			await SqlExecute.InvokeProcSaveAsync(_conStrIvxsvr, "uspModifyMailboxConfig", dto);
		}

		private static VmSiteNameDto BuildSiteNameDto(FacilityArgs args)
		{
			VmSiteNameDto dto = new VmSiteNameDto();
			dto.SiteId = args.SiteId;
			dto.City = args.City;
			dto.Contact = String.Empty;
			dto.Email = String.Empty;
			dto.Phone = args.Telephone;
			dto.SiteName = args.FacilityName;
			dto.State = args.State;
			dto.StreetAddress = args.Address;
			dto.Zip = args.Zip;

			return dto;
		}
	}
}