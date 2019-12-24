
CREATE PROCEDURE [dbo].[uspCreateBroadcastGroup]
	@GroupId NUMERIC(9) = NULL,
	@GroupName CHAR(15),
	@SiteId CHAR(10)
AS
BEGIN
	IF @GroupId IS NULL
	BEGIN
		SELECT @GroupId = MAX([group_id]) + 1 FROM [dbo].[BroadcastGroups];
	END

	IF NOT EXISTS(SELECT 1 FROM [dbo].[BroadcastGroups] WHERE [group_id] = @GroupId AND [site_id] = @SiteId)
	BEGIN
		IF NOT EXISTS(SELECT 1 FROM [dbo].[BroadcastGroups] WHERE [group_name] = @GroupName AND [site_id] = @SiteId)
		BEGIN
			INSERT INTO [dbo].[BroadcastGroups]([group_id], [group_name], [site_id])
			VALUES (@GroupId, @GroupName, @SiteId);
		END
	END
END
GO



CREATE PROCEDURE [dbo].[uspCreateMailboxConfig]
	@AdminApproval BIT,
	@AutoSave BIT,
	@Broadcast BIT,
	@CosId NUMERIC(9),
	@CosName CHAR(15),
	@DelRecFile BIT,
	@ExtPayment BIT,
	@MaxFeeRetrieve NUMERIC(3),
	@MaxFeeStore NUMERIC(3),
	@MaxListenCnt NUMERIC(2),
	@MaxLockoutTime NUMERIC(2),
	@MaxMsgLen NUMERIC(3),
	@MaxMsgs NUMERIC(3),
	@MaxRetTimeNew NUMERIC(2),
	@MaxRetTimeSaved NUMERIC(2),
	@MaxSaved NUMERIC(3),
	@MaxStorageSpace NUMERIC(5),
	@PendingEnable BIT,
	@RecGreetEnable BIT,
	@RecGreetOnceEnable BIT,
	@RecNameEnable BIT,
	@RecNameOnceEnable BIT,
	@SiteId CHAR(10)
AS
BEGIN
	IF NOT EXISTS(SELECT 1 FROM [dbo].[mbxcfg] WHERE [cos_id] = @CosId AND [site_id] = @SiteId)
	BEGIN
		INSERT INTO [dbo].[mbxcfg]
		(
			[admin_approval], [auto_save], [broadcast], [cos_id], [cos_name], 
			[del_rec_file], [ext_payment], [max_fee_retrieve], [max_fee_store], [max_listen_cnt],
			[max_lockout_time], [max_msg_len], [max_msgs], [max_ret_time_new], [max_ret_time_saved],
			[max_saved], [max_storage_space], [pending_enable], [rec_greet_enable], [rec_greet_once_enable],
			[rec_name_enable], [rec_name_once_enable], [site_id]
		)
		VALUES
		(
			@AdminApproval, @AutoSave, @Broadcast, @CosId, @CosName,
			@DelRecFile, @ExtPayment, @MaxFeeRetrieve, @MaxFeeStore, @MaxListenCnt,
			@MaxLockoutTime, @MaxMsgLen, @MaxMsgs, @MaxRetTimeNew, @MaxRetTimeSaved,
			@MaxSaved, @MaxStorageSpace, @PendingEnable, @RecGreetEnable, @RecGreetOnceEnable,
			@RecNameEnable, @RecNameOnceEnable, @SiteId
		);
	END
END
GO




CREATE PROCEDURE [dbo].[uspCreateMailboxes]
	@Mailboxes Mailbox READONLY
