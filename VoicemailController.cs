using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KineticConsole.Attributes;
using KineticConsole.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using KineticConsole.DocGen;
using KineticConsole.Dto;
using Newtonsoft.Json;
using NLog;

namespace KineticConsole.Controllers
{
	[Route("voicemail")]
	public class VoicemailController : Controller
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IUoW _uow;


		public VoicemailController(IUoW uow)
		{
			_uow = uow;
		}


		[HttpGet, Route("mailboxes")]
		[Authorize, RequirePermissions(KCPermissions.Inmates_Edit)]
		public async Task<IActionResult> LoadVoiceMailbox(String pin, String siteId)
		{
			try
			{
				if (String.IsNullOrWhiteSpace(pin) || String.IsNullOrWhiteSpace(siteId))
					return BadRequest();

				VmMailboxDto result = await _uow.VoicemailRepository.LoadInmateAccountMailbox(pin, siteId);

				return Ok(result);
			}
			catch (Exception ex)
			{
				ex.Data["pin"] = pin;
				ex.Data["siteId"] = siteId;
				_logger.Error(ex, "Failed to load inmate account voice mailbox.");
				return StatusCode((Int32)HttpStatusCode.InternalServerError, "Error loading inmate account voice mailbox.");
			}
		}

		[HttpGet, Route("mailboxes/create")]
		[Authorize, RequirePermissions(KCPermissions.Facilities_Edit)]
		public async Task<IActionResult> CreateMailboxes(String siteId)
		{
			try
			{
				if (String.IsNullOrWhiteSpace(siteId))
					return BadRequest();

				List<CreateMailboxDto> result = await _uow.VoicemailRepository.CreateSiteMailboxes(siteId);

				return Ok(result);
			}
			catch (Exception ex)
			{
				ex.Data["siteId"] = siteId;
				_logger.Error(ex, "Create site mailboxes failed.");
				return StatusCode((Int32)HttpStatusCode.InternalServerError, "Error creating site mailboxes.");
			}
		}

		[HttpPost, Route("mailboxes/created-download-excel")]
		[Authorize, RequirePermissions(KCPermissions.Facilities_Edit)]
		public async Task<IActionResult> DownloadCreatedMailboxesExcel([FromBody] List<CreateMailboxDto> mailboxes)
		{
			try
			{
				if (mailboxes?.Count > 0)
				{
					CreatedMailboxesDocGen<CreateMailboxDto> docGen = new CreatedMailboxesDocGen<CreateMailboxDto>();
					docGen.Mailboxes = mailboxes;

					MemoryStream stream = docGen.GenerateExcel();
					stream = new MemoryStream(stream.ToArray());

					BinaryReader br = new BinaryReader(stream);
					Byte[] file = br.ReadBytes((Int32)stream.Length);

					return Ok(await _uow.ReportsRepository.SaveReport("Created Mailboxes", file, "xlsx"));
				}

				return BadRequest();
			}
			catch (Exception ex)
			{
				ex.Data["mailboxes"] = JsonConvert.SerializeObject(mailboxes);
				_logger.Error(ex, "Created mailboxes excel download failed.");
				return StatusCode((Int32)HttpStatusCode.InternalServerError, "Error downloading excel for created mailboxes.");
			}
		}

		[HttpPost, Route("mailboxes/save")]
		[Authorize, RequirePermissions(KCPermissions.Inmates_Edit)]
		public async Task<IActionResult> SaveMailboxChanges([FromBody] VmMailboxModifyDto mailbox)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await _uow.VoicemailRepository.ModifyMailbox(mailbox);

