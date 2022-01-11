(function ($) {
    $.fn.uds = function () {
        return this.each(function () {
            var $this = $(this);

            //var ajaxUrl = 'http://lnf.umich.edu/';
            var ajaxUrl = 'https://ssel-apps.eecs.umich.edu/uds';

            var umichDirectorySearch = function (uniqname, callback) {
                $('.umich-directory-search-message', $this).html('');
                $.ajax({
                    'url': ajaxUrl,
                    'data': { 'uniqname': uniqname, 'action': 'umich-directory-search', 'format': 'json' },
                    'type': 'GET',
                    'dataType': 'json',
                    'success': function (json) {
                        console.log(json);
                        fillForm(json.data);
                    },
                    'error': function (err) {
                        var msg = "";
                        
                        if (err.responseJSON && err.responseJSON.message)
                            msg = err.responseJSON.message;
                        else
                            msg = err.statusText || err.responseText || "Unknown error.";
                        
                        $('.umich-directory-search-message', $this).html(msg);
                    },
                    'complete': function () {
                        if (typeof callback == 'function')
                            callback();
                    }
                });
            }

            var fillForm = function (data) {
                if (data.error) {
                    $('.umich-directory-search-message', $this).html(data.message);
                    return;
                }
                var name = parseDisplayName(data.displayName);
                $('.fname-textbox', $this).val(name.first);
                $('.mname-textbox', $this).val(name.middle);
                $('.lname-textbox', $this).val(name.last);
                $('.username-textbox', $this).val(data.uid);
                $('.email-textbox', $this).val(data.mail);
                $('.phone-textbox', $this).val(parsePhone(data.telephoneNumber));
            }

            var parseDisplayName = function (raw) {
                var result = { 'first': '', 'middle': '', 'last': '' };
                if (raw) {
                    var splitter = raw.toString().split(' ');

                    if (splitter.length >= 3) {
                        result.first = splitter[0];
                        result.middle = splitter[1];
                        result.last = splitter[2];
                    }
                    else if (splitter.length >= 2) {
                        result.first = splitter[0];
                        result.last = splitter[1];
                    }
                    else if (splitter.length >= 1) {
                        result.last = splitter[0];
                    }
                }
                return result;
            }

            var parsePhone = function (raw) {
                if (raw)
                    return raw.toString().replace('/', '-');
                else
                    return '';
            }

            var clearForm = function () {
                //$('.umich-directory-search-textbox', $this).val('');
                //$('.fname-textbox', $this).val('');
                //$('.mname-textbox', $this).val('');
                //$('.lname-textbox', $this).val('');
                //$('.username-textbox', $this).val('');
                //$('.email-textbox', $this).val('');
                //$('.phone-textbox', $this).val('');
                $('.umich-directory-search-button', $this).removeAttr('disabled');
            }

            clearForm();

            $this.on('click', '.umich-directory-search-button', function (event) {
                var uniqname = $('.umich-directory-search-textbox', $this).val();
                if (uniqname != '') {
                    var self = $(this);
                    self.attr('disabled', 'disabled');
                    umichDirectorySearch(uniqname, function () {
                        self.removeAttr('disabled');
                    });
                }
            });
        });
    }
} (jQuery));