(function ($) {
    "use strict";

    var SEARCH_FORM_SELECTOR = "#searchForm",
        SEARCH_TEXTBOX_SELECTOR = "#search-main",
        SEARCH_BUTTON_SELECTOR = "#searchButton",
        SEARCH_USERS_RADIO_SELECTOR = "#UserSearchType",
        WATERMARK_TEXT_USERS = "Enter name or email address",
        WATERMARK_TEXT_ACCOUNTS = "Enter account name, account ID or PAYE scheme";

    var changePlaceholder = function () {
        var watermark = null;

        if ($(SEARCH_USERS_RADIO_SELECTOR).is(":checked")) {
            watermark = WATERMARK_TEXT_USERS;
        } else {
            watermark = WATERMARK_TEXT_ACCOUNTS;
        }

        $(SEARCH_TEXTBOX_SELECTOR).attr("placeholder", watermark);
    };

    $(document).ready(function () {
        changePlaceholder();

        $(":radio", SEARCH_FORM_SELECTOR).change(changePlaceholder);

        $(SEARCH_BUTTON_SELECTOR).click(function () {
            if ($(SEARCH_TEXTBOX_SELECTOR).val().trim().length >= 2) {
                $(SEARCH_FORM_SELECTOR).submit();
            } else {
                $(SEARCH_TEXTBOX_SELECTOR).val("");
            }
        });
    });
}(jQuery));