					return Ok();
				}

				return BadRequest(ModelState);
			}
			catch (Exception ex)
			{
				ex.Data["mailbox"] = JsonConvert.SerializeObject(mailbox);
				_logger.Error(ex, "Failed to save mailbox changes.");
				return StatusCode((Int32)HttpStatusCode.InternalServerError, "Error modifying mailbox.");
			}
		}

		[HttpGet, Route("broadcast-groups")]
		[Authorize, RequirePermissions(KCPermissions.Inmates_Edit)]
		public async Task<IActionResult> LoadBroadcastGroups(String siteId)
		{
			try
			{
				if (String.IsNullOrWhiteSpace(siteId))
					return BadRequest();

				List<VmBroadcastGroup> result = await _uow.VoicemailRepository.LoadBroadcastGroups(siteId);

				return Ok(result);
			}
			catch (Exception ex)
			{
				ex.Data["siteId"] = siteId;
				_logger.Error(ex, "Failed to load site voicemail broadcast groups.");
				return StatusCode((Int32)HttpStatusCode.InternalServerError, "Error loading site voicemail broadcast groups.");
			}
		}

		[HttpPost, Route("broadcast-groups/create")]
		[Authorize, RequirePermissions(KCPermissions.Inmates_Edit)]
		public async Task<IActionResult> CreateBroadcastGroup([FromBody] VmBroadcastGroupCreate broadcastGroup)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await _uow.VoicemailRepository.CreateBroadcastGroup(broadcastGroup);

					return Ok();
				}

				_logger.Warn($"Bad request for create Broadcast Groups.\r\n{JsonConvert.SerializeObject(ModelState)}");
				return BadRequest(ModelState);
			}
			catch (Exception ex)
			{
				ex.Data["broadcastGroup"] = JsonConvert.SerializeObject(broadcastGroup);
				_logger.Error(ex, "Failed to load site voicemail broadcast groups.");
				return StatusCode((Int32)HttpStatusCode.InternalServerError, "Error loading site voicemail broadcast groups.");
			}
		}

		[HttpPost, Route("broadcast-groups/save")]
		[Authorize, RequirePermissions(KCPermissions.Inmates_Edit)]
		public async Task<IActionResult> SaveBroadcastGroupChanges([FromBody] VmBroadcastGroupModify broadcastGroup)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await _uow.VoicemailRepository.ModifyBroadcastGroup(broadcastGroup);

					return Ok();
				}

				_logger.Warn($"Bad request for modify Broadcast Group.\r\n{JsonConvert.SerializeObject(ModelState)}");
				return BadRequest(ModelState);
			}
			catch (Exception ex)
			{
				ex.Data["broadcastGroup"] = JsonConvert.SerializeObject(broadcastGroup);
				_logger.Error(ex, "Failed to modify broadcast group.");
				return StatusCode((Int32)HttpStatusCode.InternalServerError, "Error modifying broadcast group.");
			}
		}

		[HttpGet, Route("mailbox-configs")]
		[Authorize, RequirePermissions(KCPermissions.Inmates_View)]
		public async Task<IActionResult> LoadMailboxConfigs(String siteId)
		{
			try
			{
				if (String.IsNullOrWhiteSpace(siteId))
					return BadRequest();

				List<VmMailboxConfig> result = await _uow.VoicemailRepository.LoadMailboxConfigs(siteId);

				return Ok(result);
			}
			catch (Exception ex)
			{
				ex.Data["siteId"] = siteId;
				_logger.Error(ex, "Failed to load voice mailbox configs.");
				return StatusCode((Int32)HttpStatusCode.InternalServerError, "Error loading voice mailbox configs.");
			}
		}

		[HttpPost, Route("mailbox-configs/save")]
		[Authorize, RequirePermissions(KCPermissions.Inmates_Edit)]
		public async Task<IActionResult> SaveMailboxConfigChanges([FromBody] VmMailboxConfigModifyDto mailboxConfig)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await _uow.VoicemailRepository.ModifyMailboxConfig(mailboxConfig);

					return Ok();
				}

				_logger.Warn($"Bad request for save mailbox config changes.\r\n{JsonConvert.SerializeObject(ModelState)}");
				return BadRequest(ModelState);
			}
			catch (Exception ex)
			{
				ex.Data["mailboxConfig"] = JsonConvert.SerializeObject(mailboxConfig);
				_logger.Error(ex, "Failed to save mailbox config changes.");
				return StatusCode((Int32)HttpStatusCode.InternalServerError, "Error modifying mailbox config.");
			}
		}
	}
}