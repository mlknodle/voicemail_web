(function () {
	"use strict";

	angular.module("app").controller("VmBroadcastGroupsCtrl", ["args", "$q", "$scope", "$rootScope", "$uibModalInstance", "toaster", "VoicemailService",
		function (args, $q, $scope, $rootScope, $uibModalInstance, toaster, VoicemailService) {
			$scope.siteId = null;
			$scope.broadcastGroups = [];
			$scope.broadcastGroupModel = {
				selectedGroup: {
					id: null,
					label: null
				}
			};
			$scope.broadcastGroupOptions = {
				groups: []
			};

			$scope.textModalTitle = "Create/Edit Broadcast Group";
			$scope.mode = "Edit";

			$scope.loadingBroadcastGroups = false;
			$scope.loadingPageData = false;
			$scope.saving = false;


			$scope.initialize = function () {
				$scope.loadingPageData = true;

				$scope.siteId = args.siteId;

				const p1 = $scope.loadBroadcastGroups();

				$q.all([p1])
					.then(function () {
						$scope.loadingPageData = false;
					}, function () {
						toaster.pop('error', 'An unexpected error occurred. Failed to load page.');
						$scope.loadingPageData = false;
					});
			}

			$scope.cancel = function () {
				$uibModalInstance.dismiss();
			}

			$scope.createNewBroadcastGroup = function () {
				VoicemailService.createBroadcastGroup($scope.broadcastGroupModel)
					.then(function () {
						toaster.pop('success', 'Broadcast group created!');
						$scope.saving = false;
						$uibModalInstance.close();
					}, function (err) {
						console.log(err);

						switch (err.status) {
							case 400:
								toaster.pop('warning', 'Invalid input data');
								break;
							default:
								toaster.pop('error', 'An unexpected error occurred. Please try again.');
								break;
						}

						$scope.saving = false;
					});
			}

			$scope.getBroadcastGruop = function (id) {
				for (let i = 0; i < $scope.broadcastGroups.length; i++) {
					if ($scope.broadcastGroups[i].groupId === id) {
						return $scope.broadcastGroups[i];
					}
				}

				return null;
			}

			$scope.loadBroadcastGroups = function () {
				const deferred = $q.defer();

				if ($scope.loadingBroadcastGroups) {
					deferred.resolve();
				} else {
					$scope.loadingBroadcastGroups = true;

					VoicemailService.loadBroadcastGroups($scope.siteId)
						.then(function (result) {
							$scope.broadcastGroupOptions.groups = [];

							$scope.broadcastGroupOptions.groups.push({ id: 0, label: '- Create New -' });
							for (let i = 0; i < result.length; i++) {
								$scope.broadcastGroupOptions.groups.push({ id: result[i].groupId, label: result[i].groupName });
							}

							$scope.broadcastGroups = result;

							// Set default selected group
							$scope.broadcastGroupModel.selectedGroup.id = $scope.broadcastGroupOptions.groups[i].id;
							$scope.broadcastGroupModel.selectedGroup.label = $scope.broadcastGroupOptions.groups[i].label;

							$scope.loadingBroadcastGroups = false;
							deferred.resolve();
						}, function (err) {
							console.log(err);
							$scope.loadingBroadcastGroups = false;
							deferred.reject(err);
						});
				}

				return deferred.promise;
			}

			$scope.modifyBroadcastGroup = function () {
				VoicemailService.saveBroadcastGroupChanges($scope.broadcastGroupModel)
					.then(function () {
						toaster.pop('success', 'Broadcast group modified!');
						$scope.saving = false;
						$uibModalInstance.close();
					}, function (err) {
						console.log(err);

						switch (err.status) {
							case 400:
								toaster.pop('warning', 'Invalid input data');
								break;
							default:
								toaster.pop('error', 'An unexpected error occurred. Please try again.');
								break;
						}

						$scope.saving = false;
					});
			}

			$scope.onSelectedGroupChanged = function () {
				if ($scope.broadcastGroupModel.selectedGroup.id === 0) { // Create New option in dropdown select.
					$scope.mode = "Create";
				} else {
					$scope.mode = "Edit";
					$scope.broadcastGroupModel.groupName = $scope.broadcastGroupModel.selectedGroup.label;
				}
			}

			$scope.reset = function () {
				if ($scope.mode === "Create") {
					$scope.broadcastGroupModel.groupName = null;
				} else {
					const group = $scope.getBroadcastGroup($scope.broadcastGroupModel.selectedGroup.id);
					$scope.broadcastGroupModel.groupName = group.groupName;
				}
			}

			$scope.save = function () {
				if ($scope.saving) {
					return;
				}

				$scope.saving = false;

				if ($scope.validateGroup()) {
					$scope.broadcastGroupModel.siteId = $scope.siteId;

					if ($scope.mode === "Create") {
						$scope.broadcastGroupModel.groupId = null;
						$scope.createNewBroadcastGroup();
					} else {
						$scope.broadcastGroupModel.groupId = $scope.broadcastGroupModel.selectedGroup.id;
						$scope.modifyBroadcastGroup();
					}
				}
			}

			

			$scope.validateGroup = function () {
				let valid = true;

				if ($scope.broadcastGroupModel.groupName === null || $scope.broadcastGroupModel.groupName === undefined) {
					toaster.pop('warning', 'Please enter a name for the broadcast group.');
					valid = false;
				}

				if ($scope.mode === "Create") {
					for (let i = 0; i < $scope.broadcastGroups.length; i++) {
						if ($scope.broadcastGroups[i].groupName === $scope.broadcastGroupModel.groupName) {
							toaster.pop('warning', 'The name entered is already taken! Please use a different group name.');
							valid = false;
							break;
						}
					}
				} else {
					for (let i = 0; i < $scope.broadcastGroups.length; i++) {
						if ($scope.broadcastGroups[i].groupId !== $scope.broadcastGroupModel.groupId) {
							if ($scope.broadcastGroups[i].groupName === $scope.broadcastGroupModel.groupName) {
								toaster.pop('warning', 'The name entered is already taken! Please use a different group name.');
								valid = false;
								break;
							}
						}
					}
				}

				return valid;
			}

			$scope.initialize();
		}
	]);
})();