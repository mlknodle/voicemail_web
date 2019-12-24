using System;

namespace KineticConsole.Dto
{
	public class VmBroadcastGroup
	{
		public Decimal GroupId { get; set; } // numeric 9
		public String GroupName { get; set; } // char 15
		public String SiteId { get; set; } // char 10
	}
	
	
	public class VmBroadcastGroupCreate
	{
		[Required]
		[StringLength(15, MinimumLength = 1)]
		public String GroupName { get; set; }

		[Required]
		[StringLength(10, MinimumLength = 10)]
		public String SiteId { get; set; }
	}
	
	
	public class VmBroadcastGroupModify
	{
		[Required]
		[Integer, Range(1, 999999999)]
		public Decimal GroupId { get; set; }

		[Required]
		[StringLength(15, MinimumLength = 1)]
		public String GroupName { get; set; }

		[Required]
		[StringLength(10, MinimumLength = 10)]
		public String SiteId { get; set; }
	}
	
	
	public class VmMailboxConfig
	{
		public Boolean AdminApproval { get; set; }
		public Boolean AutoSave { get; set; }
		public Boolean Broadcast { get; set; }
		public Decimal CosId { get; set; } // numeric 9
		public String CosName { get; set; } // char 15
		public Boolean DelRecFile { get; set; }
		public Boolean ExtPayment { get; set; }
		public Decimal MaxFeeRetrieve { get; set; } // numeric 3
		public Decimal MaxFeeStore { get; set; } // numeric 3
		public Decimal MaxListenCnt { get; set; } // numeric 2
		public Decimal MaxLockoutTime { get; set; } // numeric 2
		public Decimal MaxMsgLen { get; set; } // numeric 3
		public Decimal MaxMsgs { get; set; } // numeric 3
		public Decimal MaxRetTimeNew { get; set; } // numeric 2
		public Decimal MaxRetTimeSaved { get; set; } // numeric 2
		public Decimal MaxSaved { get; set; } // numeric 3
		public Decimal MaxStorageSpace { get; set; } // numeric 5
		public Boolean PendingEnable { get; set; }
		public Boolean RecGreetEnable { get; set; }
		public Boolean RecGreetOnceEnable { get; set; }
		public Boolean RecNameEnable { get; set; }
		public Boolean RecNameOnceEnable { get; set; }
		public String SiteId { get; set; }
	}
	
	
	public class FacilityArgs
	{
		public Boolean AddGuardCheckNum { get; set; } = false;
		public String Address { get; set; } // nvarchar(100)
		public String Address2 { get; set; } // nvarchar(100)
		public Boolean Addvmaccessnum { get; set; } = false;
		public Boolean Bkna { get; set; } = false;
		public String City { get; set; } // nvarchar(100)
		public String Comments { get; set; } // ntext(16)
		public String Country { get; set; } = String.Empty; // char(50)
		public String County { get; set; } = String.Empty; // char(32)
		public Int32 DefPin { get; set; }
		public Int32 DefaultFreeCallCount { get; set; }
		public Int32 DefaultFreeCallMoney { get; set; }
		public Int32 DefaultFreeCallTime { get; set; }
		public List<Int32> DomainIds { get; set; } = new List<Int32>();
		public List<Int32> DomainIdsOld { get; set; } = new List<Int32>(); // Old domain IDs - property only used for edit facility
		public Boolean Exactmatch { get; set; }
		public Boolean Exposepwrx { get; set; } = false;
		public String FacilityName { get; set; } // nvarchar(200)
		public String Fax { get; set; } // nvarchar(100)