AS
BEGIN

	BEGIN TRY

		CREATE TABLE #CreatedMailboxes
		(
			[MailboxId] VARCHAR(15) NOT NULL,
			[CosId] NUMERIC(9) NOT NULL,
			[SiteId] VARCHAR(10) NOT NULL,
			[UserId] VARCHAR(15) NOT NULL,
			[UserName] VARCHAR(40) NOT NULL,
			[UserPassword] VARCHAR(32) NOT NULL,
			[BroadcastGroupId] NUMERIC(9) NOT NULL,
			[BroadcastGroupName] VARCHAR(15) NOT NULL
		);

		INSERT INTO [ivxsvr].[dbo].[mbx]
		(
			[mailbox_id],
			[cos_id],
			[site_id],
			[user_id],
			[user_name],
			[user_password],
			[user_greet_rec_filename],
			[user_name_rec_filename],
			[create_date],
			[create_time],
			[client_lock], [client_lockout_time], [lock_state], [client_lock_time], [balance],
			[name_recorded], [greet_recorded], [rec_password], [login_date], [login_time],
			[group_id],
			[group_name]
		)
		OUTPUT
			INSERTED.[mailbox_id] AS 'MailboxId',
			INSERTED.[cos_id] AS 'CosId',
			INSERTED.[site_id] AS 'SiteId',
			INSERTED.[user_id] AS 'UserId',
			INSERTED.[user_name] AS 'UserName',
			INSERTED.[user_password] AS 'UserPassword',
			INSERTED.[group_id] AS 'BroadcastGroupId',
			INSERTED.[group_name] AS 'BroadcastGroupName'
		INTO #CreatedMailboxes
		SELECT
			m.[MailboxId],
			m.[CosId],
			m.[SiteId],
			m.[UserId],
			m.[UserName],
			m.[UserPassword],
			NULL,
			NULL,
			m.[CreateDateStr],
			m.[CreateTimeStr],
			0, 0, 0, 0, 0,
			0, 0, NULL, NULL, NULL,
			m.[BroadcastGroupId],
			m.[BroadcastGroupName]
		FROM
			@Mailboxes m;


		UPDATE p
			SET p.[mailbox] = m.[MailboxId]
		FROM
			[nexus].[dbo].[PINID] p
		JOIN
			@Mailboxes m
			ON m.[UserId] = p.[apin]
				AND m.[SiteId] = p.[site_id]
		WHERE
			p.[mailbox] IS NULL
			OR LTRIM(RTRIM(p.[mailbox])) = '';


		SELECT
			[MailboxId],
			[CosId],
			[SiteId],
			[UserId],
			[UserName],
			[UserPassword],
			[BroadcastGroupId],
			[BroadcastGroupName]
		FROM
			#CreatedMailboxes;


		IF OBJECT_ID('tempdb..#CreatedMailboxes') IS NOT NULL DROP TABLE #CreatedMailboxes;

	END TRY
	BEGIN CATCH

		IF OBJECT_ID('tempdb..#CreatedMailboxes') IS NOT NULL DROP TABLE #CreatedMailboxes;

		DECLARE
			@Message VARCHAR(MAX) = ERROR_MESSAGE(),
			@Severity INT = ERROR_SEVERITY(),
			@State SMALLINT = ERROR_STATE();

		RAISERROR(@Message, @Severity, @State);

	END CATCH

END




CREATE PROCEDURE [dbo].[uspCreateMailboxSite]
	@GroupId CHAR(10),
	@SiteId CHAR(10),
	@IsGroup BIT = 1
AS
BEGIN
	IF NOT EXISTS(SELECT 1 FROM [ivxsvr].[dbo].[MBXSites] ms WHERE ms.[SITE_ID] = @SiteId)
	BEGIN
		INSERT INTO [ivxsvr].[dbo].[MBXSites]([GROUP_ID], [IS_GROUP], [SITE_ID])
		VALUES (@GroupId, @IsGroup, @SiteId);
	END
END




CREATE PROCEDURE [dbo].[uspCreateSiteName]
	@SiteId CHAR(10),
	@SiteName CHAR(24),
	@StreetAddress CHAR(32),
	@City CHAR(16),
	@State CHAR(2),
	@Zip CHAR(9),
	@Phone CHAR(20),
	@Contact CHAR(24) = '',
	@Email CHAR(40) = ''
AS
BEGIN
	IF NOT EXISTS(SELECT 1 FROM [ivxsvr].[dbo].[SiteNames] sn WHERE sn.[site_id] = @SiteId)
	BEGIN
		INSERT INTO [ivxsvr].[dbo].[SiteNames]([site_id], [site_name], [streetaddy], [city], [state], [zip], [phone], [contact], [email])
		VALUES (@SiteId, @SiteName, @StreetAddress, @City, @State, @Zip, @Phone, @Contact, @Email);
	END
END





CREATE PROCEDURE [dbo].[uspEnableFacilityVoicemail]
	@SiteId CHAR(10),
	@SiteName CHAR(24),
	@StreetAddress CHAR(32),
	@City CHAR(16),
	@State CHAR(2),
	@Zip CHAR(9),
	@Phone CHAR(20),
	@Contact CHAR(24) = '',
	@Email CHAR(40) = ''
