HostPipe.UI.CountriesManager = function () {
    this.element = $("#CountriesContent");
}

HostPipe.UI.CountriesManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createSettings: function () {
        this.settings = {
            addCountryUrl: this.element.attr('data-addCountry-url'),
            deleteCountryUrl: this.element.attr('data-deleteCountry-url'),
            getCountryUrl: this.element.attr('data-getCountry-url'),
            getCountryDataUrl: this.element.attr('data-getCountryData-url'),
            getCountriesSelectorsUrl: this.element.attr('data-getCountriesSelectors-url'),
            getDomainsListUrl: this.element.attr('data-getDomainsList-url'),
            getImportCountriesUrl: this.element.attr('data-getImportCountries-url'),
            importCountriesUrl: this.element.attr('data-importCountries-url'),
            saveOrderUrl: this.element.attr('data-saveOrder-url'),
            updateCountryUrl: this.element.attr('data-updateCountry-url')
        };
    },
    _createViewModel: function () {
        this.viewModel = {
            countryID: ko.observable(0),
            isDefault: ko.observable(false),
            isNewCountry: ko.observable(true),
            isCountryEdit: ko.observable(true),
            sectionHeader: ko.observable("Add New Country"),
            defaultZoneID: ko.observable(),
            domains: ko.observableArray([]),
            domainID: ko.observable(),
            lDomainID: ko.observable(),
            domainSourceID: ko.observable(),
            domainDestID: ko.observable(),
            country: ko.observable(),
            zoneID: ko.observable(),
            zones: ko.observableArray([]),
            code: ko.observable(),

            onEdit: $.proxy(this._onEditCountry, this),
            onDelete: $.proxy(this._onDeleteCountry, this),
            onDomainChange: $.proxy(this._onDomainChange, this),
            onLDomainChange: $.proxy(this._reloadLeftMenu, this),
            onNewCountry: $.proxy(this._onNewCountry, this),
            onImportCountries: $.proxy(this._onImportCountries, this),
            onImportCountriesExec: $.proxy(this._onImportCountriesExec, this),
            onSaveCountry: $.proxy(this._onSaveCountry, this),
            onSaveOrder: $.proxy(this._onSaveOrder, this)
        };

        this.viewModel.countryID.subscribe(function (value) {
            if (value > 0) {
                this.isNewCountry(false);
                this.sectionHeader("Update Country");
            }
            else {
                this.isNewCountry(true);
                this.sectionHeader("Add New Country");
            }
        }, this.viewModel);
    },
    _getDomainsList: function () {
        $.post(this.settings.getDomainsListUrl, { currentDomain: true }, $.proxy(this._getDomainsListCompleted, this));
    },
    _getDomainsListCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.domains(data.domains);
            this.viewModel.domainID(data.selected);
            this.viewModel.lDomainID(data.selected);
            this._reloadLeftMenu();
            this._onDomainChange();
        }
    },
    _onDeleteCountry: function (data, event) {
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var confirmationText = _.str.trim("Are you sure that you wish to delete " + menuItem.find('.title').text()) + "? This action cannot be undone.";
        var countryID = menuItem.attr('data-itemID');

        if (confirm(confirmationText)) {
            if (countryID == this.viewModel.countryID())
                this.reload = true;

            $.post(this.settings.deleteCountryUrl, { countryID: countryID }, $.proxy(this._onDeleteCountryCompleted, this));
        }
    },
    _onDeleteCountryCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Country successfully deleted");
                this._reloadLeftMenu();
                if (this.reload)
                    this._onNewCountry();
                this.reload = false;
            }
            else
                alert("Country was not deleted. You can not delete default country or some adresses are already assigned to this country.");
        }
    },
    _onDomainChange: function () {
        $.post(this.settings.getCountriesSelectorsUrl, { domainID: this.viewModel.domainID() }, $.proxy(this._onDomainChangeCompleted, this));
    },
    _onDomainChangeCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.zones(data.zones);
            this.viewModel.defaultZoneID(data.defaultZone);
            if (this.viewModel.isNewCountry())
                this.viewModel.zoneID(data.defaultZone);
        }
    },
    _onNewCountry: function () {
        $.post(this.settings.getCountryUrl, { countryID: 0 }, $.proxy(this._onNewCountryCompleted, this));
    },
    _onNewCountryCompleted: function (data, event) {
        if (data) {
            this.viewModel.isCountryEdit(true);
            this._insertPartialView(data);

            this.viewModel.country('');
            this.viewModel.countryID(0);
            this.viewModel.zoneID(this.viewModel.defaultZoneID());
            this.viewModel.code('');
            this.viewModel.isDefault(false);
        }
    },
    _onEditCountry: function (data, event) {
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.viewModel.countryID(menuItem.attr('data-itemID'));

        if (this.viewModel.isCountryEdit() == false) {
            $.post(this.settings.getCountryUrl, $.proxy(this._onLoadFormCompleted, this));
        }
        else {
            $.post(this.settings.getCountryDataUrl, { countryID: this.viewModel.countryID() }, $.proxy(this._onEditCountryCompleted, this));
        }
    },
    _onLoadFormCompleted: function (data) {
        if (data) {
            this._insertPartialView(data);
            $.post(this.settings.getCountryDataUrl, { countryID: this.viewModel.countryID() }, $.proxy(this._onEditCountryCompleted, this));
        }
    },
    _onEditCountryCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.isCountryEdit(true);

            this.viewModel.country(data.country.Country);
            this.viewModel.domainID(data.country.DomainID);
            this.viewModel.countryID(data.country.CountryID);
            this.viewModel.zoneID(data.country.PostageZoneID);
            this.viewModel.code(data.country.Code);
            this.viewModel.isDefault(data.country.IsDefault);
        }
    },
    _initializeChangeEvent: function () {
    },
    _insertPartialView: function (data) {
        this.element.find('.mainContainer').html(data);
        this._applyViewModel(this.element.find('.mainContainer'));
        MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
        $.validator.unobtrusive.parse(this.element);
    },
    _onImportCountries: function () {
        $.post(this.settings.getImportCountriesUrl, { domainID: this.viewModel.domainID() }, $.proxy(this._onImportCountriesCompleted, this));
    },
    _onImportCountriesCompleted: function (data) {
        if (data) {
            this.viewModel.isCountryEdit(false);
            this._insertPartialView(data);
        }
    },
    _onImportCountriesExec: function () {
        $.post(this.settings.importCountriesUrl, { sourceDomainID: this.viewModel.domainSourceID(), destDomainID: this.viewModel.domainDestID() }, $.proxy(this._onImportCountriesExecCompleted, this));
    },
    _onImportCountriesExecCompleted: function (data) {
        if (data) {
            if (data.success) {
                if (data.imported == 0) {
                    alert("No country has been imported");
                }
                else {
                    this._reloadLeftMenu();
                    alert(data.imported + " countries have been imported");
                }
            }
            else if (data.error) {
                alert(data.error);
            }
        }
    },
    _onSaveCountry: function (data, event) {
        form = $("#CountryForm");
        form.validate();
        if (form.valid()) {
            var url = this.viewModel.isNewCountry() ? this.settings.addCountryUrl : this.settings.updateCountryUrl;
            $.post(url, {
                CountryID: this.viewModel.countryID(),
                DomainID: this.viewModel.domainID(),
                Code: this.viewModel.code(),
                PostageZoneID: this.viewModel.zoneID(),
                Country: this.viewModel.country(),
                IsDefault: this.viewModel.isDefault()
            }, $.proxy(this._onSaveCountryCompleted, this));
        }
    },
    _onSaveCountryCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Country successfully saved.");
                this._reloadLeftMenu();
                $.post(this.settings.getCountryDataUrl, { countryID: data.countryID }, $.proxy(this._onEditCountryCompleted, this));
            }
            else
                alert("There was a problem saving country.");
        }
    },
    _onSaveOrder: function () {
        var orderedItems = $('.leftMenu .menuItem').map(function () {
            return $(this).attr('data-itemID')
        }).get();

        $.ajax(this.settings.saveOrderUrl, {
            type: "POST",
            data: { orderedCountryIDs: orderedItems },
            traditional: true,
            success: $.proxy(this._onSaveOrderCompleted, this)
        });
    },
    _onSaveOrderCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Countries order was successfuly saved.");
                $('#orderIndicator').removeClass('icon-exclamation').addClass('icon-ok');
            }
        }
    },
    _reloadLeftMenu: function () {
        LeftMenuManager.loadMenu(this.viewModel, "", { domainID: this.viewModel.lDomainID() });
    },
    initialize: function () {
        this._createSettings();
        this._createViewModel();
        this._applyViewModel();
        this._getDomainsList();
    }
}

$(function () {
    var CountriesManager = new HostPipe.UI.CountriesManager();
    CountriesManager.initialize();
})