var app = angular.module('app');

// ReSharper disable once InconsistentNaming
app.factory('VoicemailService', ['API', '$http',
	function (API, $http) {
		function createBroadcastGroup(broadcastGroup) {
			return $http.post(`${API}/voicemail/broadcast-groups/create`, broadcastGroup)
				.then(function (result) {
					return result.data;
				});
		}

		function createMailboxes(siteId) {
			return $http.get(`${API}/voicemail/mailboxes/create?siteid=${siteId}`)
				.then(function(result) {
					return result.data;
				});
		}

		function downloadCreatedMailboxesExcel(mailboxes) {
			return $http.post(`${API}/voicemail/mailboxes/created-download-excel`, mailboxes)
				.then(function(result) {
					return result.data;
				});
		}

		function loadBroadcastGroups(siteId) {
			return $http.get(`${API}/voicemail/broadcast-groups?siteid=${siteId}`)
				.then(function (result) {
					return result.data;
				});
		}

		function loadMailbox(pin, siteId) {
			return $http.get(`${API}/voicemail/mailboxes?pin=${pin}&siteid=${siteId}`)
				.then(function(result) {
					return result.data;
				});
		}

		function loadMailboxConfigs(siteId) {
			return $http.get(`${API}/voicemail/mailbox-configs?siteid=${siteId}`)
				.then(function(result) {
					return result.data;
				});
		}

		function loadVoicemailMessages(options) {
			return $http.post(`${API}/voicemail/messages/load-report`, options)
				.then(function (result) {
					return result.data;
				});
		}

		function saveBroadcastGroupChanges(broadcastGroup) {
			return $http.post(`${API}/voicemail/broadcast-groups/save`, broadcastGroup)
				.then(function (result) {
					return result.data;
				});
		}

		function saveMailboxConfigChanges(facilityConfig) {
			return $http.post(`${API}/voicemail/mailbox-configs/save`, facilityConfig)
				.then(function(result) {
					return result.data;
				});
		}

		function saveMailboxChanges(mailbox) {
			return $http.post(`${API}/voicemail/mailboxes/save`, mailbox)
				.then(function (result) {
					return result.data;
				});
		}

		return {
			createBroadcastGroup: createBroadcastGroup,
			createMailboxes: createMailboxes,
			downloadCreatedMailboxesExcel: downloadCreatedMailboxesExcel,
			loadBroadcastGroups: loadBroadcastGroups,
			loadMailbox: loadMailbox,
			loadMailboxConfigs: loadMailboxConfigs,
			loadVoicemailMessages: loadVoicemailMessages,
			saveBroadcastGroupChanges: saveBroadcastGroupChanges,
			saveMailboxChanges: saveMailboxChanges,
			saveMailboxConfigChanges: saveMailboxConfigChanges
		}
	}]);