AS
BEGIN

	SET NOCOUNT ON
	SET XACT_ABORT ON

	BEGIN TRY
	BEGIN TRANSACTION

		EXEC [dbo].[uspCreateSiteName] @SiteId, @SiteName, @StreetAddress, @City, @State, @Zip, @Phone, @Contact, @Email;

		EXEC [dbo].[uspCreateMailboxSite] @GroupId = @SiteId, @SiteId = @SiteId;

		-- Create the default broadcast groups
		EXEC [dbo].[uspCreateBroadcastGroup] @GroupId = 1, @GroupName = 'Broadcast GP1', @SiteId = @SiteId;
		EXEC [dbo].[uspCreateBroadcastGroup] @GroupId = 2, @GroupName = 'Broadcast GP2', @SiteId = @SiteId;
		EXEC [dbo].[uspCreateBroadcastGroup] @GroupId = 3, @GroupName = 'Broadcast GP3', @SiteId = @SiteId;

		-- Create the default mailbox configs
		INSERT INTO [dbo].[mbxcfg]
		(
			[admin_approval], [auto_save], [broadcast], [cos_id], [cos_name], 
			[del_rec_file], [ext_payment], [max_fee_retrieve], [max_fee_store], [max_listen_cnt],
			[max_lockout_time], [max_msg_len], [max_msgs], [max_ret_time_new], [max_ret_time_saved],
			[max_saved], [max_storage_space], [pending_enable], [rec_greet_enable], [rec_greet_once_enable],
			[rec_name_enable], [rec_name_once_enable], [site_id]
		)
		SELECT
			mc.[admin_approval], mc.[auto_save], mc.[broadcast], mc.[cos_id], mc.[cos_name], 
			mc.[del_rec_file], mc.[ext_payment], mc.[max_fee_retrieve], mc.[max_fee_store], mc.[max_listen_cnt],
			mc.[max_lockout_time], mc.[max_msg_len], mc.[max_msgs], mc.[max_ret_time_new], mc.[max_ret_time_saved],
			mc.[max_saved], mc.[max_storage_space], mc.[pending_enable], mc.[rec_greet_enable], mc.[rec_greet_once_enable],
			mc.[rec_name_enable], mc.[rec_name_once_enable], mc.[site_id]
		FROM
			[dbo].[mbxcfg] mc
		WHERE
			mc.[site_id] = '0000000000'
			AND NOT EXISTS(SELECT 1 FROM [dbo].[mbxcfg] mc2 WHERE mc2.[site_id] = @SiteId AND mc2.[cos_id] = mc.[cos_id]);

	COMMIT TRANSACTION
	END TRY
	BEGIN CATCH

		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK TRANSACTION
		END

		DECLARE
			@ErrMessage VARCHAR(MAX) = ERROR_MESSAGE(),
			@ErrSeverity INT = ERROR_SEVERITY(),
			@ErrState SMALLINT = ERROR_STATE();

		RAISERROR(@ErrMessage, @ErrSeverity, @ErrState);

	END CATCH

END



CREATE PROCEDURE [dbo].[uspLoadAllMailboxIds]
AS
BEGIN
	SELECT [mailbox_id] AS 'MailboxId' FROM [dbo].[mbx];
END



CREATE PROCEDURE [dbo].[uspLoadAllSiteNames]
AS
BEGIN

	SELECT
		[site_id] AS 'SiteId',
		[site_name] AS 'SiteName',
		[streetaddy] AS 'StreetAddress',
		[city] AS 'City',
		[state] AS 'State',
		[zip] AS 'Zip',
		[phone] AS 'Phone',
		[contact] AS 'Contact',
		[email] AS 'Email'
	FROM
		[ivxsvr].[dbo].[SiteNames];

END





CREATE PROCEDURE [dbo].[uspLoadBroadcastGroups]
	@SiteId CHAR(10)
AS
BEGIN

	SELECT
		bg.[group_id] AS 'GroupId',
		bg.[group_name] AS 'GroupName',
		bg.[site_id] AS 'SiteId'
	FROM
		[ivxsvr].[dbo].[BroadcastGroups] bg
	WHERE
		bg.[site_id] = @SiteId;

END





CREATE PROCEDURE [dbo].[uspLoadInmateAccountMailbox]
	@Pin CHAR(10),
	@SiteId CHAR(10)
