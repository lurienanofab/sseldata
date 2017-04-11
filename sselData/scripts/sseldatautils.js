$("#spanEnableByShortcode").hide();

$(document).ready(function () {

    $("#txtShortCode").keyup(function (e) {
        $("#spanEnableByShortcode").hide();
        var shortCodeTyped = $.trim($(this).val());
        if (shortCodeTyped == "") {
            return;
        }


        $.ajax({
            type: "GET",
            dataType: 'json',

            url: "/sselData/ajax/Util.ashx?shortcode=" + shortCodeTyped,

            success: function (jsonmsg) {
                var msg = "";
                if (jsonmsg.shortcodestatus == "active") {
                    msg = "* ShortCode already exists and is active.";
                }
                else if (jsonmsg.shortcodestatus == "inactive") {
                    msg = "* ShortCode already exists and is inactive. Do you want to enable? "
                    $("#spanEnableByShortcode").show();
                }
                else if (jsonmsg.shortcodestatus == "incache") {
                    msg = "* ShortCode already exists in cache.";
                }

                $("#spanAjaxErrorMsg").html(msg);

            },
            error: function () {
                //alert('sseldatautils.js AJAX Error');
            }

        });

    });

});