		/// <summary>
		/// Force Prompt
		/// </summary>
		public Boolean Frcaospmt { get; set; }
		public Boolean FreeCallsEnabled { get; set; }
		public String GuardCheckNum { get; set; } = String.Empty; // Char(16)
		public String GuardCheckTmClass { get; set; } = "0"; // Char(1)
		public String GuardCheckTreatment { get; set; } = "00"; // Char(2)
		/// <summary>
																/// Messages on hold
																/// </summary>
		public Boolean Imtohdpmt { get; set; }
		public Int32 MaxPin { get; set; }
		public Int32 MinPin { get; set; }
		public String Ocutthrun { get; set; } = String.Empty;
		public Boolean Oossden { get; set; }
		public String Operator2 { get; set; } = String.Empty;
		public String Osbnumber { get; set; } = String.Empty;
		/// <summary>
		/// Allow passive accept
		/// </summary>
		public Boolean PassAccpt { get; set; }
		public Int32 Pin2dig { get; set; }
		// ReSharper disable once RedundantDefaultMemberInitializer
		private Int32 _pinBalanceLimit = 0; // Numeric(10)
		public Int32? Pinbalancelimit
		{
			get { return _pinBalanceLimit; }
			set
			{
				// ReSharper disable once MergeConditionalExpression
				_pinBalanceLimit = (value == null) ? 0 : (Int32)value * 100;
			}
		}
		public Boolean Pindebit { get; set; } = false;
		public Boolean Pwrestrict { get; set; }
		public Boolean RequireDOB { get; set; } = false;
		public Boolean Requiredocid { get; set; } = false;
		public Boolean Rndvcechk { get; set; }
		public Boolean RndvcechkByPin { get; set; } = false;
		/// <summary>
		/// Block simultaneous calls
		/// </summary>
		public Boolean Rxsimltcn { get; set; }
		public Boolean ShowPrivatePin { get; set; } = false;
		public String SiteId { get; set; } // nvarchar(20)
		public String State { get; set; } // nvarchar(100) ... should be char(2) but isn't
		public String Telephone { get; set; } // nvarchar(100)
		public Int32 Timezone { get; set; }
		public Int32 Twsen { get; set; }
		public Int32 Twtmr5sen { get; set; }
		public Boolean UpdateKiosk { get; set; } = false;
		public Boolean UsePins { get; set; } = false;
		public Boolean Usedst { get; set; } = true;
		public Boolean Userfid { get; set; }
		public String Vmaccesscpi { get; set; } = String.Empty; // char(2)
		public String Vmaccessnum { get; set; } = String.Empty; // char(16)
		public String Vmaccesstreatment { get; set; } = String.Empty; // char(2)
		public Boolean Vmenable { get; set; } = false;
		public Int32 VpStatus { get; set; }
		public String Zip { get; set; } // nvarchar(100)

		/// <summary>
		/// Whether or not voice print verification is required.
		/// This value will be used for any changes that need to be made to PINID records.
		/// </summary>
		public Boolean Vprequired => (this.VpStatus == 1 || this.VpStatus == 2);