AS
BEGIN

	DECLARE @MailboxId CHAR(15);
	SELECT @MailboxId = [mailbox] FROM [nexus].[dbo].[PINID] WHERE [pin] = @Pin AND [site_id] = @SiteId;


	SELECT
		m.[mailbox_id] AS 'MailboxId',
		m.[cos_id] AS 'CosId',
		m.[site_id] AS 'SiteId',
		m.[user_id] AS 'UserId',
		m.[user_name] AS 'UserName',
		m.[user_password] AS 'UserPassword',
		m.[user_greet_rec_filename] AS 'UserGreetRecFilename',
		m.[user_name_rec_filename] AS 'UserNameRecFileName',
		m.[create_date] AS 'CreateDate',
		m.[create_time] AS 'CreateTime',
		m.[client_lock] AS 'ClientLock',
		m.[client_lockout_time] AS 'ClientLockoutTime',
		m.[lock_state] AS 'LockState',
		m.[client_lock_time] AS 'ClientLockTime',
		m.[balance] AS 'Balance',
		m.[name_recorded] AS 'NameRecorded',
		m.[greet_recorded] AS 'GreetRecorded',
		m.[rec_password] AS 'RecPassword',
		m.[login_date] AS 'LoginDate',
		m.[login_time] AS 'LoginTime',
		m.[group_id] AS 'GroupId',
		m.[group_name] AS 'GroupName'
	FROM
		[ivxsvr].[dbo].[mbx] m
	WHERE
		m.[mailbox_id] = @MailboxId;

END





CREATE PROCEDURE [dbo].[uspLoadMailboxConfigs]
	@SiteId CHAR(10)
AS
BEGIN
	SELECT
		[admin_approval] AS 'AdminApproval',
		[auto_save] AS 'AutoSave',
		[broadcast] AS 'Broadcast',
		[cos_id] AS 'CosId',
		[cos_name] AS 'CosName', 
		[del_rec_file] AS 'DelRecFile',
		[ext_payment] AS 'ExtPayment',
		[max_fee_retrieve] AS 'MaxFeeRetrieve',
		[max_fee_store] AS 'MaxFeeStore',
		[max_listen_cnt] AS 'MaxListenCnt',
		[max_lockout_time] AS 'MaxLockoutTime',
		[max_msg_len] AS 'MaxMsgLen',
		[max_msgs] AS 'MaxMsgs',
		[max_ret_time_new] AS 'MaxRetTimeNew',
		[max_ret_time_saved] AS 'MaxRetTimeSaved',
		[max_saved] AS 'MaxSaved',
		[max_storage_space] AS 'MaxStorageSpace',
		[pending_enable] AS 'PendingEnable',
		[rec_greet_enable] AS 'RecGreetEnable',
		[rec_greet_once_enable] AS 'RecGreetOnceEnable',
		[rec_name_enable] AS 'RecNameEnable',
		[rec_name_once_enable] AS 'RecNameOnceEnable',
		[site_id] AS 'SiteId'
	FROM
		[dbo].[mbxcfg]
	WHERE
		[site_id] = @SiteId;
END





CREATE PROCEDURE [dbo].[uspLoadMailboxesToCreate]
	@SiteId CHAR(10)
AS
BEGIN

	DECLARE
		@DateNow CHAR(8) = CONVERT(CHAR(8), GETDATE(), 112), -- yyyyMMdd
		@TimeNow CHAR(6) = CAST(FORMAT(GETDATE(), 'HHmmss') AS CHAR(6)),
		@DefCosId NUMERIC(9) = 1,
		@DefBroadcastGroupId NUMERIC(9) = 1,
		@DefBroadcastGroupName CHAR(15);


	SELECT
		@DefBroadcastGroupName = [group_name]
	FROM
		[ivxsvr].[dbo].[BroadcastGroups]
	WHERE
		[group_id] = @DefBroadcastGroupId
		AND [site_id] = @SiteId;


	IF @DefBroadcastGroupName IS NULL
	BEGIN
		RAISERROR('No broadcast groups exist for the given site ID.', 16, 1);
	END


	SELECT
		@DefCosId AS 'CosId',
		@SiteId AS 'SiteId',
		p.[apin] AS 'UserId',
		p.[name] AS 'UserName',
		p.[apin] AS 'UserPassword',
		@DateNow AS 'CreateDateStr',
		@TimeNow AS 'CreateTimeStr',
		@DefBroadcastGroupId AS 'BroadcastGroupId',
		@DefBroadcastGroupName AS 'BroadcastGroupName'
	FROM
		[nexus].[dbo].[PINID] p
	WHERE
		p.[site_id] = @SiteId
		AND p.[pinactive] = 1
		AND
		(
			p.[mailbox] IS NULL
			OR LTRIM(RTRIM(p.[mailbox])) = ''
		);

