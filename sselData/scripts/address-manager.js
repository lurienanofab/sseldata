(function ($) {
    $.fn.addressManager = function (options) {
        return this.each(function () {
            var $this = $(this);

            var opt = $.extend({}, { "url": "ajax/addressmanager", "type": "account", "id": 0, "addressTypes": {} }, options, $this.data());

            var template = Handlebars.compile($(".address-template", $this).html());

            $(".address-entries", $this).html(template(opt.addressTypes));

            var items = null;
            var currentEntry = null;

            var getItems = function (data) {
                return $.map(data, function (value, index) {
                    return {
                        "addressType": value.AddressType,
                        "addressTypeName": opt.addressTypes[value.AddressType],
                        "addressId": value.AddressID,
                        "attention": value.Attention,
                        "addressLine1": value.AddressLine1,
                        "addressLine2": value.AddressLine2,
                        "city": value.City,
                        "state": value.State,
                        "zip": value.Zip,
                        "country": value.Country
                    };
                });
            };

            var cancelEdit = function () {
                if (currentEntry) {
                    currentEntry.removeClass("editing");

                    currentEntry.find(".property").each(function () {
                        var prop = $(this);
                        var value = prop.data("value");
                        prop.html(value);
                        prop.removeAttr("value");
                    });

                    if (currentEntry.data("addressId") === 0) {
                        currentEntry.find(".entry-controls .add").show();
                        currentEntry.find(".entry-controls .edit-delete").hide();
                        currentEntry.find(".entry-controls .update-cancel").hide();
                    } else {
                        currentEntry.find(".entry-controls .add").hide();
                        currentEntry.find(".entry-controls .edit-delete").show();
                        currentEntry.find(".entry-controls .update-cancel").hide();
                    }

                    currentEntry = null;
                }
            };

            var editEntry = function (entry) {
                cancelEdit();

                entry.addClass("editing");

                var value;

                var editProperty = function (name) {
                    var prop = entry.find(".property.entry-" + name);
                    var value = prop.html();
                    var input = $("<input/>", { "class": "property-text", "value": value }).addClass(name);
                    prop.data("value", value).html(input);
                };

                editProperty("attention");
                editProperty("address-line1");
                editProperty("address-line2");
                editProperty("city");
                editProperty("state");
                editProperty("zip");
                editProperty("country");

                entry.find(".entry-controls .add").hide();
                entry.find(".entry-controls .edit-delete").hide();
                entry.find(".entry-controls .update-cancel").show();

                currentEntry = entry;
            };

            var updateEntry = function () {
                if (currentEntry) {
                    var model = {
                        "addressId": currentEntry.data("addressId"),
                        "addressType": currentEntry.data("addressType"),
                        "attention": currentEntry.find(".attention").val(),
                        "addressLine1": currentEntry.find(".address-line1").val(),
                        "addressLine2": currentEntry.find(".address-line2").val(),
                        "city": currentEntry.find(".city").val(),
                        "state": currentEntry.find(".state").val(),
                        "zip": currentEntry.find(".zip").val(),
                        "country": currentEntry.find(".country").val()
                    };

                    currentEntry.removeClass("editing");
                    currentEntry.find(".property").removeAttr("value");
                    currentEntry.find(".entry-controls .add").hide();
                    currentEntry.find(".entry-controls .edit-delete").hide();
                    currentEntry.find(".entry-controls .update-cancel").hide();
                    currentEntry = null;

                    $.ajax({
                        "url": opt.url + "?command=update-address&type=" + opt.type + "&id=" + opt.id,
                        "method": "POST",
                        "data": model
                    }).done(doneHandler).fail(failHandler);
                }
            };

            var deleteEntry = function (entry) {
                cancelEdit();

                var addressId = entry.data("addressId");

                $.ajax({
                    "url": opt.url + "?command=delete-address&type=" + opt.type + "&id=" + opt.id + "&addressId=" + addressId,
                    "method": "GET"
                }).done(doneHandler).fail(failHandler);
            };

            var doneHandler = function (data) {
                items = getItems(data);

                $.each(opt.addressTypes, function (key, value) {
                    var entry = $(".address-entries .address-entry[data-address-type='" + key + "']", $this);

                    entry.find(".add").hide();
                    entry.find(".edit-delete").hide();
                    entry.find(".update-cancel").hide();

                    var filter = $.grep(items, function (i) { return i.addressType === key; });

                    if (filter.length > 0) {
                        var item = filter[0];
                        entry.data("addressId", item.addressId);
                        entry.find(".entry-attention").html(item.attention);
                        entry.find(".entry-address-line1").html(item.addressLine1);
                        entry.find(".entry-address-line2").html(item.addressLine2);
                        entry.find(".entry-city").html(item.city);
                        entry.find(".entry-state").html(item.state);
                        entry.find(".entry-zip").html(item.zip);
                        entry.find(".entry-country").html(item.country);
                        entry.find(".edit-delete").show();
                    } else {
                        entry.data("addressId", 0);
                        entry.find(".add").show();
                    }
                });
            };

            var failHandler = function (jqXHR) {
                alert("Failed to get addresses: " + jqXHR.responseJSON.error);
            };

            var refresh = function () {
                $.ajax({
                    "url": opt.url + "?command=get-addresses&type=" + opt.type + "&id=" + opt.id,
                    "method": "GET"
                }).done(doneHandler).fail(failHandler);
            };

            $this.on("click", ".edit-entry", function (e) {
                e.preventDefault();
                var entry = $(this).closest(".address-entry");
                editEntry(entry);
            }).on("click", ".delete-entry", function (e) {
                e.preventDefault();
                var entry = $(this).closest(".address-entry");
                deleteEntry(entry);
            }).on("click", ".update-entry", function (e) {
                e.preventDefault();
                updateEntry();
            }).on("click", ".cancel-entry", function (e) {
                e.preventDefault();
                cancelEdit();
            }).on("click", ".add-address-button", function (e) {
                var entry = $(this).closest(".address-entry");
                editEntry(entry);
            });

            refresh();
        });
    };
}(jQuery));