		public Boolean ForcePins { get; set; }
	}
	
	
	/// <summary>
	/// Model for the table [ivxsvr].[dbo].[SiteNames]
	/// </summary>
	public class VmSiteNameDto
	{
		public String City { get; set; } // Char 16
		public String Contact { get; set; } = String.Empty; // char 24
		public String Email { get; set; } = String.Empty; // char 40
		public String Phone { get; set; } // char 20
		public String SiteId { get; set; } // char 10
		public String SiteName { get; set; } // char 32
		public String State { get; set; } // char 2
		public String StreetAddress { get; set; } // char 32
		public String Zip { get; set; } // char 9
	}
	
	
	public class CreateMailboxDto
	{
		public String MailboxId { get; set; } // char 15
		public Decimal CosId { get; set; } // numeric 9
		public String SiteId { get; set; } // char 10
		public String UserId { get; set; } // char 15
		public String UserName { get; set; } // char 40
		public String UserPassword { get; set; } // char 32
		public String CreateDateStr { get; set; } // char 8
		public String CreateTimeStr { get; set; } // char 6
		public Decimal BroadcastGroupId { get; set; } // numeric 9
		public String BroadcastGroupName { get; set; } // char 15
	}
	
	
	/// <summary>
	/// There's a reason for this, I swear...
	/// </summary>
	public class MailboxIdDto
	{
		public String MailboxId { get; set; }
	}
	
	
	public class VmMailboxDto
	{
		public String MailboxId { get; set; } // char 15
		public Decimal CosId { get; set; } // numeric 9
		public String SiteId { get; set; } // char 10
		public String UserId { get; set; } // char 15
		public String UserName { get; set; } // char 40
		public String UserPassword { get; set; } // char 32
		public String UserGreetRecFilename { get; set; } // char 40
		public String UserNameRecFilename { get; set; } // 40
		public String CreateDate { get; set; } // char 8
		public String CreateTime { get; set; } // char 6
		public DateTime? CreateDateTime { get; set; }
		public Decimal ClientLock { get; set; } // numeric 12
		public Decimal ClientLockoutTime { get; set; } // numeric 12
		public Decimal LockState { get; set; } // numeric 2
		public Decimal ClientLockTime { get; set; } // numeric 12
		public Decimal Balance { get; set; } // numeric 10
		public Boolean NameRecorded { get; set; }
		public Boolean GreetRecorded { get; set; }
		public String RecPassword { get; set; } // char 32
		public String LoginDate { get; set; } // char 8
		public String LoginTime { get; set; } // char 6
		public DateTime? LoginDateTime { get; set; }
		public Decimal? GroupId { get; set; } // numeric 9
		public String GroupName { get; set; } // char 15
	}
	
	
	public class VmMailboxModifyDto
	{
		[Required]
		[StringLength(15)]
		public String MailboxId { get; set; }

		[Required]
		[StringLength(15)]
		public String UserId { get; set; }

		[Required]
		[StringLength(40)]
		public String UserName { get; set; }

		[Required]
		[StringLength(32)]
		public String UserPassword { get; set; }

		[StringLength(32)]
		public String RecPassword { get; set; }

		[Required]
		public Decimal ClientLockoutTime { get; set; }

		[Required]
		public Decimal CosId { get; set; }

		[Required]
		public Decimal GroupId { get; set; }

		[Required]
		public Decimal LockState { get; set; }
	}
	
	
	public class VmMailboxConfigModifyDto
	{
		[Required]
		public Boolean AdminApproval { get; set; }

		[Required]
		public Boolean AutoSave { get; set; }

		[Required]
		public Boolean Broadcast { get; set; }

		[Required]
		[Integer]
		public Decimal CosId { get; set; }

		[Required]
		[StringLength(15)]
		public String CosName { get; set; }

		[Required]
		public Boolean ExtPayment { get; set; }

		[Required]
		[Integer, Range(0, 999)]
		public Decimal MaxFeeRetrieve { get; set; }

		[Required]
		[Integer, Range(0, 999)]
		public Decimal MaxFeeStore { get; set; }

		[Required]
		[Integer, Range(0, 99)]
		public Decimal MaxListenCnt { get; set; }

		[Required]
		[Integer, Range(0, 99)]
		public Decimal MaxLockoutTime { get; set; }

		[Required]
		[Integer, Range(0, 999)]
		public Decimal MaxMsgLen { get; set; }

		[Required]
		[Integer, Range(0, 999)]
		public Decimal MaxMsgs { get; set; }

		[Required]
		[Integer, Range(0, 99)]
		public Decimal MaxRetTimeNew { get; set; }

		[Required]
		[Integer, Range(0, 99)]
		public Decimal MaxRetTimeSaved { get; set; }

		[Required]
		[Integer, Range(0, 999)]
		public Decimal MaxSaved { get; set; }

		[Required]
		[Integer, Range(0, 99999)]
		public Decimal MaxStorageSpace { get; set; }

		[Required]
		public Boolean PendingEnable { get; set; }

		[Required]
		public Boolean RecGreetEnable { get; set; }

		[Required]
		public Boolean RecGreetOnceEnable { get; set; }

		[Required]
		public Boolean RecNameEnable { get; set; }

		[Required]
		public Boolean RecNameOnceEnable { get; set; }

		[Required]
		public String SiteId { get; set; }
	}
}