END





CREATE PROCEDURE [dbo].[uspModifyBroadcastGroup]
	@GroupId NUMERIC(9),
	@GroupName CHAR(15),
	@SiteId CHAR(10)
AS
BEGIN
	UPDATE [dbo].[BroadcastGroups]
		SET
			[group_name] = @GroupName
	WHERE
		[group_id] = @GroupId
		AND [site_id] = @SiteId;


	-- update mbx records too because for whatever reason there's a group_name
	-- column in the mbx table even though it's already referenced by dbo.mbx.group_id
	UPDATE [dbo].[mbx]
		SET
			[group_name] = @GroupName
	WHERE
		[group_id] = @GroupId
		AND [site_id] = @SiteId;
END






CREATE PROCEDURE [dbo].[uspModifyMailbox]
	@MailboxId CHAR(15),
	@UserId CHAR(15),
	@UserName CHAR(40),
	@UserPassword CHAR(32),
	@RecPassword CHAR(32),
	@ClientLockoutTime NUMERIC(2),
	@CosId NUMERIC(9),
	@GroupId NUMERIC(9),
	@LockState NUMERIC(2)
AS
BEGIN

	UPDATE m
		SET
			m.[user_id] = @UserId,
			m.[user_name] = @UserName,
			m.[user_password] = @UserPassword,
			m.[rec_password] = @RecPassword,
			m.[client_lockout_time] = @ClientLockoutTime,
			m.[cos_id] = @CosId,
			m.[group_id] = @GroupId,
			m.[group_name] = bg.[group_name],
			m.[lock_state] = @LockState
	FROM
		[dbo].[mbx] m
	LEFT JOIN
		[dbo].[BroadcastGroups] bg
		ON bg.[group_id] = @GroupId
	WHERE
		m.[mailbox_id] = @MailboxId;

END






CREATE PROCEDURE [dbo].[uspModifyMailboxConfig]
	@AdminApproval BIT,
	@AutoSave BIT,
	@Broadcast BIT,
	@CosId NUMERIC(9),
	@CosName CHAR(15),
	@ExtPayment BIT,
	@MaxFeeRetrieve NUMERIC(3),
	@MaxFeeStore NUMERIC(3),
	@MaxListenCnt NUMERIC(2),
	@MaxLockoutTime NUMERIC(2),
	@MaxMsgLen NUMERIC(3),
	@MaxMsgs NUMERIC(3),
	@MaxRetTimeNew NUMERIC(2),
	@MaxRetTimeSaved NUMERIC(2),
	@MaxSaved NUMERIC(3),
	@MaxStorageSpace NUMERIC(5),
	@PendingEnable BIT,
	@RecGreetEnable BIT,
	@RecGreetOnceEnable BIT,
	@RecNameEnable BIT,
	@RecNameOnceEnable BIT,
	@SiteId CHAR(10)
AS
BEGIN

	UPDATE [dbo].[mbxcfg]
		SET
			[admin_approval] = @AdminApproval,
			[auto_save] = @AutoSave,
			[broadcast] = @Broadcast,
			[cos_name] = @CosName,
			[ext_payment] = @ExtPayment,
			[max_fee_retrieve] = @MaxFeeRetrieve,
			[max_fee_store] = @MaxFeeStore,
			[max_listen_cnt] = @MaxListenCnt,
			[max_lockout_time] = @MaxLockoutTime,
			[max_msg_len] = @MaxMsgLen,
			[max_msgs] = @MaxMsgs,
			[max_ret_time_new] = @MaxRetTimeNew,
			[max_ret_time_saved] = @MaxRetTimeSaved,
			[max_saved] = @MaxSaved,
			[max_storage_space] = @MaxStorageSpace,
			[pending_enable] = @PendingEnable,
			[rec_greet_enable] = @RecGreetEnable,
			[rec_greet_once_enable] = @RecGreetOnceEnable,
			[rec_name_enable] = @RecNameEnable,
			[rec_name_once_enable] = @RecNameOnceEnable
	WHERE
		[cos_id] = @CosId
		AND [site_id] = @SiteId;

END