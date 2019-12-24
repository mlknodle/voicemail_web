var app = angular.module('app');

// ReSharper disable InconsistentNaming
app.controller('FacilityVmConfigCtrl', ['$q', '$routeParams', '$scope', '$uibModal', '$window', 'FacilitiesService', 'toaster', 'VoicemailService',
	function ($q, $routeParams, $scope, $uibModal, $window, FacilitiesService, toaster, VoicemailService) {
		$scope.siteId = $routeParams.siteId;

		$scope.facilityData = {};

		$scope.configModel = {
			selectedCos: {
				id: null,
				label: null
			},
			feeType: {
				id: null,
				label: null
			}
		};
		$scope.configModelOptions = {
			mailboxConfigs: [],
			feeTypes: ['Storage', 'Retrieval']
		};

		$scope.mailboxConfigs = [];

		$scope.disabledMaxSaved = false;
		$scope.loadingFacilityData = false;
		$scope.loadingVmConfigs = false;
		$scope.loadingInitialData = false;
		$scope.saving = false;


		$scope.init = function () {
			$scope.loadingInitialData = true;

			const p1 = $scope.loadVmConfigs();
			const p2 = $scope.loadFacilityData();

			$q.all([p1, p2])
				.then(function () {
					$scope.loadingInitialData = false;
				}, function () {
					toaster.pop('error', 'Application Error', 'An unexpected error occurred. Failed to load facility voicemail data.');
					$scope.loadingInitialData = false;
				});
		}

		$scope.cancel = function () {
			$scope.redirectToEditFacility();
		}

		$scope.loadFacilityData = function() {
			const deferred = $q.defer();

			if ($scope.loadingFacilityData) {
				deferred.resolve();
			} else {
				$scope.loadingFacilityData = true;

				FacilitiesService.loadFacilityData($scope.siteId)
					.then(function(result) {
						$scope.facilityData = result;

						$scope.loadingFacilityData = false;
						deferred.resolve();
					}, function(err) {
						console.log(err);

						$scope.loadingFacilityData = false;
						deferred.reject(err);
					});
			}

			return deferred.promise;
		}

		$scope.loadVmConfigs = function () {
			const deferred = $q.defer();

			if ($scope.loadingVmConfigs) {
				deferred.resolve();
			} else {
				$scope.loadingVmConfigs = true;

				VoicemailService.loadMailboxConfigs($scope.siteId)
					.then(function (result) {
						$scope.mailboxConfigs = result;

						$scope.configModelOptions.mailboxConfigs = [];
						for (let i = 0; i < result.length; i++) {
							$scope.configModelOptions.mailboxConfigs.push({ id: result[i].cosId, label: result[i].cosName });
						}

						// Set default selected config being edited to the first element
						$scope.configModel.selectedCos.id = result[0].cosId;
						$scope.configModel.selectedCos.label = result[0].cosName;
						$scope.onSelectedCosChanged(result[0].cosId);

						$scope.loadingVmConfigs = false;
						deferred.resolve();
					}, function (err) {
						console.log(err);

						$scope.loadingVmConfigs = false;
						deferred.reject(err);
					});
			}

			return deferred.promise;
		}

		$scope.onMaxMsgsChanged = function () {
			if ($scope.configModel.maxMsgs !== undefined && $scope.configModel.maxMsgs !== null) {
				if ($scope.configModel.maxMsgs === 0) {
					$scope.disabledMaxSaved = false;
				} else if ($scope.configModel.maxMsgs > 0) {
					$scope.configModel.maxSaved = angular.copy($scope.configModel.maxMsgs);
					$scope.disabledMaxSaved = true;
				}
			}
		}

		$scope.onSelectedCosChanged = function () {
			$scope.updateFiltersFromCosOptions($scope.configModel.selectedCos.id);
		}

		$scope.openBroadcastGroupModal = function () {
			const args = {
				siteId: $scope.siteId
			};

			const modalInstance = $uibModal.open({
				ariaLabelledBy: 'modal-title',
				ariaDescribedBy: 'modal-body',
				templateUrl: 'js/components/facilities/voicemail/vmBroadcastGroups.html',
				controller: 'VmBroadcastGroupsCtrl',
				size: 'md',
				resolve: {
					args: function () { return args; }
				}
			});

			modalInstance.result.then(
				function () {
					console.log("saved");
					// Saved
				},
				function () {
					console.log("cancelled");
					// Cancelled
				}).then(function () {
					console.log("also cancelled");
					// Also cancelled
				}).catch(function (err) {
					// Failed
					console.log(err);
				});
		}

		$scope.redirectToEditFacility = function () {
			$window.location.href = `#!/facilities/edit/${$scope.siteId}`;
		}

		$scope.reset = function () {
			$scope.updateFiltersFromCosOptions($scope.configModel.selectedCos.id);

			$scope.facilityVmConfigForm.$setPristine();
		}

		$scope.save = function () {
			if ($scope.saving) {
				return;
			}

			$scope.saving = true;

			if ($scope.validateData()) {
				VoicemailService.saveMailboxConfigChanges($scope.configModel)
					.then(function() {
							toaster.pop('success', 'Configuration Changes Saved');
							$scope.saving = false;

							$scope.redirectToEditFacility();
						},
						function(err) {
							switch (err.status) {
							case 400:
								toaster.pop('warning', 'Invalid input data.');
								break;
							default:
								toaster.pop('error', 'An unexpected error occurred. Failed to save configuration changes.');
								break;
							}

							$scope.saving = false;
						});
			} else {
				$scope.saving = false;
			}
		}

		$scope.updateFiltersFromCosOptions = function (cosId) {
			let cos = angular.copy($scope.mailboxConfigs[0]); // Default to the first cos.
			for (let i = 0; i < $scope.mailboxConfigs.length; i++) {
				if ($scope.mailboxConfigs[i].cosId === cosId) {
					cos = angular.copy($scope.mailboxConfigs[i]);
				}
			}

			Object.keys(cos).forEach(function (key) {
				$scope.configModel[key] = cos[key];
			});

			// Set the defaults for fee type and amount
			if (cos.maxFeeStore > 0) {
				$scope.configModel.feeType = 'Storage';
				$scope.configModel.fee = cos.maxFeeStore;
			} else if (cos.maxFeeRetrieve > 0) {
				$scope.configModel.feeType = 'Retrieval';
				$scope.configModel.fee = cos.maxFeeRetrieve;
			} else if (cos.cosName === 'Inmates Default') {
				$scope.configModel.feeType = 'Storage';
				$scope.configModel.fee = cos.maxFeeStore;
			} else if (cos.cosName === 'Admin') {
				$scope.configModel.feeType = 'Retrieval';
				$scope.configModel.fee = cos.maxFeeRetrieve;
			} else {
				$scope.configModel.feeType = 'Storage';
				$scope.configModel.fee = cos.maxFeeStore;
			}
		}

		$scope.validateData = function () {
			let valid = true;

			if ($scope.configModel.feeType === 'Storage') {
				$scope.configModel.maxFeeStore = $scope.configModel.fee;
				$scope.configModel.maxFeeRetrieve = 0; // Reset now irrelevant value.
			} else if ($scope.configModel.feeType === 'Retrieval') {
				$scope.configModel.maxFeeRetrieve = $scope.configModel.fee;
				$scope.configModel.maxFeeStore = 0; // Reset now irrelevant values.
			} else {
				valid = false;
				toaster.pop('warning', 'Invalid fee type selected');
			}

			return valid;
		}

		$scope.init();
	}
]);