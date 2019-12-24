var app = angular.module('app');

// ReSharper disable InconsistentNaming
app.controller('VoiceMailboxCtrl', ['$scope', 'toaster',
	function ($scope, toaster) {
		const def = {
			filters: {
				startDate: moment().subtract(1, 'months').set('date', 1).set('hours', 0).set('minutes', 0).set('seconds', 0).toDate(),
				startTime: '000000',
				endDate: moment().set('date', 1).subtract(1, 'day').set('hours', 23).set('minutes', 59).set('seconds', 59).toDate(),
				endTime: '235959'
			}
		};

		$scope.reportData = [];
		$scope.currentUserIsDisplayMilitaryTime = false;

		// Display Text Variables
		$scope.textNoResults = "No Search Results Found";
		$scope.titlePage = "Commission Report";
		$scope.titlePanelHeader = "Filters";
		$scope.titlePanelResultsHeader = "Commission Report";

		// Filter Variables
		$scope.filters = angular.copy(def.filters);
		$scope.searchOptions = {};

		// Flag Variables
		$scope.isLoadingReportData = false;
		$scope.isLoadingExport = false;
		$scope.isPageLoading = true;
		$scope.isResultsLoaded = false;
		$scope.enableUpdateFilters = true;


		$scope.init = function () {
			$scope.setFiltersToDefaults();
			$scope.loadCurrentUserPreferences();
			$scope.isPageLoading = false;
		}

		$scope.loadReport = function () {
			if ($scope.isLoadingReportData) {
				return;
			}

			$scope.isLoadingReportData = true;

			// Make sure the searchOptions values are only updated when user clicks "Run Report" button.
			if ($scope.enableUpdateFilters) {
				$scope.updateSearchOptions();
				$scope.enableUpdateFilters = false;
			}

			if (!$scope.validateSearchOptions()) {
				$scope.isLoadingReportData = false;
				return;
			}

			const commissionReportOptions = $scope.getCommissionReportOptions();

			CommissionReportService.runReport(commissionReportOptions).then(function (result) {
				$scope.reportData = result;

				if ($scope.reportData.facilityReports.length === 0) {
					toaster.pop('warning', 'No Data Found', 'No data found for the given date range!');
				}

				$scope.isLoadingReportData = false;
				$scope.isResultsLoaded = true;
			}, function (err) {
				$scope.isLoadingReportData = false;

				console.log(err);
				if (err.status === 400) {
					toaster.pop('warning', "Invalid Filters", 'The start and/or end date are invalid! Note: Report does not contain data prior to Sept. 1st, 2019.');
				} else {
					toaster.pop('error', "Report Failed", "An unexpected error occurred while attempting to retrieve the report.");
				}
			});
		}

		$scope.downloadExcel = function () {
			if ($scope.isLoadingExport)
				return;

			$scope.isLoadingExport = true;

			$scope.updateSearchOptions();

			if (!$scope.validateSearchOptions()) {
				$scope.isLoadingExport = false;
				return;
			}

			CommissionReportService.downloadExcel($scope.searchOptions)
				.then(function (result) {
					const id = result;
					$scope.isLoadingExport = false;

					ReportsService.getReport(id)
						.then(function () {
							toaster.pop('success', 'Excel Download Completed Successfully', 'Commission Report Excel Download Completed Successfully');
						}, function (err) {
							console.log(err);
							toaster.pop('error', 'Excel Report Failed', 'Failed to download excel report');
						});
				}, function (err) {
					$scope.isLoadingExport = false;
					if (err.status === 204) {
						toaster.pop('warning', 'No Results', 'No results were found matching the given filters');
					} else if (err.status === 400) {
						toaster.pop('warning', 'Invalid Filters', 'The start and/or end date are invalid! Note: Report does not contain data prior to Sept. 1st, 2019.');
					} else {
						toaster.pop('error', 'Application Error', 'An unexpected problem occurred while trying to download the excel report.');
					}
				});
		}

		$scope.getCommissionReportOptions = function () {
			const reportOptions = {
				startDate: $scope.searchOptions.startDate,
				endDate: $scope.searchOptions.endDate
			};
			return reportOptions;
		}

		$scope.getDateTimeFilter = function (startOrEnd) {
			const date = moment($scope.filters[`${startOrEnd}Date`]).format('YYYYMMDD');
			const time = $scope.filters[`${startOrEnd}Time`];

			const dtMoment = moment(`${date}T${time}`);

			return FormatService.getDateTimeNoZone(dtMoment);
		}

		$scope.isResultsEmpty = function () {
			return ($scope.isResultsLoaded &&
				!$scope.isLoadingReportData &&
				$scope.reportData.length === 0);
		}

		$scope.loadCurrentUserPreferences = function () {
			$scope.isLoadingCurrentUserPreferences = true;

			PsiHttpService.loadCurrentUserIsDisplayMilitaryTime()
				.then(function (result) {
					$scope.currentUserIsDisplayMilitaryTime = result;

					$scope.isLoadingCurrentUserPreferences = false;
				}, function () {
					$scope.isLoadingCurrentUserPreferences = false;
					toaster.pop('error', 'Application Error', 'An unexpected error occurred while retrieving user preferences.');
				});
		}

		$scope.setFiltersToDefaults = function () {
			// Start and End times should never change; They are only set here with a default value.
			$scope.filters.startDate = angular.copy(def.filters.startDate);
			$scope.filters.startTime = angular.copy(def.filters.startTime);
			$scope.filters.endDate = angular.copy(def.filters.endDate);
			$scope.filters.endTime = angular.copy(def.filters.endTime);
		}

		$scope.updateSearchOptions = function () {
			$scope.searchOptions.startDate = $scope.getDateTimeFilter('start');
			$scope.searchOptions.endDate = $scope.getDateTimeFilter('end');
		}

		$scope.validateSearchOptions = function () {
			if (moment(new Date($scope.searchOptions.startDate)) > moment(new Date($scope.searchOptions.endDate))) {
				toaster.pop('warning', 'Invalid Filter', 'The start date cannot be before the end date.');
				return false;
			}

			//if (moment(new Date($scope.searchOptions.startDate)) < moment(new Date(2019, 9, 1, 0, 0, 0, 0))) {
			//	toaster.pop('warning', 'Invalid Filter', 'This report only supports a start date on or after Sept. 1st, 2019.');
			//	return false;
			//}

			return true;
		}

		$scope.init();
	}
]);